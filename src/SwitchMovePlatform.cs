using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class SwitchMovePlatform : Controllable, IMovingPlatform
{
    // I realized that moving platform paths can't work for this object, so the logic has been reduced to only one target position
    public Vector2 travelPosition;
    public EasingFunctions easing;

    Vector2 initialPosition;
    Vector2 finalPosition;
    Vector2 targetPosition;
    Vector2 startingPosition;

    public float moveTime;
    float currentTime;

    SpriteRenderer sr;
    Rigidbody2D rb2d;

    bool started;
    public bool oneWay;

    private void Start() {
        started = true;
        initialPosition = transform.position.ToVector2();
        targetPosition = initialPosition;
        finalPosition = initialPosition + travelPosition;
        
        sr = GetComponent<SpriteRenderer>();
        rb2d = GetComponent<Rigidbody2D>();

        if (moveTime == 0) {
            Debug.LogWarning("Move time for " + gameObject + " has not been set!");
            moveTime = 1;
        }

        // Initialize moving platform track indicator
        LineRenderer lr = GetComponent<LineRenderer>();
        if (lr) {
            lr.enabled = true;
            lr.useWorldSpace = true;
            lr.startColor = sr.color;
            lr.endColor = sr.color;
            lr.positionCount = 2;
            lr.SetPositions(new Vector3[]{initialPosition.ToVector3(), finalPosition.ToVector3()});
        }
    }

    private void FixedUpdate() {
        currentTime = Mathf.Max(currentTime - Time.fixedDeltaTime, 0);
        Vector2 currentPosition = transform.position.ToVector2();
        Vector2 nextPosition = Vector2.Lerp(startingPosition, targetPosition, Easing.Interpolate((moveTime - currentTime) / moveTime, easing));
        rb2d.velocity = (nextPosition - currentPosition) / Time.fixedDeltaTime;
    }

    public override void Switch() {
        if (activated && oneWay) return;

        activated = !activated;
        
        currentTime = moveTime;
        startingPosition = transform.position.ToVector2();
        targetPosition = activated ? finalPosition : initialPosition;
    }

    public Vector2 GetVelocity() {
        return rb2d.velocity;
    }

    private void OnDrawGizmos() {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        Vector2 platformSize = sr ? sr.sprite.bounds.size * transform.localScale.ToVector2() : Vector2.one;

        Gizmos.color = Color.yellow;
        Vector2 posA = started ? initialPosition : transform.position.ToVector2();
        Vector2 posB = started ? finalPosition : transform.position.ToVector2() + travelPosition;
        
        Gizmos.DrawLine(posA, posB);
        Gizmos.DrawWireCube(posA, platformSize);
        Gizmos.DrawWireCube(posB, platformSize);
    }
}
