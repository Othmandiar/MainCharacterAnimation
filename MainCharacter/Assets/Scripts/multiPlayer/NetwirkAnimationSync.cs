using Sfs2X.Entities.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetwirkAnimationSync : MonoBehaviour
{
    Animator anim;
    List<string> boolStates = new List<string>(){ "Jump", "TwistDance", "RumbaDance", "HipHopDance", "Sit" };
    List<string> floatStates = new List<string>() { "Speed", "MotionSpeed" };
    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    public void setAnimatorValues(ISFSObject dt)
    {
        for(int i=0;i<boolStates.Count;i++)
        {
                anim.SetBool(boolStates[i], dt.GetBool(boolStates[i]));
        }

        for (int i = 0; i < floatStates.Count; i++)
        {
                anim.SetFloat(floatStates[i], dt.GetFloat(floatStates[i]));
        }

    }
}
