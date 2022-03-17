using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChatReqCanvasManger : MonoBehaviour
{
    public GameObject canvas;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T)  && SceneManager.GetActiveScene().name == SceneNames.LiveEventsScene && !canvas.active)
        {

            NetworkManager.Instance.SendPartnerListRequest();
            canvas.SetActive(true);
        }
    }
}
