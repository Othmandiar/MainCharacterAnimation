using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartnerScrollViewController : MonoBehaviour
{
    static GameObject ScrollView;
    static GameObject BtnPref;
    static Transform Content;
    [SerializeField]  GameObject ScrollView1;
    [SerializeField]  GameObject BtnPref1;
    [SerializeField]  Transform Content1;
    // Start is called before the first frame update
    void Start()
    {
        ScrollView = ScrollView1;
        BtnPref = BtnPref1;
        Content = Content1;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void LoadPlistToScrollView(string [] list)
    {
        for (int i = 0; i < list.Length; i++)
        {
            GameObject levelBtnObj = Instantiate(BtnPref, Content) as GameObject;
            levelBtnObj.GetComponent<PartnerButton>().name = list[i];
        }
    }
}
