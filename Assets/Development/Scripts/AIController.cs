using UnityEngine;
using System.Collections;

public class AIController : MonoBehaviour
{
    [Header("AI 설정")]
    [Tooltip("낮을수록 정확함 (0 ~ 10)")]
    public float errorRange = 2.0f; 

    private Player myBody;
    private BattleUnit myUnit;

    void Awake()
    {
        myBody = GetComponent<Player>(); 
        myUnit = GetComponent<BattleUnit>();
    }

    public void StartAITurn()
    {
        StartCoroutine(ThinkAndAction());
    }

    IEnumerator ThinkAndAction()
    {
        float thinkTime = Random.Range(1.0f, 2.5f);
        yield return new WaitForSeconds(thinkTime);

        Transform target = FindTarget();

        if (target == null) 
        {
            Debug.Log("AI: 타겟 없음");
            GameManager.Instance.OnActionComplete();
            yield break;
        }

        // 방향 결정
        if (target.position.x < transform.position.x) 
        {
            transform.localScale = new Vector3(-1, 1, 1); // 왼쪽 보기
        }
        else 
        {
            transform.localScale = new Vector3(1, 1, 1);  // 오른쪽 보기
        }

        // 탄도학 계산 & 조준 명령
        CalculateAndAim(target);

        yield return new WaitForSeconds(1.0f);
        myBody.Throw();
    }

    Transform FindTarget()
    {
        // 1. 씬 전체 검색(FindObjectsByType) 대신 GameManager의 명단을 사용 (훨씬 빠름)
        if (GameManager.Instance == null) return null;

        var allUnits = GameManager.Instance.allUnits;
        Transform bestTarget = null;
        float closeDist = float.MaxValue;

        foreach (var unit in allUnits)
        {
            // [예외 처리] 유닛이 없거나(파괴됨), 나 자신이거나
            if (unit == null || unit == myUnit) continue;

            // [조건 1] 같은 팀이면 패스 (팀킬 방지)
            if (unit.myTeam == myUnit.myTeam) continue;

            // [조건 2] 이미 죽은 유닛이면 패스
            if (unit.isDead) continue;

            // [조건 3] "구조물" 거르기 (중요!)
            // BattleUnit은 있는데 Player 스크립트(이동/공격 기능)가 없는 벽/바닥 등을 거릅니다.
            // 만약 구조물도 공격해야 한다면 이 부분은 주석 처리하세요.
            if (unit.playerScript == null) continue;

            // [조건 4] 가장 가까운 적 찾기
            float d = Vector2.Distance(transform.position, unit.transform.position);
            
            // (선택 사항) 만약 거리가 너무 멀면(맵 끝과 끝) 포기하거나 다른 로직을 넣을 수 있음
            // if (d > 20.0f) continue; 

            if (d < closeDist) 
            { 
                closeDist = d; 
                bestTarget = unit.transform; 
            }
        }

        return bestTarget;
    }

    void CalculateAndAim(Transform target)
    {
        Transform firePoint = myBody.firePoint; 
        
        // 1. 거리 계산 수정
        // ★ 잘못된 코드: float distance = Vector2.Distance(...); 
        // ★ 올바른 코드: 수평 거리(x)와 수직 거리(y)를 따로 구해야 함
        float targetX = target.position.x;
        float myX = firePoint.position.x;
        
        float x = Mathf.Abs(targetX - myX); // 수평 거리 (절대값)
        float y = target.position.y - firePoint.position.y; // 높이 차이 (target - start)

        // 2. 각도 랜덤 설정 (45도가 가장 멀리 가므로 40~75도 사이 추천)
        float calcAngle = Random.Range(40f, 75f);
        
        // 3. 물리 공식 적용 (v^2 역산)
        float g = Mathf.Abs(Physics2D.gravity.y); 
        float rad = calcAngle * Mathf.Deg2Rad;
        
        // 공식: v^2 = (g * x^2) / (2 * cos^2(theta) * (x * tan(theta) - y))
        // 여기서 x는 '수평 거리'여야 합니다. (이전엔 대각선 거리를 넣어서 힘이 폭주함)
        
        float bottom = 2 * Mathf.Cos(rad) * Mathf.Cos(rad) * (x * Mathf.Tan(rad) - y);
        
        float v2 = 0f;
        if (bottom > 0.001f || bottom < -0.001f) // 0으로 나누기 방지
        {
            v2 = (g * x * x) / bottom;
        }

        // 계산 불가능한 상황(너무 높거나 닿을 수 없음)이면 기본 힘으로 쏨
        if (v2 <= 0 || float.IsNaN(v2)) v2 = 15f * 15f; 

        float calcPower = Mathf.Sqrt(v2);

        // 4. 오차 적용
        float finalAngle = calcAngle + Random.Range(-errorRange, errorRange);
        float finalPower = calcPower + Random.Range(-errorRange * 0.5f, errorRange * 0.5f);

        // ★ 중요: 왼쪽을 본다고 해서 180 - angle을 할 필요가 없음!
        // Player.cs에서 transform.localScale.x를 보고 알아서 뒤집어주기 때문.
        // 그냥 0~90도 사이의 각도만 주면 됨.

        // 몸체에게 입력
        myBody.UpdateAimDirect(finalAngle, finalPower);
    }
}