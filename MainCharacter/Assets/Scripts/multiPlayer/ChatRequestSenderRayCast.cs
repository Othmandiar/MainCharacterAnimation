using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChatRequestSenderRayCast : MonoBehaviour
{
    public Partner info;
    public float disToSendRequest = 5f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnMouseDown()
    {
        if(Mathf.Abs( Vector3.SqrMagnitude( StarterAssets.ThirdPersonController.instance.GetComponent<Transform>().position-transform.position)) <=disToSendRequest*disToSendRequest)
            NetworkManager.Instance.SendChatRequest(info.sfsID);
    }
}
