using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using static MonsterDataSO;

[CreateAssetMenu(fileName = "monsterAttackSO_", menuName = "Monster/MonsterAttack")]

public class MonsterAttackSO : ScriptableObject
{
    [Header("���� Ÿ�̹� ����")]
    [Tooltip("���� �ֱ�")] public float attackCooldown = 4f;
    [Tooltip("������ ����Ǳ� ���� ��� ���ߴ� �ð�")] public float preCastingTime = 0.5f;
    [Tooltip("������ ������ ��� ���ߴ� �ð�")] public float postCastingTime = 0.5f;


    [Header("�뽬���� ���� ����")]
    [Tooltip("���� �Ÿ�")] public float dashDistance = 20f;
    [Tooltip("������ ����Ǵ� �ð�")] public float dashDuration = 0.4f;
    [Tooltip("������ ������ ��Ÿ���� ������")] public GameObject dashPreviewPrefab;


    [Header("���Ÿ� ���� ���� ����")]
    public GameObject projectilePrefab;
    // public float attackRange = 10f;
    public float projectileSpeed = 10f;


    [Header("���� ���� ���� ����")]
    public GameObject aoePrefab;
    public GameObject aoePreviewPrefab;
    public float aoeRange = 4f;
    public float aoeDuration = 3f;

    [Header("��ֹ� �ν� ���̾�")]
    public LayerMask obstacleMask;

}