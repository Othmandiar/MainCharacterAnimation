
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using Sfs2X;
using Sfs2X.Core;
using Sfs2X.Entities;
using Sfs2X.Entities.Data;
using Sfs2X.Requests;
using Sfs2X.Logging;

// The Network manager sends the messages to server and handles the response

public class NetworkManager : MonoBehaviour
{
    private bool running = false;
    public  Dictionary<int, uint> smartToAgoraID = new Dictionary<int, uint>();
    public Dictionary<int, string> smartIdToname = new Dictionary<int, string>();
    private static NetworkManager instance;
    public static NetworkManager Instance
    {
        get
        {
            return instance;
        }
    }

    public SmartFox smartFox;  // The reference to SFS client

    void Awake()
    {
        instance = null;
        instance = this;
        smartFox = SmartFoxConnection.Connection;
        if (smartFox == null)
        {
            SceneManager.LoadScene(SceneNames.SingInScene);
            return;
        }
    }

    void Start()
    {
        SubscribeDelegates();
        SendSpawnRequest();

        TimeManager.Instance.Init();

        running = true;
    }

    public void OnUserVariableUpdate(BaseEvent evt)
    {
        List<string> changedVars = (List<string>)evt.Params["changedVars"];
        SFSUser user = (SFSUser)evt.Params["user"];
        
        if (user.Id == smartFox.MySelf.Id || SceneManager.GetActiveScene().name!=SceneNames.LiveEventsScene) return;
        if (smartToAgoraID.ContainsKey(user.Id))
        {
            if (changedVars.Contains("agoraID"))
            {
                smartToAgoraID[user.Id] = uint.Parse( user.GetVariable("agoraID").GetStringValue());
            }
            if(changedVars.Contains("isAdmin"))
            {
                bool isAdmin= user.GetVariable("isAdmin").GetBoolValue();
                if (isAdmin)
                     VoiceChatManager.Instance.setRemotePlayerToAdmin(smartToAgoraID[user.Id]);
                else
                    VoiceChatManager.Instance.mutePlayer(smartToAgoraID[user.Id]);
            }
        }

    }

    // This is needed to handle server events in queued mode
    void FixedUpdate()
    {
        if (!running) return;
            smartFox.ProcessEvents();
    }

    private void SubscribeDelegates()
    {
        smartFox.AddEventListener(SFSEvent.EXTENSION_RESPONSE, OnExtensionResponse);
        smartFox.AddEventListener(SFSEvent.USER_EXIT_ROOM, OnUserLeaveRoom);
        smartFox.AddEventListener(SFSEvent.CONNECTION_LOST, OnConnectionLost);
        smartFox.AddEventListener(SFSEvent.USER_VARIABLES_UPDATE, OnUserVariableUpdate);
    }

    private void UnsubscribeDelegates()
    {
        smartFox.RemoveAllEventListeners();
    }

    /// <summary>
    /// Send the request to server to spawn my player
    /// </summary>
    public void SendSpawnRequest()
    {
        Room room = smartFox.LastJoinedRoom;
        ExtensionRequest request = new ExtensionRequest("spawnMe", new SFSObject(), room);
        smartFox.Send(request);
    }


    public void SendChatRequest(string toName)
    {
        print("send to " + toName); 
        Room room = smartFox.LastJoinedRoom;
        SFSObject sfsobj = new SFSObject();
        
        sfsobj.PutInt("to", smartFox.UserManager.GetUserByName(toName).Id);
        ExtensionRequest request = new ExtensionRequest("chatReq", sfsobj, room);
        smartFox.Send(request);
    }

    public void SendPartnerListRequest()
    {
        Room room = smartFox.LastJoinedRoom;
        ExtensionRequest request = new ExtensionRequest("PList", new SFSObject(), room);
        smartFox.Send(request);
    }

