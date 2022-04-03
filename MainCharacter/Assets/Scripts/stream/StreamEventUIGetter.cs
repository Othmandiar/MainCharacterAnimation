using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StreamEventUIGetter : MonoBehaviour
{
    public Dropdown dropdown;
    public GameObject muteButton, leaveButton,screen, ShareWindowButton, StopShareButton;
    public static StreamEventUIGetter instance;
    private void Awake()
    {
        instance = this;
    }
}
