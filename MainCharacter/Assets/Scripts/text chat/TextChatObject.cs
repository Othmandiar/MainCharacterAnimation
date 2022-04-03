using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextChatObject : MonoBehaviour
{
    public int lineSize=20;
    GameObject scroll;
    // Start is called before the first frame update
    void Start()
    {
        scroll = GameObject.FindGameObjectWithTag("textChatScroll");
    }

    public void AddText(string data)
    {
        int linesCount = 0;
        //for()
    }
}
