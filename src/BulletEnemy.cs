using UnityEngine;

public class BulletEnemy : GroundEntity
{
    public Transform gunAxis;
    public Transform tipOfGun;
    
    public GameObject bulletObj;
    public float fireTime;
    public float bulletSpeed;

    Vector2 currentAim;
    float fireTimer;

    Transform player;
    EnableByCamera enabler;
    Animator anim;

    protected override void Start() {
        base.Start();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        enabler = GetComponent<EnableByCamera>();
        anim = GetComponent<Animator>();
    }

    protected override void Update() {
        base.Update();
        sr.flipX = currentAim.x < 0;

        anim.SetBool("IsShooting", fireTimer > 0.5f);
    }

    protected override void FixedUpdate() {
        base.FixedUpdate();

        if (knockbackTimer == 0)
            rb2d.velocity = new Vector2(baseVelocity.x, rb2d.velocity.y);
        
        if (!enabler.IsOn) return;
        if (player == null) return;

        currentAim = (player.position - gunAxis.position).normalized;
        float aimAngle = Vector2.SignedAngle(Vector2.right, currentAim);
        gunAxis.rotation = Quaternion.Euler(0, 0, aimAngle);

        if (fireTimer == 0) {
            fireTimer = fireTime;
            Fire();
        } else {
            fireTimer = Mathf.Max(0, fireTimer - Time.deltaTime);
        }
    }

    void Fire() {
        Bullet b = GameObject.Instantiate(bulletObj, tipOfGun.position, gunAxis.rotation).GetComponent<Bullet>();
        b.Init(bulletSpeed);
    }
}
