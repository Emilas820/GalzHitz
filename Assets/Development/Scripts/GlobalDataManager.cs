using UnityEngine;
using System.Collections.Generic;

public class GlobalDataManager : MonoBehaviour
{
    public static GlobalDataManager Instance;

    [Header("선택된 데이터 (다음 씬으로 전달됨)")]
    // public Characters userCharacter;    // 유저가 고른 캐릭터
    // public BagData userBag;             // 유저가 고른 가방

    [Header("출전 명단 (인덱스끼리 매칭됨)")]
    // 0번 캐릭터는 0번 가방을 듭니다.
    public List<Characters> characterDeck = new List<Characters>(); 
    public List<BagData> bagDeck = new List<BagData>();

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
    
    // // 로비(UI)에서 호출할 함수들
    // public void SelectCharacter(Characters character)
    // {
    //     userCharacter = character;
    //     Debug.Log($"{character.characterName}선택");
    // }

    // public void SelectBag(BagData bag)
    // {
    //     userBag = bag;
    //     Debug.Log($"{bag.bagName}선택");
    // }

    // (참고) 로비에서 리스트에 추가하는 함수 예시
    public void AddCharacter(Characters charData)
    {
        characterDeck.Add(charData);
        Debug.Log($"{charData.characterName}선택");
    }
    public void AddBag(BagData bagData)
    {
        bagDeck.Add(bagData);
        Debug.Log($"{bagData.bagName}선택");
    }

    public void SelectStage(StageData stage)
    {
        currentStage = stage;
        Debug.Log($"{stage.stageName}선택");
    }

    public void ClearStage()
    {
        currentStage = null;
        Debug.Log("선택 해제");
    }
    public void ClearDeck()
    {
        characterDeck.Clear();
        bagDeck.Clear();
        Debug.Log("출전 명단(덱) 초기화 완료");
    }
}