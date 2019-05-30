// KNOWN ISSUE: On a PlayerLaser, the player's motion causes the laser to have a slight gap in proportion to the spot its trying to hit
// (Uncomment debug logs to see this bug at work - it's annoying and I have found no fix yet)

using System.Collections.Generic;
using UnityEngine;

public class Laser : MonoBehaviour
{
    public float maxDistance;
    public int maxBounces;
    protected float scaleMultiplier;
    protected int wallMask;
    
    protected bool hasInited;

    public GameObject laserSegment;
    float particleDensity = 50;

    protected virtual void FindUnitLengthOfLaser() {
        // Temporarily set the z rotation to 0 in order to properly measure a laser object
        Quaternion rot = transform.rotation;
        transform.rotation = Quaternion.identity;

        // Instantiate a prototype laser object to measure
        GameObject laserProto = Instantiate(laserSegment, transform.position, Quaternion.identity, transform);

        // The length of one unit of laser is the same as the current world scale of the object because sprite renderers automatically account for sprite size
        scaleMultiplier = laserProto.transform.lossyScale.x;

        // Rotate the object back into place
        transform.rotation = rot;

        // DEBUGGING
        /*Debug.Log(transform.position);
        Debug.Log(scaleMultiplier);*/

        // Destroy the prototype laser 
        Destroy(laserProto);
    }

    public virtual void Init() {
        hasInited = true;
        wallMask = LayerMask.GetMask("Wall");

        FindUnitLengthOfLaser();
    }

    protected virtual void OnEnable() {
        Init();
        ExtendLaser();
    }

    protected virtual void Start() {
        Init();
    }

    protected virtual void FixedUpdate() {
        ExtendLaser();
    }

    protected void ExtendLaser() {
        Vector3 pos = transform.position;
        Vector3 dir = transform.right;
        float distanceLeft = maxDistance;
        int bouncesLeft = maxBounces;

        List<Vector3> points = new List<Vector3>();
        points.Add(pos);
        
        while (distanceLeft > 0 && bouncesLeft > 0) {
            // note: "pos + dir * 0.05f" offset exists so that the raycast does not detect the adjacent mirror
            RaycastHit2D hit = Physics2D.Raycast(pos + dir * 0.05f, dir, distanceLeft, wallMask);

            if (hit.collider == null) {
                points.Add(pos + dir * distanceLeft);
                break;
            }
            else {
                points.Add(hit.point);
                // If the collision was with a normal wall, we are done
                if (hit.collider.gameObject.GetComponent<Mirror>() == null)
                    break;
            }

            distanceLeft -= Vector3.Distance(pos, hit.point);
            bouncesLeft--;
            pos = hit.point;
            dir = Vector2.Reflect(dir, hit.normal);
        }

        CreateLaser(points);
    }

    protected void CreateLaser(List<Vector3> points) {
        for (int p = 0; p < points.Count - 1; p++) {
            GameObject segment;
            if (p < transform.childCount)
                segment = transform.GetChild(p).gameObject;
            else
                segment = Instantiate(laserSegment, transform.position, Quaternion.identity, transform);
            
            segment.transform.position = points[p];
            segment.transform.rotation = Quaternion.Euler(0, 0, Vector2.SignedAngle(Vector2.right, points[p+1] - points[p]));

            SpriteRenderer sr = segment.GetComponent<SpriteRenderer>();
            
            Vector2 currentSize = sr.size;
            float worldSize = Vector2.Distance(points[p], points[p+1]);
            float internalSize = worldSize / scaleMultiplier;
            currentSize.x = internalSize;
            sr.size = currentSize;

            // DEBUGGING
            /*if (this.GetType() == typeof(PlayerLaser)) {
                Debug.DrawLine(points[p], points[p+1]);
                Debug.Log(transform.position.y + " " + sr.bounds.min.y);
            }*/

            // Create particles, more complex than I had hoped
            ParticleSystem[] allParticles = segment.GetComponentsInChildren<ParticleSystem>();
            foreach (ParticleSystem particles in allParticles) {
                particles.transform.localPosition = new Vector3(sr.size.x / 2, 0, 0);
                ParticleSystem.EmissionModule emissionSettings = particles.emission;
                ParticleSystem.ShapeModule shapeSettings = particles.shape;
                
                shapeSettings.scale = new Vector3(worldSize / 2, 1, 1);
                emissionSettings.rateOverTime = particleDensity * shapeSettings.scale.x;
            }
        }

        // Destroy any extraneous laser segments
        for (int t = points.Count - 1; t < transform.childCount; t++) {
            Destroy(transform.GetChild(t).gameObject);
        }
    }
}
