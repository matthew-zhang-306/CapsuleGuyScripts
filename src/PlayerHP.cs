using UnityEngine;
using UnityEngine.UI;

public class PlayerHP : MonoBehaviour
{
    PlayerController player;
    RectTransform healthSize;

    private void Start() {
        healthSize = GetComponent<RectTransform>();
        player = GameManager.Instance?.Player ?? GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
    }

    private void LateUpdate() {
        healthSize.sizeDelta = new Vector2(100 * player.GetHealth(), 100);
    }
}
