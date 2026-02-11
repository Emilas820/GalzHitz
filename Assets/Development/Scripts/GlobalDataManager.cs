using UnityEngine;
using System.Collections.Generic;

public class GlobalDataManager : MonoBehaviour
{
    public static GlobalDataManager Instance;

    [Header("출전 명단 (인덱스끼리 매칭됨)")]
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

    public void SelectStage(StageData stage)
    {
        currentStage = stage;
        Debug.Log($"{stage.stageName}선택");
    }

        public void AddCharacter(Characters charData)
    {
        characterDeck.Add(charData);
        Debug.Log($"{charData.characterName}출전");
    }
    public void AddBag(BagData bagData)
    {
        bagDeck.Add(bagData);
        Debug.Log($"{bagData.bagName}출전");
    }

    public void ClearData()
    {
        characterDeck.Clear();
        bagDeck.Clear();
        currentStage = null;
        Debug.Log("[Global] 이전 게임 데이터 초기화 완료");
    }
}