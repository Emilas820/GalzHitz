using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("턴 표시 UI")]
    public TextMeshProUGUI turnText; // 화면 상단에 "플레이어의 턴!" 표시

    [Header("게임 오버 UI")]
    public GameObject gameOverPanel;    // 평소엔 꺼져있는 패널
    public GameObject resultPanel;      // 평소엔 꺼져있는 패널
    [SerializeField] private TextMeshProUGUI resultText;  // "승리!" or "패배..." 텍스트
    [SerializeField] private TextMeshProUGUI finalRoundtext;             // 최종 라운드
    public GameObject mainBack;         // 재시작 버튼

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // 시작할 때 게임 오버 패널 끄기
        if(gameOverPanel != null) gameOverPanel.SetActive(false);
    }

    // 1. 턴이 바뀔 때 호출 (GameManager가 부름)
    public void UpdateTurnText(string name)
    {
        if (turnText != null)
        {
            turnText.text = $"[ {name}의 차례 ]";
            
            // (선택) 텍스트 애니메이션 등을 넣을 수 있음
        }
    }

    // 2. 게임 종료 시 호출 (GameManager가 부름)
    public void ShowGameOver(bool isWin, int round)
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true); // 패널 켜기
            resultPanel.SetActive(true);
            
            if (resultText != null)
            {
                if (isWin)
                {
                    resultText.text = "VICTORY!";
                    resultText.color = Color.blue;
                }
                else
                {
                    resultText.text = "DEFEAT...";
                    resultText.color = Color.red;
                }
                finalRoundtext.text = round.ToString();
            }
        }
    }

    // 3. 재시작 버튼에 연결할 함수
    public void OnRestartBtnClick()
    {
        // 현재 씬을 다시 로드 (GlobalDataManager는 살아있음)
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    // 4. 로비로 가기 버튼
    public void OnLobbyBtnClick()
    {
        // 메뉴 씬 이름으로 변경하세요
        SceneManager.LoadScene("Main");
    }
}