using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ChatReqCanvasManger : MonoBehaviour
{
    public GameObject listP_UI, chatReq_UI;
    public Text senderName;
    public Button accept, reject;
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

    //public void chatReqCanvasActive(bool state,string _senderName )
    //{
    //    chatReq_UI.SetActive(state);
    //    senderName.text = _senderName;
    //    accept = GetComponent<Button>();
    //    accept.onClick.AddListener(() => actionToMaterial(index));

    //    accept = GetComponent<Button>();
    //    accept.onClick.AddListener(() => actionToMaterial(index));
    //}

    //void AddListenerToButton(Button button)
    //{
    //    button.onClick.AddListener(() => actionToMaterial(index));
    //}
}
