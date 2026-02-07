using UnityEngine;
using System.Collections;

// ★ 기존 Enemy.cs를 대체하는 AI 두뇌 스크립트
public class AIController : MonoBehaviour
{
    [Header("AI 설정")]
    [Tooltip("낮을수록 정확함 (0 ~ 10)")]
    public float errorRange = 2.0f; 

    // 내 몸체 (명령 내릴 대상)
    private Player myBody;
    private BattleUnit myUnit;

    // 좌 우 판정
    private bool isRight;

    void Awake()
    {
        myBody = GetComponent<Player>(); 
        myUnit = GetComponent<BattleUnit>();
    }

    // GameManager가 호출할 함수
    public void StartAITurn()
    {
        StartCoroutine(ThinkAndAction());
    }

    IEnumerator ThinkAndAction()
    {
        // 고민 (랜덤 시간)
        float thinkTime = Random.Range(1.0f, 2.5f);
        yield return new WaitForSeconds(thinkTime);

        // 타겟 찾기
        Transform target = FindTarget();

        // 방향 결정

        if (target.position.x < transform.position.x) 
        {
            isRight = false;
            transform.localScale = new Vector3(-1, 1, 1); // 왼쪽 보기
        }
        else 
        {
            isRight = true;
            transform.localScale = new Vector3(1, 1, 1);  // 오른쪽 보기
        }

        if (target == null) 
        {
            Debug.Log("AI: 타겟 없음");
            GameManager.Instance.OnActionComplete();
            yield break;
        }



        // 탄도학 계산 & 조준 명령
        CalculateAndAim(target);

        // 잠시 대기 후 발사 명령
        yield return new WaitForSeconds(0.5f);
        myBody.Throw(); // ★ 몸체에게 발사 명령!
    }

    Transform FindTarget()
    {
        var units = FindObjectsByType<BattleUnit>(FindObjectsSortMode.None);
        Transform bestTarget = null;
        float closeDist = float.MaxValue;

        foreach (var unit in units)
        {
            // 적대 팀이고, 살아있는 대상
            if (unit.myTeam != myUnit.myTeam && !unit.isDead)
            {
                float d = Vector2.Distance(transform.position, unit.transform.position);
                if (d < closeDist) { closeDist = d; bestTarget = unit.transform; }
            }
        }
        return bestTarget;
    }

    void CalculateAndAim(Transform target)
    {
        // 발사 위치 (몸체의 firePoint)
        Transform firePoint = myBody.firePoint; 
        Vector2 direction = target.position - firePoint.position;
        float distance = direction.magnitude;
        float heightDiff = direction.y;

        // 랜덤 각도 (30~70도)
        float calcAngle = Random.Range(30f, 70f);
        
        // 현재 선택된 가방 정보 가져오기 (가방 무게 등 계산용)
        // Player 스크립트가 선택된 가방을 관리하므로, 안전하게 중력값 등을 가정하거나 가져와야 함.
        // 여기서는 표준 중력값으로 계산
        float g = Mathf.Abs(Physics2D.gravity.y); 
        
        // v^2 역산 공식
        float rad = calcAngle * Mathf.Deg2Rad;
        float v2 = (g * distance * distance) / (2 * Mathf.Cos(rad) * Mathf.Cos(rad) * (distance * Mathf.Tan(rad) - heightDiff));

        if (v2 <= 0 || float.IsNaN(v2)) v2 = 20f * 20f; // 계산 불가 시 최대 파워

        float calcPower = Mathf.Sqrt(v2);

        // 오차 적용
        float finalAngle = calcAngle + Random.Range(-errorRange, errorRange);
        float finalPower = calcPower + Random.Range(-errorRange * 0.5f, errorRange * 0.5f);

        // 방향 뒤집기 (왼쪽을 보고 있다면)
        if (!isRight)
        {
             finalAngle = 180f - finalAngle;
        }

        // 몸체에게 입력 (UpdateAimDirect 호출)
        myBody.UpdateAimDirect(finalAngle, finalPower);
    }
}