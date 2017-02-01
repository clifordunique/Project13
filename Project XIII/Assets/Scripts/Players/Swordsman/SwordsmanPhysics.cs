﻿using UnityEngine;
using System.Collections;

public class SwordsmanPhysics : PlayerPhysics{

    //SENSITVITY CONTROLS
    const float Y_INPUT_THRESHOLD = .5f;        //Threshold before considering input

    //Constants for managing quick dashing skill
    const float DASH_RECOVERY_TIME = 0.5f;      //Time it takes to recover dashes
    const float MAX_CHAIN_DASH = 1;             //Max amount of dashes that can be chained
    const float STOP_AFTER_IMAGE = .005f;       //Time to stop creating afterimages
    const float DASH_FORCE = 5000f;             //Amount of force to apply on character to perform movement

    //Constant for charging ground heavy slash attack
    const float MAX_CHARGE = 2f;                //Max amount of time multiplier allowed to be applied to charge distance
    const float TIER_1_CHARGE = .8f;            //Tier one charge for beginning to flash white
    const float CHARGE_FORCE_MULTIPLIER = 3000f;//Multiplier for distance to travel after charging attack

    //Constant for max combo hit types
    const int MAX_COMBO = 3;                    //Maximum combo hit types
    const float COMBO_GRACE_TIME = 0.4f;        //Time after finishing a combo hit's animation before the next combo hit no longer continues the chain

    //Attack box
    public GameObject attackBox;                //Collider for dealing all melee attacks
    public SwordsmanAttackScript attackScript;  //Script for managing attack

    //Dash variables
    float xInputAxis = 0f;                               
    float yInputAxis = 0f;
    int dashCount = 0;                          //Checks how many dashes have been chained
    bool checkGroundForDash = false;            //Bool that determines to check for grounded before resetting dash count
    bool disableDash = false;

    //Combo Variable
    bool inCombo = false;                       //Checks if swordsman able to combo
    bool comboPressed = false;                  //Check if the combo button was pressed during combo
    bool checkForCombo = false;                 //Check if script should check for next combo press input
    bool comboAnimFinished = false;             //Checks if the combo animation was finished but still should check for combo input
    int currentCombo = 0;                       //Checks what combo hit was last played

    //Heavy attack variables
    bool checkChargeTime = false;               //Determines if should check for time charging
    float timeCharged;                          //Time attack has been charged
    Material defaultMat;                        //Default sprite material
    public Material flashWhiteMat;              //White flashing material
    public Material flashGoldMat;               //Gold flashing material
    bool isFlashingWhite = false;               //Determines if flashing white color
    bool isFlashingGold = false;                //Determines if flashing gold color

    PlayerParticleEffects playerParticleEffects;

    public override void ClassSpecificStart()
    {
        playerParticleEffects = GetComponent<PlayerParticleEffects>();
        defaultMat = GetComponent<SpriteRenderer>().material;
    }

    public override void ClassSpecificUpdate()
    {
        if (inCombo)
            WatchForCombo();
        if (checkGroundForDash)
            ResetDashCount();
        if (checkChargeTime)
        {
            timeCharged += Time.fixedDeltaTime;
            if(timeCharged >= 2f && !isFlashingGold)
            {
                isFlashingGold = true;
                CancelInvoke("ChargingFlashWhite");
                InvokeRepeating("ChargingFlashGold", 0f, .09f);
            }
            else if(timeCharged >= 1f && timeCharged < 2f && !isFlashingWhite)
            {
                isFlashingWhite = true;
                InvokeRepeating("ChargingFlashWhite", 0f, .15f);
            }
        }

    }

    public override bool CheckClassSpecificInput()
    {
        float xMove = myKeyPress.horizontalAxisValue;
        float yMove = myKeyPress.verticalAxisValue;

        //Up attack input
        if (CanAttackStatus() && (yMove > Y_INPUT_THRESHOLD) && GetComponent<PlayerInput>().getKeyPress().quickAttackPress && isGrounded())
        {
            GetComponent<Animator>().SetTrigger("upQuickAttack");
            return true;
        }

        if(CanAttackStatus() && GetComponent<PlayerInput>().getKeyPress().quickAttackPress && checkForCombo)
        {
            if (comboAnimFinished)
            {
                CancelInvoke("StopCheckForCombo");
                PlayNextComboHit();
            }
            return true;
        }

        return false;
    }

    public override void MovementSkill(float xMove, float yMove)
    {
        if (disableDash)
            return;
        base.MovementSkill(xMove,yMove);

        if (Mathf.Abs(xMove) < .01 && Mathf.Abs(yMove) < .01f)
        {
            xInputAxis = transform.localScale.x;
            yInputAxis = 0f;
        }
        else
        {
            xInputAxis = xMove;
            yInputAxis = yMove;
        }

        if(dashCount < MAX_CHAIN_DASH)
            GetComponent<Animator>().SetTrigger("moveSkill");
    }

    //COMBO FUNCTIONS

    public void WatchForCombo()
    {
        if (GetComponent<PlayerInput>().getKeyPress().quickAttackPress)
        {
            comboPressed = true;
        }
    }

    public void StartCombo()
    {
        inCombo = true;
        checkForCombo = true;
        comboAnimFinished = false;
    }

    public void ResetCombo()
    {
        inCombo = false;
        comboAnimFinished = false;
        checkForCombo = false;
        CancelInvoke("StopCheckForCombo");
        currentCombo = 0;
    } 

