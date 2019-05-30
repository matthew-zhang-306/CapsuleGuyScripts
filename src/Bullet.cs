using UnityEngine;

public class Bullet : MonoBehaviour
{
    BulletCollision coll;
    Rigidbody2D rb2d;

    public float lifeTime;
    float lifeTimer;
    float speed;

    // If the bullet comes out of a cannon, this will be set on initialization
    Cannon source;

    private void Start() {
        coll = GetComponent<BulletCollision>();
        rb2d = GetComponent<Rigidbody2D>();

        lifeTimer = lifeTime;
    }

    public void Init(float speed) {
        Start();
        this.speed = speed;
    }
    public void Init(float speed, Cannon source) {
        Init(speed);
        this.source = source;
    }

    private void FixedUpdate() {
        rb2d.velocity = speed * Time.deltaTime * transform.right;
        
        // Check for lifetime end
        lifeTimer -= Time.deltaTime;
        if (lifeTimer <= 0)
            coll.EndBullet();

        // Check for collision with valid ground
        Collider2D g = Physics2D.OverlapPoint(transform.position, LayerMask.GetMask("Wall"));
        if (g != null && (source == null || g.GetComponentInParent<Cannon>() != source)) {
            coll.EndBullet();
        }
    }
}
