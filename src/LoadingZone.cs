using UnityEngine;

public class LoadingZone : MonoBehaviour
{
    public ExitType exitType;

    // This value should be either -1, 0, or 1
    [Range(-1, 1)]
    public float exitDirection;

    [SerializeField]
    private Vector2 spawnPoint;
    public Vector2 SpawnPoint { get { return transform.position + spawnPoint.ToVector3(); }}

    Collider2D coll;

    private void Start() {
        coll = GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (!other.CompareTag("Player")) return;
        if (GameManager.Instance == null) return;

        GameManager.Instance.StartSceneTransition(this);
    }

    public string GetTarget(Room r) {
        switch (exitType) {
            case ExitType.ENTRANCE:
                return r.previousRoom;
            case ExitType.EXIT:
                return r.nextRoom;
            case ExitType.SECRET:
                return r.secretRoom;
        }
        return r.nextRoom;
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(SpawnPoint, new Vector2(0.66f, 1.33f));
    }
}

public enum ExitType {
    ENTRANCE,
    EXIT,
    SECRET
}