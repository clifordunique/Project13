﻿using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class SequenceFlowController : MonoBehaviour {

    DialogueControllerScript dcScript;
    MusicManager musicManager;
    GameObject cam;

    public UnityEvent myEvent;

    void Start()
    {
        //dcScript = GameObject.FindGameObjectWithTag("Dialogue Controller").GetComponent<DialogueControllerScript>();
        musicManager = GameObject.FindGameObjectWithTag("Music").GetComponent<MusicManager>();
        cam = GameObject.FindGameObjectWithTag("MainCamera");
        //dcScript.LoadTextAsset(0);
    }


    //THIS IS ESPECIALLY TEMPORARY
    public void NextSequence()
    {
        /*
        currentSequence++;
        if (currentSequence == 1)
            enGroup1.GetComponent<FreezeEnemyScript>().UnfreezeEnemies();
        if (currentSequence == 2)
            dcScript.LoadTextAsset(1);
        if (currentSequence == 3)
        {
            musicManager.ActivateNextClip();
            StartCoroutine(cam.GetComponent<CamShakeScript>().InfiniteShake());
        }
        */
    }



}
