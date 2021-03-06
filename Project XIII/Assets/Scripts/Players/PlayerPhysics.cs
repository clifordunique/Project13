﻿using UnityEngine;
using System.Collections;


public class PlayerPhysics : MonoBehaviour {
    //Variables for bad controller callibration
    const float Y_NEGATIVE_ACCEPT = -.2f;
    const float X_ABS_ACCEPT = .2f;

    //Constants for changing gravity force for jumping
    const float DEFAULT_GRAVITY_FORCE = 8f;
    const float MIN_GRAVITY_FORCE = 4f;

    const float JUMP_RAY_RESTRAIN_TIME = .2f;
    const float GROUNDED_GRACE_TIME = .3f;

    //Use for crouching
    float previousVertical = 0;

    //healing
    bool canHeal = true;

    MultiplayerCamFollowScript cameraScript;

    protected Rigidbody2D myRigidbody;
    protected Animator myAnimator;
    protected PlayerProperties playerProperties;
    protected PlayerStats physicStats;
    protected PlayerBoostStats boostStats;
    protected KeyPress myKeyPress;
    protected PlayerInput myPlayerInput;
    protected bool isJumping;
    protected bool isFacingRight;
    protected bool cannotMovePlayer;
    protected bool cannotJump;
    protected bool cannotAttack;
    protected bool zeroVelocity;
    protected bool movementSkillActive = false;         //To prevent movement from interfering with movement skill

    //For checking held buttons
    protected bool checkQuickAttackUp;
    protected bool quickAttackReleased;
    protected bool checkHeavyAttackUp;
    protected bool heavyAttackReleased;

    //Ground detection
    float distToGround;                                 //Distance from the ground
    float characterWidth;                               //Width of character to cast secondary raycasting
    int layerMask;                                      //Layers to check for ground
    bool wasGrounded = false;
    bool running;
    public float groundCheckingOffset = 2f;

    //Ground detection delay for continuing ground attacks off ledge
    bool groundedGraceTimeInvoked = false;

    //Ground detection related to jumping
    float jumpGroundCheckNegativeOffset = 0f;
    bool jumpSpent = false;

    //Ground detecion related to movement skills
    bool moveSkillPerformed = false;
    bool moveSkillDelayCheck = false;

    //For interaction 
    NPC npcInRange = null;

    //Using for turning
    ShadowSpriteGenerator shadowSpriteGenerator;

    protected void Start () {
        cameraScript = Camera.main.transform.parent.GetComponent<MultiplayerCamFollowScript>();
        myAnimator = GetComponent<Animator>();
        myRigidbody = GetComponent<Rigidbody2D>();
        playerProperties = GetComponent<PlayerProperties>();
        physicStats = playerProperties.GetPlayerStats();
        boostStats = playerProperties.GetBoostStats();
        myPlayerInput = GetComponent<PlayerInput>();
        myKeyPress = myPlayerInput.getKeyPress();
        isFacingRight = true;
        isJumping = false;
        cannotMovePlayer = false;
        cannotJump = false;
        cannotAttack = false;
        zeroVelocity = false;

        //Holding buttons
        checkQuickAttackUp = false;
        quickAttackReleased = true;
        checkHeavyAttackUp = false;
        heavyAttackReleased = true;

        //Checking for if held buttons released
        

        distToGround = GetComponent<Collider2D>().bounds.extents.y;
        characterWidth = GetComponent<Collider2D>().bounds.extents.x;
        layerMask = (LayerMask.GetMask("Default","Item"));

        foreach (Transform child in transform)
        {
            if (child.name == "Shadow")
                shadowSpriteGenerator = child.GetComponent<ShadowSpriteGenerator>();
        }

        ClassSpecificStart();
    }

    protected void Update()
    {
        myPlayerInput.GetInput();
        myKeyPress = myPlayerInput.getKeyPress();
        
    }

