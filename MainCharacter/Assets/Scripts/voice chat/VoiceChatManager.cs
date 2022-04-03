using agora_gaming_rtc;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sfs2X;
using Sfs2X.Entities.Variables;
using Sfs2X.Requests;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class VoiceChatManager : MonoBehaviour
{
    string appID = "9e636eb0b6a9455bb0ea9ca2d60c5665";
    public Text tt;
    public static VoiceChatManager Instance;
    public Dictionary<int, uint> partnerTempChat=new Dictionary<int, uint>();
    uint myAgoraID;
    IRtcEngine rtcEngine;
    SmartFox smartFox;
    AgoraChannel channel;
    public static bool isJoinedChannel=false;
    void Awake()
    {
        smartFox = SmartFoxConnection.Connection;
        if (smartFox == null)
        {
            SceneManager.LoadScene(SceneNames.SingInScene);
            return;
        }
        if (Instance)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    void Start()
    {
        if (string.IsNullOrEmpty(appID))
        {
            Debug.LogError("App ID not set in VoiceChatManager script");
            return;
        }

        rtcEngine = IRtcEngine.GetEngine(appID);
        rtcEngine.EnableSoundPositionIndication(true);
        rtcEngine.OnJoinChannelSuccess += OnJoinChannelSuccess;
        rtcEngine.OnLeaveChannel += OnLeaveChannel;
        rtcEngine.OnError += OnError;

        rtcEngine.JoinChannel(smartFox.LastJoinedRoom.Name);


    }

    private void Update()
    {

    }

    public void SendAgoraID()
    {
        if(isJoinedChannel)
        {
            List<UserVariable> userVariables = new List<UserVariable>();
            userVariables.Add(new SFSUserVariable("agoraID", myAgoraID.ToString()));
            NetworkManager.Instance.smartFox.Send(new SetUserVariablesRequest(userVariables));
        }

    }

    void OnError(int error, string msg)
    {
        Debug.LogError("Error with Agora: " + msg);
    }

    void OnLeaveChannel(RtcStats stats)
    {
        isJoinedChannel = false;
        Debug.Log("Left channel with duration " + stats.duration);
    }

    void OnJoinChannelSuccess(string channelName, uint uid, int elapsed)
    {
        myAgoraID = uid;
        Debug.Log("Joined channel " + channelName + " uint  "+ uid);
        isJoinedChannel = true;
        if (NetworkManager.Instance.smartToAgoraID.ContainsKey(smartFox.MySelf.Id))
        {
            
            NetworkManager.Instance.smartToAgoraID[smartFox.MySelf.Id] = uid;
            SendAgoraID();
        }
        else
        {
            NetworkManager.Instance.smartToAgoraID.Add(smartFox.MySelf.Id, uid);
            SendAgoraID();
        }
        tt.text = uid.ToString();
    }


    public IRtcEngine GetRtcEngine()
    {
        return rtcEngine;
    }

    void OnDestroy()
    {
        IRtcEngine.Destroy();
    }

    public void makeTempVoiceChatWithPartner(int smartID)
    {
        if(!partnerTempChat.ContainsKey(smartID))
        {
            uint partnerAgoraId = NetworkManager.Instance.smartToAgoraID[smartID];
            partnerTempChat.Add(smartID, partnerAgoraId);
            setRemotePlayerToAdmin(partnerAgoraId);
        }
    }

    public void closeTempVoiceChatWithPartner(int smartID)
    {
        if (partnerTempChat.ContainsKey(smartID))
        {
            uint partnerAgoraId = NetworkManager.Instance.smartToAgoraID[smartID];
            partnerTempChat.Remove(smartID);
            mutePlayer(partnerAgoraId);
        }
    }


    public void setRemotePlayerToAdmin(uint id)
    {
        //int test = rtcEngine.GetAudioEffectManager().SetRemoteVoicePosition(id, 0f, 100f);
        rtcEngine.AdjustUserPlaybackSignalVolume(id, 100);
        //print(" agoraAudioEffects call   " + test);
    }

    public void mutePlayer(uint id)
    {
        print("mutePlayer agoraID  "+ id + "   myself id  "+NetworkManager.Instance.smartToAgoraID[ SmartFoxConnection.Connection.MySelf.Id] );
        //int test= rtcEngine.GetAudioEffectManager().SetRemoteVoicePosition(id, 0f,0f);
        int t= rtcEngine.AdjustUserPlaybackSignalVolume(id, 0);
        print(t);
        //print(" agoraAudioEffects call  mutePlayer " + test);
    }

    public void muteAllRemoteAudio(bool state)
    {
        rtcEngine.MuteAllRemoteAudioStreams(state);
    }
}
