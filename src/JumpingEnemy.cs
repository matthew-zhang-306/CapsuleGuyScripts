using System;
using System.Collections;
using UnityEngine;

public class JumpingEnemy : GroundEntity
{
    JumpingEnemyState enemyState;
    Transform playerT;

    Collision wallCheck;
    Animator anim;
    EnableByCamera enabler;

    public float approachDistance;
    public float jumpAgroDistance;
    public float jumpPower;
    public float jumpChargeTime;
    public float walkSpeed;
    public float jumpSpeed;
    public float horizAccel;

    float currentMaxWalkSpeed;
    float currentDesiredDirection;
    float xInput;

    protected override void Start() {
        base.Start();
        enemyState = JumpingEnemyState.IDLE;
        playerT = GameObject.Find("Player").transform;
        
        wallCheck = GetComponentsInChildren<Collision>()[1];
        anim = GetComponent<Animator>();
        enabler = GetComponent<EnableByCamera>();
    }

    protected override void Update() {
        base.Update();

        anim.SetBool("IsApproaching", enemyState == JumpingEnemyState.APPROACH);
        anim.SetBool("IsCharging", enemyState == JumpingEnemyState.CHARGE);
        anim.SetBool("IsJumping", enemyState == JumpingEnemyState.JUMP);
        anim.SetFloat("HorizontalSpeed", Mathf.Abs(RealVelocity.x));
    }

    protected override void FixedUpdate() {
        base.FixedUpdate();

        if (!IsKnockedBack) {
            DoMotion();
            if (xInput != 0 && (transform.localScale.x > 0 ^ xInput > 0)) {
                transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
            }
        }
    }

    void DoMotion() {
        Vector2 velocity = rb2d.velocity - baseVelocity;

        if (enemyState == JumpingEnemyState.JUMP) {
            // Recharge if hit the ground
            if (IsGrounded)
                enemyState = JumpingEnemyState.RECHARGE;
        }
        if (enemyState == JumpingEnemyState.APPROACH) {
            // If the enemy has reached max speed or is against a ledge, recharge
            if (Mathf.Abs(velocity.x) >= currentMaxWalkSpeed - 0.1f || wallCheck.IsColliding)
                enemyState = JumpingEnemyState.RECHARGE;
        }
        if (enemyState == JumpingEnemyState.RECHARGE) {
            // Slide to a halt when recharging
            xInput = 0;
            
            // If halted, return to the idle state
            if (Mathf.Abs(velocity.x) < 0.5f)
                enemyState = JumpingEnemyState.IDLE;
        }
        if (enemyState == JumpingEnemyState.IDLE) {
            xInput = 0;
            if (playerT) {
                if (Vector2.Distance(transform.position, playerT.position) <= jumpAgroDistance) {
                    // Jump at the player if they are close enough
                    enemyState = JumpingEnemyState.CHARGE;
                    StartCoroutine(Charge());
                }
                else if (!enabler.IsInside()) {
                    // Stay idle if the player is too far away
                    enemyState = JumpingEnemyState.IDLE;
                }
                else if (wallCheck.IsColliding) {
                    // Do a jump if there is a ledge in front of the enemy
                    enemyState = JumpingEnemyState.CHARGE;
                    StartCoroutine(Charge());
                } else {
                    // Otherwise, walk slowly towards the player
                    enemyState = JumpingEnemyState.APPROACH;
                    currentMaxWalkSpeed = walkSpeed;
                    xInput = Mathf.Sign(playerT.position.x - transform.position.x);
                }
            }
        }

        // Do actual motion
        if (xInput == 0)
            velocity.x -= Math.Sign(velocity.x) * Math.Min(horizAccel * Time.deltaTime, Math.Abs(velocity.x));
        else
            velocity.x += xInput * horizAccel * Time.deltaTime;
        velocity.x = Mathf.Clamp(velocity.x, -currentMaxWalkSpeed, currentMaxWalkSpeed);

        rb2d.velocity = velocity + baseVelocity;
    }

    IEnumerator Charge() {
        yield return new WaitForSeconds(jumpChargeTime);
        if (playerT == null) yield break;
        while (!IsGrounded)
            yield return 0;
        
        enemyState = JumpingEnemyState.JUMP;
        currentMaxWalkSpeed = jumpSpeed;
        rb2d.velocity = new Vector2(rb2d.velocity.x, jumpPower * Time.fixedDeltaTime);
        xInput = Mathf.Sign(playerT.position.x - transform.position.x);
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, jumpAgroDistance);    
    }
}

public enum JumpingEnemyState {
    IDLE,
    APPROACH,
    CHARGE,
    JUMP,
    RECHARGE,
}
