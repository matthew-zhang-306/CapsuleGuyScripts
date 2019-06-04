using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuStar : MonoBehaviour
{
    private void OnEnable() {
        GetComponent<Image>().enabled = PlayerPrefs.HasKey("beatGame") && PlayerPrefs.GetInt("beatGame") > 0;
    }
}
