using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("컴포넌트")]
    public Rigidbody2D rd;

    [Header("가방 데이터셋")]
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



    void Awake()
    {
        myBattleUnit = GetComponent<BattleUnit>();
        myStats = GetComponent<UnitStats>();
    }

    void Start()
    {
        // 시작하자마자 인벤토리의 첫 번째 가방을 손에 듭니다.
        if (myBags.Count > 0) selectedBag = myBags[0];
    }

    // 공통 로직을 담은 핵심 계산 함수
    private void CalculateAndDrawPath(float angle, float power)
    {
        // 가방 정보 가져오기 로직 (중복되던 부분)
        if (selectedBag == null && myBags.Count > 0) selectedBag = myBags[0];
        
        float bagMass = 1.0f;
        float bagDrag = 0.0f;
        float bagGravity = 1.0f;

        if (selectedBag?.bagPrefab != null)
        {
            Rigidbody2D rb = selectedBag.bagPrefab.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                bagMass = rb.mass;
                bagGravity = rb.gravityScale;
                bagDrag = rb.linearDamping;
            }
        }

        float rad = angle * Mathf.Deg2Rad;
        Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
        Vector2 startVelocity = dir * (power / bagMass);

        if (trajectory != null)
        {
            trajectory.DrawSimulatedPath(firePoint.position, startVelocity, bagDrag, bagGravity);
        }
    }

    // 2. 플레이어용: 비율로 호출
    public void UpdateAim(float angleRatio, float powerRatio)
    {
        currentAngle = Mathf.Lerp(minAng, maxAng, angleRatio);
        currentPower = Mathf.Lerp(minPow, maxPow, powerRatio);
        CalculateAndDrawPath(currentAngle, currentPower);
    }

    // 3. AI용: 직접 값으로 호출
    public void UpdateAimDirect(float exactAngle, float exactPower)
    {
        currentAngle = Mathf.Clamp(exactAngle, minAng, maxAng);
        currentPower = Mathf.Clamp(exactPower, minPow, maxPow);
        CalculateAndDrawPath(currentAngle, currentPower);
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
    
        // 이동 경로 결정 (이 힘이 궤도를 만듭니다)
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