﻿using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {
    public int PlayerNumber = -1;
	public bool sidePerspective;
    private bool isJumping = false;
    public KeyCode Up;
	public KeyCode Down;
	public KeyCode Left;
	public KeyCode Right;
	public KeyCode Jump;
    public KeyCode Attack1;
    public KeyCode Attack2;
    public KeyCode Block;
    public PlayerCharacter character = null;
    Animator anim;
    private bool isAttacking;
    int AttackPower;
    int AttackSpeed;

    void Start()
	{
        sidePerspective = true;
        SetPlayerNumber(-1);
	}

    public void SetPlayerNumber(int num)
    {
        PlayerNumber = num;
        SetInput();
    }
    private void SetInput()
    {
        switch (PlayerNumber)
        {
            case 0:
                Up = KeyCode.UpArrow;
                Down = KeyCode.DownArrow;
                Left = KeyCode.LeftArrow;
                Right = KeyCode.RightArrow;
                Jump = KeyCode.Space;
                Attack1 = KeyCode.A;
                Attack2 = KeyCode.S;
                Block = KeyCode.D;
                break;
            case 1:
                break;
            case 2:
                break;
            case 3:
                break;
            default:
                Up = KeyCode.UpArrow;
                Down = KeyCode.DownArrow;
                Left = KeyCode.LeftArrow;
                Right = KeyCode.RightArrow;
                Jump = KeyCode.Space;
                Attack1 = KeyCode.A;
                Attack2 = KeyCode.S;
                Block = KeyCode.D;
                break;
        }
    }
	public void SelectClass(int choice  = 0)
	{
		switch (choice)
		{
			case 0:
				character = new CowBoyClass();
				break;
			default:
				character = new CowBoyClass();
				break;
		}
        anim = GetComponent<Animator>();
	}
	void swapPerspective()
	{
		sidePerspective = false;
	}
    void Update()
    {
        if (PlayerNumber != -1 || true) //or true for testing purposes.
        {
            int dir = 0;
            if(character == null)
            {
                SelectClass(0);
            }
            if (sidePerspective == false && (Input.GetKey(Up) || Input.GetKey(Down)))
            {
                dir = Input.GetKey(Up) ? 1 : -1;
                var move = new Vector3(0, dir, 0);
                transform.position += move * 2 * Time.deltaTime;
            }
            else if (Input.GetKey(Left) || Input.GetKey(Right))
            {
                dir = Input.GetKey(Left) ? -1 : 1;
                var move = new Vector3(dir, 0, 0);

                anim.SetInteger("x", dir);
                transform.position += move * 4 * Time.deltaTime;
            }
            if(dir == 0)
            {
                anim.SetInteger("idle", dir);
            }
            if (Input.GetKeyDown(Jump) && !isJumping)
            {
                isJumping = true;
                GetComponent<Rigidbody2D>().AddForce(new Vector2(0, 5), ForceMode2D.Impulse);
            }
            if (Input.GetKeyDown(KeyCode.J))
            {
                print("ATTACKIN MAHSELF.");
                TakeDamage(10);
            }
            if (Input.GetKeyDown(KeyCode.K))
            {
                print("ATTACKIN MAHSELFMORE.");
                TakeDamage(10, 4);
                print(character.GetCurrentHealth());
            }
            if (Input.GetKeyDown(KeyCode.A))
            {
                //AnimateAttack
                AttackPower = character.Attack(0);
                isAttacking = AttackPower < 0 ? false : true;
            }
            if (Input.GetKeyDown(KeyCode.S))
            {
                //AnimateAttack
                AttackPower = character.Attack(1);
                isAttacking = AttackPower < 0 ? false : true;
            }
        }
    }


    public void TakeDamage(int dmg, int knockBackForce = 0)
    {
        character.TakeDamage(dmg);
        GetComponent<Rigidbody2D>().AddForce(new Vector2(knockBackForce, 0), ForceMode2D.Impulse);
        if(character.GetCurrentHealth() <= 0)
        {
            //Animate Death
            //Reset Location
            character.PlayerDeath();
        }
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if(col.collider.tag == "Ground")
        {
            isJumping = false;
        }
        if(isAttacking && col.collider.tag == "Enemy")
        {
            col.gameObject.GetComponent<Enemy>().Damage(AttackPower);
            isAttacking = false;
        }
    }
}
