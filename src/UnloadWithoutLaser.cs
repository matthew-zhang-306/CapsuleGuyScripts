using UnityEngine;

public class UnloadWithoutLaser : MonoBehaviour
{
    public bool loadInstead;

    void Start() {
        if (!GameManager.Instance?.PlayerHasLaser ?? false)
            gameObject.SetActive(loadInstead);
        else
            gameObject.SetActive(!loadInstead);
    }
}
