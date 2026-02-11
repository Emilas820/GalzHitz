using UnityEngine;
using UnityEngine.UI;
using DG.Tweening; // DoTween 필수
using TMPro;

public class MainUIManager : MonoBehaviour
{
    [Header("UI References")]
    public Image handleImage;          // 토글할 핸들 버튼의 이미지

    [Header("UI References")]
    public StageData selectedStage;

    [Header("Toggle Animation (DoTween)")]
    [SerializeField] private float defaultScale = 1f;
    [SerializeField] private float toggleScale = 1.2f;
    [SerializeField] private float duration = 0.2f;
    
    [Header("Shader Settings (All In 1)")]
    [SerializeField] private string glowProperty = "_Glow";
    [SerializeField] private float defaultGlow = 0f;
    [SerializeField] private float targetGlow = 1.0f;
    [SerializeField] private float glowDuration = 0.5f;

    private bool isToggled = false;
    private Material handleMaterial;

    void Start()
    {
        // 핸들의 머티리얼 인스턴스화 (중요: 다른 UI에 영향 방지)
        if (handleImage != null)
        {
            handleImage.material = new Material(handleImage.material);
            handleMaterial = handleImage.material;
        }
    }

    public void OnHandleClick()
    {
        // 1. 상태를 반전시킵니다. (true -> false, false -> true)
        isToggled = !isToggled;

        // 2. 상태에 따라 다른 함수를 호출합니다.
        if (isToggled)
        {
            DoTurnOn();
        }
        else
        {
            DoTurnOff();
        }
    }

    // --- 실제 실행될 함수들 ---

    private void DoTurnOn()
    {
        Debug.Log("On 함수 실행");
        
        // DoTween 연출
        handleImage.transform.DOScale(toggleScale, duration).SetEase(Ease.OutBack);
        
        // Shader 효과 (Glow 켜기)
        handleMaterial.DOFloat(targetGlow, glowProperty, glowDuration);
        
        // 여기에 메뉴 열기나 사운드 재생 등 추가 로직을 넣으세요.
        LobbyManager.Instance.LobbySelectStage(selectedStage);
    }

    private void DoTurnOff()
    {
        Debug.Log("Off 함수 실행");
        
        // DoTween 연출 (원래 크기로)
        handleImage.transform.DOScale(defaultScale, duration).SetEase(Ease.InBack);
        
        // Shader 효과 (Glow 끄기)
        handleMaterial.DOFloat(defaultGlow, glowProperty, glowDuration);
        
        // 여기에 메뉴 닫기 등 추가 로직을 넣으세요.
        LobbyManager.Instance.LobbyCancelStageSelection();
    }
}