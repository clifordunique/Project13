﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC : MonoBehaviour {

    public GameObject interactionPrompt;

    HashSet<GameObject> playerHash = new HashSet<GameObject>();

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player" && !playerHash.Contains(collision.gameObject))
        {
            playerHash.Add(collision.gameObject);
            interactionPrompt.SetActive(true);
        }

    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.tag == "Player" && playerHash.Contains(collision.gameObject))
        {
            playerHash.Remove(collision.gameObject);
            if (playerHash.Count <= 0)
                interactionPrompt.SetActive(false);
        }
    }
}
