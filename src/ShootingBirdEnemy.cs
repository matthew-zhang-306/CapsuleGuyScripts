using UnityEngine;

public class ShootingBirdEnemy : BirdEnemy
{
    Transform player;
    EnableByCamera enabler;

    int wallMask;
    PredictiveLaser pred;
    public GameObject laserObj;

    public float aimTime;
    float aimTimer;
    public float lockTime;
    float lockTimer;
    public float shootTime;
    float shootTimer;

    ShootingEnemyState enemyState;

    public static event BirdAction OnLock;
    public static event BirdAction OnShoot;
    
    protected override void Start() {
        base.Start();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        wallMask = LayerMask.GetMask("Wall");
        pred = GetComponentInChildren<PredictiveLaser>();

        enemyState = ShootingEnemyState.IDLE;

        enabler = GetComponent<EnableByCamera>();
    }

    protected override void Update() {
        base.Update();

        anim.SetBool("IsLocked", enemyState == ShootingEnemyState.LOCK);
        anim.SetBool("IsFiring", enemyState == ShootingEnemyState.SHOOT);
    }

    protected virtual void FixedUpdate() {
        if (!enabler.IsOn) return;
        if (player == null) return;

        if (enemyState <= ShootingEnemyState.AIM) {
            // Check if the player has moved to the other side of the bird
            float dir = player.position.x - transform.position.x;
            if (dir != 0 && (transform.localScale.x > 0 ^ dir > 0))
                Flip();
            
            // If the bird has stopped being able to see the player, return to idle
            bool canSeePlayer = IsPlayerOpen();
            enemyState = canSeePlayer ? ShootingEnemyState.AIM : ShootingEnemyState.IDLE;
            pred.SetEnabled(canSeePlayer);

            // Rotate the dotted line towards the player
            Vector2 currentAim = (player.transform.position - pred.transform.position).normalized;
            float aimAngle = Vector2.SignedAngle(Vector2.right, currentAim);
            pred.transform.rotation = Quaternion.Euler(0, 0, aimAngle);
        }

        // Do all of the main timers: aim -> lock -> shoot -> idle, in that order
        aimTimer = DoTimer(aimTimer, ShootingEnemyState.AIM);
        lockTimer = DoTimer(lockTimer, ShootingEnemyState.LOCK);
        shootTimer = DoTimer(shootTimer, ShootingEnemyState.SHOOT);
    
        if (aimTimer >= aimTime)
            Lock();
        if (lockTimer >= lockTime)
            Shoot();
        if (shootTimer >= shootTime)
            DidShoot();
    }

    float DoTimer(float timer, ShootingEnemyState desiredState) {
        return enemyState == desiredState ? timer + Time.deltaTime : 0;
    }

    bool IsPlayerOpen() {
        RaycastHit2D hit = Physics2D.Linecast(transform.position, player.position, wallMask);
        return hit.collider == null;
    }

    void Lock() {
        enemyState = ShootingEnemyState.LOCK;
        OnLock?.Invoke(this);
    }

    void Shoot() {
        OnShoot?.Invoke(this);
    
        enemyState = ShootingEnemyState.SHOOT;

        PlayerLaser laser = GameObject.Instantiate(laserObj, pred.transform.position, pred.transform.rotation, transform).GetComponent<PlayerLaser>();
        laser.Init(shootTime);

        pred.SetEnabled(false);
    }

    void DidShoot() {
        enemyState = ShootingEnemyState.IDLE;
    }

    protected override void Flip() {
        base.Flip();
        Vector3 scale = pred.transform.localScale;
        scale.x *= -1;
        pred.transform.localScale = scale;
    }
}


enum ShootingEnemyState {
    IDLE = 0,
    AIM = 1,
    LOCK = 2,
    SHOOT = 3
}
