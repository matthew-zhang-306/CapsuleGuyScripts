using UnityEngine;

public class BirdEnemy : Entity
{
    protected Animator anim;

    public delegate void BirdAction(BirdEnemy bird);
    public static event BirdAction OnStartFlap;

    protected override void Start() {
        base.Start();

        anim = GetComponent<Animator>();
    }

    protected void StartFlap() {
        OnStartFlap?.Invoke(this);
    }

    protected override void Update() {
        base.Update();
        
        float dir = rb2d.velocity.x;
        if (dir != 0 && (transform.localScale.x > 0 ^ dir > 0))
            Flip();
    }

    protected virtual void Flip() {
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    private void OnDrawGizmos() {
        MovingPlatform move = GetComponent<MovingPlatform>();
        if (move)
            move.path.DrawGizmos(GetComponentInChildren<SpriteRenderer>().gameObject);
    }
}
