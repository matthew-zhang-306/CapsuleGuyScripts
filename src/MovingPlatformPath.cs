using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MovingPlatformPath
{
    public List<Vector2> points;
    List<float> distances;
    public float TotalDistance { get { return distances[distances.Count - 1]; }}

    public EasingFunctions easingForward;
    public EasingFunctions easingBackward;

    Vector2 basePosition;
    public Vector2 BasePosition { get { return basePosition; }}
    bool inited;

    public void Init(bool finalInit, Vector2 basePos) { // finalInit is expected to be true when in the game
        if (inited)
            return;
        if (finalInit)
            inited = true;

        basePosition = basePos;

        if (points == null)
            points = new List<Vector2>();
        if (points.Count == 0 || points[0] != Vector2.zero)
            points.Insert(0, Vector2.zero);

        float dist = 0;
        distances = new List<float>();
        distances.Add(0);
        for (int p = 1; p < points.Count; p++) {
            dist += Vector2.Distance(points[p], points[p-1]);
            distances.Add(dist);
        }
    }

    public Vector2 GetPointOnPath(float time, bool goingBackwards, bool doesReverse) {
        if (doesReverse && time > 1) {
            goingBackwards = !goingBackwards;
            time--;
        }
        time = Mathf.Clamp(time, 0, 1);

        float easedTime;
        if (goingBackwards)
            easedTime = 1 - Easing.Interpolate(time, easingBackward);
        else
            easedTime = Easing.Interpolate(time, easingForward);

        float distance = easedTime * TotalDistance;
        int segmentIndex = distances.FindLastIndex(d => d <= distance);
        segmentIndex = Mathf.Clamp(segmentIndex, 0, distances.Count - 2);

        float fractionOfSegment = (distance - distances[segmentIndex]) / (distances[segmentIndex + 1] - distances[segmentIndex]);
        Vector2 pointOnPath = Vector2.Lerp(points[segmentIndex], points[segmentIndex + 1], fractionOfSegment);

        return pointOnPath + basePosition;
    }
    
    public Vector2 GetVelocity(float time, float totalTime, bool goingBackwards, bool doesReverse) {
        Vector2 deltaX = GetPointOnPath((time + Time.fixedDeltaTime) / totalTime, goingBackwards, doesReverse) - GetPointOnPath((time) / totalTime, goingBackwards, doesReverse);

        return deltaX / Time.fixedDeltaTime;
    }

    // To be used by a LineRenderer
    public Vector3[] GetPointsArray() {
        return points.Select(v => (v + BasePosition).ToVector3()).ToArray();
    }

    public void DrawGizmos(GameObject platform) {
        SpriteRenderer sr = platform.GetComponent<SpriteRenderer>();
        if (sr == null)
            return;

        Init(false, platform.transform.position);
        Vector2 platformSize = sr.sprite.bounds.size * platform.transform.localScale.ToVector2();

        Gizmos.color = Color.yellow;
        for (int p = 0; p < points.Count; p++) {
            Gizmos.DrawWireCube(points[p] + basePosition, platformSize);
            Gizmos.DrawLine(p == 0 ? basePosition : points[p-1] + basePosition, points[p] + basePosition);
        }
    } 
}