    /// <summary>
    /// Send local transform to the server
    /// </summary>
    /// <param name="ntransform">
    /// A <see cref="NetworkTransform"/>
    /// </param>
    public void SendTransform(NetworkTransform ntransform)
    {
        Room room = smartFox.LastJoinedRoom;
        ISFSObject data = new SFSObject();
        ntransform.ToSFSObject(data);

        ExtensionRequest request = new ExtensionRequest("sendTransform", data, room); // True flag = UDP
        smartFox.Send(request);
    }

    /// <summary>
    /// Send local animation state to the server
    /// </summary>
    /// <param name="message">
    /// A <see cref="System.String"/>
    /// </param>
    /// <param name="layer">
    /// A <see cref="System.Int32"/>
    /// </param>
    /// 


    public void SendAnimationStates(List<KeyValuePair<string, bool>> boolStates, List<KeyValuePair<string, float>> floatStates)
    {
        Room room = smartFox.LastJoinedRoom;
        ISFSObject data = new SFSObject();
        for (int i = 0; i < boolStates.Count; i++)
        {
            data.PutBool(boolStates[i].Key, boolStates[i].Value);
        }
        for (int i = 0; i < floatStates.Count; i++)
        {
            data.PutFloat(floatStates[i].Key, floatStates[i].Value);
        }
        ExtensionRequest request = new ExtensionRequest("sendAnim", data, room);
        smartFox.Send(request);
    }

    /// <summary>
    /// Request the current server time. Used for time synchronization
    /// </summary>	
    public void TimeSyncRequest()
    {
        Room room = smartFox.LastJoinedRoom;
        ExtensionRequest request = new ExtensionRequest("getTime", new SFSObject(), room);
        smartFox.Send(request);
    }

    /// <summary>
    /// When connection is lost we load the login scene
    /// </summary>
    private void OnConnectionLost(BaseEvent evt)
    {
        UnsubscribeDelegates();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneManager.LoadScene(SceneNames.SingInScene);
    }

    // This method handles all the responses from the server
    private void OnExtensionResponse(BaseEvent evt)
    {
        try
        {
            string cmd = (string)evt.Params["cmd"];
            ISFSObject dt = (SFSObject)evt.Params["params"];

            if (cmd == "spawnPlayer")
            {
                HandleInstantiatePlayer(dt);
            }
            else if (cmd == "transform")
            {
                HandleTransform(dt);
            }
            else if (cmd == "notransform")
            {
                HandleNoTransform(dt);
            }
            else if (cmd == "anim")
            {
                HandleAnimation(dt);
            }
            else if (cmd == "time")
            {
                HandleServerTime(dt);
            }
            else if(cmd=="PList")
            {
                HandlePListe(dt);
            }
            else if(cmd == "chatReq")
            {
                HandleChatReq(dt);
            }
            else if (cmd == "ansChatReq")
            {
                HandleAnsChatReq(dt);
            }
            else if(cmd == "closeChatReq")
            {
                closeChatRequestHandler(dt);
            }
        }
        catch (Exception e)
        {
            Debug.Log("Exception handling response: " + e.Message + " >>> " + e.StackTrace);
        }

    }

    public void SendCloseChatRequest(int smartId)
    {
        Room room = smartFox.LastJoinedRoom;
        SFSObject sfsobj = new SFSObject();

        sfsobj.PutInt("to", smartId);
        ExtensionRequest request = new ExtensionRequest("closeChatReq", sfsobj, room);
        smartFox.Send(request);
    }

    void closeChatRequestHandler(ISFSObject dt)
    {
        int userId = dt.GetInt("toId");
        if (userId == smartFox.MySelf.Id)
        {
            ChatReqCanvasManger.Instance.closeChatWithPartnerRemotly(dt.GetInt("fromId"));
        }
    }

    public void sendAnswerForChatReq(int id, bool ans)
    {

        Room room = smartFox.LastJoinedRoom;
        ISFSObject data = new SFSObject();
        data.PutInt("to", id);
        data.PutBool("ans", ans);
        ExtensionRequest request = new ExtensionRequest("ansChatReq", data, room); // True flag = UDP
        smartFox.Send(request);

    }

