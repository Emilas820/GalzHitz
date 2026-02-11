using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LobbyBagSelectButton : MonoBehaviour
{
    [Header("데이터 연결")]
    public BagData myBagData; // 이 버튼이 담당하는 가방 데이터

    [Header("UI 컴포넌트")]
    public Button btn;
    public TextMeshProUGUI nameText;

    void Start()
    {
        // 1. 버튼 클릭 시 로비매니저에게 "이 가방 장착해줘" 요청
        if (btn != null)
        {
            btn.onClick.AddListener(() => {
                if (LobbyManager.Instance != null)
                    LobbyManager.Instance.LobbyAddBag(myBagData);
            });
        }

        // 2. 상태 변화 감지 (구독)
        if (LobbyManager.Instance != null)
        {
            LobbyManager.Instance.OnDeckChanged += RefreshUI;
        }

        // 시작 시 초기화
        if (myBagData != null) RefreshUI();
    }

    void OnDestroy()
    {
        // 구독 해제
        if (LobbyManager.Instance != null)
        {
            LobbyManager.Instance.OnDeckChanged -= RefreshUI;
        }
    }

    // 외부에서 데이터 주입용 (Instantiate로 생성 시 사용)
    public void Setup(BagData data)
    {
        myBagData = data;
        RefreshUI();
    }

    // ★ 핵심: 현재 상황에 맞춰 버튼 상태 갱신
    private void RefreshUI()
    {
        if (LobbyManager.Instance == null || myBagData == null) return;

        // 1. 현재 선택된 캐릭터 슬롯이 있는지 확인 (없으면 가방 장착 불가)
        int currentIndex = LobbyManager.Instance.currentSelectedIndex;
        bool hasCharacterSelected = (currentIndex >= 0 && currentIndex < LobbyManager.Instance.lobbyCharacterDeck.Count);

        // 2. 현재 선택된 슬롯의 캐릭터가 '이미 이 가방'을 끼고 있는지 확인
        bool isEquipped = false;
        if (hasCharacterSelected && currentIndex < LobbyManager.Instance.lobbyBagDeck.Count)
        {
            BagData currentEquippedBag = LobbyManager.Instance.lobbyBagDeck[currentIndex];
            if (currentEquippedBag == myBagData) // 같은 가방인가?
            {
                isEquipped = true;
            }
        }

        // 3. 버튼 활성화/비활성화
        if (btn != null)
        {
            // 캐릭터가 선택되어 있어야 하고 && 이미 끼고 있는 가방이 아니어야 클릭 가능
            btn.interactable = hasCharacterSelected && !isEquipped;
        }

        // 4. 텍스트 변경
        if (nameText != null)
        {
            if (isEquipped)
            {
                // 이미 장착 중일 때
                nameText.text = $"{myBagData.bagName} <size=70%>(장착 중)</size>";
                nameText.color = Color.gray; 
            }
            else
            {
                // 장착 가능하거나, 선택된 캐릭터가 없을 때
                nameText.text = myBagData.bagName;
                
                // 캐릭터가 선택 안 되어 있으면 텍스트를 흐리게 표시 (선택사항)
                nameText.color = hasCharacterSelected ? Color.black : Color.gray; 
            }
        }
    }
}