using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerController : MonoBehaviour {

    private Rigidbody2D rbody;
    private playerCollider pcollider;

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

    //dash variables
    public float dashTime = 0.3f;
    public float dashMultiplier = 1.5f;
    public float dashCooldownTime = 0.3f;
    private bool dashState = false;
    private float dashStart = 0;
    private float dashEnd = 0;
    private bool dashCooldownState = false;

    //collision states
    private bool isGround = false;
    private bool isSide = false;
    private bool isRight = false;
    private bool isLeft = false;

    //gravity value
    private float gravityScale = 1;

    //size variables
    public float playerSize = 0.8f;

    //sprite handling
    public bool flipped = false; //false is face right, true is face left
    private SpriteRenderer sprite;

    //death handling
    private bool killer = false;
    private bool killed = false;

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
    }
	
	void Update ()
    {
        killed = pcollider.getKiller(); killer = pcollider.getKilled();
        if (killed || killer)
        {
            //can do death animation before reset
            pcollider.reset();
            if (killed) print(transform.name + " died");
            //updatePoints();
            return;
        }

        playerSize = 0.8f * transform.localScale.x;

        updateCollision();
        walkCheck();
        jumpCheck();
        dropCheck();
        dashCheck();
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

    //handling walking
    void walkCheck()
    {
        string axis = (playerNum == 1) ? "p1Horizontal" : "p2Horizontal";
        float walk = Input.GetAxis(axis);
        if (walk != 0)
        {
            float velX = Mathf.Sign(walk) * walkSpeed;
            flipped = (velX > 0) ? true : false;

            if (isLeft && velX < 0) rbody.velocity = new Vector2(rbody.velocity.x, -wallHoldSpeed);
            else if (isRight && velX > 0 ) rbody.velocity = new Vector2(rbody.velocity.x, -wallHoldSpeed);
            else rbody.velocity = new Vector2(velX, rbody.velocity.y);
        }
        else rbody.velocity = new Vector2(0, rbody.velocity.y);
    }

    //handling jumping
    void jumpCheck()
    {
        string axis = (playerNum == 1) ? "p1Jump" : "p2Jump";
        float jump = Input.GetAxis(axis);

        if (isGround && !jumpState) jumpCount = 0; //reset jump count when on ground
        else if (isSide && !jumpState & jump != 1) jumpCount = 1; //set jump  count to 1 for wall jump
        else if (!isGround && !jumpState && jumpCount == 0) jumpCount += 1; //set jump count to 1 when in air

        //change jump state based on keypress
        if (!jumpState && jump == 1 && jumpCount < maxJumpCount)
        {
            jumpCount += 1;
            jumpState = true;
            fallState = false;
            jumpStart = Time.time;
            wallJump = isSide && !isGround;
            wallJumpSide = (isLeft) ? 1 : -1;
        }
        else if (jumpState && !fallState && jump != 1)
        {
            fallStart();
            jumpState = false;
        }
        else if (jumpState && fallState && jump != 1)
        {
            jumpState = false;
        }

        //jumping
        if (jumpState && !fallState && jump == 1)
        {
            if (Time.time - jumpStart < jumpTime)
            {
                float jumpPercent = (Time.time - jumpStart) / jumpTime; //x
                float jumpFunction = (jumpPercent-1)*(jumpPercent-1); //y
                jumpFunction = (jumpFunction < 0.1f) ? 0.1f : jumpFunction;

                float jumpValue = jumpSpeed * jumpFunction * 2.5f;

                if (wallJump) {
                    rbody.velocity = new Vector2(rbody.velocity.x + wallJumpSide * jumpValue*0.5f, jumpValue*0.7f);
                    if(jumpFunction <= 0.3f) wallJump = false;
                }
                else rbody.velocity = new Vector2(rbody.velocity.x, jumpValue);

            }
            else fallStart();
        }

    }

    void fallStart()
    {
        if (!fallState) rbody.velocity = new Vector2(rbody.velocity.x, -1*fallSpeed);
        fallState = true;
    }

    void dropCheck()
    {
        string axis = (playerNum == 1) ? "p1Vertical" : "p2Vertical";
        float drop = Input.GetAxis(axis);

        if (!isGround && drop <= -0.5)
        {
            rbody.AddForce(new Vector2(0, dropForce));
            //rbody.velocity = new Vector2(rbody.velocity.x, dropSpeed);
        }
    }

    void dashCheck()
    {
        string axis = (playerNum == 1) ? "p1Dash" : "p2Dash";
        float dash = Input.GetAxis(axis);

        if (!dashState && !dashCooldownState && dash == 1)
        {
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
            if (Time.time - dashStart < dashTime)
            {
                rbody.gravityScale = 0;
                rbody.velocity = new Vector2(speed, 0);
            }
            else dashCooldown();
        }
    }

    void dashCooldown()
    {
        if (!dashCooldownState) dashEnd = Time.time;
        rbody.gravityScale = gravityScale;
        dashCooldownState = true;
    }

    public float getDashCooldown()
    {
        if (!dashState && !dashCooldownState) return 1; //dash available
        else if (dashState && !dashCooldownState) return 0;//dashing
        else return (Time.time - dashStart) / dashTime;//dash on cooldown
    }

    public float getPlayerSize()
    {
        return playerSize;
    }
}
