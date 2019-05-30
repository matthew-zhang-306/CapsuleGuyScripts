using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDisplay : MonoBehaviour
{
    RectTransform displaySize;

    int numEnemies;
    List<int> targetEnemyGroups;

    public float sizeLerpSpeed;

    private void OnEnable() {
        ArenaTrigger.OnExistArena += ExistArena;
        ArenaTrigger.OnEnterArena += EnterArena;
        Entity.OnKilled += EnemyDied;
    }
    private void OnDisable() {
        ArenaTrigger.OnExistArena -= ExistArena;
        ArenaTrigger.OnEnterArena -= EnterArena;
        Entity.OnKilled -= EnemyDied;
    }

    private void Start() {
        if (targetEnemyGroups == null)
            targetEnemyGroups = new List<int>();
        displaySize = GetComponent<RectTransform>();
    }

    private void LateUpdate() {
        float desiredSize = 100 * Mathf.Max(0, numEnemies);
        displaySize.sizeDelta = new Vector2(Mathf.Lerp(displaySize.sizeDelta.x, desiredSize, sizeLerpSpeed), 100);
    }

    private void ExistArena(int groupNum) {
        // When the level contains an arena, make a note of the desired group number
        if (targetEnemyGroups == null)
            targetEnemyGroups = new List<int>();
        targetEnemyGroups.Add(groupNum);
    }

    private void EnterArena(int enemyCount) {
        // When entering the arena, add the count of existing enemies
        numEnemies += enemyCount;
    }

    private void EnemyDied(Entity entity) {
        // When any enemy belonging to an arena dies, decrement the counter
        if (targetEnemyGroups?.Any(t => t == entity.enemyGroup) ?? false)
            numEnemies--;
    }
    
}
