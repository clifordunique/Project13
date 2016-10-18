﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class PlayerSelectScript : MonoBehaviour {

    public GameObject[] selectReticles;             //Reticles to be activated upon player join

    //Determine if game should be able to start
    int players = 0;                                //Amount of players joined in the game
    int charactersSelected = 0;                     //Determines number of characters selected to check if all players are ready


    void Start()
    {
        Cursor.visible = false;
    }

    void Update()
    {
        if (players < 4)
            WatchForPlayerJoin();
    }

    void WatchForPlayerJoin()
    {
        WatchForPlayer1Join();
 
        if (Input.GetButtonDown("2_X") && !selectReticles[1].activeSelf)
            JoinPlayer(1);
        if (Input.GetButtonDown("3_X") && !selectReticles[2].activeSelf)
            JoinPlayer(2);
        if (Input.GetButtonDown("4_X") && !selectReticles[3].activeSelf)
            JoinPlayer(3);

    }

    //Watches specifically for player 1 to join based on keyboard or controller
    void WatchForPlayer1Join()
    {
        if ((Input.GetMouseButton(0) || Input.GetButtonDown("1_X")) && !selectReticles[0].activeSelf)
        {
            JoinPlayer(0);
        }
    }


    void JoinPlayer(int index)
    {
        selectReticles[index].SetActive(true);
        players++;
    }
}