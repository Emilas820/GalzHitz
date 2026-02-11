using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LobbyCharacterSelectButton : MonoBehaviour
{
    [Header("데이터 연결")]
    public Characters myCharacterData; 

    [Header("UI 컴포넌트")]
    public Button btn;              
    public TextMeshProUGUI nameText; 

    void Start()
    {
        SoundManager.Instance.PlayBGM(SoundManager.Instance.main_Background);

        if (btn != null)
        {
            btn.onClick.AddListener(() => {
                if (LobbyManager.Instance != null)
                    LobbyManager.Instance.LobbyAddCharacter(myCharacterData);
            });
        }

        if (LobbyManager.Instance != null)
        {
            LobbyManager.Instance.OnDeckChanged += RefreshUI;
        }

        // 시작 시 데이터가 이미 인스펙터에 연결되어 있다면 바로 갱신
        if (myCharacterData != null) RefreshUI();
    }

    void OnDestroy()
    {
        if (LobbyManager.Instance != null)
        {
            LobbyManager.Instance.OnDeckChanged -= RefreshUI;
        }
    }

    // ★ [추가] 외부(매니저)에서 버튼을 생성한 직후 데이터를 넣어주는 함수
    public void Setup(Characters data)
    {
        myCharacterData = data;
        RefreshUI(); // 데이터 들어오자마자 UI 갱신!
    }

    private void RefreshUI()
    {

        // 로비 매니저는 없어도 되지만, 내 데이터(myCharacterData)가 없으면 아무것도 못 함
        if (myCharacterData == null) return;

        // 로비 매니저가 아직 준비 안 됐으면(Start 시점 등), 기본 상태로라도 텍스트를 띄워야 함
        bool isSelected = false;
        if (LobbyManager.Instance != null)
        {
            isSelected = LobbyManager.Instance.IsCharacterSelected(myCharacterData);
        }

        // 1. 버튼 활성화/비활성화
        if (btn != null)
        {
            btn.interactable = !isSelected; 
        }

        // 2. 텍스트 변경
        if (nameText != null)
        {
            if (isSelected)
            {
                nameText.text = $"{myCharacterData.characterName} <size=70%>(선택 중)</size>";
                nameText.color = Color.gray; 
            }
            else
            {
                // ★ 여기서 기본 이름이 뜹니다!
                nameText.text = myCharacterData.characterName;
                nameText.color = Color.black; 
            }
        }
    }
}