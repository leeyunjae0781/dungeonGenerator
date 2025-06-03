using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Monster : MonoBehaviour
{
    #region variable
    [Tooltip("공격할 때 필요한 데이터")] public MonsterDataSO monsterData;
    [Header("공격용 스크립트")]
    [Tooltip("사실 그냥 여기에다 해도 되는데 이 스크립트가 지저분해 질까봐 스크립트를 분리하고 싶었음")] public MonsterAttackSO monsterAttackData;

    private Collider2D targetPlayer;
    private float monsterHealth;        // 몬스터 체력
    private SpriteRenderer spriteRenderer;  // 스프라이트 렌더러 참조

    [Header("감지할 레이어")]
    [Tooltip("감지 범위에 들어왔을때 공격, 추격을 시도할 레이어")] public LayerMask playerLayer;
    private NavMeshAgent agent;
    private float attackTimer = 0;

    [Header("!옵션!")]
    [Tooltip("이거 키면 타겟 추적중에도 주기적으로 더 가까운 플레이어를 타겟으로 삼음")] public bool IsChangeTarget = false;


    Vector2 Vector2ToTarget ; float distanceToTarget ; Vector2 directionToTarget ; // 나중에 타겟과의 위치 계산할 때 쓸 변수들 코드 깔끔할라고 위로 뺌
    private Collider2D monsterCollider;     // 몬스터의 콜라이더
    private Rigidbody2D rigid; 
    Animator anim;
    #endregion

    public LayerMask wallLayer;
    private GameObject PreviewInstance;
    Coroutine attackCoroutine; private Coroutine hitRecoverCoroutine;

    enum MonterState
    {
        Idle,
        Chase,
        Attack,
        Hit,    // 피격 상태 추가
        Die
    }
    MonterState monterState = MonterState.Idle;


    // Start is called before the first frame update
    void Start()
    {
        rigid = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        monsterCollider = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();  // 스프라이트 렌더러 가져오기

        monsterHealth = monsterData.maxHealth;  // 초기 체력 설정
        InvokeRepeating(nameof(CheckForPlayer), 0f, monsterData.detectionInterval);
        attackTimer = -monsterAttackData.attackCooldown;
    }

    void CheckForPlayer()
    {
        if (targetPlayer == null){
            Collider2D tempCollider = DetectClosestPlayer();
            if (tempCollider != null){targetPlayer = tempCollider;}
        } else if (IsChangeTarget){
            Collider2D tempCollider = DetectClosestPlayer();
            if (tempCollider != null){targetPlayer = tempCollider;}
        }

        // 타겟 탐지를 하지 않는 경우 = IsChangeTarget이 false고 targetPlayer가 null이 아닌 경우
        if (targetPlayer != null) 
        {
            agent.destination = targetPlayer.transform.position;
            
            if (monterState == MonterState.Idle) {
                attackTimer = 0;
                SetChaseState(); 
            }
        }
    }

    
    public void Setup(Transform target)
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        agent.speed = monsterData.moveSpeed;  // SO에서 속도 설정
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(monterState);

        if (targetPlayer == null) return;
        

        switch (monterState)
        {
            case MonterState.Idle:
                Updata_In_Idle();
                break;
            case MonterState.Chase:
                Updata_In_Chase();
                break;
            case MonterState.Attack:
                Updata_In_Attack();
                break;
            case MonterState.Hit:
                Updata_In_Hit();
                break;
            case MonterState.Die:
                Updata_In_Die();
                break;
        }

        UpdateState();
        
    }



    void UpdateState()
    {
        if (targetPlayer == null) return;
        Vector2ToTarget = targetPlayer.transform.position - transform.position ;  
        distanceToTarget = Vector2ToTarget.magnitude;
        directionToTarget = Vector2ToTarget.normalized;
        
    }

    void Updata_In_Idle(){

    }

    void Updata_In_Chase(){
        UpdateSpriteDirection();  // 스프라이트 방향 업데이트
        
        //Debug.Log(attackTimer);
        attackTimer -= Time.deltaTime;

        if (distanceToTarget >= monsterData.stopChaseDistance)
        {
            targetPlayer = null;
            SetIdleState();
        }


        // 공격사거리에 들어왔을 때
        if ( distanceToTarget <= monsterData.attackRange)
        {

            if (attackTimer <= 0f)
            {
                ExecuteAttack();
                attackTimer = monsterAttackData.attackCooldown;
            }
        }
    }

    void Updata_In_Attack(){
        
    }

    void Updata_In_Hit(){
        
    }

    void Updata_In_Die(){
        
    }

    public void SetIdleState()
    {
        
        agent.isStopped = true; agent.destination = transform.position;
        monterState = MonterState.Idle;
        anim.SetBool("IsWalking", false);
    }

    public void SetChaseState()
    {
        Vector2ToTarget = targetPlayer.transform.position - transform.position ;  
        distanceToTarget = Vector2ToTarget.magnitude;
        directionToTarget = Vector2ToTarget.normalized;

        agent.isStopped = false;
        monterState = MonterState.Chase;
        anim.SetBool("IsWalking", true);
    }

    public void SetAttackState()
    {
        // monsterCollider.isTrigger = true;
        agent.isStopped = true; agent.velocity = Vector3.zero;
        monterState = MonterState.Attack;
        anim.SetTrigger("Attack");
    }

    void SetHitState(){
        agent.isStopped = true; agent.velocity = Vector3.zero;  // 속도를 0으로 설정
        monterState = MonterState.Hit;
        anim.SetTrigger("Hit");

        // 이전에 실행 중인 피격 회복 코루틴이 있다면 중단
        if (hitRecoverCoroutine != null)
        {
            StopCoroutine(hitRecoverCoroutine);
        }

        // 0.42초 후 상태 복귀
        hitRecoverCoroutine = StartCoroutine(HitRecoverCoroutine());
    }

    private IEnumerator HitRecoverCoroutine()
    {
        yield return new WaitForSeconds(0.42f);

        if (monterState == MonterState.Hit) // 여전히 Hit 상태일 때만 변경
        {
            SetChaseState();
        }
    }

    void SetDieState(){
        StopAttack();

        monterState = MonterState.Die;
        agent.isStopped = true; targetPlayer = null;
        agent.velocity = Vector3.zero;  // 속도를 0으로 설정
        monsterCollider.enabled = false;
        rigid.simulated = false;
        anim.SetTrigger("Die");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.gameObject.CompareTag("Weapon"))
        {
            // 체력감소 
            monsterHealth--;
            if(monsterHealth > 0)
            {
                SetHitState();
            }
            else 
            {
                SetDieState();
            }
        }
    }

    void StopAttack(){
        switch (monsterData.attackType){
            case (MonsterDataSO.AttackType.Dash) :
                HidePreview();
                StopCoroutine(attackCoroutine);
                break;

            case (MonsterDataSO.AttackType.Projectile) :
                StopCoroutine(attackCoroutine);
                break;

            case (MonsterDataSO.AttackType.AoE) :
                StopCoroutine(attackCoroutine);
                HidePreview();
                break;
        }
    }


    Collider2D DetectClosestPlayer()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, monsterData.detectionRange, playerLayer);

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
        if (targetPlayer != null && monterState == MonterState.Chase)
        {
            
            // 방향에 따라 스프라이트 뒤집기
            if (directionToTarget.x > 0)
            {
                spriteRenderer.flipX = false;  // 오른쪽 보기
            }
            else if (directionToTarget.x < 0)
            {
                spriteRenderer.flipX = true;   // 왼쪽 보기
            }
        }
    }
    
    #region AttackLogic
    public void ExecuteAttack()
    {
        switch (monsterData.attackType)
        {
            case MonsterDataSO.AttackType.Dash:
                attackCoroutine = StartCoroutine(DashAttackSequence());
                break;

            case MonsterDataSO.AttackType.Projectile:
                attackCoroutine = StartCoroutine(ProjectileAttackSequence());
                break;

            case MonsterDataSO.AttackType.AoE:
                attackCoroutine = StartCoroutine(AoEAttackSequence());
                break;
        }
    }
    #region DashCoroutine
    private IEnumerator DashAttackSequence()
    {

        // 1. 돌진 거리, 방향 정하기
        Vector2 start = transform.position;

        float adjustedDistance = distanceToTarget;

        // 🔴 2. 벽 체크: 몬스터 -> 플레이어 사이에 벽이 있는 경우 공격 중단
        RaycastHit2D wallCheck = Physics2D.Raycast(start, directionToTarget, Vector2.Distance(start, targetPlayer.transform.position), wallLayer);
        if (wallCheck.collider != null)
        {
            attackTimer = monsterAttackData.attackCooldown / 2;   // 공격이 캔슬나면 공격쿨타임의 반
            // SetIdleState(); // 또는 SetChaseState()
            yield break;
        }

        // 3. 상태 업데이트
        SetAttackState();



        // 4. 돌격 범위 계산
        RaycastHit2D hit = Physics2D.Raycast(start, directionToTarget, distanceToTarget, wallLayer); // 벽 충돌 체크
        if (hit.collider != null)
        {
            adjustedDistance = hit.distance - 0.1f; // 충돌 지점 앞까지
        }

        Vector2 dashTarget = start + directionToTarget * adjustedDistance;

        // 5. 돌진 경로 시각화
        ShowDashPreview(start, dashTarget); 

        // 6. 선딜레이
        yield return new WaitForSeconds(monsterAttackData.preCastingTime);

        // 7. 돌진 실행
        float distance = Vector2.Distance(start, dashTarget);
        float defaultSpeed = monsterAttackData.dashDistance / monsterAttackData.dashDuration;
        float duration = distance / defaultSpeed;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            rigid.MovePosition(Vector2.Lerp(start, dashTarget, t));
            elapsed += Time.deltaTime;
            yield return null;
        }

        rigid.MovePosition(dashTarget);

        // 8. 후딜레이
        yield return new WaitForSeconds(monsterAttackData.postCastingTime);

        // 9. 돌진범위 숨기기
        HidePreview(); 

        // 10. 공격 끝 상태 업데이트
        // monsterCollider.isTrigger = false;
        SetChaseState();
    }

    private void ShowDashPreview(Vector2 start, Vector2 end)
    {
        if (PreviewInstance == null && monsterAttackData.dashPreviewPrefab != null)
        {
            PreviewInstance = Instantiate(monsterAttackData.dashPreviewPrefab);
        }

        if (PreviewInstance != null)
        {
            PreviewInstance.transform.position = (start + end) / 2;
            Vector2 dir = end - start;
            float length = dir.magnitude;
            PreviewInstance.transform.right = dir;
            PreviewInstance.transform.localScale = new Vector3(length, 1f, 1f); // y 스케일은 알아서 맞춰
        }
    }

    private void HidePreview()
    {
        if (PreviewInstance != null)
        {
            Destroy(PreviewInstance);
        }
    }
    #endregion 
    


    private IEnumerator ProjectileAttackSequence()
    {
        
        

         // ▶ 공격 전에 벽이 있는지 체크
        RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToTarget, distanceToTarget, wallLayer);
        if (hit.collider != null)
        {
            attackTimer = monsterAttackData.attackCooldown / 2;   // 공격이 캔슬나면 공격쿨타임의 반
            // 벽에 막혀 있으면 공격 취소
            yield break;
        }

        // 1. 공격 상태로 전환
        SetAttackState();

        // 2. 선딜레이
        yield return new WaitForSeconds(monsterAttackData.preCastingTime);

        // 3. 투사체 생성 및 발사
        if (monsterAttackData.projectilePrefab != null)
        {
            GameObject proj = Instantiate(monsterAttackData.projectilePrefab, transform.position, Quaternion.identity);
            Rigidbody2D projRb = proj.GetComponent<Rigidbody2D>();

            if (projRb != null)
            {
                projRb.linearVelocity = directionToTarget * monsterAttackData.projectileSpeed;
            }
        }

        // 4. 후딜레이
        yield return new WaitForSeconds(monsterAttackData.postCastingTime);

        // 5. 상태 복귀
        SetChaseState();
    }

    private IEnumerator AoEAttackSequence()
{
    // 1. 공격 상태 전환
    SetAttackState();

    // 2. 미리 공격 위치 계산
    Vector3 aoePosition = targetPlayer.transform.position;

    // 3. 시각적 예고 프리팹 생성
    if (monsterAttackData.aoePreviewPrefab != null)
    {
        PreviewInstance = Instantiate(monsterAttackData.aoePreviewPrefab, aoePosition, Quaternion.identity);
        float visualScale = monsterAttackData.aoeRange * 2f;
        PreviewInstance.transform.localScale = new Vector3(visualScale, visualScale, 1f);
    }

    // 4. 선딜레이
    yield return new WaitForSeconds(monsterAttackData.preCastingTime);

    // 5. 예고 프리팹 제거
    HidePreview();

    // 6. 실제 AoE 생성
    if (monsterAttackData.aoePrefab != null)
    {
        GameObject aoe = Instantiate(monsterAttackData.aoePrefab, aoePosition, Quaternion.identity);
        aoe.transform.localScale = new Vector3(monsterAttackData.aoeRange * 2f, monsterAttackData.aoeRange * 2f, 1f);
        Destroy(aoe, monsterAttackData.aoeDuration);
    }

    // 7. 후딜레이
    yield return new WaitForSeconds(monsterAttackData.postCastingTime);

    // 8. 상태 복귀
    SetChaseState();
}
    #endregion
}