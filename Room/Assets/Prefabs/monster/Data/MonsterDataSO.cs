using UnityEngine;

[CreateAssetMenu(fileName = "MonsterData_", menuName = "Monsters/Data")]
public class MonsterDataSO : ScriptableObject
{
    [Header("기본 스탯")]
    public float health = 3f;
    public float speed = 2f;
    
    [Header("전투 스탯")]
    public float attackRange = 1.5f;
    public float attackCooldown = 1.5f;
    public float attackDamage = 1f;
    
    [Header("감지 설정")]
    public float detectionRadius = 5f;
    public float stopChaseDistance = 20f;
    public float detectionInterval = 0.5f;
}
