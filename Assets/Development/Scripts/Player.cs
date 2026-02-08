using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("컴포넌트")]
    public Rigidbody2D rd;
    public Animator anim;

    [Header("가방 데이터셋")]
    public List<BagData> myBags;
    private BagData selectedBag;
    public Transform firePoint;

    [Header("플레이어 정보")]
    private BattleUnit myBattleUnit; 
    private UnitStats myStats;       

    [Header("발사 정보")]
    [SerializeField] float minAng = 0f;
    [SerializeField] float maxAng = 90f;
    [SerializeField] float minPow = 0f;
    [SerializeField] float maxPow = 20f;

    [Header("이동 설정")]
    public float moveSpeed = 3.0f;      
    public float maxMoveFuel = 100f;    
    public float currentFuel;           
    public float fuelConsumption = 20f;
    private float currentXInput = 0f; 

    public float MinAng => minAng;
    public float MaxAng => maxAng;
    public float MinPow => minPow;
    public float MaxPow => maxPow;
    
    public float currentAngle { get; private set; }
    public float currentPower { get; private set; }

    [Header("연결")]
    public TrajectoryLine trajectory; 

    void Awake()
    {
        myBattleUnit = GetComponent<BattleUnit>();
        myStats = GetComponent<UnitStats>();
    }

    void Start()
    {
        if (myBags.Count > 0) selectedBag = myBags[0];
        if (anim == null) anim = GetComponentInChildren<Animator>();
    }

    // =========================================================
    // ★ 1. 물리 이동 & 걷기 애니메이션 (FixedUpdate)
    // =========================================================
    void FixedUpdate()
    {
        // [이동 로직]
        if (Mathf.Abs(currentXInput) < 0.1f)
        {
            // 입력 없으면 정지
            Vector2 stopVel = rd.linearVelocity;
            stopVel.x = 0;
            rd.linearVelocity = stopVel;
            // 주의: 여기서 return을 하면 안 됨! 아래 애니메이션 갱신을 위해 통과시킴
        }
        else
        {
            // 입력 있으면 이동
            Vector2 velocity = rd.linearVelocity;
            velocity.x = currentXInput * moveSpeed;
            rd.linearVelocity = velocity;

            // 연료 소모
            float fuelCost = Time.fixedDeltaTime * fuelConsumption;
            currentFuel -= fuelCost;

            if (UIManager.Instance != null)
                UIManager.Instance.UpdateFuelUI(currentFuel, maxMoveFuel);
        }

        // [걷기 애니메이션 업데이트]
        // ★ 핵심: Aiming 로직은 여기서 뺐습니다. 오직 이동 속도만 반영합니다.
        if (anim != null)
        {
            // 현재 물리적인 속도의 절대값을 spd에 전달
            // 멈추면 0이 들어가므로 자동으로 Idle이 됨
            anim.SetFloat("spd", Mathf.Abs(rd.linearVelocity.x));
        }        
    }

    // =========================================================
    // ★ 2. 조준 (UpdateAim - 조이스틱 당길 때 호출됨)
    // =========================================================
    // GameManager.UpdateInput -> 여기서 호출
    public void UpdateAim(float angleRatio, float powerRatio)
    {
        // 1. 값 계산
        currentAngle = Mathf.Lerp(minAng, maxAng, angleRatio);
        currentPower = Mathf.Lerp(minPow, maxPow, powerRatio);
        
        // 2. 궤적 그리기
        CalculateAndDrawPath(currentAngle, currentPower);

        // 3. ★ [수정] 조준 애니메이션 ON
        // 이 함수가 호출된다는 건 유저가 조이스틱을 잡고 있다는 뜻!
        if (anim != null)
        {
            anim.SetBool("isAiming", true); 
        }
    }

    // AI용 직접 호출
    public void UpdateAimDirect(float exactAngle, float exactPower)
    {
        currentAngle = Mathf.Clamp(exactAngle, minAng, maxAng);
        currentPower = Mathf.Clamp(exactPower, minPow, maxPow);
        CalculateAndDrawPath(currentAngle, currentPower);
        
        // AI도 쏠 때 자세 잡기
        if (anim != null) anim.SetBool("isAiming", true);
    }

    // =========================================================
    // ★ 3. 발사 (Throw - 조이스틱 놓을 때 호출됨)
    // =========================================================
    public void Throw()
    {
        // 1. 궤적 지우기
        if (trajectory != null) trajectory.ClearPath();

        if (myBags.Count > 0 && selectedBag == null) selectedBag = myBags[0]; 
        if (selectedBag == null) return;

        // 2. ★ [수정] 애니메이션 트리거 처리
        if (anim != null)
        {
            // 손을 놓았으므로 조준 상태 해제 (Ready -> Throw 넘어가는 조건)
            anim.SetBool("isAiming", false); 
            
            // 던지기 트리거 당김!
            anim.SetTrigger("doThrow");
        }

        // 3. 가방 생성 및 발사
        GameObject bagObj = Instantiate(selectedBag.bagPrefab, firePoint.position, Quaternion.identity);
        Bag bagScript = bagObj.GetComponent<Bag>();
        float finalDamage = selectedBag.damage + myStats.atk;

        if (bagScript != null) bagScript.Setup(finalDamage, myBattleUnit);

        if (CameraManager.Instance != null) CameraManager.Instance.SetTarget(bagObj.transform);
        
        Rigidbody2D bagRd = bagObj.GetComponent<Rigidbody2D>();
    
        float rad = currentAngle * Mathf.Deg2Rad;
        Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
        
        if (transform.localScale.x < 0) dir.x *= -1; 

        bagRd.AddForce(dir * currentPower, ForceMode2D.Impulse);
        bagRd.AddTorque(0.2f, ForceMode2D.Impulse);
    }


    // =========================================================
    // 기타 유틸리티 함수들
    // =========================================================

    public void Move(float xInput)
    {
        currentXInput = xInput;

        if (Mathf.Abs(xInput) < 0.1f) return;

        if (currentFuel <= 0)
        {
            GameManager.Instance.EndTurnManual();
            currentXInput = 0f; 
            return;
        }

        if (!GameManager.Instance.hasMovedThisTurn)
        {
            GameManager.Instance.ReportPlayerMovement();
        }

        if (xInput > 0.1f) transform.localScale = new Vector3(1, 1, 1);
        else if (xInput < -0.1f) transform.localScale = new Vector3(-1, 1, 1);
    }

    public void StopMove()
    {
        currentXInput = 0f;
        rd.linearVelocity = new Vector2(0, rd.linearVelocity.y); 

        if (anim != null)
        {
            anim.SetFloat("spd", 0f);
            
            // 턴 종료 시 혹시 조준 상태로 굳는 것 방지
            anim.SetBool("isAiming", false);
        }
    }

    public void ResetTurnData()
    {
        currentFuel = maxMoveFuel;
        if (UIManager.Instance != null) UIManager.Instance.UpdateFuelUI(currentFuel, maxMoveFuel);
        
        // 턴 시작 시 확실하게 애니메이션 리셋
        if(anim != null) anim.SetBool("isAiming", false);
    }

    private void CalculateAndDrawPath(float angle, float power)
    {
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
        if (transform.localScale.x < 0) dir.x *= -1; 

        Vector2 startVelocity = dir * (power / bagMass);

        if (trajectory != null)
        {
            trajectory.DrawSimulatedPath(firePoint.position, startVelocity, bagDrag, bagGravity);
        }
    }

    public void SetDirection(float direction)
    {
        // direction이 1이면 오른쪽(Scale 1), -1이면 왼쪽(Scale -1)
        if (direction > 0) transform.localScale = new Vector3(1, 1, 1);
        else if (direction < 0) transform.localScale = new Vector3(-1, 1, 1);
    }
}