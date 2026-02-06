using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("컴포넌트")]
    public Rigidbody2D rd;

    [Header("가방 인벤토리")]
    public List<BagData> myBags;
    private BagData selectedBag;
    public Transform firePoint;

    [Header("플레이어 정보")]
    private BattleUnit myBattleUnit; // 내 명찰
    private UnitStats myStats;       // 내 스탯

    [Header("발사 정보")]
    [SerializeField] float minAng = 0f;
    [SerializeField] float maxAng = 90f;
    [SerializeField] float minPow = 0f;
    [SerializeField] float maxPow = 20f;

    public float MinAng => minAng;
    public float MaxAng => maxAng;
    public float MinPow => minPow;
    public float MaxPow => maxPow;
    
    // 실제 적용될 값
    public float currentAngle { get; private set; }
    public float currentPower { get; private set; }

    [Header("연결")]
    public TrajectoryLine trajectory; // ★ 인스펙터에서 TrajectoryLine 오브젝트 연결

    // 매니저가 호출할 조준 함수 (0~1 비율을 받음)

    void Awake()
    {
        myBattleUnit = GetComponent<BattleUnit>();
        myStats = GetComponent<UnitStats>();
    }

    // Player.cs
    void Start()
    {
        // 시작하자마자 인벤토리의 첫 번째 가방을 손에 듭니다.
        if (myBags.Count > 0) selectedBag = myBags[0];
    }

    public void UpdateAim(float angleRatio, float powerRatio)
    {
        // 1. 비율을 실제 게임 수치로 변환 (Lerp)
        // (currentAngle, currentPower 변수가 선언되어 있어야 합니다)
        currentAngle = Mathf.Lerp(minAng, maxAng, angleRatio);
        currentPower = Mathf.Lerp(minPow, maxPow, powerRatio);

        // 2. 가방의 물리 정보 가져오기 (무게, 저항, 중력)
        // 안전장치: 선택된 가방이 없으면 첫 번째 가방 사용
        if (selectedBag == null && myBags.Count > 0) selectedBag = myBags[0];

        float bagMass = 1.0f;
        float bagDrag = 0.0f;
        float bagGravity = 1.0f;

        if (selectedBag != null && selectedBag.bagPrefab != null)
        {
            Rigidbody2D rb = selectedBag.bagPrefab.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                bagMass = rb.mass;
                bagGravity = rb.gravityScale;
                
                // 유니티 버전에 따라 아래 중 하나를 사용하세요.
                bagDrag = rb.linearDamping;
            }
        }

        // 3. 초기 속도(Velocity) 계산
        // ForceMode2D.Impulse 공식: V = F / m
        float rad = currentAngle * Mathf.Deg2Rad;
        Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
        
        // 힘(Power)을 질량(Mass)으로 나누어야 실제 날아가는 속도가 됩니다.
        Vector2 startVelocity = dir * (currentPower / bagMass); 

        // 4. 궤적 그리기 요청 (변경된 함수 호출)
        if (trajectory != null)
        {
            // 이제 단순히 위치만 주는 게 아니라, 물리 속성까지 다 줍니다.
            trajectory.DrawSimulatedPath(firePoint.position, startVelocity, bagDrag, bagGravity);
        }
    }

    // ★ AI 전용: 비율(0~1)이 아니라 계산된 실제 각도와 파워를 넣는 함수
    public void UpdateAimDirect(float exactAngle, float exactPower)
    {
        // 범위 제한 (Clamp)
        currentAngle = Mathf.Clamp(exactAngle, minAng, maxAng);
        currentPower = Mathf.Clamp(exactPower, minPow, maxPow);

        // 2. 가방 물리 정보 가져오기 (기존 로직 재사용을 위해 함수 분리 추천하지만, 일단 복붙)
        if (selectedBag == null && myBags.Count > 0) selectedBag = myBags[0];

        float bagMass = 1.0f;
        float bagDrag = 0.0f;
        float bagGravity = 1.0f;

        if (selectedBag != null && selectedBag.bagPrefab != null)
        {
            Rigidbody2D rb = selectedBag.bagPrefab.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                bagMass = rb.mass;
                bagGravity = rb.gravityScale;
                bagDrag = rb.linearDamping; 
            }
        }

        // 3. 궤적 계산을 위한 속도
        float rad = currentAngle * Mathf.Deg2Rad;
        Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
        Vector2 startVelocity = dir * (currentPower / bagMass); 

        // 4. 궤적 그리기 (AI는 궤적을 안 보여주고 싶다면 이 부분은 생략 가능)
        // if (trajectory != null) trajectory.DrawSimulatedPath(firePoint.position, startVelocity, bagDrag, bagGravity);
    }


    // ★ 2. 실제 발사 함수
    public void Throw()
    {
        // 궤적 지우기
        if (trajectory != null) trajectory.ClearPath();

        if (myBags.Count > 0 && selectedBag == null) selectedBag = myBags[0]; // 임시 안전장치
        if (selectedBag == null) return;

        GameObject bagObj = Instantiate(selectedBag.bagPrefab, firePoint.position, Quaternion.identity);
        Bag bagScript = bagObj.GetComponent<Bag>();
        float finalDamage = selectedBag.damage + myStats.atk;

        if (bagScript != null) bagScript.Setup(finalDamage, myBattleUnit.myTeam);

        if (CameraManager.Instance != null) CameraManager.Instance.SetTarget(bagObj.transform);
        
        // 계산된 currentAngle, currentPower 사용
        Rigidbody2D bagRd = bagObj.GetComponent<Rigidbody2D>();
    
        // 1. 이동 경로 결정 (이 힘이 궤도를 만듭니다)
        float rad = currentAngle * Mathf.Deg2Rad;
        Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
        
        // ForceMode2D.Impulse를 사용하여 초기 속도를 즉시 부여합니다.
        bagRd.AddForce(dir * currentPower, ForceMode2D.Impulse);

        // 살짝 회전
        float randomRotation = 0.2f;
        // float randomRotation = UnityEngine.Random.Range(0.1f, 0.5f); // 던질 때마다 다른 회전감
        bagRd.AddTorque(randomRotation, ForceMode2D.Impulse);
    }
}