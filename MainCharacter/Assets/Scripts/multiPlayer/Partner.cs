using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Partner : MonoBehaviour
{
    public PartnerInfo info;
    public int sfsID;
    private bool showingInfo = false;

    private float timeSinceLastRaycast = 0;
    private readonly float showInfoTime = 0.5f;

    public void Init(string name ,int smartID)
    {
        info.SetName(name);
        sfsID = smartID;
        info.Hide();
        showingInfo = false;
    }

    public void ShowInfo()
    {
        if (!showingInfo)
        {
            info.Show();
            showingInfo = true;
        }
    }

    public void HideInfo()
    {
        if (showingInfo)
        {
            info.Hide();
            showingInfo = false;
        }
    }

    void RaycastMessage()
    {
        timeSinceLastRaycast = 0;
    }


    void Update()
    {
        timeSinceLastRaycast += Time.deltaTime;
        if (timeSinceLastRaycast < showInfoTime)
        {
            ShowInfo();
        }
        else
        {
            HideInfo();
        }
    }
}
