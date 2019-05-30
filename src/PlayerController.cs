using System;
using System.Collections;
using UnityEngine;

public class PlayerController : GroundEntity
{
    LaserGun laserGun;

    bool DidFire { get {
        return !IsKnockedBack && rechargeTimer == 0 && mInput > 0f && oldMInput == 0f;
    }}

    bool canInput;
    PlayerMovementState playerMovementState;

    public float maxWalkSpeed;
    public float walkAcceleration;
    public float jumpPower;
    public float maxJumpTime;
    public float jumpBufferTime;
    public float coyoteTime;
    public float fireTime;
    public float fireRecoil;
    public float rechargeTime;
    public float resetTime;

    float xInput;
    float yInput;
    float oldYInput;
    float mInput;
    float oldMInput;

    float jumpTimer;
    float jumpBufferTimer;
    float coyoteTimer;
    float fireTimer;
    float rechargeTimer;
    float resetTimer;

    Animator anim;
    bool currentlyFlashing;

    public delegate void PlayerAction();
    public static event PlayerAction OnJump;
    public static event PlayerAction OnFire;
    public static event PlayerAction OnPlayerDeath;

    protected override void Start() {
        base.Start();
        
        canInput = true;
        laserGun = GetComponentInChildren<LaserGun>(false);
        
        anim = GetComponent<Animator>();
    }

    public void ActivateLaserGun() {
        laserGun = GetComponentInChildren<LaserGun>(true);
        laserGun.gameObject.SetActive(true);
    }
    
    protected override void Update() {
        base.Update();

        if (canInput) {
            xInput = Input.GetAxisRaw("Horizontal");
            yInput = Input.GetAxisRaw("Jump");
            mInput = Input.GetAxisRaw("Fire");
        }

        // Check for reset
        if (Input.GetAxisRaw("Reset") > 0) {
            resetTimer += Time.deltaTime;
            if (resetTimer >= resetTime)
                ForceDeath();
        } else {
            resetTimer = 0;
        }

        DoAnimations();
    }

    void DoAnimations() {
        // Code-based animation
        if (laserGun != null)
            sr.flipX = !laserGun.IsFacingRight;
        else if (RealVelocity.x != 0 && (sr.flipX ^ RealVelocity.x < 0))
            sr.flipX = !sr.flipX;
        
        sr.transform.rotation = Quaternion.Euler(0, 0, -10 * RealVelocity.x / maxWalkSpeed);
        
        // Animator-based animation
        anim.SetFloat("IFrameTimer", iFrameTimer);
        anim.SetFloat("HitTimer", knockbackTimer);
        anim.SetBool("IsShooting", playerMovementState == PlayerMovementState.FIRING);
        anim.SetFloat("RechargeTimer", rechargeTimer);
        anim.SetFloat("VerticalSpeed", RealVelocity.y);
        anim.SetBool("IsJumping", playerMovementState == PlayerMovementState.JUMPING);
        anim.SetBool("IsGrounded", IsGrounded);
    }

    protected override void FixedUpdate() {
        base.FixedUpdate();

        if (baseVelocity != Vector2.zero && !Ground)
            Debug.LogWarning("Player's base velocity is " + baseVelocity + " in the air!");

        // Recharge gun every frame
        rechargeTimer = Math.Max(0, rechargeTimer - Time.deltaTime);

        // Buffered jump input
        if (yInput > 0 && oldYInput == 0)
            jumpBufferTimer = jumpBufferTime;
        else
            jumpBufferTimer = Mathf.Max(jumpBufferTimer - Time.deltaTime, 0);
        
        // Coyote time
        if (IsGrounded)
            coyoteTimer = coyoteTime;
        else
            coyoteTimer = Mathf.Max(coyoteTimer - Time.deltaTime, 0);

        // Main movement
        if (playerMovementState <= PlayerMovementState.JUMPING) {
            HandleMovement();        
            if (laserGun != null) {    
                laserGun.Aim();
                if (canInput)
                    CheckForFire();
            }
        }

        // DEBUGGING VELOCITY CHECKS
        /*transform.GetChild(4).gameObject.SetActive(IsGrounded);
        Debug.Log(
            groundcheck.IsColliding + " / " +
            (groundcheck.IsColliding ? (groundcheck.Collider.bounds.max.y + " vs " + coll.bounds.min.y + 0.2f) : "Null") + " / " +
            (groundcheck.Collider?.GetComponent<MovingPlatform>()?.GetVelocity().ToString() ?? "Null") + " / " +
            (rb2d.velocity.y + " " + baseVelocity.y));
            */

        oldYInput = yInput;
        oldMInput = mInput;
    }

    void HandleMovement() {
        Vector2 velocity = rb2d.velocity - baseVelocity;
        
        // HORIZONTAL VELOCITY
        velocity.x = GetXVelocity(velocity.x);

        // VERTICAL VELOCITY
        if (playerMovementState == PlayerMovementState.NORMAL && jumpBufferTimer > 0 && coyoteTimer > 0) {
            jumpBufferTimer = 0;
            coyoteTimer = 0;
            jumpTimer = 0;

            rb2d.velocity = new Vector2(rb2d.velocity.x, 0.5f); // set explicitly to bypass the ceiling check
            playerMovementState = PlayerMovementState.JUMPING;

            OnJump?.Invoke();
        }
        if (playerMovementState == PlayerMovementState.JUMPING) {
            velocity.y = jumpPower * Time.deltaTime;

            jumpTimer += Time.deltaTime;
            if (yInput == 0 || jumpTimer >= maxJumpTime || rb2d.velocity.y < 0.2f) {
                velocity.y *= 0.5f;
                playerMovementState = PlayerMovementState.NORMAL;
            }
        }

        rb2d.velocity = velocity + baseVelocity;
    }

