using UnityEngine;

public class PatrollingEnemy : GroundEntity
{
    Collision edgecheck;
    Collision wallcheck;

    EnableByCamera enabler;

    public float walkSpeed;

    protected override void Start() {
        base.Start();

        Collision[] checks = GetComponentsInChildren<Collision>();
        edgecheck = checks[1]; // Note that checks[0] contains the groundcheck, as is expected for all grounded entities
        wallcheck = checks[2];

        enabler = GetComponent<EnableByCamera>();
    }

    protected override void FixedUpdate() {
        base.FixedUpdate();

        if (IsKnockedBack)
            return;
        if (!enabler.IsOn)
            return;

        if (wallcheck.IsColliding || (groundcheck.IsColliding && !edgecheck.IsColliding))
            Flip();

        Vector2 velocity = rb2d.velocity - baseVelocity;
        velocity.x = Mathf.Sign(transform.localScale.x) * walkSpeed;
        rb2d.velocity = velocity + baseVelocity;
    }

    void Flip() {
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }
}
