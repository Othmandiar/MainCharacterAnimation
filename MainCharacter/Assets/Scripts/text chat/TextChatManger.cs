using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextChatManger : MonoBehaviour
{
    GameObject textChatUI;
    public InputField inputfield;
    GameObject scroll;
    public GameObject content,textPrefab ;
    public static TextChatManger instance;
    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        textChatUI = GameObject.FindGameObjectWithTag("textChatUI");
        scroll = GameObject.FindGameObjectWithTag("textChatScroll");

    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Alpha1))
        {
            textChatUI.SetActive(!textChatUI.active);
        }

        if(textChatUI.active && inputfield.text!="" && Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            NetworkManager.Instance.SendTextChatMsgRequest(inputfield.text);
            inputfield.text = "";
        }

    }

    public void addMsgToView(string msg )
    {

        GameObject msgobj = Instantiate(textPrefab, content.transform) as GameObject;
        msgobj.GetComponent<Text>().text = msg;
    }
}
