using UnityEngine;
using UnityEngine.EventSystems;

// Meant to make there only be one event system at a time
public class EventSystemSingleton : MonoBehaviour
{
    protected static EventSystem instance;
    public static EventSystem Instance { get { return instance; }}
    protected void Awake() {
        if (instance == null) {
            instance = GetComponent<EventSystem>();
            if (instance != null)
                DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }
    }
}
