using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerController : MonoBehaviour {

    private Rigidbody2D rbody;
    private playerCollider pcollider;

    public KillPointHandler points;

    private int playerNum; //player 1 or 2

    public float speedMultiplier = 1.0f;

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

    //kill handling
    private bool killed = false;

    //gravity value
    private float gravityScale = 0.9f;

    //size variables
    public float playerSize = 0.8f;

    //sprite handling
    public bool flipped = false; //false is face right, true is face left
    private SpriteRenderer sprite;
    private Animator anim;

    //powerup handling
    private bool mushroom = false;
    public float mushroomTime = 1.0f;

    public AudioClip jump1Sound;
    public AudioClip jump2Sound;
    public AudioClip dashSound;
    public AudioClip parrySound;
    public AudioClip getParriedSound;
    public AudioClip deathSound;

    private AudioSource source;

    private bool flipPowerup = false;
    private float flipPowerup_start;
    public float flipPowerup_time = 1.0f;

    private bool slowPowerup = false;
    private float slowPowerup_start;
    public float slowPowerup_time = 1.0f;

    private Vector3 originPos;

    public overall_data overall;
    public RuntimeAnimatorController char1;
    public RuntimeAnimatorController char2;
    public RuntimeAnimatorController char3;

    public GameObject slow;
    public GameObject flip;

    private void Awake()
    {
        originPos = transform.localPosition;
        rbody = transform.GetComponent<Rigidbody2D>();
        pcollider = transform.GetComponent<playerCollider>();
        sprite = transform.Find("sprite").GetComponent<SpriteRenderer>();
        anim = transform.Find("sprite").GetComponent<Animator>();

        playerNum = (transform.name == "Player1") ? 1 : 2;

        setUp();

        source = GetComponent<AudioSource>();
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

    private void OnEnable()
    {
        Reset();
    }

    public void Reset()
    {
        int charNum = (playerNum == 1) ? overall.getP1() : overall.getP2();
        if (charNum == 0) anim.runtimeAnimatorController = char1;
        if (charNum == 1) anim.runtimeAnimatorController = char2;
        if (charNum == 2) anim.runtimeAnimatorController = char3;

        getStunnedState = false;
        killedSafeState = false;
        sprite.color = Color.white;

        resetPowerup();
        transform.localPosition = originPos;
        transform.rotation = Quaternion.identity;
    }

    void resetPowerup()
    {
        if (mushroom) transform.localScale *= (1.0f / 1.5f);
        mushroom = false;
        flipPowerup = false;
        flip.SetActive(false);
        if (slowPowerup) speedMultiplier = 1.0f;
        slowPowerup = false;
        slow.SetActive(false);
    }

    void Update()
    {
        playerSize = 0.8f * transform.localScale.x;
        updateCollision();

        killCheck();
        killSafeStateCheck();
        getParriedCheck();
        check_slowPowerup();
        check_flipPowerup();

        if (!getStunnedState && !pcollider.getKill())
        {
            walkCheck();
            jumpCheck();
            dropCheck();
            dashCheck();
            parryCheck();
        }

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
        bool killedCheck = pcollider.getKill();

        if (killedCheck && !killed)
        {
            killed = true;
            //can do death animation before reset
            source.PlayOneShot(deathSound);

            anim.SetBool("isDead", true);
            StartCoroutine("DeathPause");
        }
    }

    IEnumerator DeathPause()
    {
        StartCoroutine("DeathRotation");
        yield return new WaitForSeconds(0.3f);

        GameObject killer = pcollider.getKiller();
        if (killer.tag == "Feet")
        {
            int num = (transform.name == "Player1") ? 2 : 1;
            points.updateScore(num, 1);
        }
        else if (killer.tag == "AreaHazard")
        {
            points.updateScore(playerNum, -1);
        }
        print(transform.name + " died");
        anim.SetBool("isDead", false);
        resetPowerup();
        pcollider.reset(points.getSpawnPoint(pcollider.getOtherPlayerPosition()));
        pcollider.setKill(false);
        killed = false;
    }

    IEnumerator DeathRotation()
    {
        while (pcollider.getKill())
        {
            transform.Rotate(Vector3.forward, 5);
            yield return new WaitForSeconds(0.01f);
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

    IEnumerator KillSafeBlinking()
    {
        while (killedSafeState)
        {
            sprite.color = Color.gray;
            yield return new WaitForSeconds(0.01f);
            sprite.color = Color.white;
        }
    }

    public void setKilledSafeState()
    {
        killedSafeState = true;
        killedSafeStart = Time.time;
        //set to invulnerable animation
        StartCoroutine("KillSafeBlinking");
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
            float velX = Mathf.Sign(walk) * walkSpeed * speedMultiplier;
            if (flipPowerup) velX = -velX;
            flipped = (velX > 0) ? true : false;

            if (isLeft && velX < 0 && walk < -0.85f) rbody.velocity = new Vector2(0, -wallHoldSpeed);
            else if (isRight && velX > 0 && walk > 0.85f) rbody.velocity = new Vector2(0, -wallHoldSpeed);
            else if (pcollider.getPlayerLeftTouch()) rbody.velocity = (rbody.velocity.x > 0) ? rbody.velocity : new Vector2(0, rbody.velocity.y);
            else if (pcollider.getPlayerRightTouch()) rbody.velocity = (rbody.velocity.x < 0) ? rbody.velocity : new Vector2(0, rbody.velocity.y);
            else if ((isLeft && velX < 0) || (isRight && velX > 0)) rbody.velocity = new Vector2(0, rbody.velocity.y);
            else rbody.velocity = new Vector2(velX, rbody.velocity.y);
            //set to walking animation
            anim.SetBool("isWalking", true);
        }
        else
        {
            rbody.velocity = new Vector2(0, rbody.velocity.y); //set to stationary animation
            anim.SetBool("isWalking", false);
        }
    }

    //handling jumping
    void jumpCheck()
    {
        string jumpBtn = (playerNum == 1) ? "p1Jump" : "p2Jump";
        bool jumpPress = Input.GetButtonDown(jumpBtn);
        bool jumpRelease = Input.GetButtonUp(jumpBtn);

        string dashBtn = (playerNum == 1) ? "p1Dash" : "p2Dash";
        bool dashPress = Input.GetButtonDown(dashBtn);

        string vAxis = (playerNum == 1) ? "p1Vertical" : "p2Vertical";
        float v = Input.GetAxis(vAxis);
        bool dropPress = (!isGround && v <= -0.8f && !dropState) ? true : false;

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
            if (wallJump) jumpCount = 1;

            jumpStart = Time.time;
            //set to jumping animation (if wall jump, set to wall jump animation. if not wall jump & jumpCount = 0, jump from ground animation. if not wall jump & jumpCount = 1, second jump animation.)
            if (jumpCount == 1 && !wallJump) source.PlayOneShot(jump1Sound);
            else source.PlayOneShot(jump2Sound);

            anim.SetBool("isJumping", true);
        }

        if ((jumpState && !jumpRelease) && !fallState && !dropState) //jumping & not yet falling
        {
            if (Time.time - jumpStart >= jumpTime) fallStart();
            else Jump();
        }

        if ((jumpState && jumpRelease) || dropPress || dashPress) fallStart();
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
        else rbody.velocity = new Vector2(rbody.velocity.x * speedMultiplier, jumpValue);
    }

    void fallStart()
    {
        //set to falling animation
        if (!fallState) rbody.velocity = new Vector2(rbody.velocity.x, -1 * fallSpeed);
        fallState = true;
        jumpState = false;

        anim.SetBool("isJumping", false);
        anim.SetBool("isFalling", true);
    }

    void dropCheck()
    {
        //string dropBtn = (playerNum == 1) ? "p1Drop" : "p2Drop";
        //bool drop = Input.GetButtonDown(dropBtn);

        string vAxis = (playerNum == 1) ? "p1Vertical" : "p2Vertical";
        float v = Input.GetAxis(vAxis);

        string dashBtn = (playerNum == 1) ? "p1Dash" : "p2Dash";
        bool dashPress = Input.GetButtonDown(dashBtn);

        string jumpBtn = (playerNum == 1) ? "p1Jump" : "p2Jump";
        bool jumpPress = Input.GetButtonDown(jumpBtn);

        if (dropState && (dashPress || jumpPress)) dropState = false;

        if (!isGround && v <= -0.8f && !dropState)
        {
            //set to drop animation
            float vSpeed = (rbody.velocity.y > 0) ? -dropSpeed : rbody.velocity.y - dropSpeed;
            rbody.velocity = new Vector2(rbody.velocity.x, vSpeed);
            dropState = true;

            anim.SetBool("isFalling", true);
        }
        if (isGround)
        {
            dropState = false; //set to end drop animation
            anim.SetBool("isFalling", false);
        }

        if (dropState) rbody.gravityScale *= 1.4f;
        else rbody.gravityScale = gravityScale;

        if (rbody.velocity.y < -200.0f) rbody.velocity = new Vector2(rbody.velocity.x, -200.0f);


    }

    void dashCheck()
    {
        string dashBtn = (playerNum == 1) ? "p1Dash" : "p2Dash";
        bool dash = Input.GetButtonDown(dashBtn);

        string jumpBtn = (playerNum == 1) ? "p1Jump" : "p2Jump";
        bool jumpPress = Input.GetButtonDown(jumpBtn);

        string vAxis = (playerNum == 1) ? "p1Vertical" : "p2Vertical";
        float v = Input.GetAxis(vAxis);
        bool dropPress = (!isGround && v <= -0.8f && !dropState) ? true : false;

        if (dashState && (dropPress || jumpPress)) dashCooldown();

        if (!dashState && !dashCooldownState && dash)
        {
            //start dash animation
            if (jumpState) jumpState = false;
            dashState = true;
            dashStart = Time.time;

            source.PlayOneShot(dashSound);
            anim.SetBool("isDashing", true);
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
                rbody.velocity = new Vector2(speed * speedMultiplier, 0);
                rbody.gravityScale = gravityScale;
            }
            else dashCooldown();
            //if (Input.GetButtonUp(dashBtn)) dashCooldown();
        }
    }

    void dashCooldown()
    {
        //stop dash animation
        if (!dashCooldownState) dashEnd = Time.time;
        dashCooldownState = true;

        anim.SetBool("isDashing",false);
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

            source.PlayOneShot(parrySound);
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

        source.PlayOneShot(getParriedSound);
        StartCoroutine("getParriedColour");
    }

    IEnumerator getParriedColour()
    {
        while (getStunnedState)
        {
            sprite.color = Color.red;
            yield return new WaitForSeconds(0.01f);
            sprite.color = Color.white;
        }
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

    public void get_flipPowerup()
    {
        slowPowerup = false; speedMultiplier = 1.0f;
        slow.SetActive(false);
        flipPowerup = true;
        flipPowerup_start = Time.time;
        flip.SetActive(true);
    }

    void check_flipPowerup()
    {
        if (Time.time - flipPowerup_start > flipPowerup_time) { flipPowerup = false; flip.SetActive(false); }
    }

    public void get_slowPowerup()
    {
        flipPowerup = false;
        flip.SetActive(false);
        slowPowerup = true;
        slowPowerup_start = Time.time;
        speedMultiplier = 0.5f;
        slow.SetActive(true);
    }

    void check_slowPowerup()
    {
        if (Time.time - slowPowerup_start > slowPowerup_time) { slowPowerup = false; speedMultiplier = 1.0f; slow.SetActive(false); }
    }
}
