using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ChatReqCanvasManger : MonoBehaviour
{
    public GameObject listP_UI, chatReq_UI,closeChatWithPartner_UI;
    public Text senderName;
    public Button accept, reject, closeChatWithPartnerButton;
    public static ChatReqCanvasManger Instance;
    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T)  && SceneManager.GetActiveScene().name == SceneNames.LiveEventsScene && !listP_UI.active)
        {
            NetworkManager.Instance.SendPartnerListRequest();
            listP_UI.SetActive(true);
        }
    }

    public void chatReqCanvasActive(bool state, string partnerName,int partnerId)
    {
        chatReq_UI.SetActive(state);
        if(state)
        {
            senderName.text = partnerName;
            AddListenerToChatReqButton(accept.GetComponent<Button>(), "accept", partnerId);
            AddListenerToChatReqButton(reject.GetComponent<Button>(), "reject", partnerId);
        }
        else
        {
            RemoveListenerFromChatReqButtons();
        }

    }

    public void closeChatWithPartnerRemotly(int partnerId)
    {
        closeChatWithPartnerButton.GetComponent<Button>().onClick.RemoveAllListeners();
        chatReqCanvasActive(false, "", 0);
        VoiceChatManager.Instance.closeTempVoiceChatWithPartner(partnerId);
    }

    void RemoveListenerFromChatReqButtons()
    {
        RemoveListenerFromChatReqButton(accept.GetComponent<Button>(), "accept");
        RemoveListenerFromChatReqButton(reject.GetComponent<Button>(), "reject");
    }

    void AddListenerToChatReqButton(Button button,string buttonActionName, int partnerId)
    {
        switch(buttonActionName)
        {
            case "accept":
                button.onClick.AddListener(() => acceptChat(partnerId));
                break;
            case "reject":
                button.onClick.AddListener(() => rejectChat(partnerId));
                break;
        }
        
    }

    void RemoveListenerFromChatReqButton(Button button, string name)
    {
        switch (name)
        {
            case "accept":
                button.onClick.RemoveAllListeners();
                break;
            case "reject":
                button.onClick.RemoveAllListeners();
                break;
        }

    }

    void acceptChat(int partnerId)
    {
        NetworkManager.Instance.sendAnswerForChatReq(partnerId,true);
        VoiceChatManager.Instance.makeTempVoiceChatWithPartner(partnerId);
        closeChatWithPartner_UI.SetActive(true);
        closeChatWithPartnerButton.GetComponent<Button>().onClick.AddListener(() => closeChatWithPartnerButtonHandler(partnerId));
    }

    void closeChatWithPartnerButtonHandler(int partnerId)
    {
        NetworkManager.Instance.SendCloseChatRequest(partnerId);
        closeChatWithPartnerRemotly(partnerId);
    }

    void rejectChat(int partnerId)
    {
        NetworkManager.Instance.sendAnswerForChatReq(partnerId, false);
    }
}
