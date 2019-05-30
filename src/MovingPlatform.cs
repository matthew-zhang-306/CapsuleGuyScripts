using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MovingPlatform : MonoBehaviour, IMovingPlatform
{
    public MovingPlatformPath path;
    public MovingPlatformPath Path { get { return path; }}

    public float moveTime;
    float currentTime;
    public float MoveSpeed { get { return path.TotalDistance / moveTime; }}
    
    bool goingBackwards;

    SpriteRenderer sr;
    Rigidbody2D rb2d;

    void Start() {
        sr = GetComponent<SpriteRenderer>();
        rb2d = GetComponent<Rigidbody2D>();
        path.Init(true, transform.position);

        if (moveTime == 0) {
            Debug.LogWarning("Move time for " + gameObject + " has not been set!");
            moveTime = 1;
        }

        // Initialize moving platform track indicator
        LineRenderer lr = GetComponent<LineRenderer>();
        if (lr) {
            lr.enabled = true;
            lr.useWorldSpace = true;
            lr.positionCount = path.points.Count;
            lr.SetPositions(path.GetPointsArray());
        }
    }

    void FixedUpdate() {
        goingBackwards = (currentTime / moveTime) % 2 >= 1;
        rb2d.velocity = path.GetVelocity(currentTime % moveTime, moveTime, goingBackwards, true);

        currentTime += Time.fixedDeltaTime;
    }

    protected Vector2 GetPointOnPath(float time, bool goingBackwards) {
        return path.GetPointOnPath(time, goingBackwards, true);
    }

    public Vector2 GetVelocity() {
        return rb2d.velocity;
    }

    protected void OnDrawGizmos() {
        path.DrawGizmos(gameObject);
    }

}
