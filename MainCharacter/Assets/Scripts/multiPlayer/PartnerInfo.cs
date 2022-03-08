using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartnerInfo : MonoBehaviour
{
    public new TextMesh name;

    private Renderer[] renderers;

    void Awake()
    {
        renderers = this.GetComponentsInChildren<Renderer>();
    }

    public void SetName(string name)
    {
        this.name.text = name;
    }

    public void Hide()
    {
        foreach (Renderer rend in renderers)
        {
            //rend.enabled = false;
        }
    }

    public void Show()
    {
        foreach (Renderer rend in renderers)
        {
            rend.enabled = true;
        }
    }

    void LateUpdate()
    {
        transform.LookAt(Camera.main.transform);
    }

}
