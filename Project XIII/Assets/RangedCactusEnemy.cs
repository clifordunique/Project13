﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedCactusEnemy : BasicTurretEnemy
{
    public Transform sprite;
    Animator myAnim;


    protected override void Start()
    {
        base.Start();
        myAnim = GetComponent<Animator>();
    }

    public override void ApproachTarget()
    {
        if (myAnim.enabled == false)
            myAnim.enabled = true;
        base.ApproachTarget();
    }


    protected override void Turn()
    {
        Debug.Log("what");
        Vector3 scale = sprite.transform.localScale;
        scale.x *= -1;
        sprite.localScale = scale;
        facingRight = !facingRight;
    }

    protected override void ForceHandler(float xForce, float yForce){}

    public override void SetPos(float x, float y){}
}
