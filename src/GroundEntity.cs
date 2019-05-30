using UnityEngine;

public class GroundEntity : Entity
{
    protected Collision groundcheck;
    protected Collider2D Ground { get { return groundcheck.GetCollider(c => c.bounds.max.y <= coll.bounds.min.y + 0.2f); }}
    protected bool IsGrounded { get {
        return groundcheck.IsColliding && Ground != null && RealVelocity.y <= 0.2f;
    }}
    protected bool oldIsGrounded;

    public float verticalJerk;
    public float maxFallSpeed;
    protected float baseGravity;

    protected Vector2 baseVelocity;
    public Vector2 RealVelocity { get { return rb2d.velocity - baseVelocity; }}
    protected Vector2 previousBaseVelocity;

    public float knockbackPower;
    public float knockbackTime { get { return hitTime; }}
    protected float knockbackTimer;
    protected float knockbackDir;
    
    protected bool IsKnockedBack { get {
        return knockbackTimer > 0;
    }}

    public static event EntityAction OnLand;
    float ignoreFirstLandingDelay;

    static GameObject dustParticles;

    private void Awake() {
        if (dustParticles == null)
            dustParticles = Resources.Load<GameObject>("Dust Particles");
    }

    protected override void Start() {
        base.Start();

        // All grounded entities are assumed to be grounded at first until proven not so
        oldIsGrounded = true;
        ignoreFirstLandingDelay = 0.1f;

        groundcheck = GetComponentInChildren<Collision>();
        baseGravity = rb2d.gravityScale;
    }

    protected virtual void FixedUpdate() {
        previousBaseVelocity = baseVelocity;
        baseVelocity = GetBaseVelocity();

        // If the new base velocity's y component has drastically decreased, don't bother staying on the platform
        // This is dangerous but is still in the game and is probably the cause of at least two bugs
        if (IsGrounded && previousBaseVelocity.y - baseVelocity.y > 100 * Time.deltaTime) {
            baseVelocity = previousBaseVelocity;
        }

        // Check for landing
        if (ignoreFirstLandingDelay == 0 && IsGrounded && !oldIsGrounded) {
            OnLanded();
        }
        ignoreFirstLandingDelay = Mathf.Max(0, ignoreFirstLandingDelay - Time.deltaTime);
        oldIsGrounded = IsGrounded;

        // Cap y velocity
        Vector2 velocity = rb2d.velocity - previousBaseVelocity;
        velocity.y = Mathf.Max(velocity.y, -maxFallSpeed * Time.deltaTime);
        rb2d.velocity = velocity + baseVelocity;

        ApplyVerticalJerk();

        // Knockback
        knockbackTimer = Mathf.Max(knockbackTimer - Time.deltaTime, 0);
        if (knockbackTimer > 0) {
            float fullVel = knockbackPower * Time.deltaTime;
            float desiredVel = Mathf.LerpUnclamped(fullVel / 2, fullVel, knockbackTimer / knockbackTime);
            rb2d.velocity = new Vector2(knockbackDir * desiredVel, rb2d.velocity.y);
        }
    }

    protected Vector2 GetBaseVelocity() {
        Vector2 output = Vector2.zero;
        if (!(groundcheck.IsColliding && Ground != null))
            return output;
        
        IMovingPlatform platform = Ground?.GetComponent<IMovingPlatform>();
        if (platform == null)
            return output;

        return platform.GetVelocity();
    }

    protected void ApplyVerticalJerk() {
        if (!IsGrounded && -rb2d.velocity.y < maxFallSpeed)
            rb2d.gravityScale += verticalJerk * Time.deltaTime;
        else
            rb2d.gravityScale = baseGravity;
    }

    protected void OnLanded() {
        if (dustParticles != null)
            GameObject.Instantiate(dustParticles, new Vector2(transform.position.x, sr.bounds.min.y), Quaternion.identity);
        OnLand?.Invoke(this);
    }

    public override bool Damage(float h, HazardCollision hitColl) {
        if (!base.Damage(h, hitColl))
            return false;
        
        if (health > 0)
            Knockback(Mathf.Sign(hitColl.GetVelocity().x - rb2d.velocity.x));
        
        return true;
    }

    public virtual void Knockback(float xDir) {
        knockbackTimer = knockbackTime;
        rb2d.gravityScale = baseGravity;
        rb2d.velocity = new Vector2(0, knockbackPower * Time.deltaTime);
        knockbackDir = xDir;
    }
}
