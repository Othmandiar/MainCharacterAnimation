using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloseChatReqCanvas : MonoBehaviour
{
    public GameObject canvas, content;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void onPlayerClick()
    {
        for (int i = 0; i < content.transform.childCount; i++)
        {
            Destroy( content.transform.GetChild(i).gameObject);
        }
        canvas.SetActive(false);
    }
}
