﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets._2D;

public class PlayerWin : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject player1;
    public GameObject player2;
    private bool player1Win, player2Win;
    public bool gameOver;
    private Animator player1Anim, player2Anim;
    

    private void Awake() {
        player1Anim = player1.GetComponent<Animator>(); 
        player2Anim = player2.GetComponent<Animator>();    
    }
    void Start()
    {
        player1Win = false;
        player2Win = false;
        
    }

    // Update is called once per frame
    void Update()
    {
        if (player1Anim.GetCurrentAnimatorStateInfo(0).IsName("Die")) {
            if (!player2Win)
                player2Anim.SetTrigger("Win");
            player2Anim.SetBool("Won", true);
            player2Win = true;
            gameOver = true;
            player2.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;

        }
        if (player2Anim.GetCurrentAnimatorStateInfo(0).IsName("Die")) {
            if (!player1Win)
                player1Anim.SetTrigger("Win");
            player1Anim.SetBool("Won", true);
            player1Win = true;
            gameOver = true;
            player1.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;

        }


    }

    public void timeoutWin() {
        if (player1.GetComponent<PlatformerCharacter2D>().healthBarObject.GetComponent<SimpleHealthBar>().GetCurrentFraction 
        < player2.GetComponent<PlatformerCharacter2D>().healthBarObject.GetComponent<SimpleHealthBar>().GetCurrentFraction) {
            if (!player2Win)
                player2Anim.SetTrigger("Win");
            player2Anim.SetBool("Won", true);
            player2Win = true;
            gameOver = true;
            player2.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
            player1.GetComponent<PlatformerCharacter2D>().healthBarObject.GetComponent<SimpleHealthBar>().UpdateBar(0, player1.GetComponent<PlatformerCharacter2D>().playerHealth);
        } else {
            if (!player1Win)
                player1Anim.SetTrigger("Win");
            player1Anim.SetBool("Won", true);
            player1Win = true;
            gameOver = true;
            player1.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
            player2.GetComponent<PlatformerCharacter2D>().healthBarObject.GetComponent<SimpleHealthBar>().UpdateBar(0, player2.GetComponent<PlatformerCharacter2D>().playerHealth);
        }
    }
    
}
