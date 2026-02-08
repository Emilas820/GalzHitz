using UnityEngine;

// 아이템의 종류를 정의
public enum ItemType
{
    Heal,           // HP 회복 (즉발))
    AtkBuff,        // 공격력 상승 (지속)
    DefBuff,        // 방어력 상승 (지속)
    SpdBuff,        // 이동력/속도 상승 (지속)
    BagChange       // 가방 교체(강화) (지속)
}

[CreateAssetMenu(fileName = "New Item", menuName = "Game Data/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("기본 정보 (Name, Prefab)")]
    public string itemName;         // 아이템 이름
    public GameObject itemPrefab;   // 씬에 떨어질 아이템 프리팹 (획득 전 껍데기)
    public Sprite StatusIcon;       // 적용 버프 아이콘

    [Header("효과 타입 (Type)")]
    public ItemType itemType;       // 아이템 속성 정의

    [Header("효과 수치 (Rate) - 타입에 맞춰 사용")]
    [Tooltip("회복량, 공격력 증가량 등 '숫자'가 필요할 때 사용")]
    public float rateValue;         

    [Tooltip("가방 교체(BagChange) 타입일 때만 '가방 데이터'를 연결")]
    public BagData rateBagData;     

    [Header("지속 시간 (Duration)")]
    [Tooltip("0 = 즉시 사용(1회성), 1 이상 = 해당 턴만큼 지속")]
    public int duration;            
}