    void HandleAnsChatReq(ISFSObject dt)
    {
        int userId = dt.GetInt("toId");
        if (userId == smartFox.MySelf.Id)
        {
            print("HandleAnsChatReq  " + dt.GetBool("ans"));
        }
    }

    void HandleChatReq(ISFSObject dt)
    {
        if(dt.GetUtfString("null") !=null)
        {
            print("HandleChatReq name is null");
            return;
        }
        int userId = dt.GetInt("toId");
        if (userId == smartFox.MySelf.Id)
        {
            int from = dt.GetInt("fromId");
            ChatReqCanvasManger.Instance.chatReqCanvasActive(true, smartFox.UserManager.GetUserById(from).Name, from);

        }
    }

    void HandlePListe(ISFSObject dt)
    {
        int userId = dt.GetInt("id");
        if(userId==smartFox.MySelf.Id)
        {
            PartnerScrollViewController scrollViewController=new PartnerScrollViewController();
            scrollViewController.LoadPlistToScrollView(dt.GetUtfStringArray("PartnerList"));
        }
    }

    // Instantiating player (our local FPS model, or remote 3rd person model)
    private void HandleInstantiatePlayer(ISFSObject dt)
    {
        ISFSObject playerData = dt.GetSFSObject("player");
        int userId = playerData.GetInt("id");
        NetworkTransform ntransform = NetworkTransform.FromSFSObject(playerData);
        
        User user = smartFox.UserManager.GetUserById(userId);
        string name = user.Name;

        if (userId == smartFox.MySelf.Id)
        {
            PlayerManager.Instance.SpawnPlayer(ntransform, name);
        }
        else
        {
            PlayerManager.Instance.SpawnEnemy(userId, ntransform, name);
        }

        if(SceneManager.GetActiveScene().name==SceneNames.LiveEventsScene)
        {
            smartToAgoraID.Add(userId,uint.MaxValue);
            VoiceChatManager.Instance.SendAgoraID();
        }
    }

    // Updating transform of the remote player from server
    private void HandleTransform(ISFSObject dt)
    {
        int userId = dt.GetInt("id");
        NetworkTransform ntransform = NetworkTransform.FromSFSObject(dt);
        if (userId != smartFox.MySelf.Id)
        {
            // Update transform of the remote user object
            NetworkTransformReceiver recipient = PlayerManager.Instance.GetRecipient(userId);
            if (recipient != null)
            {
                recipient.ReceiveTransform(ntransform);
            }
        }
    }

    // Server rejected transform message - force the local player object to what server said
    private void HandleNoTransform(ISFSObject dt)
    {
        int userId = dt.GetInt("id");
        NetworkTransform ntransform = NetworkTransform.FromSFSObject(dt);

        if (userId == smartFox.MySelf.Id)
        {
            // Movement restricted!
            // Update transform of the local object
            ntransform.Update(PlayerManager.Instance.GetPlayerObject().transform);
        }
    }

    // Synchronize the time from server
    private void HandleServerTime(ISFSObject dt)
    {
        long time = dt.GetLong("t");
        TimeManager.Instance.Synchronize(Convert.ToDouble(time));
    }


    // Synchronizing remote animation
    private void HandleAnimation(ISFSObject dt)
    {
        int userId = dt.GetInt("id");

        if (userId != smartFox.MySelf.Id)
        {
            NetwirkAnimationSync remoteAnim = PlayerManager.Instance.GetRecipient(userId).GetComponent<NetwirkAnimationSync>();
            if (remoteAnim != null)
            {
                remoteAnim.setAnimatorValues(dt);
            }
        }
    }


    // When a user leaves room destroy his object
    private void OnUserLeaveRoom(BaseEvent evt)
    {
        User user = (User)evt.Params["user"];
        Room room = (Room)evt.Params["room"];
        smartToAgoraID.Clear();
        PlayerManager.Instance.DestroyEnemy(user.Id);
        Debug.Log("User " + user.Name + " left");
    }

    void OnApplicationQuit()
    {
        UnsubscribeDelegates();
    }
}
