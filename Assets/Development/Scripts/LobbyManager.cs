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
    public StageData currentStage;
    public string gameSceneName = "Stage1"; // 실제 게임 씬 이름으로 변경하세요

    // UI 버튼에 연결할 함수
    public void OnClickStartGame()
    {
        // 1. 글로벌 데이터 매니저가 있는지 확인
        if (GlobalDataManager.Instance == null)
        {
            Debug.LogError("오류: GlobalDataManager가 씬에 없습니다! (Main 씬에서 시작했나요?)");
            return;
        }

        // 2. 기존 데이터 초기화 (중복 추가 방지)
        GlobalDataManager.Instance.ClearDeck();
        GlobalDataManager.Instance.ClearStage();

        // 3. 로비에 설정된 덱을 글로벌 매니저로 복사 (이사짐 싸기)
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

            // ★ [핵심] 0번 캐릭터 + 0번 가방을 세트로 등록
            GlobalDataManager.Instance.AddCharacter(charData);
            GlobalDataManager.Instance.AddBag(bagData);

            // ★ [로그 출력 추가] 가방이 null이면 "없음"으로 표시
            string bagNameStr = (bagData != null) ? bagData.bagName : "가방 없음";
            Debug.Log($"[덱 {i}번] \"{charData.characterName}\", \"{bagNameStr}\" 장착 완료");

        }

        // 4. 스테이지 설정
        if (currentStage != null)
        {
            GlobalDataManager.Instance.SelectStage(currentStage);
        }
        else
        {
            Debug.LogError("입장할 스테이지(StageData)가 설정되지 않았습니다!");
            return;
        }

        Debug.Log($"데이터 설정 완료! (덱 {GlobalDataManager.Instance.characterDeck.Count}명) -> 게임 시작");

        // 5. 게임 씬 로드
        SceneManager.LoadScene(gameSceneName);
    }
}