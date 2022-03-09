
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Spawns player and items objects, stores them in collections and provides access to them
    public class PlayerManager : MonoBehaviour
    {

        public GameObject PartnerPrefab;
        public GameObject playerPrefab;


        private GameObject playerObj;

        private static PlayerManager instance;
        public static PlayerManager Instance
        {
            get
            {
                return instance;
            }
        }

        public GameObject GetPlayerObject()
        {
            return playerObj;
        }

        private Dictionary<int, NetworkTransformReceiver> recipients = new Dictionary<int, NetworkTransformReceiver>();
        void Awake()
        {
            instance = this;
            Cursor.visible = true;
            //Cursor.lockState = CursorLockMode.Confined;
        }

        public void SpawnPlayer(NetworkTransform ntransform, string name, int score)
        {
            if (Camera.main != null)
            {
                Destroy(Camera.main.gameObject);
            }
            playerObj = GameObject.Instantiate(playerPrefab) as GameObject;
        for(int i=0;i< playerObj.transform.GetChildCount();i++)
        {
            if(playerObj.transform.GetChild(i).tag=="Player")
            {
                playerObj = playerObj.transform.GetChild(i).gameObject ;

            }
        }
        playerObj.transform.position = ntransform.Position;
        //playerObj.transform.position = new Vector3(0,10,0);
        playerObj.transform.localEulerAngles = ntransform.AngleRotationFPS;
        // playerObj.SendMessage("StartSendTransform");
        playerObj.GetComponent<NetworkTransformSender>().StartSendTransform();
        }

        public void SpawnEnemy(int id, NetworkTransform ntransform, string name, int score)
        {
            GameObject playerObj = GameObject.Instantiate(PartnerPrefab) as GameObject;
            playerObj.transform.position = ntransform.Position;
        //playerObj.transform.position = new Vector3(0, 10, 0);
        playerObj.transform.localEulerAngles = ntransform.AngleRotationFPS;
        //AnimationSynchronizer animator = playerObj.GetComponent<AnimationSynchronizer>();
        //animator.StartReceivingAnimation();

        Partner enemy = playerObj.GetComponent<Partner>();
            enemy.Init(name);

            recipients[id] = playerObj.GetComponent<NetworkTransformReceiver>();
        }

        public NetworkTransformReceiver GetRecipient(int id)
        {
            if (recipients.ContainsKey(id))
            {
                return recipients[id];
            }
            return null;
        }

        public void DestroyEnemy(int id)
        {
            NetworkTransformReceiver rec = GetRecipient(id);
            if (rec == null) return;
            Destroy(rec.gameObject);
            recipients.Remove(id);
        }

        //public void SyncAnimation(int id, string msg, int layer)
        //{
        //    NetworkTransformReceiver rec = GetRecipient(id);

        //    if (rec == null) return;

        //    if (layer == 0)
        //    {
        //        rec.GetComponent<AnimationSynchronizer>().RemoteStateUpdate(msg);
        //    }
        //    else if (layer == 1)
        //    {
        //        rec.GetComponent<AnimationSynchronizer>().RemoteSecondStateUpdate(msg);
        //    }
        //}

        public void KillMe()
        {
            if (playerObj == null) return;
            Camera.main.transform.parent = null;
            Destroy(playerObj);
            playerObj = null;
        }

        
    }

