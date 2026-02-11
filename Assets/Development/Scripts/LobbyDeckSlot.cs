using UnityEngine;
using UnityEngine.UI;

public class LobbyDeckSlot : MonoBehaviour
{
    [Header("UI 컴포넌트 연결")]
    public Image characterImage;  // 캐릭터 아이콘 표시용
    public Image bagImage;        // 가방 아이콘 표시용
    public GameObject selectedHighlight; // ★ [추가] 선택된 상태 테두리(또는 배경)

    [Header("버튼 연결")]
    public Button selectButton;   // ★ [추가] 캐릭터 이미지에 붙은 투명 버튼
    public Button removeButton;   // ★ [추가] X 버튼

    // 내부 변수
    private int myIndex;
    
    // Setup 함수 업그레이드 (인덱스와 함수들을 매개변수로 받음)
    public void Setup(int index, Sprite charIcon, Sprite bagIcon, bool isSelected, System.Action<int> onSelect, System.Action<int> onRemove)
    {
        myIndex = index;

        // 1. 아이콘 설정 (기존과 동일)
        if (characterImage != null)
        {
            characterImage.sprite = charIcon;
            characterImage.enabled = (charIcon != null);
        }

        if (bagImage != null)
        {
            if (bagIcon != null)
            {
                bagImage.sprite = bagIcon;
                bagImage.gameObject.SetActive(true);
            }
            else
            {
                bagImage.gameObject.SetActive(false);
            }
        }

        // 2. ★ 선택된 상태 표시 (선택된 놈만 테두리 켜기)
        if (selectedHighlight != null)
        {
            selectedHighlight.SetActive(isSelected);
        }

        // 3. ★ 버튼 기능 연결 (이전 리스너 제거 후 다시 연결)
        selectButton.onClick.RemoveAllListeners();
        selectButton.onClick.AddListener(() => onSelect(myIndex));

        removeButton.onClick.RemoveAllListeners();
        removeButton.onClick.AddListener(() => onRemove(myIndex));
    }
}