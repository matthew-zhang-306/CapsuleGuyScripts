using UnityEngine;

// It says "Sawblade" but really this is for any automatically rotating object, useful for spinning groups of objects that look the same in all rotation angles
public class Sawblade : MonoBehaviour
{
    public float spinSpeed;

    void Update() {
        transform.rotation *= Quaternion.Euler(0, 0, 360 * spinSpeed * Time.deltaTime);
    }
}
