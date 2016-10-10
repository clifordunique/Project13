﻿using UnityEngine;
using System.Collections;

public class EnrageZoneScript : MonoBehaviour {

    void OnTriggerEnter2D(Collider2D col)
    {
        Debug.Log("test ");
        if(col.tag == "Player")
        {
            foreach (Transform child in transform)
            {
                child.gameObject.GetComponent<Enemy>().target = col.gameObject;
                child.gameObject.GetComponent<Enemy>().inPursuit = true;
            }
        }
    }
}
