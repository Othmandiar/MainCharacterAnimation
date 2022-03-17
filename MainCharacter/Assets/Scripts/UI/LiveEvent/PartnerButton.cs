using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartnerButton : MonoBehaviour
{
    public string name;
    [SerializeField] Text ButtonText;
    float timeToSendReq = 0;

    void Start()
    {
        ButtonText.text = name;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnButtonClick()
    {
        if(timeToSendReq<=Time.time)
        {
            NetworkManager.Instance.SendChatRequest(name);
            timeToSendReq = Time.time + 4f;
        }

    }
}
