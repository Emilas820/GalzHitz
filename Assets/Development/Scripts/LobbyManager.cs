using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyManager : MonoBehaviour
{
    [Header("테스트용 데이터 연결")]
    public Characters testCharacter; // 플레이어 캐릭터 데이터(ScriptableObject)
    public BagData testBag;          // 플레이어 가방 데이터
    public StageData testStage;      // 스테이지 데이터

    [Header("이동할 씬 이름")]
    public string gameSceneName = "Stage1"; // 실제 게임 씬 이름으로 변경하세요

    // UI 버튼에 연결할 함수
    public void OnClickStartGame()
    {
        // 1. 글로벌 데이터 매니저가 있는지 확인
        if (GlobalDataManager.Instance == null)
        {
            Debug.LogError("GlobalDataManager가 씬에 없습니다!");
            return;
        }

        // 2. 선택한 데이터 주입 (GlobalDataManager에 저장)
        GlobalDataManager.Instance.SelectCharacter(testCharacter);
        GlobalDataManager.Instance.SelectBag(testBag);
        GlobalDataManager.Instance.SelectStage(testStage);

        Debug.Log("데이터 설정 완료! 게임 씬으로 이동합니다.");

        // 3. 게임 씬 로드
        SceneManager.LoadScene(gameSceneName);
    }
}