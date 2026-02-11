using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.PostProcessing; // ★ 추가: 포스트 프로세싱 제어용

public class SkillJoystick : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("이동 설정")]
    public float radius = 200f;     // 왼쪽보다 조금 작게 설정 추천

    [Header("감도 설정")]
    public float stiffness = 2.0f; 
    public float smoothSpeed = 15f; 

    [Header("제스처 설정")]
    public float triggerThreshold = 0.5f; // 반경의 50% 이상 당겨야 인식

    [Header("UI 연출 (새로 추가됨)")]
    public RectTransform skillIcon;   // ★ 왼쪽에 배치할 아이콘 (스킬 발동)
    public RectTransform cancelIcon;  // ★ 위쪽에 배치할 아이콘 (취소)
    public RectTransform gradientBg;  
    public float bgMaxScale = 2.5f;   // 최대로 당겼을 때 배경 크기
    public float normalScale = 1.0f;  // 기본 크기
    public float selectScale = 1.5f;  // 선택됐을 때 최대 크기
    public CanvasGroup uiCanvasGroup; // (선택) 페이드 효과를 위해 부모에 달아주면 좋음

    // 내부 변수
    private Vector3 originPos;
    private Vector3 targetPos;
    private Vector2 clickOriginOffset;
    private bool isPressed = false;

    [Header("Post Process 설정")]
    public PostProcessVolume postVolume; // 인스펙터에서 볼륨 오브젝트 연결
    private Vignette _vignette;
    private Bloom _bloom;

    // 기본값 저장용 (원상복구를 위함)
    private float _originVignetteIntensity;
    private Color _originVignetteColor;
    private float _originBloomIntensity;



    void Awake()
    {
        originPos = transform.localPosition;
        targetPos = originPos;

        ToggleIcons(false);

        // 볼륨에서 효과 찾아오기
        if (postVolume != null)
        {
            postVolume.profile.TryGetSettings(out _vignette);
            postVolume.profile.TryGetSettings(out _bloom);

            // 기존 값 백업
            if (_vignette != null)
            {
                _originVignetteIntensity = _vignette.intensity.value;
                _originVignetteColor = _vignette.color.value;
            }
            if (_bloom != null)
            {
                _originBloomIntensity = _bloom.intensity.value;
            }
        }
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

        // ★ 드래그 중일 때 실시간으로 아이콘 크기 업데이트
        if (isPressed)
        {
            UpdateIconScale();
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

        // ★ 슬로우 모션 효과 적용
        ApplyPostProcess(true);
        ToggleIcons(true);

        // ★ 터치 시작 시 아이콘 보여주기
        ToggleIcons(true);

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

        // ★ 효과 원상 복구
        ApplyPostProcess(false);
        ToggleIcons(false);

        // ★ 터치 종료 시 아이콘 숨기기
        ToggleIcons(false);

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

    // =========================================================
    // ★ [추가] UI 연출 로직
    // =========================================================

    // 아이콘 끄고 켜기
    void ToggleIcons(bool isOn)
    {
        if (skillIcon != null) skillIcon.gameObject.SetActive(isOn);
        if (cancelIcon != null) cancelIcon.gameObject.SetActive(isOn);

        // ★ [추가] 배경도 같이 껐다 켰다
        if (gradientBg != null) gradientBg.gameObject.SetActive(isOn);

        // CanvasGroup이 있다면 투명도 조절 (더 부드러움)
        if (uiCanvasGroup != null)
        {
            uiCanvasGroup.alpha = isOn ? 1f : 0f;
        }
    }

    // 현재 조이스틱 위치에 따라 아이콘 크기 조절
    void UpdateIconScale()
    {
        Vector2 currentOffset = transform.localPosition - originPos;
        Vector2 normalizedDir = currentOffset.normalized;
        
        // 현재 얼마나 당겼는지 (0.0 ~ 1.0)
        float distRatio = Mathf.Clamp01(currentOffset.magnitude / radius);

        // ★ [추가] 배경 크기 조절 (당길수록 커짐)
        if (gradientBg != null)
        {
            // 0에서 시작해서 최대 bgMaxScale까지 커짐
            float bgScale = Mathf.Lerp(0f, bgMaxScale, distRatio);
            gradientBg.localScale = Vector3.one * bgScale;
        }

        // 1. 스킬 아이콘 (왼쪽: Vector2.left 와의 내적값)
        if (skillIcon != null)
        {
            // 내적(Dot Product): 방향이 일치할수록 1에 가까움
            // 왼쪽(-1, 0) 방향과 현재 방향이 얼마나 비슷한가?
            float intensity = Vector2.Dot(normalizedDir, Vector2.left); 
            intensity = Mathf.Max(0, intensity); // 반대 방향이면 0 처리

            // 당긴 거리(distRatio)와 방향 일치도(intensity)를 곱해서 크기 결정
            float targetScale = Mathf.Lerp(normalScale, selectScale, intensity * distRatio);
            skillIcon.localScale = Vector3.one * targetScale;
        }

        // 2. 취소 아이콘 (위쪽: Vector2.up 와의 내적값)
        if (cancelIcon != null)
        {
            // 위쪽(0, 1) 방향과 현재 방향이 얼마나 비슷한가?
            float intensity = Vector2.Dot(normalizedDir, Vector2.up);
            intensity = Mathf.Max(0, intensity);

            float targetScale = Mathf.Lerp(normalScale, selectScale, intensity * distRatio);
            cancelIcon.localScale = Vector3.one * targetScale;
        }
    }

    // 포스트 프로세싱 값 변경 함수
    private void ApplyPostProcess(bool active)
    {
        if (postVolume == null) return;

        if (active)
        {
            // 요청하신 세팅 적용
            if (_vignette != null)
            {
                _vignette.color.value = new Color(0.239f, 0.466f, 0.549f); // 3D778C
                _vignette.intensity.value = 0.3f;
            }
            if (_bloom != null)
            {
                _bloom.intensity.value = 8f;
            }
        }
        else
        {
            // 원래대로 복구
            if (_vignette != null)
            {
                _vignette.color.value = _originVignetteColor;
                _vignette.intensity.value = _originVignetteIntensity;
            }
            if (_bloom != null)
            {
                _bloom.intensity.value = _originBloomIntensity;
            }
        }
    }
}