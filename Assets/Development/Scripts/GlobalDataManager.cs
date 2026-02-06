using UnityEngine;

public class GlobalDataManager : MonoBehaviour
{
    public static GlobalDataManager Instance;

    [Header("선택된 데이터 (다음 씬으로 전달됨)")]
    public Characters userCharacter;    // 유저가 고른 캐릭터
    public BagData userBag;             // 유저가 고른 가방
    public StageData currentStage;      // 입장할 스테이지

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // ★ 씬이 바뀔 때 파괴되지 않음
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    // 로비(UI)에서 호출할 함수들
    public void SelectCharacter(Characters character)
    {
        userCharacter = character;
    }

    public void SelectBag(BagData bag)
    {
        userBag = bag;
    }

    public void SelectStage(StageData stage)
    {
        currentStage = stage;
    }
}