    public void FinishCombo()
    {
        inCombo = false;
        currentCombo++;
        comboAnimFinished = true;

        if (currentCombo < MAX_COMBO)
        {
            if (comboPressed)
                PlayNextComboHit();
            else
                Invoke("StopCheckForCombo", COMBO_GRACE_TIME);
        }
        else
            StopCheckForCombo();
        
    }

    void PlayNextComboHit()
    {
        string triggerString = "combo" + currentCombo.ToString();
        comboPressed = false;
        comboAnimFinished = false;

        if (isGrounded())
            GetComponent<Animator>().SetTrigger(triggerString);
        else
            GetComponent<Animator>().SetTrigger("air" + triggerString);
    }

    void StopCheckForCombo()
    {
        checkForCombo = false;
        currentCombo = 0;
        comboAnimFinished = false;
    }

    //END COMBO FUNCTIONS

    public void HeavyTransistionToAir()
    {
        GetComponent<Animator>().SetTrigger("heavyToAerial");
    }

    //UP + QUICK ATTACK ATTACK FUNCTIONS

    public void ExecuteDragAttack()
    {
        attackScript.Reset();
        attackScript.SetAttackType("drag");
    }

    public void EndDragAttack()
    {
        attackBox.GetComponent<Collider2D>().enabled = false;
        attackScript.ResetDrag();
    }

    public void EndDragDamage()
    {
        attackScript.CancelDragAttackApplyDamage();
    }

    //END UP + QUICK ATTACK ATTACK FUNCTIONS

    //DASHING FUNCTIONS

    public void ExecuteDashSkill()
    {
        dashCount++;
        CancelInvoke("StopAfterImage");
        CancelInvoke("ResetDashCount");

        StartCoroutine("Dashing");

        playerParticleEffects.PlayDashAfterImage(true);
        gameObject.layer = 14;
    }

    IEnumerator Dashing()
    {
        DeactivateAttackMovementJump();
        VelocityY(0f);
        VelocityX(0f);

        GetComponent<Rigidbody2D>().gravityScale = 0f;

        GetComponent<Rigidbody2D>().AddForce(new Vector2(xInputAxis, yInputAxis).normalized * DASH_FORCE);
        yield return new WaitForSeconds(.1f);
        VelocityX(0);
        VelocityY(0);

        GetComponent<Rigidbody2D>().gravityScale = GetDefaultGravityForce();

        if (isGrounded())
            GetComponent<Animator>().SetTrigger("exitDash");
        else
            GetComponent<Animator>().SetTrigger("heavyToAerial");

        ActivateAttackMovementJump();

        Invoke("ResetDashCount", DASH_RECOVERY_TIME);
        Invoke("StopAfterImage", STOP_AFTER_IMAGE);

        gameObject.layer = 15;
    }

    void ResetDashCount()
    {
        if (isGrounded())
        {
            dashCount = 0;
            checkGroundForDash = false;
        }
        else
            checkGroundForDash = true;
    }

    void StopAfterImage()
    {
        playerParticleEffects.PlayDashAfterImage(false);
    }

    //END DASHING FUNCTIONS

    //HEAVY CHARGING ATTACK FUNCTIONS

    void StartHeavyGroundCharge()
    {
        checkChargeTime = true;
        timeCharged = 0f;
        GetComponent<SwordsmanParticleEffects>().PlayChargingDust(true);
        InvokeRepeating("ChargingShake", .7f, .1f);
    }

    void ExecuteHeavyAttack()
    {
        GetComponent<SwordsmanParticleEffects>().PlayChargingDust(false);
        CancelFlashing();

        checkChargeTime = false;
        timeCharged = Mathf.Min(timeCharged, MAX_CHARGE);
        attackScript.SetForceMulti(timeCharged);
        AddForceX(CHARGE_FORCE_MULTIPLIER * timeCharged);
    }

    void EndHeavyAttack()
    {
        attackBox.GetComponent<Collider2D>().enabled = false;
        attackScript.Launch();
        attackScript.Reset();
    }

    void ChargingShake()
    {
        if (transform.rotation.z == 0f)
            transform.Rotate(new Vector3(0f, 0f, -1f));
        else
            transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, 0f));

    }

    void FlashColor(Material mat)
    {
        if (GetComponent<SpriteRenderer>().material == defaultMat)
            GetComponent<SpriteRenderer>().material = mat;
        else
            GetComponent<SpriteRenderer>().material = defaultMat;
    }

    void ChargingFlashWhite()
    {
        FlashColor(flashWhiteMat);
    }

    void ChargingFlashGold()
    {
        FlashColor(flashGoldMat);
    }

    void CancelFlashing()
    {
        CancelInvoke("ChargingFlashWhite");
        CancelInvoke("ChargingFlashGold");
        CancelInvoke("ChargingShake");
        GetComponent<SpriteRenderer>().material = defaultMat;
        isFlashingWhite = false;
        isFlashingGold = false;
    }

    public void CancelHeavyCharge()
    {
        GetComponent<SwordsmanParticleEffects>().PlayChargingDust(false);
        checkChargeTime = false;
        CancelFlashing();
        attackScript.Reset();
    }

    //END HEAVY CHARGING ATTACK FUNCTIONS

    public void HeavyAttackScreenShake()
    {
        ScreenShake(.1f, .03f);
    }

    public void SetAttackType(string type)
    {
        attackScript.SetAttackType(type);
    }

    public void EnableDash()
    {
        disableDash = false;
    }

    public void DisableDash()
    {
        disableDash = true;
    }

}
