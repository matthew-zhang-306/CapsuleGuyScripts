using UnityEngine;

public class Hyperlink : MonoBehaviour
{
    // Note that this script will need to be more complex when doing an HTML build
    public void OpenURL(string link) {
        Application.OpenURL(link);
    }
}
