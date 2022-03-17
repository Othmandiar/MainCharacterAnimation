using agora_gaming_rtc;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sfs2X;
using Sfs2X.Core;
using Sfs2X.Entities;
using Sfs2X.Entities.Data;
using Sfs2X.Requests;
using Sfs2X.Logging;

public class VoiceChatManager : MonoBehaviour
{
    string appID = "";

    public static VoiceChatManager Instance;
    public int agoraID;
    public static Dictionary<int, int> smartToAgoraID=new Dictionary<int, int>();
    IRtcEngine rtcEngine;
    IAudioEffectManager agoraAudioEffects;

    void Awake()
    {
        if (Instance)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
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

        rtcEngine.OnJoinChannelSuccess += OnJoinChannelSuccess;
        rtcEngine.OnLeaveChannel += OnLeaveChannel;
        rtcEngine.OnError += OnError;
        SmartFox smartFox = SmartFoxConnection.Connection;
        rtcEngine.JoinChannel(smartFox.LastJoinedRoom.Name);
        rtcEngine.EnableSoundPositionIndication(true);
        agoraAudioEffects = rtcEngine.GetAudioEffectManager();
    }

    void OnError(int error, string msg)
    {
        Debug.LogError("Error with Agora: " + msg);
    }

    void OnLeaveChannel(RtcStats stats)
    {
        Debug.Log("Left channel with duration " + stats.duration);
    }

    void OnJoinChannelSuccess(string channelName, uint uid, int elapsed)
    {
        Debug.Log("Joined channel " + channelName);
        agoraID = (int)uid;
        smartToAgoraID.Add(SmartFoxConnection.Connection.MySelf.Id,(int) uid);
        NetworkManager.Instance.SendAdminLiveEventListReq();
        NetworkManager.Instance.SendUsersLiveEventListReq();
    }

    public void spatialAudioAdmins()
    {

    }

    public IRtcEngine GetRtcEngine()
    {
        return rtcEngine;
    }

    void OnDestroy()
    {
        IRtcEngine.Destroy();
    }
}
