using UnityEngine;

[CreateAssetMenu(fileName = "monsterDataSO_", menuName = "Monster/MonsterData")]
public class MonsterDataSO : ScriptableObject
{

    [Header("���� ����")]
    [Tooltip("�̵��ӵ�")] public float moveSpeed;
    [Tooltip("Ÿ�� �÷��̾���� �Ÿ��� attackRange��ŭ �پ��� ���� ����")] public float attackRange;
    [Tooltip("�ִ� ü��")] public int maxHealth;

    [Header("�÷��̾� ��������")]
    [Tooltip("�� �� ���� ������ ���� �� �� ����")] public float detectionInterval;  // ���� �ֱ�
    [Tooltip("�÷��̾ �����ϴ� ���� �������� ����")] public float detectionRange;      // Ž�� �ݰ�
    [Tooltip("�÷��̾���� �Ÿ��� �󸶳� �������� �߰��� �׸� �� �� ����")] public float stopChaseDistance;   // ���� �ߴ� �Ÿ�

    [Header("���� Ÿ��")]
    [Tooltip("� Ÿ������ ���� �� ��")] public AttackType attackType;

    public enum AttackType
    {
        Dash,
        Projectile,
        AoE
    }
}