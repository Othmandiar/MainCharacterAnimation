using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sfs2X;
using Sfs2X.Core;
using Sfs2X.Entities;
using Sfs2X.Entities.Data;
using Sfs2X.Entities.Variables;
using Sfs2X.Requests;
using Sfs2X.Logging;

public class AdminButton : MonoBehaviour
{
    public Text label;
    public static bool isAdmin = true;
    SmartFox smartFox;
    // Start is called before the first frame update
    void Start()
    {

        smartFox = SmartFoxConnection.Connection;
        string labeltext = isAdmin ? "be user " : "be admin ";
        if (label != null)
        {
            label.text = labeltext;
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AdminButtonTapped()
    {
        string labeltext = !isAdmin ? "be user " : "be admin ";
        if (label != null)
        {
            label.text = labeltext;
        }
        isAdmin = !isAdmin;
        List<UserVariable> userVariables = new List<UserVariable>();
        userVariables.Add(new SFSUserVariable("isAdmin", isAdmin));
        NetworkManager.Instance.smartFox.Send(new SetUserVariablesRequest(userVariables));
    }

}
