using UnityEngine;
using TMPro;

public class DeathCounter : MonoBehaviour
{
    private void OnEnable() {
        TextMeshProUGUI textmesh = GetComponent<TextMeshProUGUI>();
        textmesh.text = "Deaths: " + (GameManager.Instance?.DeathCount ?? 0);
    }
}
