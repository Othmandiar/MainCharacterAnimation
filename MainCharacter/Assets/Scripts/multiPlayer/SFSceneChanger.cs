using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Sfs2X;
using Sfs2X.Logging;
using Sfs2X.Util;
using Sfs2X.Core;
using Sfs2X.Entities;
using Sfs2X.Requests;
using UnityEngine.SceneManagement;

public class SFSceneChanger : MonoBehaviour
{
    private SmartFox sfs;
    string nameOfScene;
    // Start is called before the first frame update
    void Start()
    {
        sfs = SmartFoxConnection.Connection;
        sfs.AddEventListener(SFSEvent.ROOM_JOIN, OnRoomJoin);
    }

    // Update is called once per frame
    void Update()
    {
        if (sfs != null)
            sfs.ProcessEvents();
    }

    public void JoinLiveEventRoom()
    {
        string room = "liveEvent";
        print("in sfs change scene   " + room);
        nameOfScene = SceneNames.LiveEventsScene;
        sfs.Send(new JoinRoomRequest(room));
    }

    public void JoinStreamEventRoom()
    {
        string room = "stream";
        print("in sfs change scene   " + room);
        nameOfScene = SceneNames.StreamEventsScene;
        sfs.Send(new JoinRoomRequest(room));
    }

    //public void JoinLiveEventRoom()
    //{
    //    string room = "liveEvent";
    //    nameOfScene = SceneNames.LiveEventsScene;
    //    sfs.Send(new JoinRoomRequest(room));
    //}

    private void OnRoomJoin(BaseEvent evt)
    {
        print("in sfs change scene   OnRoomJoin");
        reset();
        //if(!(sfs.LastJoinedRoom.Name== "streamEvent" ))
        SceneManager.LoadScene(nameOfScene);
    }
    private void reset()
    {
        // Remove SFS2X listeners
        sfs.RemoveAllEventListeners();
    }
}
