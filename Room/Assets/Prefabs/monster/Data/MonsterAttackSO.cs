using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using static MonsterDataSO;

[CreateAssetMenu(fileName = "monsterAttackSO_", menuName = "Monster/MonsterAttack")]

public class MonsterAttackSO : ScriptableObject
{
    [Header("공격 타이밍 변수")]
    [Tooltip("공격 주기")] public float attackCooldown = 4f;
    [Tooltip("공격이 실행되기 전에 잠깐 멈추는 시간")] public float preCastingTime = 0.5f;
    [Tooltip("공격이 끝나고 잠깐 멈추는 시간")] public float postCastingTime = 0.5f;


    [Header("대쉬공격 전용 변수")]
    [Tooltip("돌진 거리")] public float dashDistance = 20f;
    [Tooltip("돌진이 실행되는 시간")] public float dashDuration = 0.4f;
    [Tooltip("돌진이 범위를 나타내는 프리팹")] public GameObject dashPreviewPrefab;


    [Header("원거리 공격 전용 변수")]
    public GameObject projectilePrefab;
    // public float attackRange = 10f;
    public float projectileSpeed = 10f;


    [Header("범위 공격 전용 변수")]
    public GameObject aoePrefab;
    public GameObject aoePreviewPrefab;
    public float aoeRange = 4f;
    public float aoeDuration = 3f;

    [Header("장애물 인식 레이어")]
    public LayerMask obstacleMask;

}