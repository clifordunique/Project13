﻿using UnityEngine;
using System.Collections;

public class GunnerPhysics : PlayerPhysics{

    //Constants for Dodge Roll
    const float DODGE_DIST_PER_INVOKE = 1f;     //Amount gunner rolls per invoke
    const float DODGE_RECOVERY_TIME = .5f;

    GunnerStats gunnerStat;
    BulletProjectile bulletScript;
    Vector3 gunPoint;
    Vector2 velocity;
    float bulletSpeed;

    public GameObject bulletSource;
    public GameObject meleeAttackBox;
    public DownKickScript downKickScript;

    //Dodge rolling variables
    float xInputAxis;
    bool dodgeOnCD = false;

    public override void ClassSpecificStart()
    {
        gunnerStat = GetComponent<GunnerProperties>().GetGunnerStats();
        meleeAttackBox.GetComponent<MeleeAttackScript>().SetAttackStrength(GetComponent<PlayerProperties>().GetPhysicStats().quickAirAttackStrength);
        downKickScript.enabled = false;
    }

    public override void MovementSkill(float xMove, float yMove)
    {
        base.MovementSkill(xMove, yMove);

        float dir;

        dir = (xMove >= 0f) ? 1f : -1f;
        dir = (xMove == 0f) ? transform.localScale.x : dir;

        xInputAxis = dir;

        if (!dodgeOnCD && isGrounded())
            GetComponent<Animator>().SetTrigger("moveSkill");
    }

    void ShootQuickBullet()
    {
        bulletSource.GetComponent<BulletSourceScript>().QuickShot(physicStats.quickAttackStrength);
    }

    void ShootHeavyBullet()
    {
        KnockBack(gunnerStat.heavyAttackKnockBackForce);
        bulletSource.GetComponent<BulletSourceScript>().HeavyShot(physicStats.heavyAttackStrength);
    }

    void ExecuteDownKick()
    {
        downKickScript.Reset();
        downKickScript.enabled = true;
        downKickScript.InvokeRepeating("ApplyDamageEffect",0f,.1f);
        GetComponent<PlayerProperties>().SetStunnableState(false);
    }

    void CancelDownKick()
    {
        downKickScript.CancelInvoke("ApplyDamageEffect");
        downKickScript.enabled = false;
        GetComponent<PlayerProperties>().SetStunnableState(true);
    }

    void ApplyBounce()
    {
        downKickScript.ApplyBounce();
    }

    void ExecuteDodgeSkill()
    {
        InvokeRepeating("Roll", 0f, .04f);
        dodgeOnCD = true;
        Invoke("FinishDodgeCD", DODGE_RECOVERY_TIME);

    }

    void Roll()
    {
        transform.position = Vector3.MoveTowards(transform.position, transform.position + new Vector3(xInputAxis*DODGE_DIST_PER_INVOKE,transform.position.y,transform.position.z), DODGE_DIST_PER_INVOKE);
    }

    void CancelRoll()
    {
        CancelInvoke("Roll");

        if (isGrounded())
            GetComponent<Animator>().SetTrigger("exitDash");
        else
            GetComponent<Animator>().SetTrigger("heavyToAerial");
    }

    void FinishDodgeCD()
    {
        dodgeOnCD = false;
    }
}
