using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Sfs2X;
using Sfs2X.Core;
using Sfs2X.Entities;
using Sfs2X.Entities.Data;
using Sfs2X.Entities.Variables;
using Sfs2X.Requests;
using Sfs2X.Logging;


public class MultiPlayerGameManger : MonoBehaviour
{
    
    public GameObject playerPrefab,remotePrefab;
    private SmartFox sfs;
    private GameObject localPlayer;
    private StarterAssets.ThirdPersonController localPlayerController;
    private Dictionary<SFSUser, GameObject> remotePlayers = new Dictionary<SFSUser, GameObject>();


    // Start is called before the first frame update
    void Start()
    {

        if (!SmartFoxConnection.IsInitialized)
        {
            SceneManager.LoadScene(2);
            return;
        }

        sfs = SmartFoxConnection.Connection;

        // Register callback delegates
        sfs.AddEventListener(SFSEvent.OBJECT_MESSAGE, OnObjectMessage);
        sfs.AddEventListener(SFSEvent.CONNECTION_LOST, OnConnectionLost);
        sfs.AddEventListener(SFSEvent.USER_VARIABLES_UPDATE, OnUserVariableUpdate);
        sfs.AddEventListener(SFSEvent.USER_EXIT_ROOM, OnUserExitRoom);
        sfs.AddEventListener(SFSEvent.USER_ENTER_ROOM, OnUserEnterRoom);
        sfs.AddEventListener(SFSEvent.EXTENSION_RESPONSE, OnExtensionResponse);
        SpawnLocalPlayer();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    //string getCurAnimStateName()
    //{
    //    List<string> stateNames = new List<string>(){ "endDance", "Idle Walk Run Blend", "JumpStart", "InAir" };
    //    for(int i=0;i<stateNames.Count;i++)
    //    {
    //        if (localPlayerController._animator.GetCurrentAnimatorStateInfo(0).IsName(stateNames[i]))
    //        {
    //            return stateNames[i];
    //        }
    //    }
    //    return "";

    //}
     
    //public string GetCurrentClipName()
    //{
    //    AnimatorClipInfo[] clipInfo;
    //    if (localPlayerController._animator!=null)
    //    {
    //        clipInfo = localPlayerController._animator.GetCurrentAnimatorClipInfo(0);
    //        if (clipInfo.Length>0)
    //            return clipInfo[0].clip.name;
            
    //    }
    //    return "";

    //}
    void FixedUpdate()
    {
        if (sfs != null)
        {

            sfs.ProcessEvents();


            ISFSObject isfs=new SFSObject();
            isfs.PutInt("id", sfs.MySelf.Id);
           //string stateName = GetCurrentClipName();
           // if (stateName != "")
           // {
           //     print("send");
           //     isfs.PutText("state", stateName);
           //     sfs.Send(new ExtensionRequest("anim", isfs));
           // }


            // If we spawned a local player, send position if movement is dirty
            if (localPlayer != null && localPlayerController != null && localPlayerController.isMove)
            {
                List<UserVariable> userVariables = new List<UserVariable>();
                userVariables.Add(new SFSUserVariable("x", (double)localPlayer.transform.position.x));
                userVariables.Add(new SFSUserVariable("y", (double)localPlayer.transform.position.y));
                userVariables.Add(new SFSUserVariable("z", (double)localPlayer.transform.position.z));
                userVariables.Add(new SFSUserVariable("rot", (double)localPlayer.transform.rotation.eulerAngles.y));
                //userVariables.Add(new SFSUserVariable("anim", (string)localPlayerController._animator.GetCurrentAnimatorStateInfo(0).nameHash));
                localPlayerController.isMove = false;
                sfs.Send(new SetUserVariablesRequest(userVariables));
            }
        }
    }


    void OnExtensionResponse(BaseEvent e)
    {
        string cmd = (string)e.Params["cmd"];
        ISFSObject obj = (SFSObject)e.Params["params"];
        print(cmd);
        if (cmd == "anim")
        {
            int id = obj.GetInt("id");
            string layer = obj.GetText("state");
            foreach (KeyValuePair<SFSUser, GameObject> entry in remotePlayers)
            {
                if (entry.Key.Id == id)
                {
                    entry.Value.GetComponent<SimpleRemoteInterpolation>().SetAnim(layer);
                    break;
                }
            }
        }
    }

    public void OnUserVariableUpdate(BaseEvent evt)
    {
        
        List<string> changedVars = (List<string>)evt.Params["changedVars"];
        SFSUser user = (SFSUser)evt.Params["user"];

        if (user == sfs.MySelf) return;
       
        if (!remotePlayers.ContainsKey(user))
        {
            // New client just started transmitting - lets create remote player
            Vector3 pos = new Vector3(0, 1, 0);
            if (user.ContainsVariable("x") && user.ContainsVariable("y") && user.ContainsVariable("z"))
            {
                pos.x = (float)user.GetVariable("x").GetDoubleValue();
                pos.y = (float)user.GetVariable("y").GetDoubleValue();
                pos.z = (float)user.GetVariable("z").GetDoubleValue();
            }
            float rotAngle = 0;
            if (user.ContainsVariable("rot"))
            {
                rotAngle = (float)user.GetVariable("rot").GetDoubleValue();
            }
            
            SpawnRemotePlayer(user, pos, Quaternion.Euler(0, rotAngle, 0));
        }

        // Check if the remote user changed his position or rotation
        if (changedVars.Contains("x") && changedVars.Contains("y") && changedVars.Contains("z") && changedVars.Contains("rot"))
        {
            // Move the character to a new position...
            remotePlayers[user].GetComponent<SimpleRemoteInterpolation>().SetTransform(
                new Vector3((float)user.GetVariable("x").GetDoubleValue(), (float)user.GetVariable("y").GetDoubleValue(), (float)user.GetVariable("z").GetDoubleValue()),
                Quaternion.Euler(0, (float)user.GetVariable("rot").GetDoubleValue(), 0),
                true);
        }

        
    }

    private void SpawnRemotePlayer(SFSUser user,  Vector3 pos, Quaternion rot)
    {
        // See if there already exists a model so we can destroy it first
        if (remotePlayers.ContainsKey(user) && remotePlayers[user] != null)
        {
            Destroy(remotePlayers[user]);
            remotePlayers.Remove(user);
        }

        // Lets spawn our remote player model
        GameObject remotePlayer = Instantiate(remotePrefab) ;
        remotePlayer.GetComponent<SimpleRemoteInterpolation>().SetTransform(pos, rot, false);
        // Lets track the dude
        remotePlayers.Add(user, remotePlayer);
    }

    private void SpawnLocalPlayer()
    {
        Vector3 pos;
        Quaternion rot;

        // See if there already exists a model - if so, take its pos+rot before destroying it
        if (localPlayer != null)
        {
            pos = localPlayer.transform.position;
            rot = localPlayer.transform.rotation;
            Camera.main.transform.parent = null;
            Destroy(localPlayer);
        }
        else
        {
            pos = new Vector3(2.35f, 1, -26.3f);
            rot = Quaternion.identity;
        }

        // Lets spawn our local player model
        localPlayer = Instantiate(playerPrefab) ;
        localPlayer.GetComponentInChildren<Text>().text = sfs.MySelf.Name;
        localPlayerController = localPlayer.GetComponentInChildren<StarterAssets.ThirdPersonController>();
        for (int i = 0; i < localPlayer.transform.childCount; i++)
        {
            GameObject child = localPlayer.transform.GetChild(i).gameObject;
            if (child.tag == "Player")
            {
                localPlayer=child;
                break;
            }
        }
        localPlayer.transform.position = pos;
        localPlayer.transform.rotation = rot;
        
        

        List<UserVariable> userVariables = new List<UserVariable>();
        userVariables.Add(new SFSUserVariable("model", 1));

        sfs.Send(new SetUserVariablesRequest(userVariables));
    }

    public void OnConnectionLost(BaseEvent evt)
    {
        // Reset all internal states so we kick back to login screen
        sfs.RemoveAllEventListeners();
        SceneManager.LoadScene(2);
    }

    public void OnObjectMessage(BaseEvent evt)
    {
        // The only messages being sent around are remove messages from users that are leaving the game
        ISFSObject dataObj = (SFSObject)evt.Params["message"];
        SFSUser sender = (SFSUser)evt.Params["sender"];

        if (dataObj.ContainsKey("cmd"))
        {
            switch (dataObj.GetUtfString("cmd"))
            {
                case "rm":
                    Debug.Log("Removing player unit " + sender.Id);
                    RemoveRemotePlayer(sender);
                    break;
            }
        }
    }

    private void RemoveRemotePlayer(SFSUser user)
    {
        if (user == sfs.MySelf) return;

        if (remotePlayers.ContainsKey(user))
        {
            Destroy(remotePlayers[user]);
            remotePlayers.Remove(user);
        }
    }
    public void OnUserEnterRoom(BaseEvent evt)
    {
        // User joined - and we might be standing still (not sending position updates); so let's send him our position
        if (localPlayer != null)
        {
            List<UserVariable> userVariables = new List<UserVariable>();
            userVariables.Add(new SFSUserVariable("x", (double)localPlayer.transform.position.x));
            userVariables.Add(new SFSUserVariable("y", (double)localPlayer.transform.position.y));
            userVariables.Add(new SFSUserVariable("z", (double)localPlayer.transform.position.z));
            userVariables.Add(new SFSUserVariable("rot", (double)localPlayer.transform.rotation.eulerAngles.y));
            sfs.Send(new SetUserVariablesRequest(userVariables));
        }
    }

    public void OnUserExitRoom(BaseEvent evt)
    {
        // Someone left - lets make certain they are removed if they didn't nicely send a remove command
        SFSUser user = (SFSUser)evt.Params["user"];
        RemoveRemotePlayer(user);
    }

}
