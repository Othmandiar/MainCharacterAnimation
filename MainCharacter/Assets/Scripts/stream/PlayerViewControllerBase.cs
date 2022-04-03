﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using agora_gaming_rtc;
using agora_utilities;
using Sfs2X;
using Sfs2X.Entities.Variables;
using Sfs2X.Requests;

public class PlayerViewControllerBase : IVideoChatClient
{
    public event Action OnViewControllerFinish;
    protected IRtcEngine mRtcEngine;
    protected Dictionary<uint, VideoSurface> UserVideoDict = new Dictionary<uint, VideoSurface>();
    protected const string SelfVideoName = "MyView";
    protected string mChannel;
    //    string logFilepath =
    //#if UNITY_EDITOR
    //    Application.dataPath + "/testagora.log";
    //#else
    //    Application.persistentDataPath + "/tesagora.log";
    //#endif
    protected bool remoteUserJoined = false;
    protected bool _enforcing360p = false; // the local view of the remote user resolution

    public PlayerViewControllerBase()
    {
        // Constructor, nothing to do for base
    }

    /// <summary>
    ///   Join a RTC channel
    /// </summary>
    /// <param name="channel"></param>
    public void Join(string channel)
    {
        Debug.Log("calling join (channel = " + channel + ")");

        if (mRtcEngine == null)
            return;

        mChannel = channel;

        // set callbacks (optional)
        mRtcEngine.OnJoinChannelSuccess = OnJoinChannelSuccess;
        //mRtcEngine.OnUserJoined = OnUserJoined;
        mRtcEngine.OnUserOffline = OnUserOffline;
        mRtcEngine.OnVideoSizeChanged = OnVideoSizeChanged;
        // Calling virtual setup function
        PrepareToJoin();

        // join channel
        mRtcEngine.JoinChannel(channel, null, 0);

        Debug.Log("initializeEngine done");
    }

    /// <summary>
    ///    Preparing video/audio/channel related characteric set up
    /// </summary>
    protected virtual void PrepareToJoin()
    {
        // enable video
        mRtcEngine.EnableVideo();
        // allow camera output callback
        //if(SceneNames.temp)
            mRtcEngine.EnableVideoObserver();

        
    }

    /// <summary>
    ///   Leave a RTC channel
    /// </summary>
    public virtual void Leave()
    {
        Debug.Log("calling leave");

        if (mRtcEngine == null)
            return;

        // leave channel
        mRtcEngine.LeaveChannel();
        // deregister video frame observers in native-c code
        mRtcEngine.DisableVideoObserver();
    }

    protected bool MicMuted { get; set; }

    public virtual void SetupUI()
    {
        StreamEventUIGetter.instance.leaveButton.GetComponent<Button>().onClick.AddListener(OnLeaveButtonClicked);

        GameObject muteButton= StreamEventUIGetter.instance.muteButton ;
        muteButton.GetComponent<Button>().onClick.AddListener(() => OnMuteButtonClicked(muteButton));
    }
     
    protected void OnMuteButtonClicked(GameObject muteButton)
    {
        MicMuted = !MicMuted;
        mRtcEngine.EnableLocalAudio(!MicMuted);
        mRtcEngine.MuteLocalAudioStream(MicMuted);
        Text text = muteButton.GetComponentInChildren<Text>();
        text.text = MicMuted ? "Unmute" : "Mute";
    }

    protected void OnLeaveButtonClicked()
    {
        Leave();
        UnloadEngine();

        if (OnViewControllerFinish != null)
        {
            OnViewControllerFinish();
        }
    }

    protected virtual void OnVideoSizeChanged(uint uid, int width, int height, int rotation)
    {
        Debug.LogWarningFormat("uid:{3} OnVideoSizeChanged width = {0} height = {1} for rotation:{2}", width, height, rotation, uid);
         
        if (UserVideoDict.ContainsKey(uid))
        {
            GameObject go = UserVideoDict[uid].gameObject;
            Vector2 v2 = new Vector2(width, height);
            RawImage image = go.GetComponent<RawImage>();
            if (_enforcing360p)
            {
                v2 = AgoraUIUtils.GetScaledDimension(width, height, 240f);
            }

            if (IsPortraitOrientation(rotation))
            {
                v2 = new Vector2(v2.y, v2.x);
            }
            image.rectTransform.sizeDelta = v2;
        }
    }

    bool IsPortraitOrientation(int rotation)
    {
        return rotation == 90 || rotation == 270;
    }

