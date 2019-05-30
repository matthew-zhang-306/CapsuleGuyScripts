using UnityEngine;

public class LaserPedestal : MonoBehaviour
{
    bool hasTaken;
    public GameObject laserGunMannequin;
    public GameObject laserGetParticles;

    private void Start() {
        if (GameManager.Instance?.PlayerHasLaser ?? false) {
            Destroy(laserGunMannequin);
            hasTaken = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (hasTaken) return;

        if (other.CompareTag("Player")) {
            hasTaken = true;
            GameManager.Instance?.PlayerGotLaser();
            GameObject.Instantiate(laserGetParticles, laserGunMannequin.transform.position, Quaternion.identity);
            Destroy(laserGunMannequin);
        }
    }   
}
