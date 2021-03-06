﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
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

    protected enum playerState { idle, running, rolling, jumping, walking, sliding, attacking, jumpAttacking, hurting, blocking, braking }
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

    public bool isAtDoor;
    public Door doorScript;

    public bool isChatting;
    public bool isAllowedToFight;

    protected bool touchingWall, touchingFloor; //States which show when touching a wall or floor
    protected string currentAttackAnim;

    public void Initialize()
    {
        playerAction = playerState.idle;
        rb = GetComponent<Rigidbody2D>();
        speed = 0.01f;
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
    }

    public void attackMove(Vector2 dir)
    {
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
        hurtType = 'Z';
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
            combo1Stack.Clear();
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
            combo2Stack.Clear();
            comboDelay = true; //used in fighter
            StartCoroutine("comboDelayer");
        }
        StartCoroutine("resetComboChain");
    }
    int triggerToCheck;
    Vector3 pushDir;
    protected char hurtType;
    string hitAttack;

    public void setTriggerForAttack(int x, Vector3 push, char hurtT, string attkName)
    {
        triggerToCheck = x;
        pushDir = push;
        hurtType = hurtT;
        hitAttack = attkName;
    }

    protected int specialCombo = 0;
    protected int dischargeCombo = 0;

    public void specialAttackCombo()
    {
        if (specialCombo == 0)
        {
            if (sr.flipX == true)
            {
                pauseEnemiesInTrigger(7, true);
     
                var hitDir = (Vector3.up * 4f) + (-Vector3.right * 1.5f);
                attackMove(-Vector3.right);
                setTriggerForAttack(7, hitDir, 'N', "special1");
                if (applyDamageToTrigger())
                {
                    var newObj = Instantiate(hitEffectObject);
                    newObj.transform.position = transform.localPosition + new Vector3(-0.052f, 0.279f);
                    newObj.GetComponent<SpriteRenderer>().flipX = true;
                }
                else
                {
                    setTriggerForAttack(7, hitDir, 'N', "special1");
                    if (applyDamageToTrigger())
                    {
                        var newObj = Instantiate(hitEffectObject);
                        newObj.transform.position = transform.localPosition + new Vector3(-0.052f, 0.279f);
                        newObj.GetComponent<SpriteRenderer>().flipX = true;
                    }
                    else
                    {
                        unPauseEnemiesInSpecial();
                    }
                }
            }
            else
            {
                pauseEnemiesInTrigger(6, true);
                var hitDir = (Vector3.up * 4f) + (Vector3.right * 1.5f);
                setTriggerForAttack(6, hitDir, 'N', "special1");
                attackMove(Vector3.right);
                if (applyDamageToTrigger())
                {
                    var newObj = Instantiate(hitEffectObject);
                    newObj.transform.position = transform.localPosition + new Vector3(0.052f, 0.279f);
                }
                else
                {
                    setTriggerForAttack(6, hitDir, 'N', "special1");
                    if (applyDamageToTrigger())
                    {
                        var newObj = Instantiate(hitEffectObject);
                        newObj.transform.position = transform.localPosition + new Vector3(0.052f, 0.279f);
                    }
                    else
                    {
                        unPauseEnemiesInSpecial();
                    }
                }
            }
            specialCombo++;
        }
        else if (specialCombo == 1)
        {
            playerAction = playerState.rolling;
            if (sr.flipX == true)
            {

            }
            else
            {

            }
            specialCombo++;
        }
        else if (specialCombo == 2)
        {
            playerAction = playerState.attacking;
            rb.velocity = Vector3.zero;
            if (sr.flipX == true)
            {
                pauseEnemiesInTrigger(0, true);
                var hitDir = (Vector3.up * 4f) + (-Vector3.right * 1.5f);
                attackMove(-Vector3.right);
                setTriggerForAttack(0, hitDir, 'O', "special2");
                if (applyDamageToTrigger())
                {
                    var newObj = Instantiate(hitEffectObject);
                    newObj.transform.position = transform.localPosition + new Vector3(-0.157f, 0.157f);
                    newObj.GetComponent<SpriteRenderer>().flipX = true;
                //    PlayerAuthenticator.instance.sendDamagePacket('O', hitDir);
                }
                else
                {
                    unPauseEnemiesInSpecial();
                }
            }
            else
            {
                pauseEnemiesInTrigger(1, true);
                var hitDir = (Vector3.up * 4f) + (Vector3.right * 1.5f);
                setTriggerForAttack(1, hitDir, 'O', "special2");
                attackMove(Vector3.right);
                if (applyDamageToTrigger())
                {
                    var newObj = Instantiate(hitEffectObject);
                    newObj.transform.position = transform.localPosition + new Vector3(0.157f, 0.157f);
                //    PlayerAuthenticator.instance.sendDamagePacket('O', hitDir);
                }
                else
                {
                    unPauseEnemiesInSpecial();
                }
            }
            specialCombo = 0;
        }

    }
    public void dischargeAttack()
    {
        if (dischargeCombo == 0)
        {

        }
        else if (dischargeCombo == 1)
        {
            if (sr.flipX == false)
            {
                setTriggerForAttack(1, Vector3.zero, 'E', "discharge");
                applyDamageToTrigger();
                //   PlayerAuthenticator.instance.sendDamagePacket('E', Vector2.zero);
            }
            else
            {
                setTriggerForAttack(0, Vector3.zero, 'E', "discharge");
                applyDamageToTrigger();
                //    PlayerAuthenticator.instance.sendDamagePacket('E', Vector2.zero);
            }

            dischargeCombo++;
        }
        else if (dischargeCombo == 2)
        {
            if (sr.flipX == false)
            {
                setTriggerForAttack(1, Vector3.zero, 'E', "discharge");
            }
            else
            {
                setTriggerForAttack(0, Vector3.zero, 'E', "discharge");
            }

            if (applyDamageToTrigger())
            {
                dischargeCombo++;
            }
            else
            {
                dischargeCombo = 0;
            }
        }
        if (dischargeCombo == 3)
        {
            if (sr.flipX == false)
            {
                setTriggerForAttack(1, Vector3.zero, 'F', "discharge");
            }
            else
            {
                setTriggerForAttack(0, Vector3.zero, 'F', "discharge");
            }
            anim.Play("Fighter_dischargeHit");
            applyDamageToTrigger();
                //PlayerAuthenticator.instance.sendDamagePacket('F', Vector2.zero);
            dischargeCombo++;
        }
        else if (dischargeCombo == 4)
        {
            if (sr.flipX == false)
            {
                setTriggerForAttack(1, Vector3.zero, 'F', "discharge");
            }
            else
            {
                setTriggerForAttack(0, Vector3.zero, 'F', "discharge");
            }

            applyDamageToTrigger();
                //PlayerAuthenticator.instance.sendDamagePacket('E', Vector2.zero);
            dischargeCombo++;
        }
        else if (dischargeCombo == 5)
        {
            if (sr.flipX == false)
            {
                setTriggerForAttack(1, Vector3.zero, 'F', "discharge");
            }
            else
            {
                setTriggerForAttack(0, Vector3.zero, 'F', "discharge");
            }

            applyDamageToTrigger();
             //   PlayerAuthenticator.instance.sendDamagePacket('F', Vector2.zero);
            dischargeCombo++;
        }
        else if (dischargeCombo == 6)
        {
            if (sr.flipX == false)
            {
                setTriggerForAttack(1, (Vector3.right * 5f) + (Vector3.up * 4f), 'O', "discharge");
            }
            else
            {
                setTriggerForAttack(0, (-Vector3.right * 5f) + (Vector3.up * 4f), 'O', "discharge");
            }

            applyDamageToTrigger();
             //   PlayerAuthenticator.instance.sendDamagePacket('O', (-Vector3.right * 5f) + (Vector3.up * 5f));
            dischargeCombo = 0;
        }
    }

    public void triggerTemplate(int id) //For applyDamageToTrigger
    {
        if (triggerChecks[id].enemyObject.GetComponent<Enemy>().canBeHurt)
        {
            if(hurtType != 'C' || hurtType != 'B')
                PlayerAuthenticator.instance.pausePackets[triggerChecks[id].enemyObject.GetComponent<Enemy>().id] = true;
            PlayerAuthenticator.instance.sendDamagePacket(hurtType, pushDir, 5f, returnHitLocation(hitAttack), triggerChecks[id].enemyObject.GetComponent<Enemy>().id);
            triggerChecks[id].enemyObject.GetComponent<Enemy>().depleteHealth(hurtType, pushDir, 5f); //change damage
            triggerChecks[id].enemyObject.GetComponent<SpriteRenderer>().flipX = false;
            createHitEffect(hitAttack);
        }
    }

    protected void pauseEnemiesInTrigger(int triggerId, bool isSpecial = false)
    {
        if (triggerChecks[triggerId].enemyInside)
        {
            PlayerAuthenticator.instance.pausePackets[triggerChecks[triggerId].enemyObject.GetComponent<Enemy>().id] =
                true;

            if (isSpecial)
                currentSpecialAttackEnemy = triggerChecks[triggerId].enemyObject.GetComponent<Enemy>();
        }
    }

    private Enemy currentSpecialAttackEnemy;
    void unPauseEnemiesInSpecial()
    {
        PlayerAuthenticator.instance.pausePackets[currentSpecialAttackEnemy.id] = false;
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
                    triggerTemplate(0);
                    return true;
                }
                break;
            case 1:
                if (triggerChecks[1].enemyInside == true)
                {
                    if (triggerChecks[1].enemyObject.GetComponent<Enemy>().canBeHurt == true)
                    {
                        triggerTemplate(1);
                        return true;
                    
                    }
                }
                break;
            case 2:
                if (triggerChecks[2].enemyInside == true)
                {
                    if (triggerChecks[2].enemyObject.GetComponent<Enemy>().canBeHurt == true)
                    {
                        triggerTemplate(2);
                        return true;
                    }
                }
                break;
            case 3:
                if (triggerChecks[3].enemyInside == true)
                {
                    if (triggerChecks[3].enemyObject.GetComponent<Enemy>().canBeHurt == true)
                    {
                        triggerTemplate(3);
                        return true;
                    }
                }
                break;
            case 4:
                if (triggerChecks[4].enemyInside == true)
                {
                    if (triggerChecks[4].enemyObject.GetComponent<Enemy>().canBeHurt == true)
                    {
                        triggerTemplate(4);
                        return true;
                    }
                }
                break;
            case 5:
                if (triggerChecks[5].enemyInside == true)
                {
                    if (triggerChecks[5].enemyObject.GetComponent<Enemy>().canBeHurt == true)
                    {
                        triggerTemplate(5);
                        return true;
                    }
                }
                break;
            case 6:
                if (triggerChecks[6].enemyInside == true)
                {
                    if (triggerChecks[6].enemyObject.GetComponent<Enemy>().canBeHurt == true)
                    {
                        triggerTemplate(6);
                        return true;
                    }
                }
                break;
            case 7:
                if (triggerChecks[7].enemyInside == true)
                {
                    if (triggerChecks[7].enemyObject.GetComponent<Enemy>().canBeHurt == true)
                    {
                        triggerTemplate(7);
                        return true;
                    }
                }
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

    protected IEnumerator runCancelDelay()
    {
        yield return new WaitForSeconds(0.2f);
        isRunning = false;
    }
    IEnumerator checkJumpAttacks()
    {
        var time = 0f;

        if (hurtType == 'D')
        {
            while (time < 0.3f)
            {
                if (applyDamageToTrigger())
                {
                    if (hurtType == 'D')
                    {
                        var dir = triggerChecks[triggerToCheck].enemyObject.transform.position - transform.position;
                        dir.Normalize();
                        rb.velocity = Vector3.zero;
                        rb.AddForce(dir * -1f, ForceMode2D.Impulse);
                        //PlayerAuthenticator.instance.sendDamagePacket(hurtType, new Vector2(-dir.x, 0));
                    }
                    time = 0.3f;
                }
                time += Time.deltaTime;
                yield return null;
            }
            playerAction = playerState.jumping;
        }
        else
        {
            if (sr.flipX)
            {
                rb.velocity = rb.velocity - Vector2.right;
            }
            else
            {
                rb.velocity = rb.velocity + Vector2.right;
            }
            while (time < 0.6f)
            {

                if (applyDamageToTrigger())
                {
                    time = 0.6f;
                    //PlayerAuthenticator.instance.sendDamagePacket(hurtType, new Vector2(-rb.velocity.x, 0));
                }
                time += Time.deltaTime;
                yield return null;
            }

            touchingPos = transform.position - new Vector3((-GetComponent<BoxCollider2D>().bounds.extents.x), (GetComponent<BoxCollider2D>().bounds.extents.y * 0.5f));
            touchingPos2 = transform.position - new Vector3((GetComponent<BoxCollider2D>().bounds.extents.x), (GetComponent<BoxCollider2D>().bounds.extents.y * 0.5f));
            RaycastHit2D hit = Physics2D.Raycast(touchingPos, -Vector3.up, 0.18f, 1 << 8);
            RaycastHit2D hit2 = Physics2D.Raycast(touchingPos2, -Vector3.up, 0.18f, 1 << 8);
            
            if (hit.collider != null || hit2.collider != null)
            {
                playerAction = playerState.idle;
                anim.Play("Figher_idle");
                hurtType = 'A';
            }
            else
            {
                playerAction = playerState.jumping;
                hurtType = 'A';
            }


        }

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

    protected bool isHurt;
    protected bool flyingHurt;

    public HealthBarHandler healthBar;

    public bool hasDied()
    {
        if (health <= 0)
            return true;
        else
            return false;
    }

    protected string currentHurtState;
    public void receiveAttack(char hurtType, Vector3 pushDir, float damage)
    {
        healthBar.healthDepleter(damage);
        health -= damage;
        
        if (hasDied())
        {
            BattleManager.Instance.gameOver();
        }
        
        switch (hurtType)
        {
            case 'B':
                anim.Play("Fighter_hurtB");
                currentHurtState = "Enemy_hurtB";
                break;
            case 'C':
                anim.Play("Fighter_hurtC");
                currentHurtState = "Enemy_hurtC";
                break;
            case 'J':
                flyingHurt = true;
                anim.Play("Fighter_hurtJ");
                currentHurtState = "Enemy_hurtJ";
                break;
            case 'G':
                isHurt = true;
                anim.Play("Fighter_hurtG");
                currentHurtState = "Enemy_hurtG";
                break;
            case 'H':
                flyingHurt = true;
                isHurt = true;
                anim.Play("Fighter_hurtH");
                currentHurtState = "Enemy_hurtH";
                break;
            case 'D':
                anim.Play("Fighter_hurtD");
                currentHurtState = "Enemy_hurtD";
                break;
            case 'N':
                flyingHurt = true;
                anim.Play("Fighter_hurtN");
                currentHurtState = "Enemy_hurtN";
                break;
            case 'O':
                flyingHurt = true;
                anim.Play("Fighter_hurtO");
                currentHurtState = "Enemy_hurtO";
                break;
            case 'E':
                anim.Play("Fighter_hurtE");
                currentHurtState = "Enemy_hurtE";
                break;
            case 'F':
                anim.Play("Fighter_hurtF");
                currentHurtState = "Enemy_hurtF";
                break;
        }
        rb.AddForce(pushDir, ForceMode2D.Impulse);
    }
    
    public void canNowBeHurt(){
        isHurt = false;
        playerAction = playerState.idle;
        anim.Play("Figher_idle");
        flyingHurt = false;
    }

    protected Stack<bool> combo1Stack = new Stack<bool>();
    protected Stack<bool> combo2Stack = new Stack<bool>();
    public int stackIDToCheck;

    public void addToComboStack(int stackID)
    {
        if (stackID == 1 && combo2Stack.Count == 0)
        {
            combo1Stack.Push(true);
        }
        else if (stackID == 2 && combo1Stack.Count == 0)
        {
            combo2Stack.Push(true);
        }
    }
    public void checkComboStack() //Ends before last frame
    {
        if (combo1Stack.Count == 0 && combo2Stack.Count == 0)
        {
            completeAttack();
        }
        if (combo1Stack.Count > 0)
        {
            combo1Stack.Pop();
            #region punches
            if (comboDelay == false)
            {
                anim.SetBool("WalkTrigger", false);
                anim.SetBool("RunTrigger", false);
                if (comboChainNumber == 2)
                {
                    anim.Play("Fighter_kick1");
                    currentAttackAnim = "Enemy_kick1";
                    playerAction = playerState.attacking;
                    isRunning = false;
                    if (sr.flipX == false)
                    {
                        setTriggerForAttack(1, Vector3.right + Vector3.up, 'H', "kick1");
                        attackMove(Vector2.right);
                    }
                    else
                    {
                        setTriggerForAttack(0, -Vector3.right + Vector3.up, 'H', "kick1");
                        attackMove(-Vector2.right);
                    }

                }
                else
                {
                    anim.Play("Fighter_punch1");
                    currentAttackAnim = "Enemy_punch1";
                    playerAction = playerState.attacking;
                    isRunning = false;
                    if (sr.flipX == false)
                    {
                        if (comboChainNumber == 1)
                        {
                            setTriggerForAttack(1, Vector3.right, 'B', "punch1");
                            attackMove(Vector2.right);
                        }
                        else
                            setTriggerForAttack(1, Vector3.zero, 'B', "punch1");
                    }
                    else
                    {
                        if (comboChainNumber == 1)
                        {
                            setTriggerForAttack(0, Vector3.right, 'B', "punch1");
                            attackMove(-Vector2.right);
                        }
                        else
                            setTriggerForAttack(0, Vector3.zero, 'B', "punch1");
                    }
                }
                incrementComboChain();
            }
            #endregion
        }
        else if (combo2Stack.Count > 0)
        {
            combo2Stack.Pop();
            #region punches
            if (comboDelay == false)
            {
                anim.SetBool("WalkTrigger", false);
                anim.SetBool("RunTrigger", false);
                if (comboChainNumber2 == 1)
                {
                    anim.Play("Fighter_kick3");
                    currentAttackAnim = "Enemy_kick3";
                    playerAction = playerState.attacking;
                    isRunning = false;
                    if (sr.flipX == false)
                    {
                        var hitDir = Vector3.right + (Vector3.up * 4f);
                        setTriggerForAttack(1, hitDir, 'J', "kick3");
                        attackMove(Vector2.right);
                    }
                    else
                    {
                        var hitDir = -Vector3.right + (Vector3.up * 4f);
                        setTriggerForAttack(0, hitDir, 'J', "kick3");
                        attackMove(-Vector2.right);
                    }
                }
                else
                {
                    anim.Play("Fighter_punch1");
                    currentAttackAnim = "Enemy_punch1";
                    playerAction = playerState.attacking;
                    isRunning = false;
                    if (sr.flipX == false)
                    {
                        setTriggerForAttack(1, Vector3.right, 'C', "punch1");
                        attackMove(Vector2.right);
                    }
                    else
                    {
                        setTriggerForAttack(0, -Vector3.right, 'C', "punch1");
                        attackMove(-Vector2.right);
                    }
                }
                incrementComboChain2();
            }
            #endregion
        }
    }

    public GameObject hitEffectObject;

    Vector2 returnHitLocation(string attackName)
    {
        switch (attackName)
        {
            case "punch1":
                if (sr.flipX == false)
                    return transform.localPosition + new Vector3(0.188f, 0.108f);
                else
                    return transform.localPosition + new Vector3(-0.188f, 0.108f);

            case "dashkick":
                if (sr.flipX == false)
                    return transform.localPosition + new Vector3(0.197f, -0.115f);
                else
                    return transform.localPosition + new Vector3(-0.197f, -0.115f);

            case "dashpunch":
                if (sr.flipX == false)
                    return transform.localPosition + new Vector3(0.177f, 0.103f);
                else
                    return transform.localPosition + new Vector3(-0.177f, 0.103f);

            case "jumpatk1":
                if (sr.flipX == false)
                    return transform.localPosition + new Vector3(0.199f, -0.055f);
                else
                    return transform.localPosition + new Vector3(-0.199f, -0.055f);
  

            case "jumpatk2":
                if (sr.flipX == false)
                    return transform.localPosition + new Vector3(0.11f, -0.135f);
                else
                    return transform.localPosition + new Vector3(-0.11f, -0.135f);

            case "kick1":
                if (sr.flipX == false)
                    return transform.localPosition + new Vector3(0.186f, 0.025f);
                else
                    return transform.localPosition + new Vector3(-0.186f, 0.025f);

            case "kick3":
                if (sr.flipX == false)
                    return transform.localPosition + new Vector3(0.151f, 0.111f);
                else
                    return transform.localPosition + new Vector3(-0.151f, 0.111f);

            case "special":
                return Vector2.zero;
        }
        return Vector2.zero;
    }

    public void createHitEffect(string attackName)
    {
        //  punch1 0.188 0.108
        // dashkick 0.197 -0.115
        // dashpunch 0.177 0.103
        // jumpatk1 0.199 -0.055
        // jumpatk2 0.11  -0.135
        // kick1 0.186 0.025
        // kick3 0.151 0.111
        // special 0.093 0.226      0.141 0.144

        switch (attackName)
        {

            case "punch1":
                var newObj = Instantiate(hitEffectObject);
                if (sr.flipX == false)
                    newObj.transform.position = transform.localPosition + new Vector3(0.188f, 0.108f);
                else
                {
                    newObj.transform.position = transform.localPosition + new Vector3(-0.188f, 0.108f);
                    newObj.GetComponent<SpriteRenderer>().flipX = true;
                }
                break;

            case "dashkick":
                var newObj1 = Instantiate(hitEffectObject);
                if (sr.flipX == false)
                    newObj1.transform.position = transform.localPosition + new Vector3(0.197f, -0.115f);
                else
                {
                    newObj1.transform.position = transform.localPosition + new Vector3(-0.197f, -0.115f);
                    newObj1.GetComponent<SpriteRenderer>().flipX = true;
                }
                break;

            case "dashpunch":
                var newObj3 = Instantiate(hitEffectObject);
                if (sr.flipX == false)
                    newObj3.transform.position = transform.localPosition + new Vector3(0.177f, 0.103f);
                else
                {
                    newObj3.transform.position = transform.localPosition + new Vector3(-0.177f, 0.103f);
                    newObj3.GetComponent<SpriteRenderer>().flipX = true;
                }
                break;

            case "jumpatk1":
                var newObj4 = Instantiate(hitEffectObject);
                if (sr.flipX == false)
                    newObj4.transform.position = transform.localPosition + new Vector3(0.199f, -0.055f);
                else
                {
                    newObj4.transform.position = transform.localPosition + new Vector3(-0.199f, -0.055f);
                    newObj4.GetComponent<SpriteRenderer>().flipX = true;
                }
                break;

            case "jumpatk2":
                var newObj5 = Instantiate(hitEffectObject);
                if (sr.flipX == false)
                    newObj5.transform.position = transform.localPosition + new Vector3(0.11f, -0.135f);
                else
                {
                    newObj5.transform.position = transform.localPosition + new Vector3(-0.11f, -0.135f);
                    newObj5.GetComponent<SpriteRenderer>().flipX = true;
                }
                break;

            case "kick1":
                var newObj6 = Instantiate(hitEffectObject);
                if (sr.flipX == false)
                    newObj6.transform.position = transform.localPosition + new Vector3(0.186f, 0.025f);
                else
                {
                    newObj6.transform.position = transform.localPosition + new Vector3(-0.186f, 0.025f);
                    newObj6.GetComponent<SpriteRenderer>().flipX = true;
                }
                break;

            case "kick3":
                var newObj7 = Instantiate(hitEffectObject);
                if (sr.flipX == false)
                    newObj7.transform.position = transform.localPosition + new Vector3(0.151f, 0.111f);
                else
                {
                    newObj7.transform.position = transform.localPosition + new Vector3(-0.151f, 0.111f);
                    newObj7.GetComponent<SpriteRenderer>().flipX = true;
                }
                break;

            case "special":
                break;

        }

    }

    public void rollAfterWait()
    {
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

    protected void startEchipDelay()
    {
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
    IEnumerator echipDelayer()
    {
        yield return new WaitForSeconds(0.5f);
        echipDeployable = false;
    }

}
