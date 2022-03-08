using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Partner : MonoBehaviour
{
    public PartnerInfo info;

    private bool showingInfo = false;

    private float timeSinceLastRaycast = 0;
    private readonly float showInfoTime = 0.5f;

    public void Init(string name)
    {
        info.SetName(name);
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
