using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerController : MonoBehaviour {

    private Rigidbody2D rbody;
    private playerCollider pcollider;

    public KillPointHandler points;

    private int playerNum; //player 1 or 2

    //walk variables
    public float walkSpeed = 5.0f;          //speed of walk
    public float wallHoldSpeed = 1.0f;

    //jump variables
    public float jumpSpeed = 5.0f;          //speed of jump
    public float jumpTime = 0.5f;           //duration of jump
    public int maxJumpCount = 2;            //max number of jumps
    private bool jumpState = false;         //state of jump
    private bool wallJump = false;
    private int wallJumpSide = 0;
    private float jumpStart = 0;            //start time of jump
    private int jumpCount = 0;

    //fall variables
    private bool fallState = false;         //state of fall
    public float fallSpeed = 1.0f;          //initial falling speed

    //drop variables
    public float dropForce = -80f;
    public float dropSpeed = 3.0f;
    private bool dropState = false;

    //dash variables
    public float dashTime = 0.3f;
    public float dashMultiplier = 1.5f;
    public float dashCooldownTime = 0.3f;
    private bool dashState = false;
    private float dashStart = 0;
    private float dashEnd = 0;
    private bool dashCooldownState = false;

    //parry variables
    private GameObject parryright;
    private GameObject parryleft;
    private GameObject parryup;
    private bool parryState = false;
    private bool parryCooldown = false;
    public float parryCooldownTime = 0.5f;
    public float parryTime = 0.1f;
    private float parryStart;

    //get parried variables
    public float getParriedTime = 0.3f;
    public float getParriedKnockbackTime = 0.15f;
    public float getParriedKnockbackSpeed = 8f;
    private float getParriedStart;
    private string getParriedSide;

    //get stunned variables
    private bool getStunnedState = false;

    //killed safe variables
    public float killedSafeTime = 0.3f;
    private bool killedSafeState = false;
    private float killedSafeStart;

    //collision states
    private bool isGround = false;
    private bool isSide = false;
    private bool isRight = false;
    private bool isLeft = false;

    //gravity value
    private float gravityScale = 0.9f;

    //size variables
    public float playerSize = 0.8f;

    //sprite handling
    public bool flipped = false; //false is face right, true is face left
    private SpriteRenderer sprite;

    //powerup handling
    private bool mushroom = false;
    public float mushroomTime = 1.0f;

    private void Awake()
    {
        rbody = transform.GetComponent<Rigidbody2D>();
        pcollider = transform.GetComponent<playerCollider>();
        sprite = transform.transform.Find("sprite").GetComponent<SpriteRenderer>();

        playerNum = (transform.name == "Player1") ? 1 : 2;

        setUp();
    }

    void setUp()
    {
        walkSpeed *= transform.localScale.x;
        wallHoldSpeed *= transform.localScale.x;
        jumpSpeed *= transform.localScale.x;
        fallSpeed *= transform.localScale.x;
        dropForce *= transform.localScale.x;
        gravityScale *= transform.localScale.x;
        rbody.gravityScale = gravityScale;

        parryright = transform.Find("parryright").gameObject;
        parryleft = transform.Find("parryleft").gameObject;
        parryup = transform.Find("parryup").gameObject;
        enableParryDirection(false, false, false);
    }

    void Update()
    {
        playerSize = 0.8f * transform.localScale.x;
        updateCollision();

        killCheck();
        killSafeStateCheck();

        if (!getStunnedState)
        {
            walkCheck();
            jumpCheck();
            dropCheck();
            dashCheck();
            parryCheck();
        }
        else getParriedCheck();

        spriteFlipCheck();
    }

    void updateCollision()
    {
        isGround = pcollider.isBottom();
        isSide = pcollider.isLeft() || pcollider.isRight();
        isLeft = pcollider.isLeft();
        isRight = pcollider.isRight();
        //isWall = pcollider.isWall();
    }

    void spriteFlipCheck()
    {
        sprite.flipX = flipped;
    }

    //handle death
    void killCheck()
    {
        bool killed = pcollider.getKill();

        if (killed)
        {
            //can do death animation before reset
            GameObject killer = pcollider.getKiller();
            if (killer.tag == "Player")
            {
                int num = (transform.name == "Player1") ? 2 : 1;
                points.updateScore(num, 1);
            }
            else if (killer.tag == "AreaHazard")
            {
                points.updateScore(playerNum, -1);
            }
            pcollider.reset(points.getSpawnPoint(transform.position));
            print(transform.name + " died");
        }
    }

    void killSafeStateCheck()
    {
        if (killedSafeState)
        {
            if (Time.time - killedSafeStart > killedSafeTime)
            {
                killedSafeState = false; //set back to normal animation
                pcollider.checkAreaHazardTouch();
                pcollider.checkPlayerTopTouch();
            }
        }
    }

    private void setKilledSafeState()
    {
        killedSafeState = true;
        killedSafeStart = Time.time;
        //set to invulnerable animation
    }

    public bool getKilledSafeState()
    {
        return killedSafeState;
    }

    //handling walking
    void walkCheck()
    {
        string axis = (playerNum == 1) ? "p1Horizontal" : "p2Horizontal";
        float walk = Input.GetAxis(axis);
        if (walk != 0)
        {
            float velX = Mathf.Sign(walk) * walkSpeed;
            flipped = (velX > 0) ? true : false;

            if (isLeft && velX < 0 && walk < -0.85f) rbody.velocity = new Vector2(0, -wallHoldSpeed);
            else if (isRight && velX > 0 && walk > 0.85f) rbody.velocity = new Vector2(0, -wallHoldSpeed);
            else if ((isLeft && velX < 0) || (isRight && velX > 0)) rbody.velocity = new Vector2(0, rbody.velocity.y);
            else rbody.velocity = new Vector2(velX, rbody.velocity.y);
            //set to walking animation
        }
        else rbody.velocity = new Vector2(0, rbody.velocity.y); //set to stationary animation
    }

    //handling jumping
    void jumpCheck()
    {
        string jumpBtn = (playerNum == 1) ? "p1Jump" : "p2Jump";
        bool jumpPress = Input.GetButtonDown(jumpBtn);
        bool jumpRelease = Input.GetButtonUp(jumpBtn);

        //handling jump counter
        if (isGround && !jumpState) jumpCount = 0; //reset jump count when on ground
        else if (isSide && !jumpState & jumpPress) jumpCount = 1; //set jump  count to 1 for wall jump
        else if (!isGround && !jumpState && jumpCount == 0) jumpCount += 1; //set jump count to 1 when in air

        if (jumpPress && !jumpState && jumpCount < maxJumpCount) //start jump
        {
            jumpCount += 1;
            jumpState = true;
            fallState = false;
            dropState = false;

            wallJump = isSide && !isGround;
            wallJumpSide = (isLeft) ? 1 : -1;

            jumpStart = Time.time;
            //set to jumping animation (if wall jump, set to wall jump animation. if not wall jump & jumpCount = 0, jump from ground animation. if not wall jump & jumpCount = 1, second jump animation.)
        }

        if ((jumpState && !jumpRelease) && !fallState && !dropState) //jumping & not yet falling
        {
            if (Time.time - jumpStart >= jumpTime) fallStart();
            else Jump();
        }

        if ((jumpState && jumpRelease) || dropState) fallStart();
    }

    void Jump()
    {
        float jumpPercent = (Time.time - jumpStart) / jumpTime; //x
        float jumpFunction = (jumpPercent - 1) * (jumpPercent - 1); //y
        jumpFunction = (jumpFunction < 0.1f) ? 0.1f : jumpFunction;

        float jumpValue = jumpSpeed * jumpFunction * 2.5f;

        if (wallJump)
        {
            string vAxis = (playerNum == 1) ? "p1Vertical" : "p2Vertical";
            float v = Input.GetAxis(vAxis);
            rbody.velocity = new Vector2(rbody.velocity.x + wallJumpSide * jumpValue * 0.4f, jumpValue * 0.7f);
            if (jumpFunction <= 0.3f || rbody.velocity.x * wallJumpSide < 0 || v >= 0.8f) wallJump = false;
        }
        else rbody.velocity = new Vector2(rbody.velocity.x, jumpValue);
    }

    //handling jumping
    void jumpCheck2()
    {
        string axis = (playerNum == 1) ? "p1Jump" : "p2Jump";
        float jump = Input.GetAxis(axis);
        string vAxis = (playerNum == 1) ? "p1Vertical" : "p2Vertical";
        float v = Input.GetAxis(vAxis);

        if (isGround && !jumpState) jumpCount = 0; //reset jump count when on ground
        else if (isSide && !jumpState & jump != 1) jumpCount = 1; //set jump  count to 1 for wall jump
        else if (!isGround && !jumpState && jumpCount == 0) jumpCount += 1; //set jump count to 1 when in air

        //change jump state based on keypress
        if (!jumpState && jump == 1 && jumpCount < maxJumpCount)
        {
            jumpCount += 1;
            jumpState = true;
            fallState = false;
            dropState = false;
            jumpStart = Time.time;
            wallJump = isSide && !isGround;
            wallJumpSide = (isLeft) ? 1 : -1;
        }
        else if ((jumpState && !fallState && jump != 1) || dropState) //start falling immediately on let go of jump button or on drop
        {
            fallStart();
            jumpState = false;
        }
        //jumping
        if (jumpState && !fallState && jump == 1)
        {
            if (Time.time - jumpStart < jumpTime)
            {
                float jumpPercent = (Time.time - jumpStart) / jumpTime; //x
                float jumpFunction = (jumpPercent - 1) * (jumpPercent - 1); //y
                jumpFunction = (jumpFunction < 0.1f) ? 0.1f : jumpFunction;

                float jumpValue = jumpSpeed * jumpFunction * 2.5f;

                if (wallJump) {
                    rbody.velocity = new Vector2(rbody.velocity.x + wallJumpSide * jumpValue * 0.4f, jumpValue * 0.7f);
                    if (jumpFunction <= 0.3f || rbody.velocity.x * wallJumpSide < 0 || v >= 0.8f) wallJump = false;
                }
                else rbody.velocity = new Vector2(rbody.velocity.x, jumpValue);

            }
            else fallStart();
        }

    }

    void fallStart()
    {
        //set to falling animation
        if (!fallState) rbody.velocity = new Vector2(rbody.velocity.x, -1 * fallSpeed);
        fallState = true;
        jumpState = false;
    }

    void dropCheck()
    {
        //string dropBtn = (playerNum == 1) ? "p1Drop" : "p2Drop";
        //bool drop = Input.GetButtonDown(dropBtn);

        string vAxis = (playerNum == 1) ? "p1Vertical" : "p2Vertical";
        float v = Input.GetAxis(vAxis);

        if (!isGround && v <= -0.8f && !dropState)
        {
            //set to drop animation
            float vSpeed = (rbody.velocity.y > 0) ? -dropSpeed : rbody.velocity.y - dropSpeed;
            rbody.velocity = new Vector2(rbody.velocity.x, vSpeed);
            dropState = true;
        }
        if (isGround) dropState = false; //set to end drop animation

        if (dropState) rbody.gravityScale *= 1.4f;
        else rbody.gravityScale = gravityScale;

        if (rbody.velocity.y < -200.0f) rbody.velocity = new Vector2(rbody.velocity.x, -200.0f);
    }

    void dashCheck()
    {
        string dashBtn = (playerNum == 1) ? "p1Dash" : "p2Dash";
        bool dash = Input.GetButtonDown(dashBtn);

        string vAxis = (playerNum == 1) ? "p1Vertical" : "p2Vertical";
        float v = Input.GetAxis(vAxis);

        if (dashState && (dropState || jumpState)){
            dashCooldown();
        }

        if (!dashState && !dashCooldownState && dash)
        {
            //start dash animation
            dashState = true;
            dashStart = Time.time;
        }

        if (dashCooldownState && (Time.time - dashEnd >= dashCooldownTime))
        {
            dashState = false;
            dashCooldownState = false;
        }

        if (dashState && !dashCooldownState) {
            float direction = (flipped) ? 1 : -1;
            float speed = direction * walkSpeed * dashMultiplier;
            float vspeed = (rbody.velocity.y > 0) ? 0 : rbody.velocity.y;
            if (Time.time - dashStart < dashTime)
            {
                rbody.gravityScale = 0;
                rbody.velocity = new Vector2(speed, 0);
                rbody.gravityScale = gravityScale;
            }
            else dashCooldown();
            if (Input.GetButtonUp(dashBtn)) dashCooldown();
        }
    }

    void dashCooldown()
    {
        //stop dash animation
        if (!dashCooldownState) dashEnd = Time.time;
        dashCooldownState = true;
    }

    public float getDashCooldown()
    {
        if (getStunnedState) return 0;
        if (!dashState && !dashCooldownState) return 1; //dash available
        else if (dashState && !dashCooldownState) return 0;//dashing
        else return (Time.time - dashEnd) / dashCooldownTime;//time left to available dash
    }

    void parryCheck()
    {
        string parryBtn = (playerNum == 1) ? "p1Parry" : "p2Parry";
        bool parry = Input.GetButtonDown(parryBtn);

        string vAxis = (playerNum == 1) ? "p1Vertical" : "p2Vertical";
        float v = Input.GetAxis(vAxis);

        if (parry && !parryState && !parryCooldown) //start parry
        {
            //start parry animation
            parryState = true;
            parryStart = Time.time;
            if (v >= 0.7) enableParryDirection(false, false, true);
            else if (!flipped) enableParryDirection(true, false, false);
            else enableParryDirection(false, true, false);
        }

        if (parryState) { //parrying
            float elapsed = Time.time - parryStart;
            if (elapsed >= parryTime) { //stop parry
                enableParryDirection(false, false, false);
                parryState = false;
                parryCooldown = true;
                parryStart = Time.time;
            }
        }

        if (parryCooldown) //parryCooldown
        {
            float elapsed = Time.time - parryStart;
            if (elapsed >= parryCooldownTime)
            {
                parryCooldown = false;
            }
        }
        
    }

    public float getParryCooldown()
    {
        if (getStunnedState) return 0;
        if (!parryState && !parryCooldown) return 1; //parry available
        else if (parryState && !parryCooldown) return 0;//parrying
        else return (Time.time - parryStart) / parryCooldownTime;//time left to available dash
    }

    void enableParryDirection(bool left, bool right, bool up) //left, right, up
    {
        parryleft.SetActive(left);
        parryright.SetActive(right);
        parryup.SetActive(up);
    }

    public void getParried(string side)
    {
        if (dashState) dashCooldown();
        if (jumpState) fallStart();

        getStunnedState = true;
        getParriedStart = Time.time;
        getParriedSide = side;
    }

    void getParriedCheck()
    {
        if (!getStunnedState) return;
        if (Time.time - getParriedStart > getParriedTime) getStunnedState = false;
        else if (Time.time - getParriedStart <= getParriedKnockbackTime)
        {
            float xSpeed = rbody.velocity.x;
            float ySpeed = rbody.velocity.y;
            switch (getParriedSide)
            {
                case "top":
                    xSpeed = (flipped) ? -getParriedKnockbackSpeed / 2 : getParriedKnockbackSpeed / 2;
                    rbody.velocity = new Vector2(xSpeed, -getParriedKnockbackSpeed/2);
                    break;
                case "bottom":
                    xSpeed = (flipped) ? -getParriedKnockbackSpeed / 2 : getParriedKnockbackSpeed / 2;
                    rbody.velocity = new Vector2(xSpeed, getParriedKnockbackSpeed/2);
                    break;
                case "left":
                    rbody.velocity = new Vector2(getParriedKnockbackSpeed, rbody.velocity.y);
                    break;
                case "right":
                    rbody.velocity = new Vector2(-getParriedKnockbackSpeed, rbody.velocity.y);
                    break;
                default:
                    break;
            }
        }
    }

    public float getPlayerSize() //default size
    {
        return playerSize;
    }

    public void powerup_ActivateMushroom()
    {
        if (mushroom) return; //already activated
        mushroom = true;
        transform.localScale *= 1.5f;
    }

    public void powerup_DectivateMushroom()
    {
        mushroom = false;
        transform.localScale *= (1.0f/1.5f);
        setKilledSafeState();
    }

    public bool powerup_getMushroom()
    {
        return mushroom;
    }

    public void destroy_powerup(GameObject powerup) { points.destroyPowerup(powerup); }
}
