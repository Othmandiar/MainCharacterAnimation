using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Sfs2X.Entities.Variables;
using Sfs2X.Requests;
#if (UNITY_2018_3_OR_NEWER && UNITY_ANDROID)
using UnityEngine.Android;
#endif
using agora_gaming_rtc;

/// <summary>
///    TestHome serves a game controller object for this application.
/// </summary>
public class StreamConnection : MonoBehaviour
{

    // Use this for initialization
#if (UNITY_2018_3_OR_NEWER && UNITY_ANDROID)
    private ArrayList permissionList = new ArrayList();
#endif
    public  IVideoChatClient app = null;
    public static StreamConnection instance;
    private string HomeSceneName = SceneNames.MainScene;
    // PLEASE KEEP THIS App ID IN SAFE PLACE
    // Get your own App ID at https://dashboard.agora.io/
    [Header("Agora Properties")]
    [SerializeField]
    private string AppID = "your_appid";
    public bool isJoinedChannel = false;
    public uint myAgoraID;
    SFSceneChanger sFScene;
    public IRtcEngine mRtcEngine;

    void Awake()
    {
#if (UNITY_2018_3_OR_NEWER && UNITY_ANDROID)
		permissionList.Add(Permission.Microphone);         
		permissionList.Add(Permission.Camera);               
#endif
        
        if (SceneNames.temp)
            GameObject.FindGameObjectWithTag("streamUI").SetActive(true);
        else
            GameObject.FindGameObjectWithTag("streamUI").SetActive(false);
        instance = this;
        connectToAgoraVideo();
        print("Awake StreamConnection");
        // keep this alive across scenes
        //DontDestroyOnLoad(this.gameObject);
    }
    public void setSFSSceneChanger(SFSceneChanger _sFScene)
    {
        sFScene = _sFScene;
    }

    void Update()
    {
       // CheckExit();
    }


    public void setRemotePlayerToAdmin(uint uid)
    {
        if(isJoinedChannel)
        {
            Vector3 pos = StreamEventUIGetter.instance.screen.transform.position;
            VideoSurface videoSurface = StreamEventUIGetter.instance.screen.GetComponent<VideoSurface>();
            videoSurface.SetForUser(uid);
            videoSurface.SetEnable(true);


            //videoSurface.SetVideoSurfaceType(AgoraVideoSurfaceType.Renderer);
            //videoSurface.SetGameFps(30);
            //videoSurface.EnableFilpTextureApply(enableFlipHorizontal: true, enableFlipVertical: false);
            //videoSurface.transform.localPosition = new Vector3(pos.x, pos.y, pos.z);
        }
        
    }

    public void mutePlayer(uint id)
    {
        if(isJoinedChannel)
            mRtcEngine.AdjustUserPlaybackSignalVolume(id, 0);

    }

    public void SendAgoraInfo()
    {
        if (isJoinedChannel)
        {
            List<UserVariable> userVariables = new List<UserVariable>();
            userVariables.Add(new SFSUserVariable("agoraID", myAgoraID.ToString()));
            userVariables.Add(new SFSUserVariable("isAdmin", SceneNames.temp));
            NetworkManager.Instance.smartFox.Send(new SetUserVariablesRequest(userVariables));
        }

    }

    /// <summary>
    ///   Checks for platform dependent permissions.
    /// </summary>
    private void CheckPermissions()
    {
#if (UNITY_2018_3_OR_NEWER && UNITY_ANDROID)
        foreach(string permission in permissionList)
        {
            if (!Permission.HasUserAuthorizedPermission(permission))
            {                 
				Permission.RequestUserPermission(permission);
			}
        }
#endif
    }




    public void connectToAgoraVideo()
    {
        // get parameters (channel name, channel profile, etc.)
        string channelName = "streamEvent";

        app = new DesktopScreenShare(); // create app

        if (app == null) return;

        app.OnViewControllerFinish += OnViewControllerFinish;
        // load engine
        app.LoadEngine(AppID);
        // join channel and jump to next scene
        app.Join(channelName);
        //SceneManager.sceneLoaded += OnLevelFinishedLoading; // configure GameObject after scene is loaded
        //SceneManager.LoadScene(SceneNames.StreamEventsScene);
    }

    //bool _previewing = false;
    //public void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
    //{
    //    // Stop preview
    //    if (_previewing)
    //    {
    //        var engine = IRtcEngine.QueryEngine();
    //        if (engine != null)
    //        {
    //            engine.StopPreview();
    //            _previewing = false;
    //        }
    //    }

    //    if (!ReferenceEquals(app, null))
    //    {
    //        app.OnSceneLoaded(); // call this after scene is loaded
    //    }
    //    SceneManager.sceneLoaded -= OnLevelFinishedLoading;
    //}

    public void OnViewControllerFinish()
    {
        if (!ReferenceEquals(app, null))
        {
            app = null; // delete app
            SceneManager.LoadScene(HomeSceneName, LoadSceneMode.Single);
        }
        Destroy(gameObject);
    }



    void OnApplicationPause(bool paused)
    {
        if (!ReferenceEquals(app, null))
        {
            app.EnableVideo(paused);
        }
    }

    void OnApplicationQuit()
    {
        Debug.Log("OnApplicationQuit, clean up...");
        //if (_previewing)
        //{
        //    var engine = IRtcEngine.QueryEngine();
        //    if (engine != null)
        //    {
        //        engine.StopPreview();
        //        _previewing = false;
        //    }
        //}
        if (!ReferenceEquals(app, null))
        {
            app.UnloadEngine();
        }
        IRtcEngine.Destroy();
    }

    void CheckExit()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            // Gracefully quit on OS like Android, so OnApplicationQuit() is called
            Application.Quit();
#endif
        }
    }

    /// <summary>
    ///   This method shows the CheckVideoDeviceCount API call.  It should only be used
    //  after EnableVideo() call.
    /// </summary>
    /// <param name="engine">Video Engine </param>
    void CheckDevices(IRtcEngine engine)
    {
        VideoDeviceManager deviceManager = VideoDeviceManager.GetInstance(engine);
        deviceManager.CreateAVideoDeviceManager();

        int cnt = deviceManager.GetVideoDeviceCount();
        Debug.Log("Device count =============== " + cnt);
    }
}
