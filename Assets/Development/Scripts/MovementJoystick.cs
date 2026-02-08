using UnityEngine;
using UnityEngine.EventSystems;

public class MovementJoystick : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("설정")]
    public float radius = 100f; // 이동 조이스틱은 좀 작아도 됨
    
    // 내부 변수
    private Vector3 originPos;
    private Vector3 targetPos;
    private bool isPressed = false;

    void Awake()
    {
        originPos = transform.localPosition;
        targetPos = originPos; 
    }

    void Update()
    {
        // 조이스틱 핸들 이동 (부드럽게)
        if (Vector3.Distance(transform.localPosition, targetPos) > 0.1f)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, targetPos, Time.deltaTime * 20f);
        }

        // 누르고 있을 때만 입력값 전달
        if (isPressed)
        {
            float xInput = (transform.localPosition.x - originPos.x) / radius;
            // -1.0(왼쪽) ~ 1.0(오른쪽) 값 전달
            GameManager.Instance.UpdateMoveInput(xInput);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isPressed = true;
        UpdateHandlePosition(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        UpdateHandlePosition(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPressed = false;
        targetPos = originPos; // 원위치 복귀
        GameManager.Instance.UpdateMoveInput(0f); // 멈춤 신호 전달
    }

    void UpdateHandlePosition(PointerEventData eventData)
    {
        Vector2 localMousePos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            transform.parent as RectTransform, eventData.position, eventData.pressEventCamera, out localMousePos
        );

        Vector2 offset = localMousePos - (Vector2)originPos;
        
        // Y축 이동 제한 (좌우로만 움직이게)
        offset.y = 0; 

        // 반경 제한
        if (offset.magnitude > radius)
        {
            offset = offset.normalized * radius;
        }

        targetPos = originPos + (Vector3)offset;
    }
}