    float GetXVelocity(float currXVel) {
        float accel = walkAcceleration * (playerMovementState == PlayerMovementState.FIRING ? 0.2f : 1);

        if (xInput == 0)
            currXVel -= Math.Sign(currXVel) * Math.Min(accel * Time.deltaTime, Math.Abs(currXVel));
        else
            currXVel += xInput * accel * Time.deltaTime;
        currXVel = Mathf.Clamp(currXVel, -maxWalkSpeed, maxWalkSpeed);
        return currXVel;
    }
    

    void CheckForFire() {
        if (DidFire) {
            OnFire?.Invoke();
            
            fireTimer = fireTime;
            rechargeTimer = rechargeTime;
            laserGun.Fire(fireTime);
            StartCoroutine(HandleRecoil(playerMovementState == PlayerMovementState.JUMPING));
            playerMovementState = PlayerMovementState.FIRING;
        }
    }

    IEnumerator HandleRecoil(bool wasJumping) {
        rb2d.gravityScale = baseGravity;

        Vector2 moveVelocity = rb2d.velocity - baseVelocity;
        Vector2 prevMoveVelocity = moveVelocity;

        Vector2 dir = -laserGun.CurrentAim.normalized;
        Vector2 recoilVelocity = dir * fireRecoil * Time.deltaTime * (IsGrounded ? 0.5f : 1);
        
        // Horizontal boost should happen if the player is moving in the same direction they fire, but very, very little if the player is grounded and still
        // bool hBoost = false; (note: this bool currently does nothing)
        if (Mathf.Round(dir.x) == Mathf.Sign(moveVelocity.x)) {
            recoilVelocity.x += moveVelocity.x;
            // hBoost = true;
        }
        else if (IsGrounded && xInput == 0) {
            recoilVelocity.x *= 0.2f;
        }
        
        // Vertical boost should happen if the player is either jumping and firing downward or falling and firing upward
        bool vBoost = false;
        if (wasJumping && Mathf.Round(dir.y) == 1) {
            recoilVelocity.y += moveVelocity.y;
            recoilVelocity.y /= 2;
            moveVelocity.y /= 2;
            fireTimer /= 2;
            vBoost = true;
        } if (Mathf.Round(dir.y) == -1 && Mathf.Sign(moveVelocity.y) == -1) {
            recoilVelocity.y += moveVelocity.y;
            vBoost = true;
        }

        while (fireTimer > 0) {
            // If the player took a hit, the recoil should end
            if (playerMovementState == PlayerMovementState.KNOCKBACK)
                yield break;

            // Update non-recoil velocity: apply gravity if the player is vertically boosting, and do horizontal movement with way lower acceleration
            moveVelocity.y = vBoost ? prevMoveVelocity.y - rb2d.gravityScale * Time.deltaTime : 0;
            moveVelocity.x = GetXVelocity(prevMoveVelocity.x);
            prevMoveVelocity = moveVelocity;

            Vector2 desiredVelocity = Vector2.Lerp(moveVelocity, recoilVelocity, Easing.CubicOut(fireTimer / fireTime));
            rb2d.velocity = desiredVelocity + baseVelocity;

            yield return 0;
            fireTimer = Mathf.Max(fireTimer - Time.deltaTime, 0);
        }

        playerMovementState = PlayerMovementState.NORMAL;
    }

    public override bool Damage(float h, HazardCollision hitColl) {
        if (!canInput)
            return false;
        if (hitColl.CompareTag("PlayerLaser"))
            return false;
        
        return base.Damage(h, hitColl);
    }

    public override void Knockback(float xDir) {
        base.Knockback(xDir);
        playerMovementState = PlayerMovementState.KNOCKBACK;
    }
    protected override void EndHit() {
        base.EndHit();
        playerMovementState = PlayerMovementState.NORMAL;
        StartCoroutine(DoIFrameFlash(3));
    }

    // Should have done this in the animator, actually
    IEnumerator DoIFrameFlash(int interval) {
        if (currentlyFlashing) yield break;
        currentlyFlashing = true;

        int numFrames = 0;
        Color c = sr.color;
        Color ca = c;
        ca.a = 0.5f;

        while (iFrameTimer > 0) {
            Color theC = Color.white;
            if (numFrames % (interval*2) < interval) 
                theC = ca;
            else
                theC = c;

            sr.color = theC;
            if (laserGun != null)
                laserGun.GunSprite.color = theC;
            
            if (Time.timeScale > 0)
                numFrames++;
            yield return 0;
        }

        sr.color = c;
        if (laserGun != null)
            laserGun.GunSprite.color = c;
        currentlyFlashing = false;
    }

    public float GetHealth() {
        return health;
    }

    protected override void Die() {
        OnPlayerDeath?.Invoke();
        base.Die();        
    }

    public void StartLevelTransition(float exitDirection) {
        canInput = false;
        xInput = exitDirection;
    }
}


enum PlayerMovementState {
    NORMAL = 0,
    JUMPING = 1,
    FIRING = 2,
    KNOCKBACK = 3
}