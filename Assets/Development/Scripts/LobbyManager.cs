using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class LobbyManager : MonoBehaviour
{
    static public LobbyManager Instance;

    [Header("로비에서 설정할 출전 명단")]
    // 인스펙터에서 0번 인덱스끼리, 1번 인덱스끼리 짝을 맞춰서 넣어주세요.
    public List<Characters> lobbyCharacterDeck = new List<Characters>(); 
    public List<BagData> lobbyBagDeck = new List<BagData>();

    [Header("설정")]
    public StageData selectStage;

    [Header("선택 중인 캐릭터 슬롯")]
    public int currentSelectedIndex = -1;

    [Header("상태 신호기")]
    public System.Action OnDeckChanged;

    [Header("UI 설정")]
    public Transform deckListParent;    // ★ 프리팹이 생성될 부모 (Content 혹은 Panel)
    public GameObject deckSlotPrefab;   // ★ "SelectedUI" 프리팹

    void Awake()
    {
        if(Instance == null) Instance = this;
    }

    void Start()
    {
        // 1. 씬 시작 시 현재 상태(빈 화면 혹은 초기값) 그려주기
        UpdateDeckUI();

        // 2. 앞으로 데이터가 변할 때마다(OnDeckChanged) UpdateDeckUI를 실행하라고 등록
        OnDeckChanged += UpdateDeckUI;
    }

    // UI 버튼에 연결할 함수
    public void OnClickStartGame()
    {
        // 글로벌 데이터 매니저가 있는지 확인
        if (GlobalDataManager.Instance == null)
        {
            Debug.LogError("오류: GlobalDataManager가 씬에 없습니다! (Main 씬에서 시작했나요?)");
            return;
        }

        GlobalDataManager.Instance.ClearData();

        // 캐릭터 리스트 개수만큼 반복
        for (int i = 0; i < lobbyCharacterDeck.Count; i++)
        {
            Characters charData = lobbyCharacterDeck[i];
            BagData bagData = null;

            // 가방 리스트에도 해당 순번(i)이 있는지 확인 (안전을 위해)
            if (i < lobbyBagDeck.Count)
            {
                bagData = lobbyBagDeck[i];
            }
            else
            {
                Debug.LogWarning($"{i}번 캐릭터({charData.characterName})의 짝이 되는 가방이 없습니다!");
                // 필요하다면 기본 가방을 넣어주는 로직 추가 가능
            }

            // 0번 캐릭터 + 0번 가방을 세트로 등록
            GlobalDataManager.Instance.AddCharacter(charData);
            GlobalDataManager.Instance.AddBag(bagData);

            // 가방이 null이면 "없음"으로 표시
            string bagNameStr = (bagData != null) ? bagData.bagName : "가방 없음";
            Debug.Log($"[덱 {i}번] \"{charData.characterName}\", \"{bagNameStr}\" 장착 완료");
        }

        // 4. 스테이지 설정
        if (selectStage != null)
        {           
            Debug.Log($"데이터 설정 완료! 미소녀 {GlobalDataManager.Instance.characterDeck.Count}명, 게임 시작");

            GlobalDataManager.Instance.SelectStage(selectStage);

            // 초기화
            ClearDeck();
            LobbyCancelStageSelection();

            SceneManager.LoadScene(GlobalDataManager.Instance.currentStage.name);
        }
        else
        {
            Debug.LogError("입장할 스테이지(StageData)가 설정되지 않았습니다!");
            return;
        }
    }

    // 캐릭터, 가방 관리
    public void LobbyAddCharacter(Characters charData)
    {
        // 1. 이미 덱에 있는지 검사
        if (lobbyCharacterDeck.Contains(charData))
        {
            Debug.LogWarning($"[Lobby] 이미 선택된 캐릭터입니다: {charData.characterName}");
            return; // 중복이면 함수 종료 (추가 안 함)
        }

        lobbyCharacterDeck.Add(charData);
        
        // 캐릭터가 생겼으니 가방 자리도 null로 채워서 자릿수 맞춰줌
        lobbyBagDeck.Add(null); 

        // 방금 추가한 녀석을 "선택된 상태"로 변경
        currentSelectedIndex = lobbyCharacterDeck.Count - 1; 

        Debug.Log($"[Lobby] 캐릭터 추가: {charData.characterName} (Index: {currentSelectedIndex})");
        OnDeckChanged?.Invoke();
    }

    public bool IsCharacterSelected(Characters charData)
    {
        return lobbyCharacterDeck.Contains(charData);
    }

    public void LobbyAddBag(BagData bagData)
    {
        // 선택된 캐릭터가 없거나 인덱스가 이상하면 리턴
        if (currentSelectedIndex < 0 || currentSelectedIndex >= lobbyCharacterDeck.Count)
        {
            Debug.LogWarning("[Lobby] 가방을 장착할 캐릭터를 먼저 선택해주세요!");
            return;
        }

        // 선택된 위치에 가방 덮어쓰기
        // (LobbyAddCharacter에서 이미 null로 공간을 만들어뒀으므로 안전)
        if (currentSelectedIndex < lobbyBagDeck.Count)
        {
            lobbyBagDeck[currentSelectedIndex] = bagData;
            Debug.Log($"[Lobby] {currentSelectedIndex}번 슬롯에 가방 장착: {bagData.bagName}");
            OnDeckChanged?.Invoke();
        }
    }

    // 덱 초기화
    public void ClearDeck()
    {
        lobbyCharacterDeck.Clear();
        lobbyBagDeck.Clear();

        OnDeckChanged?.Invoke();
        Debug.Log("출전 명단(덱) 초기화 완료");
    }

    // 슬롯 선택
    public void SelectSlot(int index)
    {
        if (index >= 0 && index < lobbyCharacterDeck.Count)
        {
            currentSelectedIndex = index;
            Debug.Log($"[Lobby] {index}번 슬롯 선택됨");
            
            // 선택된 게 바뀌었으니 UI 갱신 (테두리 옮기기 위해)
            OnDeckChanged?.Invoke(); 
        }
    }

    // 슬롯 삭제
    public void RemoveItemAt(int index)
    {
        if (index >= 0 && index < lobbyCharacterDeck.Count)
        {
            Debug.Log($"[Lobby] {index}번 슬롯 삭제");

            lobbyCharacterDeck.RemoveAt(index);
            
            // 가방도 같이 삭제 (리스트 싱크 유지)
            if (index < lobbyBagDeck.Count)
            {
                lobbyBagDeck.RemoveAt(index);
            }

            // 삭제 후 선택 인덱스 조정
            // 만약 내가 선택한 놈을 지웠거나, 내 뒤에 있는 놈을 지웠다면 인덱스 조정 필요
            if (currentSelectedIndex >= lobbyCharacterDeck.Count)
            {
                // 마지막 놈을 지웠으면 그 앞 놈을 선택, 다 지웠으면 -1
                currentSelectedIndex = lobbyCharacterDeck.Count - 1;
            }
            
            OnDeckChanged?.Invoke();
        }
    }

    // 스테이지 관리
    public void LobbySelectStage(StageData stage)
    {
        if (stage != null)
        {
            selectStage = stage;
            Debug.Log($"[Lobby] 스테이지 선택됨: {stage.stageName}");
            OnDeckChanged?.Invoke();
        }
    }

    // 스테이지 선택 취소
    public void LobbyCancelStageSelection()
    {
        if (selectStage != null)
        {
            Debug.Log($"[Lobby] 스테이지 선택 취소: {selectStage.stageName}");
            selectStage = null;
            OnDeckChanged?.Invoke();
        }
        else
        {
            Debug.Log("[Lobby] 선택된 스테이지가 없습니다.");
        }
    }

    // =========================================================
    // ★ [추가] 덱 UI 갱신 함수 (GenerateUnitStatusUI와 유사 구조)
    // =========================================================
    private void UpdateDeckUI()
    {
        if (deckListParent != null)
        {
            foreach (Transform child in deckListParent) Destroy(child.gameObject);
        }

        for (int i = 0; i < lobbyCharacterDeck.Count; i++)
        {
            Characters charData = lobbyCharacterDeck[i];
            BagData bagData = (i < lobbyBagDeck.Count) ? lobbyBagDeck[i] : null;

            if (deckSlotPrefab != null && deckListParent != null)
            {
                GameObject slotObj = Instantiate(deckSlotPrefab, deckListParent);
                LobbyDeckSlot slotScript = slotObj.GetComponent<LobbyDeckSlot>();
                
                
                if (slotScript != null)
                {
                    // ★ 핵심: 현재 그리는 i번 슬롯이 "선택된 인덱스"인지 확인
                    bool isSelected = (i == currentSelectedIndex);

                    // Setup 함수에 "몇 번인지(i)", "선택 함수", "삭제 함수"를 전달
                    slotScript.Setup(i, 
                        charData.characterImage, 
                        (bagData != null ? bagData.bagIcon : null),
                        isSelected,
                        SelectSlot,    // 클릭하면 실행될 함수 전달
                        RemoveItemAt   // 삭제 누르면 실행될 함수 전달
                    );
                }
            }
        }
    }

    void OnDestroy()
    {
        OnDeckChanged -= UpdateDeckUI;
    }
}