    protected void FixedUpdate()
    {
        if (!GetComponent<PlayerProperties>().GetStunState() && GetComponent<PlayerProperties>().alive && GetComponent<PlayerInput>().InputActiveState())
        {

            if (!CheckClassSpecificInput())
            {
                float xMove = myKeyPress.horizontalAxisValue;
                float yMove = myKeyPress.verticalAxisValue;

                Movement();
                Crouching();

                //Prioritizes jump in the case both buttons pressed at the same time
                if (myKeyPress.jumpPress)
                    Jump();
                else
                {
                    if (myKeyPress.quickAttackPress)
                        QuickAttack();
                    if (myKeyPress.heavyAttackPress)
                        HeavyAttack();
                }
                if (myKeyPress.jumpReleased)
                    JumpReleased();

                if (myKeyPress.dashPress)
                    MovementSkill(xMove, yMove);
                if (myKeyPress.recoveryPress)
                    Heal();
                CheckForButtonReleases();

                if (myKeyPress.interactionPress && npcInRange != null)
                    npcInRange.ActivateInteraction();
            }
        }
        ClassSpecificUpdate();
        myPlayerInput.ResetKeyPress();
        Landing();
        
    }


    public virtual void ClassSpecificStart()
    {
        //This function is used when a specific class need to use Start
    }

    public virtual void ClassSpecificUpdate()
    {
        //This function is used when a specific class need to use FixedUpdate
    }

