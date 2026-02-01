using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("컴포넌트")]
    public Rigidbody2D rd;

    [Header("스테이터스")]
    
    [SerializeField] float moveSpeed = 2.0f;
    [SerializeField] float maxHP = 5.0f;
    [SerializeField] float hp = 5.0f;
    public float p_HP
    {
        get => hp;
        set => hp = Mathf.Clamp(value, 0, maxHP); // 외부에서 어떤 값을 넣든 0~maxHP 사이로 고정
    }

    [Header("가방 인벤토리")]
    public List<BagData> myBags; // 인스펙터에서 던질 수 있는 가방 데이터들을 드래그해서 넣어줍니다.
    
    private BagData selectedBag; // 현재 던지려고 선택한 가방
    public GameObject firePoint;    // 발사 포인트
    private int selectedIndex = 0;

    [Header("발사 정보")]
    [SerializeField] float minAng = 0f; // 최소 각
    [SerializeField] float maxAng = 90f; // 최대 각
    [SerializeField] float minPow = 0f; // 최소 강도
    [SerializeField] float maxPow = 20f; // 최대 강도
    public float throwAng = 0;  // 발사 각
    public float throwPow = 0;  // 발사 강도


    void FixedUpdate()
    {
        Movement();
    }

    void Movement()
    {
        float h = Input.GetAxisRaw("Horizontal"); 

        // 현재 속도와 입력값이 같으면 굳이 새로 대입하지 않음 (떨림 방지)
        if (Mathf.Approximately(rd.linearVelocity.x, h * moveSpeed)) return;

        rd.linearVelocity = new Vector2(h * moveSpeed, rd.linearVelocity.y);
    }
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.TryGetComponent<Bag>(out var damage))
        {
            // + 피격 애니메이션
            // + 데미지 적용
        }
    }

    void PlayHitEffect()
    {
        // 피격 이펙트 전용
    }



    // 애니메이션 메소드 구현 위치
    // trigger 파라미터 "state", float 파라미터 "speed"사용.
    // state는 입력 신호를 통해, speed는 플레이어 속도값을 받아 적용.
    // "Idle" -> "Throw" 변환은 Throw() 실행 시 수행할 예정.


    // state == trigger off: Idle
    // state == trigger on: Throw
    // speed < 0.1 : Idle
    // speed > 0.1 && speed< 0.5  : Walk
    // speed > 0.5 : Run


    // ===========================================
    // ============= 플레이어 공격 로직 =============
    // ===========================================

    void Throw()
    {
        if (selectedBag == null) return;

        // 1. 데이터에 등록된 프리팹을 소환합니다.
        GameObject bagObj = Instantiate(selectedBag.bagPrefab, firePoint.transform.position, Quaternion.identity);
        
        // 2. 소환된 가방 스크립트에 데이터를 주입합니다.
        Bag bagScript = bagObj.GetComponent<Bag>();
        if (bagScript != null)
        {
            bagScript.Setup(selectedBag); // 가방이 자기 데이터(관통 횟수 등)를 알게 합니다.
        }

        // 3. 카메라 타겟 변경 (아까 논의한 부분)
        if (CameraManager.Instance != null)
        {
            CameraManager.Instance.SetTarget(bagObj.transform);
        }
        
        // 4. 물리 발사 실행
        Rigidbody2D bagRd = bagObj.GetComponent<Rigidbody2D>();
        float rad = throwAng * Mathf.Deg2Rad;
        Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
        bagRd.AddForce(dir * throwPow, ForceMode2D.Impulse);
    }
}
