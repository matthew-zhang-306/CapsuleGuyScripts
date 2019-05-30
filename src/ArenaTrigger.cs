using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class ArenaTrigger : Switch
{
    public List<Entity> entityList;
    
    // Group number should be unique to each arena trigger in a room
    public int groupNumber = 1;

    bool isInArena;
    bool isCompleted;

    public delegate void ArenaTriggerAction(int num);
    public static event ArenaTriggerAction OnExistArena;
    public static event ArenaTriggerAction OnEnterArena;
    public delegate void ArenaAction();
    public static event ArenaAction OnClear;

    private void Start() {
        OnExistArena?.Invoke(groupNumber);
        foreach (Entity entity in entityList)
            entity.enemyGroup = groupNumber;
    }

    private void Update() {
        if (isCompleted) return;

        // Prune entities for deleted (aka killed) ones, then check if none remain
        entityList = entityList.Where(e => e != null && e.gameObject.activeInHierarchy).ToList();
        if (isInArena && entityList.Count == 0) {
            OnClear?.Invoke();
            isCompleted = true;
            SwitchTriggered();
        }
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (isInArena) return;

        if (other.CompareTag("Player")) {
            isInArena = true;
            OnEnterArena?.Invoke(entityList.Count);
        }
    }

    protected override void OnDrawGizmosSelected() {
        base.OnDrawGizmosSelected();

        Gizmos.color = new Color(1, 0.5f, 0);
        foreach (Entity entity in entityList)
            if (entity != null)
                Gizmos.DrawLine(transform.position, entity.transform.position);
    }
}
