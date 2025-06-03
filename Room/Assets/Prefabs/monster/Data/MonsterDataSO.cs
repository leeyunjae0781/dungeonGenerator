using UnityEngine;

[CreateAssetMenu(fileName = "monsterDataSO_", menuName = "Monster/MonsterData")]
public class MonsterDataSO : ScriptableObject
{

    [Header("몬스터 스텟")]
    [Tooltip("이동속도")] public float moveSpeed;
    [Tooltip("타겟 플레이어와의 거리가 attackRange만큼 줄어들면 공격 실행")] public float attackRange;
    [Tooltip("최대 체력")] public int maxHealth;

    [Header("플레이어 인지관련")]
    [Tooltip("몇 초 마다 감지를 실행 할 지 결정")] public float detectionInterval;  // 감지 주기
    [Tooltip("플레이어를 감지하는 원의 반지금을 정의")] public float detectionRange;      // 탐지 반경
    [Tooltip("플레이어와의 거리가 얼마나 벌어지면 추격을 그만 둘 지 결정")] public float stopChaseDistance;   // 추적 중단 거리

    [Header("공격 타입")]
    [Tooltip("어떤 타입으로 공격 할 지")] public AttackType attackType;

    public enum AttackType
    {
        Dash,
        Projectile,
        AoE
    }
}