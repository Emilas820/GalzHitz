using System.Collections.Generic;
using UnityEngine;

// 인스펙터에서 보기 좋게 묶어주는 클래스
[System.Serializable]
public class EnemySpawnInfo
{
    public Characters character; // 등장할 적 캐릭터
    public BagData customBag;    // 쥐어줄 가방 (비워두면 캐릭터 기본 가방 사용)
}

[CreateAssetMenu(fileName = "New Stage", menuName = "Game Data/Stage Data")]
public class StageData : ScriptableObject
{
    [Header("스테이지 정보")]
    public string stageName;
    public string sceneName;

    [Header("등장하는 적들 (캐릭터 + 가방)")]
    // 기존: public List<Characters> enemies; (삭제)
    
    // 변경: 캐릭터와 가방 정보를 함께 담는 리스트
    public List<EnemySpawnInfo> enemySpawns; 
}