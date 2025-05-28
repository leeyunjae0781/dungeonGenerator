using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Monster : MonoBehaviour
{
    private Rigidbody2D rigid;
    private Collider2D targetPlayer;
    public MonsterDataSO monsterData;  // 몬스터 데이터 SO 참조
    private float monsterHealth;        // 몬스터 체력
    private SpriteRenderer spriteRenderer;  // 스프라이트 렌더러 참조

    public LayerMask playerLayer;

    private NavMeshAgent agent;
    private float lastAttackTime;
    private bool canAttack = true;
    private bool isStunned = false;         // 피격 상태 체크
    private Collider2D monsterCollider;     // 몬스터의 콜라이더
    private Coroutine stunCoroutine;  // 피격 코루틴 참조 저장용

    public enum EnemyState
    {
        Idle,
        Walk,
        Attack,
        Hit,    // 피격 상태 추가
        Die
    }
    EnemyState enemyState = EnemyState.Idle;

    Animator anim;

    // Start is called before the first frame update
    void Start()
    {
        rigid = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        monsterCollider = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();  // 스프라이트 렌더러 가져오기
        monsterHealth = monsterData.health;  // 초기 체력 설정
        InvokeRepeating(nameof(CheckForPlayer), 0f, monsterData.detectionInterval);
        lastAttackTime = -monsterData.attackCooldown;
    }

    void CheckForPlayer()
    {
        if (targetPlayer == null)
        {
            targetPlayer = DetectClosestPlayer();
        }
        else
        {
            float distance = Vector2.Distance(transform.position, targetPlayer.transform.position);
            if (distance > monsterData.stopChaseDistance)
            {
                targetPlayer = null;
            }
        }
    }

    public void Setup(Transform target)
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        agent.speed = monsterData.speed;  // SO에서 속도 설정
    }

    // Update is called once per frame
    void Update()
    {
        if (enemyState == EnemyState.Die) return;

        if (enemyState == EnemyState.Hit)
        {
            // 피격 상태에서는 공격 쿨다운만 업데이트하고 이동은 완전히 중지
            agent.isStopped = true;
            agent.velocity = Vector3.zero;  // 속도를 0으로 설정
            UpdateAttackCooldown();
            return;
        }

        UpdateAttackCooldown();
        UpdateState();
        UpdateSpriteDirection();  // 스프라이트 방향 업데이트
    }

    void UpdateAttackCooldown()
    {
        if (Time.time >= lastAttackTime + monsterData.attackCooldown)
        {
            canAttack = true;
        }
    }

    void UpdateState()
    {
        if (targetPlayer == null)
        {
            SetIdleState();
            return;
        }

        float distanceToTarget = Vector2.Distance(transform.position, targetPlayer.transform.position);
        
        if (distanceToTarget <= monsterData.attackRange && canAttack)
        {
            Attack();
        }
        else if (enemyState != EnemyState.Attack)  // 공격 상태가 아닐 때만 추적
        {
            SetChaseState();
        }
    }

    void SetIdleState()
    {
        agent.isStopped = true;
        enemyState = EnemyState.Idle;
        anim.SetBool("IsWalking", false);
    }

    void SetChaseState()
    {
        agent.isStopped = false;
        agent.SetDestination(targetPlayer.transform.position);
        enemyState = EnemyState.Walk;
        anim.SetBool("IsWalking", true);
    }

    void Attack()
    {
        if (!canAttack) return;

        enemyState = EnemyState.Attack;
        agent.isStopped = true;
        agent.velocity = Vector3.zero;  // 속도를 0으로 설정
        anim.SetTrigger("Attack");
        lastAttackTime = Time.time;
        canAttack = false;
        
        // 임시 공격 효과
        Debug.Log("공격!");
        // TODO: 여기에 나중에 플레이어 체력 감소 로직 추가

        StartCoroutine(EndAttack());
    }

    IEnumerator EndAttack()
    {
        yield return new WaitForSeconds(0.9f); // 공격 애니메이션 시간에 맞춰 조정
        if (enemyState != EnemyState.Die)
        {
            SetChaseState();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.gameObject.CompareTag("Weapon"))
        {
            monsterHealth--;
            if(monsterHealth > 0)
            {
                // 이전 피격 코루틴이 있다면 중지
                if (stunCoroutine != null)
                {
                    StopCoroutine(stunCoroutine);
                }
                
                enemyState = EnemyState.Hit;
                agent.isStopped = true;
                agent.velocity = Vector3.zero;  // 속도를 0으로 설정
                anim.SetTrigger("Hit");
                stunCoroutine = StartCoroutine(RecoverFromStun());
            }
            else 
            {
                enemyState = EnemyState.Die;
                agent.isStopped = true;
                agent.velocity = Vector3.zero;  // 속도를 0으로 설정
                monsterCollider.enabled = false;
                rigid.simulated = false;
                anim.SetTrigger("Die");
            }
        }
    }

    IEnumerator RecoverFromStun()
    {
        yield return new WaitForSeconds(1.5f);
        if (enemyState != EnemyState.Die)
        {
            // 피격 상태에서 복구할 때 적절한 상태로 전환
            if (targetPlayer != null)
            {
                float distanceToTarget = Vector2.Distance(transform.position, targetPlayer.transform.position);
                if (distanceToTarget <= monsterData.attackRange && canAttack)
                {
                    Attack();
                }
                else
                {
                    SetChaseState();
                }
            }
            else
            {
                SetIdleState();
            }
        }
        stunCoroutine = null;  // 코루틴 참조 초기화
    }

    Collider2D DetectClosestPlayer()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, monsterData.detectionRadius, playerLayer);

        float closestDistance = Mathf.Infinity;
        Collider2D closestPlayer = null;

        foreach (Collider2D col in colliders)
        {
            float distance = Vector2.Distance(transform.position, col.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestPlayer = col;
            }
        }

        return closestPlayer;
    }

    void UpdateSpriteDirection()
    {
        if (targetPlayer != null && enemyState != EnemyState.Hit)
        {
            // 타겟과의 x축 방향 차이를 확인
            float directionX = targetPlayer.transform.position.x - transform.position.x;
            
            // 방향에 따라 스프라이트 뒤집기
            if (directionX > 0)
            {
                spriteRenderer.flipX = false;  // 오른쪽 보기
            }
            else if (directionX < 0)
            {
                spriteRenderer.flipX = true;   // 왼쪽 보기
            }
        }
    }
}
