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
    [SerializeField] private GameObject turnPanel;
    [SerializeField] private TextMeshProUGUI resultText;  // "승리!" or "패배..." 텍스트
    [SerializeField] private TextMeshProUGUI finalRoundtext;    // 최종 라운드
    public GameObject mainBack;         // 재시작 버튼

    [Header("조작 UI")]
    public GameObject aimJoystick;      // 기존 슈팅 조이스틱 (HeavyJoystick)
    public GameObject moveJoystick;     // 새로 만든 이동 조이스틱
    public GameObject modeSwitchBtn;    // "이동/조준 변경" 버튼패널
    public GameObject skillJoystick;    // 스킬 조이스틱
    public TextMeshProUGUI modeBtnText; // 버튼 텍스트 변경용

    [Header("유닛 스테이터스 패널")]
    public Transform playerStatusPanel; // 왼쪽 패널 (부모)
    public Transform enemyStatusPanel;  // 오른쪽 패널 (부모)

    [Header("프리팹 설정 (각각 따로 연결)")]
    public GameObject playerItemPrefab; // ★ 아군용 프리팹 (얼굴이 왼쪽)
    public GameObject enemyItemPrefab;  // ★ 적군용 프리팹 (얼굴이 오른쪽)

    [Header("이동량 게이지")]
    public Image fuelGaugeImage;

    

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // 시작할 때 게임 오버 패널 끄기
        if(gameOverPanel != null) gameOverPanel.SetActive(false);

        // 게임 시작 시에는 일단 다 꺼두기 (Init 상태)
        DisablePlayerControls();
    }

    // 전투 시작 시 GameManager가 호출할 함수
   public void GenerateUnitStatusUI()
    {
        // 1. 초기화 (기존 목록 삭제)
        foreach (Transform child in playerStatusPanel) Destroy(child.gameObject);
        foreach (Transform child in enemyStatusPanel) Destroy(child.gameObject);

        if (GameManager.Instance != null)
        {
            foreach (BattleUnit unit in GameManager.Instance.allUnits)
            {
                GameObject itemObj = null;

                // 2. 팀에 따라 다른 프리팹 생성!
                if (unit.myTeam == Team.Player)
                {
                    // 아군 -> 아군 프리팹 사용, 왼쪽 패널에 넣기
                    itemObj = Instantiate(playerItemPrefab);
                    itemObj.transform.SetParent(playerStatusPanel, false);
                }
                else
                {
                    // 적군 -> 적군 프리팹 사용, 오른쪽 패널에 넣기
                    itemObj = Instantiate(enemyItemPrefab);
                    itemObj.transform.SetParent(enemyStatusPanel, false);
                }

                // 3. 데이터 세팅 (스크립트는 똑같음)
                UnitStatusItem itemScript = itemObj.GetComponent<UnitStatusItem>();
                if (itemScript != null)
                {
                    itemScript.Setup(unit);
                }
            }
        }
    }

    // =========================================================
    // 조작 UI 전체 숨기기 (적 턴, 발사 중일 때 사용)
    // =========================================================
    public void DisablePlayerControls()
    {
        if (modeSwitchBtn != null) modeSwitchBtn.SetActive(false);
        if (aimJoystick != null) aimJoystick.SetActive(false);
        if (moveJoystick != null) moveJoystick.SetActive(false);
        if (skillJoystick != null) skillJoystick.SetActive(false);
    }

    // =========================================================
    // 조작 모드 설정 (켜기)
    // =========================================================
    public void SetJoystickMode(bool isAimingMode)
    {
        // 1. 모드 변경 버튼 활성화
        if (modeSwitchBtn != null) modeSwitchBtn.SetActive(true);
        if (skillJoystick != null) skillJoystick.SetActive(true);

        // 2. 모드에 따라 조이스틱 교체
        if (aimJoystick != null) aimJoystick.SetActive(isAimingMode);
        if (moveJoystick != null) moveJoystick.SetActive(!isAimingMode);
        
        // 3. 버튼 텍스트 변경 (현재 상태를 보여줌)
        // 조준 모드일 땐 -> "이동하기" 라고 써있어야 버튼 눌러서 이동하겠죠? 
        // 반대로 이동 모드일 땐 -> "조준하기"
        if (modeBtnText != null)
        {
            modeBtnText.text = isAimingMode ? "조준모드" : "이동모드";
        }
    }

    // =========================================================
    // 발사 중 (Firing) - 스킬 조이스틱만 남기고 나머지 숨김
    // =========================================================
    public void SetFiringMode()
    {
        // 이동/조준 관련은 다 끔
        if (modeSwitchBtn != null) modeSwitchBtn.SetActive(false);
        if (aimJoystick != null) aimJoystick.SetActive(false);
        if (moveJoystick != null) moveJoystick.SetActive(false);

        // ★ 스킬 조이스틱만 켜둠 (발사체 조작용)
        if (skillJoystick != null) skillJoystick.SetActive(true);
    }

    // "모드 변경" 버튼에 연결할 함수
    public void OnClickSwitchMode()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ToggleMoveMode();
        }
    }

    // 1. 턴이 바뀔 때 호출 (GameManager가 부름)
    public void UpdateTurnText(string name)
    {
        if (turnText != null)
        {
            turnPanel.SetActive(true);
            turnText.text = $"[ {name}의 차례 ]";
            Invoke("OffturnPanel", 3f);
            
            // (선택) 텍스트 애니메이션 등을 넣을 수 있음
        }
    }

    // 잔여 이동량 계산
    public void UpdateFuelUI(float current, float max)
    {
        if (fuelGaugeImage != null)
        {
            // 0 ~ 1 사이의 비율로 변환
            float ratio = current / max;
            fuelGaugeImage.fillAmount = ratio;
            
            // (선택) 연료가 거의 없으면 빨간색으로 깜빡이게 하거나 색 변경 가능
            if (ratio < 0.3f) fuelGaugeImage.color = Color.red;
            else fuelGaugeImage.color = Color.green; // 혹은 원래 색
        }
    }

    public void OffturnPanel()
    {
        turnPanel.SetActive(false);
    }

    // 2. 게임 종료 시 호출 (GameManager가 부름)
    public void ShowGameOver(bool isWin, int round)
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true); // 패널 켜기
            mainBack.SetActive(true);
            
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
                turnPanel.SetActive(false);
                aimJoystick.SetActive(false);
                moveJoystick.SetActive(false);
                modeSwitchBtn.SetActive(false);
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
        // 리스트 구조에 맞춰서 Clear() 사용
        if (GlobalDataManager.Instance != null)
        {
            GlobalDataManager.Instance.currentStage = null;
            
            // 기존 userCharacter = null; 방식 삭제
            GlobalDataManager.Instance.characterDeck.Clear(); // 덱 비우기
            GlobalDataManager.Instance.bagDeck.Clear();       // 가방 비우기
        }
        
        // 메뉴 씬 이름으로 변경하세요
        SceneManager.LoadScene("Main");
    }
}