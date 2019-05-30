using UnityEngine;

public class LaserCannon : MonoBehaviour
{
    public bool doesCycle;
    public float cycleTime;
    float cycleTimer;

    GameObject laser;

    private void Start() {
        laser = GetComponentInChildren<Laser>().gameObject;
    }

    private void FixedUpdate() {
        if (!doesCycle) return;

        cycleTimer += Time.deltaTime;
        if (cycleTimer >= cycleTime) {
            cycleTimer -= cycleTime;
            FlipState();
        }
    }

    void FlipState() {
        laser.SetActive(!laser.activeSelf);
    }
}
