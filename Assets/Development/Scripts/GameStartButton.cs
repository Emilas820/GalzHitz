using UnityEngine;
using UnityEngine.UI;

public class GameStartButton : MonoBehaviour
{
    public GameObject buttonObject;

    void Start()
    {
        // 시작할 때 한 번 체크
        RefreshButtonStatus();

        // LobbyManager의 데이터가 바뀔 때만 RefreshButtonStatus를 실행하라고 등록
        if (LobbyManager.Instance != null)
        {
            LobbyManager.Instance.OnDeckChanged += RefreshButtonStatus;
        }
    }
    
    public void RefreshButtonStatus()
    {
        if (LobbyManager.Instance == null) return;

        bool isDeckReady = LobbyManager.Instance.lobbyCharacterDeck.Count > 0 &&
                           LobbyManager.Instance.lobbyCharacterDeck.Count == LobbyManager.Instance.lobbyBagDeck.Count;
        bool isStageSelected = LobbyManager.Instance.selectStage != null;

        buttonObject.SetActive(isDeckReady && isStageSelected);
    }

    private void OnDestroy()
    {
        // 메모리 누수 방지를 위해 이벤트 구독 해제
        if (LobbyManager.Instance != null)
        {
            LobbyManager.Instance.OnDeckChanged -= RefreshButtonStatus;
        }
    }
}