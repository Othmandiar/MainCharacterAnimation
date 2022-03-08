using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SimpleRemoteInterpolation : MonoBehaviour
{

    Animator anim;
    private Vector3 desiredPos;
    private Quaternion desiredRot;

    private float dampingFactor = 50f;

    void Start()
    {
        anim = GetComponent<Animator>();
        desiredPos = this.transform.position;
        desiredRot = this.transform.rotation;
    }

    public void SetTransform(Vector3 pos, Quaternion rot, bool interpolate)
    {
        // If interpolation, then set the desired pososition+rotation; else force set (for spawning new models)
        if (interpolate)
        {
            desiredPos = pos;
            desiredRot = rot;
        }
        else
        {
            this.transform.position = pos;
            this.transform.rotation = rot;
        }
    }

    public void SetAnim(string layer)
    {
        print("layer   " + layer);
        anim.Play(layer);
    }

    void Update()
    {
        this.transform.position = Vector3.Lerp(transform.position, desiredPos, Time.deltaTime * dampingFactor);
        this.transform.rotation = Quaternion.Slerp(transform.rotation, desiredRot, Time.deltaTime * dampingFactor);
    }
}
