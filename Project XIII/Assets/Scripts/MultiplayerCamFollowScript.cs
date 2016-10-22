﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MultiplayerCamFollowScript : MonoBehaviour {


    float zoomMultiplier = 1.5f;
    float followDelay = .8f;

    GameObject[] players;

	// Use this for initialization
	void Start () {
        players = GameObject.FindGameObjectsWithTag("Player");
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        Vector3 midpoint = GetPlayersMidpoint();


        if(midpoint != new Vector3())
        {
            float distance = GetDistance();

            Vector3 cameraDestination = midpoint - transform.forward * distance * zoomMultiplier;
            GetComponent<Camera>().orthographicSize = distance;

            transform.position = Vector3.Slerp(transform.position, cameraDestination, followDelay);

            if ((cameraDestination - transform.position).magnitude <= 0.05f)
                transform.position = cameraDestination;
        }

	}

    //Get midpoint position between all players
    Vector3 GetPlayersMidpoint()
    {
        Vector3 midpoint = new Vector3();
        int activeCount = 0;
        
        foreach(GameObject player in players)
        {
            if (player.activeSelf)
            {
                midpoint += player.transform.position;
                activeCount++;
            }
        }

        if (activeCount != 0)
            return midpoint / activeCount;
        else
            return new Vector3();
    }

    //Get the distance from the minimum to maximum point
    float GetDistance()
    {
        Vector2[] pointArray = new Vector2[2];  //Min position, max position
        bool gotFirstPoint = false;

        foreach (GameObject player in players)
        {
            if (player.activeSelf)
            {
                if (!gotFirstPoint)
                {
                    pointArray[0] = new Vector2();
                    pointArray[1] = new Vector2();

                    pointArray[0].x = player.transform.position.x;
                    pointArray[1].x = player.transform.position.x;
                    pointArray[0].y = player.transform.position.y;
                    pointArray[1].y = player.transform.position.y;

                    gotFirstPoint = true;
                }
                else
                {
                    pointArray[0].x = Mathf.Min(pointArray[0].x, player.transform.position.x);
                    pointArray[1].x = Mathf.Max(pointArray[1].x, player.transform.position.x);
                    pointArray[0].y = Mathf.Min(pointArray[0].y, player.transform.position.y);
                    pointArray[1].y = Mathf.Max(pointArray[1].y, player.transform.position.y);
                }

            }
        }
        return (pointArray[1] - pointArray[0]).magnitude;

    }
}
