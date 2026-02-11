using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class LobbyManager : MonoBehaviour
{
    [Header("로비에서 설정할 출전 명단")]
    // 인스펙터에서 0번 인덱스끼리, 1번 인덱스끼리 짝을 맞춰서 넣어주세요.
    public List<Characters> lobbyCharacterDeck = new List<Characters>(); 
    public List<BagData> lobbyBagDeck = new List<BagData>();

    [Header("설정")]
    public StageData selectStage;


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
        lobbyCharacterDeck.Add(charData);
        Debug.Log($"{charData.characterName}선택");
    }
    public void LobbyAddBag(BagData bagData)
    {
        lobbyBagDeck.Add(bagData);
        Debug.Log($"{bagData.bagName}선택");
    }

    // 덱 초기화
    public void ClearDeck()
    {
        lobbyCharacterDeck.Clear();
        lobbyBagDeck.Clear();
        Debug.Log("출전 명단(덱) 초기화 완료");
    }

    // 스테이지 관리
    public void LobbySelectStage(StageData stage)
    {
        if (stage != null)
        {
            selectStage = stage;
            Debug.Log($"[Lobby] 스테이지 선택됨: {stage.stageName}");
        }
    }

    // 스테이지 선택 취소
    public void LobbyCancelStageSelection()
    {
        if (selectStage != null)
        {
            Debug.Log($"[Lobby] 스테이지 선택 취소: {selectStage.stageName}");
            selectStage = null;
        }
        else
        {
            Debug.Log("[Lobby] 선택된 스테이지가 없습니다.");
        }
    }
}