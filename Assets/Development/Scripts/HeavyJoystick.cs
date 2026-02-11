using UnityEngine;
using UnityEngine.EventSystems;

public class HeavyJoystick : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("이동 설정")]
    public float radius = 300f;    // 최대 이동 반경 (px)

    [Header("뻑뻑함(저항) 설정")]
    [Tooltip("값이 클수록 당기기 힘들어집니다. (추천: 1.5 ~ 3.0)")]
    public float stiffness = 2.0f; 
    [Tooltip("마우스 반응 속도 (낮을수록 묵직함)")]
    public float smoothSpeed = 15f;

    [Header("옵션")]
    public LineRenderer rubberBand;

    [Header("버튼 클릭 여부")]
    private bool isPressed = false;

    // 내부 변수
    private Vector3 originPos;
    private Vector3 targetPos;
    private Vector2 clickOriginOffset;
    
    // ★ 계산된 데이터 (외부에서 가져다 쓸 수 있음)
    public float InputPower { get; private set; } // 0.0 ~ 1.0
    public float InputAngle { get; private set; } // 0.0 ~ 1.0

    void Awake()
    {
        originPos = transform.localPosition;
        targetPos = originPos; 
    }

    void Update()
    {
        // 1. 물리적 이동 (묵직한 움직임)
        if (Vector3.Distance(transform.localPosition, targetPos) > 0.1f)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, targetPos, Time.deltaTime * smoothSpeed);
        }
        else
        {
            transform.localPosition = targetPos;
        }

        if (isPressed)
        {
            CalculateAndSendData();
        }
    }

    // =========================================================
    // ★ 핵심: UI 위치를 (Power, Angle) 데이터로 변환하는 함수
    // =========================================================
    void CalculateAndSendData()
    {
        Vector3 offset = transform.localPosition - originPos;

        // [추가] 조이스틱 X축 방향에 따른 캐릭터 반전 요청
        if (Mathf.Abs(offset.x) > 0.1f) // 미세한 떨림 방지
        {
            float direction = offset.x > 0 ? 1f : -1f;
            GameManager.Instance.UpdatePlayerDirection(direction);
        }

        float normalizedPower = Mathf.Clamp(Mathf.Abs(offset.x) / radius, 0f, 1f); // Abs 추가 (왼쪽으로 당겨도 파워는 양수)
        float normalizedAngle = Mathf.Clamp(offset.y / radius, 0f, 1f);

        InputPower = normalizedPower;
        InputAngle = normalizedAngle;

        GameManager.Instance.UpdateInput(InputAngle, InputPower);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isPressed = true;   // 누름 상태 ON

        Vector2 localMousePos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            transform.parent as RectTransform, eventData.position, eventData.pressEventCamera, out localMousePos
        );
        clickOriginOffset = localMousePos - (Vector2)originPos;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 localMousePos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            transform.parent as RectTransform, eventData.position, eventData.pressEventCamera, out localMousePos
        );

        Vector2 rawOffset = (localMousePos - (Vector2)originPos) - clickOriginOffset;
        
        // 1사분면(우상단) 제한 로직 (필요 시 주석 해제)
        //rawOffset.x = Mathf.Max(0, rawOffset.x);
        rawOffset.y = Mathf.Max(0, rawOffset.y);

        float rawDistance = rawOffset.magnitude;
        float resistanceFactor = 1.0f + (rawDistance * stiffness * 0.002f); 
        float dampedDistance = rawDistance / resistanceFactor;

        if (dampedDistance > radius) dampedDistance = radius;

        targetPos = originPos + (Vector3)(rawOffset.normalized * dampedDistance);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPressed = false; // 누름 상태 OFF

        targetPos = originPos; 
        // 발사도 매니저에게 요청
        GameManager.Instance.RequestThrow();
    }
}