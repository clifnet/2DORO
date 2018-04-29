﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{


    string Class;
    GameObject weapon;
    GameObject eChip;
    GameObject discharger;
    protected Rigidbody2D rb;
    protected Animator anim;
    protected SpriteRenderer sr;

    protected enum playerState { idle, running, rolling, jumping, walking, sliding, attacking, jumpAttacking, hurting, blocking }
    protected playerState playerAction;
    public TriggerCheck[] triggerChecks;

    protected float health = 100;
    [SerializeField]
    float power;
    [SerializeField]
    float intelligence;
    [SerializeField]
    protected float speed;
    [SerializeField]
    float jumpHeight;
    [SerializeField]
    float defense;
    [SerializeField]
    float defaultSpeed;
    float dashDuration = 0.2f;
    protected bool leftDash, rightDash, isRunning, echipDeployable;
    protected int numbOfJumps;
    protected bool canRollAgain = true;
    protected bool comboDelay = false;
    protected int comboChainNumber, comboChainNumber2;
    protected Vector3 touchingPos, touchingPos2;

    public void Initialize()
    {
        playerAction = playerState.idle;
        rb = GetComponent<Rigidbody2D>();
        speed = 0.01f;
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
    }

    public void attackMove(Vector2 dir){
        rb.AddForce(dir, ForceMode2D.Impulse);
    }
    public void jump(float jumpPow)
    {
        rb.AddForce(new Vector2(0, jumpPow), ForceMode2D.Impulse);
    }
    public void jumpLeft(float jumpPow)
    {
        rb.AddForce(new Vector2(-0.5f, jumpPow), ForceMode2D.Impulse);
    }
    public void jumpRight(float jumpPow)
    {
        rb.AddForce(new Vector2(0.5f, jumpPow), ForceMode2D.Impulse);
    }
    public void moveSlideJumpLeft()
    {
        rb.AddForce(new Vector2(-3, 0));
    }

    public void moveSlideJumpRight()
    {
        rb.AddForce(new Vector2(3, 0));
    }

    public void moveLeft()
    {
        rb.velocity = new Vector2(-1 * speed, rb.velocity.y);
        //rb.AddForce(new Vector2(-500, 0) * speed);
        sr.flipX = true;
    }
    public void moveRight()
    {
        rb.velocity = new Vector2(1 * speed, rb.velocity.y);
        //rb.AddForce(new Vector2(500, 0) * speed);
        sr.flipX = false;
    }
    public void moveLeftJump()
    {
        //rb.AddForce(new Vector2(-100, 0) * 0.05f);
        rb.velocity = new Vector2(-1 * speed, rb.velocity.y);
        sr.flipX = true;
    }
    public void moveRightJump()
    {
        //rb.AddForce(new Vector2(100, 0) * 0.05f);
        rb.velocity = new Vector2(1 * speed, rb.velocity.y);
        sr.flipX = false;
    }

    public void rollMovement()
    {
        if (sr.flipX == true)
        {
            //rb.AddForce(new Vector2(-500, 0) * speed);
            rb.velocity = new Vector2(-3, rb.velocity.y);
        }
        else
        {
            rb.velocity = new Vector2(3, rb.velocity.y);
        }
    }

    protected playerState prevState;

    protected void roll(string rollAnimName, float normSpeed, playerState ps)
    {
        prevState = ps;
        canRollAgain = false;
        defaultSpeed = normSpeed;
        anim.Play(rollAnimName);
        StartCoroutine("waitForRoll");
    }

    public void leftDashCheck()
    {
        if (leftDash == false)
        {
            StartCoroutine(dashDelay(1));
            leftDash = true;
        }
    }
    public void rightDashCheck()
    {
        if (rightDash == false)
        {
            StartCoroutine(dashDelay(2));
            rightDash = true;
        }
    }

    public void completeAttack()
    {
        playerAction = playerState.idle;
    }
    public void incrementComboChain()
    {
        StopCoroutine("resetComboChain");
        if (comboChainNumber < 2)
        {
            comboChainNumber++;
        }
        else
        {
            comboDelay = true; //used in fighter
            StartCoroutine("comboDelayer");
        }
        StartCoroutine("resetComboChain");
    }
    public void incrementComboChain2()
    {
        StopCoroutine("resetComboChain");
        if (comboChainNumber2 < 1)
        {
            comboChainNumber2++;
        }
        else
        {
            comboDelay = true; //used in fighter
            StartCoroutine("comboDelayer");
        }
        StartCoroutine("resetComboChain");
    }
    int triggerToCheck;
    Vector3 pushDir;
    char hurtType;
    
    public void setTriggerForAttack(int x, Vector3 push, char hurtT)
    {
        triggerToCheck = x;
        pushDir = push;
        hurtType = hurtT;
    }

    protected int specialCombo = 0;

    public void specialAttackCombo(){
        if(specialCombo == 0){
            if(sr.flipX == true){
                var hitDir = (Vector3.up * 4f) + (-Vector3.right * 1.5f); 
                attackMove(-Vector3.right);
                setTriggerForAttack(0, hitDir, 'N');
                applyDamageToTrigger();
            }else{
                var hitDir = (Vector3.up * 4f) + (Vector3.right * 1.5f); 
                setTriggerForAttack(1, hitDir, 'N');
                attackMove(Vector3.right);
                applyDamageToTrigger();
            }
            specialCombo++;
        }else if(specialCombo == 1){
            playerAction = playerState.rolling;
            if(sr.flipX == true){

            }else{
                
            }
            specialCombo++;
        }else if(specialCombo == 2){
            playerAction = playerState.attacking;
            rb.velocity = Vector3.zero;
            if(sr.flipX == true){
                var hitDir = (Vector3.up * 4f) + (-Vector3.right * 1.5f); 
                attackMove(-Vector3.right);
                setTriggerForAttack(0, hitDir, 'O');
                applyDamageToTrigger();
            }else{
                var hitDir = (Vector3.up * 4f) + (Vector3.right * 1.5f);
                setTriggerForAttack(1, hitDir, 'O');
                attackMove(Vector3.right);
                applyDamageToTrigger();
            }
            specialCombo = 0;
        }

    }
    public bool applyDamageToTrigger()
    {
        //0 = left
        //1 = right
        //2 = top
        //3 = bottom
        //4 = bottom right
        //5 = bottom left
        //6 = top right
        //7 = top left

        //punch - left/right
        //kick - bottom left/ bottom right

        switch (triggerToCheck)
        {
            case 0:
                if (triggerChecks[0].enemyInside == true)
                {
                    if(triggerChecks[0].enemyObject.GetComponent<Enemy>().canBeHurt == true)
                    {
                    triggerChecks[0].enemyObject.GetComponent<Enemy>().depleteHealth(hurtType, pushDir);
                    triggerChecks[0].enemyObject.GetComponent<SpriteRenderer>().flipX = false;
                    return true;
                    }
                }
                break;
            case 1:
                if (triggerChecks[1].enemyInside == true)
                {
                    if(triggerChecks[1].enemyObject.GetComponent<Enemy>().canBeHurt == true)
                    {
                    triggerChecks[1].enemyObject.GetComponent<Enemy>().depleteHealth(hurtType, pushDir);
                    triggerChecks[1].enemyObject.GetComponent<SpriteRenderer>().flipX = true;
                    return true;
                    }
                }
                break;
            case 2:
                break;
            case 3:
                break;
            case 4:
                break;
            case 5:
                break;
            case 6:
                break;
            case 7:
                break;
        }
        return false;

    }
    public void performJumpAttackDamage(int x, Vector2 push, char hurtT)
    {
        triggerToCheck = x;
        pushDir = push;
        hurtType = hurtT;
        StartCoroutine("checkJumpAttacks");
    }

    public void stopJumpAttack()
    {
        StopCoroutine("checkJumpAttacks");
    }

    public void receiveDamage(char hurtType)
    {
        switch (hurtType)
        {
            case 'B':

                break;
            case 'C':
                break;
            case 'D':
                break;
            case 'E':
                break;
            case 'K':
                break;
            case 'O':
                break;
            case 'F':
                break;
            case 'G':
                break;
        }
    }

    protected IEnumerator runCancelDelay()
    {
        yield return new WaitForSeconds(0.2f);
        isRunning = false;
    }
    IEnumerator checkJumpAttacks()
    {
        var time = 0f; 
        while (time < 0.3f)
        {
            if (applyDamageToTrigger())
            {
                time = 0.3f;
            }
            time += Time.deltaTime;
            yield return null;
        }
        playerAction = playerState.jumping;
    }
    IEnumerator resetComboChain()
    {
        yield return new WaitForSeconds(1f);
        comboChainNumber = 0;
        comboChainNumber2 = 0;
    }

    IEnumerator comboDelayer()
    {
        yield return new WaitForSeconds(0.3f);
        comboDelay = false;
        comboChainNumber = 0;
        comboChainNumber2 = 0;
    }

    public IEnumerator waitForRoll()
    {
        yield return new WaitForSeconds(0.5f);
        rollAfterWait();
    }

    public void rollAfterWait(){
        speed = defaultSpeed;
        if (prevState == playerState.jumping)
            playerAction = playerState.jumping;
        else
            playerAction = playerState.idle;
        anim.SetBool("RollTrigger", false);

        touchingPos = transform.position - new Vector3((-GetComponent<BoxCollider2D>().bounds.extents.x), (GetComponent<BoxCollider2D>().bounds.extents.y * 0.5f));
        touchingPos2 = transform.position - new Vector3((GetComponent<BoxCollider2D>().bounds.extents.x), (GetComponent<BoxCollider2D>().bounds.extents.y * 0.5f));
        RaycastHit2D hit = Physics2D.Raycast(touchingPos, -Vector3.up, 1f, 1 << 8);
        RaycastHit2D hit2 = Physics2D.Raycast(touchingPos2, -Vector3.up, 1f, 1 << 8);
        if (hit.collider == null && hit2.collider == null)
        {
            playerAction = playerState.jumping;
        }
        speed = 0.75f;
        StartCoroutine("rollDelay");
    }

    protected void startEchipDelay(){
        echipDeployable = true;
        StartCoroutine("echipDelayer");
    }

    protected IEnumerator rollDelay()
    {
        yield return new WaitForSeconds(2);
        canRollAgain = true;
    }

    IEnumerator dashDelay(int i)
    {
        yield return new WaitForSeconds(dashDuration);
        if (i == 1)
        {
            leftDash = false;
        }
        if (i == 2)
        {
            rightDash = false;
        }
    }
    IEnumerator echipDelayer(){
        yield return new WaitForSeconds(0.5f);
        echipDeployable = false;
    }

}
