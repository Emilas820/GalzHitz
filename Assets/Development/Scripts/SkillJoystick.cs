using UnityEngine;
using UnityEngine.EventSystems;

public class SkillJoystick : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("이동 설정")]
    public float radius = 200f;     // 왼쪽보다 조금 작게 설정 추천

    [Header("감도 설정")]
    public float stiffness = 2.0f; 
    public float smoothSpeed = 15f; 

    [Header("제스처 설정")]
    public float triggerThreshold = 0.5f; // 반경의 50% 이상 당겨야 인식

    // 내부 변수
    private Vector3 originPos;
    private Vector3 targetPos;
    private Vector2 clickOriginOffset;
    private bool isPressed = false;

    void Awake()
    {
        originPos = transform.localPosition;
        targetPos = originPos; 
    }

    void Update()
    {
        // ★ 핵심 변경점: Time.deltaTime 대신 Time.unscaledDeltaTime 사용
        // 게임이 슬로우 모션이어도 조이스틱 UI는 부드럽게 움직여야 함
        float dt = Time.unscaledDeltaTime; 

        if (Vector3.Distance(transform.localPosition, targetPos) > 0.1f)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, targetPos, dt * smoothSpeed);
        }
        else
        {
            transform.localPosition = targetPos;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // 1. 발사 중(Firing) 상태가 아니면 반응 안 함
        if (GameManager.Instance == null || !GameManager.Instance.CanUseSkillJoystick()) 
            return;

        isPressed = true;

        // 2. ★ 게임 슬로우 모션 시작!
        GameManager.Instance.SetTimeScale(GameManager.Instance.slowMotionFactor);

        // (터치 보정 로직 - HeavyJoystick과 동일)
        Vector2 localMousePos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            transform.parent as RectTransform, eventData.position, eventData.pressEventCamera, out localMousePos
        );
        clickOriginOffset = localMousePos - (Vector2)originPos;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isPressed) return;

        Vector2 localMousePos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            transform.parent as RectTransform, eventData.position, eventData.pressEventCamera, out localMousePos
        );

        Vector2 rawOffset = (localMousePos - (Vector2)originPos) - clickOriginOffset;
        
        float rawDistance = rawOffset.magnitude;
        float resistanceFactor = 1.0f + (rawDistance * stiffness * 0.002f); 
        float dampedDistance = rawDistance / resistanceFactor;

        if (dampedDistance > radius) dampedDistance = radius;

        targetPos = originPos + (Vector3)(rawOffset.normalized * dampedDistance);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isPressed) return;
        isPressed = false;

        // 3. ★ 게임 속도 원상복구
        if (GameManager.Instance != null)
            GameManager.Instance.SetTimeScale(1.0f);

        // 4. 제스처 판별 (어느 쪽으로 당겼는가?)
        Vector2 dragVector = transform.localPosition - originPos;
        float dragRatio = dragVector.magnitude / radius; // 0.0 ~ 1.0

        // 충분히 당겼을 때만 판정
        if (dragRatio >= triggerThreshold)
        {
            float x = dragVector.x;
            float y = dragVector.y;

            // 절대값을 비교하여 수평인지 수직인지 판별
            if (Mathf.Abs(x) > Mathf.Abs(y))
            {
                // 가로 방향 (Horizontal)
                if (x < 0) 
                {
                    // <<<< 왼쪽: 스킬 사용
                    Debug.Log(">>> 스킬 발동! (Left Drag)");
                    if (GameManager.Instance != null) 
                        GameManager.Instance.ActivateCurrentProjectileSkill();
                }
            }
            else
            {
                // 세로 방향 (Vertical)
                if (y > 0)
                {
                    // ^^^^ 위쪽: 조작 취소
                    Debug.Log(">>> 조작 취소 (Up Drag)");
                    // 아무 일도 안 하고 그냥 놓음
                }
            }
        }

        // 제자리로 복귀
        targetPos = originPos; 
    }
}