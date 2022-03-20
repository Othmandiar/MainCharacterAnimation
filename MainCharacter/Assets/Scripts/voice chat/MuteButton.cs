using agora_gaming_rtc;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MuteButton : MonoBehaviour
{
    public Text MuteButtonlabel;
    public Text MuteAllButtonlabel;
    // Start is called before the first frame update
    void Start()
    {
        string labeltext =  "Mute all ";
        if (MuteAllButtonlabel != null)
        {
            MuteAllButtonlabel.text = labeltext;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    bool isMuted = false;
    public void MuteButtonTapped()
    {
        string labeltext = isMuted ? "Mute" : "Unmute";
        if (MuteButtonlabel != null)
        {
            MuteButtonlabel.text = labeltext;
        }
        isMuted = !isMuted;
        VoiceChatManager.Instance.GetRtcEngine().EnableLocalAudio(!isMuted);

    }

    public void MuteAllButtonTapped()
    {
        string labeltext = isMuted ? "Mute all " : "Unmute all";
        if (MuteAllButtonlabel != null)
        {
            MuteAllButtonlabel.text = labeltext;
        }
        isMuted = !isMuted;
        VoiceChatManager.Instance.muteAllRemoteAudio(isMuted);

    }

}
