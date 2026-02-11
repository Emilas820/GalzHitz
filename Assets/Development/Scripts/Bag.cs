using UnityEngine;

public class Bag : MonoBehaviour
{
    private float finalDamage;
    private BattleUnit shooter; // 던진 사람
    private Team shooterTeam;   // 던진 팀
    
    public GameObject damageEffect;

    [Header("설정")]
    public LayerMask targetLayer; // 인스펙터에서 'Unit'만 체크!
    public Vector2 detectionSize = new Vector2(0.8f, 0.6f); // 인지 범위

    public void Setup(float damageAmount, BattleUnit owner) 
    {
        this.finalDamage = damageAmount;
        this.shooter = owner;           
        this.shooterTeam = owner.myTeam; 
        
        Invoke("ExplodeAndDestroy", 7f);
    }

    // =========================================================
    // 1. 물리 충돌 (바닥) - Matrix 켜진 놈들
    // =========================================================
    void OnCollisionEnter2D(Collision2D collision)
    {
        // [조건 3] 바닥에 닿음 -> 충돌 발생(튕김), 폭발 안 함.
        // 여기에 "통~" 하는 효과음 넣으면 좋습니다.
    }

    // =========================================================
    // 2. 센서 감지 (유닛) - Matrix 꺼진 놈들
    // =========================================================
    void FixedUpdate()
    {
        // 내 위치에 'Unit'이 겹쳐있는지 검사
        Collider2D hit = Physics2D.OverlapBox(transform.position, detectionSize, transform.eulerAngles.z, targetLayer);

        if (hit != null)
        {
            BattleUnit targetUnit = hit.GetComponent<BattleUnit>();
            if (targetUnit != null)
            {
                // [조건 1] 나 자신이거나 같은 팀(아군)이면?
                if (targetUnit == shooter || targetUnit.myTeam == shooterTeam)
                {
                    // 아무것도 안 함 -> 자연스럽게 통과됨 (return)
                    return;
                }

                // [조건 2] 적군이면?
                // 통과하면서 데미지 주고 터짐
                UnitStats targetStats = hit.GetComponent<UnitStats>();
                if (targetStats != null)
                {                    
                    targetStats.TakeDamage((int)finalDamage);
                }

                // 폭발 이펙트 + 삭제
                SoundManager.Instance.PlaySFX(SoundManager.Instance.hit);
                ExplodeAndDestroy();
            }

            // 2. ★ [추가] 구조물인지 확인
            Structure targetStructure = hit.GetComponent<Structure>();
            if (targetStructure != null)
            {
                // 구조물은 팀 구분 없이 무조건 데미지
                targetStructure.TakeDamage(finalDamage);

                // 폭발하고 가방 삭제
                ExplodeAndDestroy();
            }
        }
    }

    void OnDrawGizmos()
    {
        // 1. 색상 지정 (빨간색)
        Gizmos.color = new Color(1, 0, 0, 0.5f); // 반투명 빨강

        // 2. 가방의 회전 각도와 위치를 기즈모에도 적용 (핵심!)
        // 이 코드가 없으면 가방은 도는데 박스는 가만히 서 있게 됨
        Matrix4x4 rotationMatrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
        Gizmos.matrix = rotationMatrix; 

        // 3. 박스 그리기
        // (이미 위치/회전은 matrix로 맞췄으니 중심점은 0,0,0으로 잡음)
        if (detectionSize != Vector2.zero)
        {
            Gizmos.DrawWireCube(Vector3.zero, detectionSize); // 테두리만
            Gizmos.DrawCube(Vector3.zero, detectionSize);     // 내부 채우기 (선택)
        }
    }

    void ExplodeAndDestroy()
    {
        // 폭발 이펙트 생성
        if (damageEffect != null)
        {
            GameObject ef = Instantiate(damageEffect, transform.position, Quaternion.identity);
            Destroy(ef, 2f);
        }

        // 턴 종료 보고
        if (GameManager.Instance != null) 
            GameManager.Instance.OnActionComplete();

        // 가방 삭제
        Destroy(gameObject);
        
    }

    private bool isSkillUsed = false;

    public void ActivateSkill()
    {
        if (isSkillUsed) return; // 이미 썼으면 무시

        isSkillUsed = true;
        Debug.Log($"[{gameObject.name}] 특수 스킬 발동!");
        
        // 여기에 실제 스킬 로직 구현 (예: 분열, 가속, 방향 전환 등)
        // 지금은 이펙트만 살짝 보여주거나 로그만 띄움
    }
}