    /// <summary>
    ///   Load the Agora RTC engine with given AppID
    /// </summary>
    /// <param name="appId">Get the APP ID from Agora account</param>
    public void LoadEngine(string appId)
    {
        // init engine
        mRtcEngine = IRtcEngine.GetEngine(appId);

        //mRtcEngine.OnError = (code, msg) =>
        //{
        //    Debug.LogErrorFormat("RTC Error:{0}, msg:{1}", code, IRtcEngine.GetErrorDescription(code));
        //};

        //mRtcEngine.OnWarning = (code, msg) =>
        //{
        //    Debug.LogWarningFormat("RTC Warning:{0}, msg:{1}", code, IRtcEngine.GetErrorDescription(code));
        //};

        // mRtcEngine.SetLogFile(logFilepath);
        // enable log
        mRtcEngine.SetLogFilter(LOG_FILTER.DEBUG | LOG_FILTER.INFO | LOG_FILTER.WARNING | LOG_FILTER.ERROR | LOG_FILTER.CRITICAL);
    }

    // unload agora engine
    public virtual void UnloadEngine()
    {
        Debug.Log("calling unloadEngine");

        // delete
        if (mRtcEngine != null)
        {
            IRtcEngine.Destroy();  // Place this call in ApplicationQuit
            mRtcEngine = null;
        }
    }

    /// <summary>
    ///   Enable/Disable video
    /// </summary>
    /// <param name="pauseVideo"></param>
    public void EnableVideo(bool pauseVideo)
    {
        if (mRtcEngine != null)
        {
            if (!pauseVideo)
            {
                mRtcEngine.EnableVideo();
            }
            else
            {
                mRtcEngine.DisableVideo();
            }
        }
    }

    public virtual void OnSceneLoaded()
    {
        SetupUI();
    }

    // implement engine callbacks
    protected virtual void OnJoinChannelSuccess(string channelName, uint uid, int elapsed)
    {
        Debug.Log("JoinChannelSuccessHandler: uid = " + uid);
        StreamConnection.instance.mRtcEngine = mRtcEngine;
        StreamConnection.instance.isJoinedChannel = true;
        StreamConnection.instance.myAgoraID = uid;
        StreamConnection.instance.SendAgoraInfo();
        DesktopScreenShare app =(DesktopScreenShare)StreamConnection.instance.app;
        app.SetupUI();
    }

    // When a remote user joined, this delegate will be called. Typically
    // create a GameObject to render video on it
    //protected virtual void OnUserJoined(uint uid, int elapsed)
    //{
    //    Debug.Log("onUserJoined: uid = " + uid + " elapsed = " + elapsed);
    //    if(StreamConnection.instance.networkManager.streamAgoraIsAdmin.ContainsKey(uid))
    //    {
    //        if(StreamConnection.instance.networkManager.streamAgoraIsAdmin[])
    //        {
    //            VideoSurface videoSurface = makeImageSurface(uid.ToString());
    //            if (!ReferenceEquals(videoSurface, null))
    //            {
    //                // configure videoSurface
    //                videoSurface.SetForUser(uid);
    //                videoSurface.SetEnable(true);
    //                videoSurface.SetVideoSurfaceType(AgoraVideoSurfaceType.RawImage);
    //                videoSurface.SetGameFps(30);
    //                videoSurface.EnableFilpTextureApply(enableFlipHorizontal: true, enableFlipVertical: false);
    //                UserVideoDict[uid] = videoSurface;
    //            }
    //        }
            
    //    }
        
    //}

    // When remote user is offline, this delegate will be called. Typically
    // delete the GameObject for this user
    protected virtual void OnUserOffline(uint uid, USER_OFFLINE_REASON reason)
    {
        // remove video stream
        Debug.Log("onUserOffline: uid = " + uid + " reason = " + reason);
        if (UserVideoDict.ContainsKey(uid))
        {
            var surface = UserVideoDict[uid];
            surface.SetEnable(false);
            UserVideoDict.Remove(uid);
            GameObject.Destroy(surface.gameObject);
        }
    }

    //protected VideoSurface makeImageSurface(string goName)
    //{
    //    GameObject go = new GameObject();

    //    if (go == null)
    //    {
    //        return null;
    //    }

    //    go.name = goName;

    //    // to be renderered onto
    //    RawImage image = go.AddComponent<RawImage>();
    //    image.rectTransform.sizeDelta = new Vector2(1, 1);// make it almost invisible

    //    // make the object draggable
    //    go.AddComponent<UIElementDragger>();
    //    GameObject canvas = GameObject.Find("Canvas");
    //    if (canvas != null)
    //    {
    //        go.transform.SetParent(canvas.transform);
    //    }
    //    // set up transform
    //    go.transform.Rotate(0f, 0.0f, 180.0f);
    //    Vector2 v2 = AgoraUIUtils.GetRandomPosition(200);
    //    go.transform.position = new Vector3(v2.x, v2.y, 0);
    //    go.transform.localScale = Vector3.one;

    //    // configure videoSurface
    //    VideoSurface videoSurface = go.AddComponent<VideoSurface>();
    //    return videoSurface;
    //}
}