    public virtual bool CheckClassSpecificInput()
    {
        //This function is used when a specific class has specific inputs to look for
        return false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("NPC"))
            npcInRange = collision.GetComponent<NPC>();
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("NPC"))
            npcInRange = null;
    }

    public virtual void MovementSkill(float xMove, float yMove)
    {
        MoveSkillExecuted();
        if (!cannotMovePlayer)
            return;
        //This function is used for when specific class movement based skills
    }

    public void MoveSkillExecuted()
    {
        CancelInvoke("EndMoveSkillDelayCheck");
        moveSkillPerformed = true;
        moveSkillDelayCheck = true;
        Invoke("EndMoveSkillDelayCheck", JUMP_RAY_RESTRAIN_TIME);
    }

    void EndMoveSkillDelayCheck()
    {
        moveSkillDelayCheck = false;
    }

    void Heal()
    {
        if (canHeal && !isJumping && playerProperties.healingItem > 0 && !myAnimator.GetCurrentAnimatorStateInfo(0).IsName("Ground.Item Use"))
            myAnimator.SetTrigger("itemUse");
    }
    protected void Movement()
    {
        if (!cannotMovePlayer && !myAnimator.GetBool("crouch") && !movementSkillActive)
        {
            myRigidbody.velocity = new Vector2(myKeyPress.horizontalAxisValue * physicStats.movementSpeed, myRigidbody.velocity.y);
            myAnimator.SetFloat("speed", Mathf.Abs(myKeyPress.horizontalAxisValue));
            
            if (!isJumping)
                Flip();
        }
        if (zeroVelocity)
        {
            myRigidbody.velocity = new Vector2(0, 0);
        }

    }

    public void ReportMovementSkillInactive()
    {
        movementSkillActive = false;
    }

    void Crouching()
    {
        if (myKeyPress.verticalAxisValue < previousVertical && myKeyPress.verticalAxisValue < 0 && myKeyPress.verticalAxisValue < Y_NEGATIVE_ACCEPT && (Mathf.Abs(myKeyPress.horizontalAxisValue) <= X_ABS_ACCEPT))
        {
            cameraScript.SetCrouch(true);
            myAnimator.SetBool("crouch", true);

        }
        else if (myKeyPress.verticalAxisValue > previousVertical || myKeyPress.verticalAxisValue >= 0 && myKeyPress.verticalAxisValue >= Y_NEGATIVE_ACCEPT)
        {
            cameraScript.SetCrouch(false);
            myAnimator.SetBool("crouch", false);
        }
        previousVertical = myKeyPress.verticalAxisValue;
    }

    protected void Jump()
    {
        if (!jumpSpent && !cannotJump)
        {
            VelocityY(0);
            CancelInvoke("CancelWasGrounded");
            CancelWasGrounded();

            Invoke("CancelWaitJump", JUMP_RAY_RESTRAIN_TIME);
            jumpGroundCheckNegativeOffset = 1f;
            isJumping = true;
            jumpSpent = true;

            GetComponent<Rigidbody2D>().gravityScale = MIN_GRAVITY_FORCE;

            if (Mathf.Abs(myKeyPress.horizontalAxisValue) > 0.1)
                myAnimator.SetTrigger("jumpForward");
            else
                myAnimator.SetTrigger("jumpIdle");
            
            GetComponent<Rigidbody2D>().AddForce(new Vector2(0, physicStats.jumpForce), ForceMode2D.Impulse);
        }
    }

    void CancelWaitJump()
    {
        jumpGroundCheckNegativeOffset = 0f;
        groundedGraceTimeInvoked = false;
        CancelInvoke("CancelWasGrounded");
    }

    protected void JumpReleased()
    {
        GetComponent<Rigidbody2D>().gravityScale = DEFAULT_GRAVITY_FORCE;
    }

    protected void Landing()
    {
        if (isGrounded())
        {
            isJumping = false;
        }

        else
        {
            isJumping = true;
        }


        if (!isJumping)
        {
            myAnimator.SetTrigger("landing");
            myAnimator.SetBool("land", true);
        }
        else
            myAnimator.SetBool("land", false);
    }

    protected virtual void QuickAttack()
    {
        if (!cannotAttack)
        {

            if (isJumping)
                myAnimator.SetTrigger("airQuickAttack");
            else
                myAnimator.SetTrigger("quickAttack");
        }
    }
    protected virtual void HeavyAttack()
    {       
        if (!cannotAttack)
        {
            if (isJumping)
                myAnimator.SetTrigger("airHeavyAttack");
            else
                myAnimator.SetTrigger("heavyAttack");
        }
    }
    protected virtual void Block()
    {
        if (!cannotAttack)
        {
            if (!isJumping)
            {
                myAnimator.SetTrigger("block");
            }
        }
    }

    protected void Flip()
    {
        if (myKeyPress.horizontalAxisValue > 0 && !isFacingRight || myKeyPress.horizontalAxisValue < 0 && isFacingRight)
        {
            ApplyFlip();
        }
    }

    public void SetFacing(bool inRightDirection)
    {
        if((inRightDirection && !isFacingRight) || (!inRightDirection && isFacingRight))
        {
            ApplyFlip();
        }     
    }   

    void ApplyFlip()
    {
        if (myAnimator == null)
            myAnimator = GetComponent<Animator>();
        myAnimator.SetTrigger("switch");
        isFacingRight = !isFacingRight;

        Vector3 scale = transform.localScale;
        scale.x *= -1;

        transform.localScale = scale;

        if(shadowSpriteGenerator != null)
            shadowSpriteGenerator.ChangeFacingDirection();
    }

    protected void KnockBack(float knockBackForce)
    {
        knockBackForce *= isFacingRight ? -1 : 1;
        GetComponent<Rigidbody2D>().AddForce(new Vector2(knockBackForce * 2, 0), ForceMode2D.Impulse);
    }

    //All function below is use for Animation Event
    public void ActivateMovement()
    {
        cannotMovePlayer = false;
    }

    public void DeactivateMovement()
    {
        cannotMovePlayer = true;
    }

    public void ActivateJump()
    {
        cannotJump = false;
    }

    public void DeactivateJump()
    {
        cannotJump = true;
    }

    public void ActivateAttack()
    {
        cannotAttack = false;
    }

    public void DeactivateAttack()
    {
        cannotAttack = true;
    }

    public bool CanAttackStatus()
    {
        return !cannotAttack;
    }

    public void DeactivateVelocity()
    {
        zeroVelocity = true;
    }

    public void ActivateVelocity()
    {
        zeroVelocity = false;
    }
    
    public void VelocityY(float velocityY)
    {
        myRigidbody.velocity = new Vector2(0, velocityY);  
    }

    public void VelocityX(float velocityX)
    {
        myRigidbody.velocity = new Vector2(velocityX, 0);
    }

    public void AddForceX(float forceX)
    {
        float xDir = 1f;
        if (transform.localScale.x < 0f)
            xDir = -1f;
        myRigidbody.AddForce(new Vector2(forceX * xDir, 0f));
    }

    public void AddForceY(float forceY)
    {
        myRigidbody.AddForce(new Vector2(0f, forceY));
    }

    public void ActivateAttackMovementJump()
    {
        cannotAttack = false;
        cannotJump = false;
        cannotMovePlayer = false;
        canHeal = true;
        GetComponent<PlayerProperties>().ActivateKnockback();
    }

    public void DeactivateAttackMovementJump()
    {
        cannotAttack = true;
        cannotJump = true;
        cannotMovePlayer = true;
        canHeal = false;
    }

    public bool isGrounded()
    {
        if (Physics2D.Raycast(transform.position, -Vector3.up, distToGround +groundCheckingOffset - jumpGroundCheckNegativeOffset, layerMask)
            || Physics2D.Raycast(transform.position + new Vector3(characterWidth,0f,0f), -Vector3.up, distToGround + groundCheckingOffset - jumpGroundCheckNegativeOffset, layerMask)
            || Physics2D.Raycast(transform.position - new Vector3(characterWidth, 0f, 0f), -Vector3.up, distToGround + groundCheckingOffset - jumpGroundCheckNegativeOffset, layerMask)
            )
        {
            jumpSpent = false;
            wasGrounded = true;
            CancelInvoke("CancelWasGrounded");
            groundedGraceTimeInvoked = false;

            if (!moveSkillDelayCheck)
                moveSkillPerformed = false;
            return true;
        }
        else if (!groundedGraceTimeInvoked)
        {
            groundedGraceTimeInvoked = true;
            Invoke("CancelWasGrounded", GROUNDED_GRACE_TIME);
        }

        if (wasGrounded && !jumpSpent && !moveSkillPerformed)
            return true;
        return false;
    }
    
    void CancelWasGrounded()
    {
        wasGrounded = false;
        groundedGraceTimeInvoked = false;
    }

    public void CancelWasGroundedInvoke()
    {
        CancelInvoke("CancelWasGrounded");
        CancelWasGrounded();
    }
    
    protected void CheckForButtonReleases()
    {
        if (checkQuickAttackUp)
        {
            if (myKeyPress.quickAttackReleased)
            {
                myAnimator.enabled = true;
                quickAttackReleased = true;
                checkQuickAttackUp = false;
            }
        }

        if (checkHeavyAttackUp)
        {
            if (myKeyPress.heavyAttackReleased)
            {
                myAnimator.enabled = true;
                heavyAttackReleased = true;
                checkHeavyAttackUp = false;
                ExecuteHeavyButtonRelease();
            }
        }
    }

    public void CancelCheckForButtonReleases()
    {
        checkQuickAttackUp = false;
        checkHeavyAttackUp = false;
    }

    public virtual void ExecuteHeavyButtonRelease(){}

    public void CheckForQuickRelease()
    {
        quickAttackReleased = false;
        checkQuickAttackUp = true;
    }

    //Specific to freezing animations connected to whether a button has been released or not
    public void DisableAnimator()
    {
        if(!(quickAttackReleased && heavyAttackReleased) && GetComponent<PlayerInput>().InputActiveState())
            myAnimator.enabled = false;
    }

    void TotalDisableAnimator()
    {
        myAnimator.enabled = false;
    }

    public void CheckForHeavyRelease()
    {
        heavyAttackReleased = false;
        checkHeavyAttackUp = true;
    }

    public void ConstrainY()
    {
        myRigidbody.constraints = RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;
    }

    public void DeconstrainY()
    {
        myRigidbody.constraints &= ~RigidbodyConstraints2D.FreezePositionY;
    }

    public void ConstrainX()
    {
        myRigidbody.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
    }

    public void DeconstrainX()
    {
        myRigidbody.constraints &= ~RigidbodyConstraints2D.FreezePositionX;
    }

    public void AlterGravityForce(float force)
    {
        GetComponent<Rigidbody2D>().gravityScale = force;
    }

    public void RevertOriginalGravityForce()
    {
        GetComponent<Rigidbody2D>().gravityScale = DEFAULT_GRAVITY_FORCE;
    }

    public float GetDefaultGravityForce()
    {
        return DEFAULT_GRAVITY_FORCE;
    }

}
