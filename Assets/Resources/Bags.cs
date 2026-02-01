using UnityEngine;

public enum BagType
{
    Normal,     // 기본
    Area,       // 범위
    Pierce,     // 관통
    Trap,       // 트랩
    MultiHit    // 다단 히트
}

[CreateAssetMenu(fileName = "New Bag", menuName = "Bag/BagData")]
public class BagData : ScriptableObject
{
    public string bagName;      // 가방 이름
    public GameObject bagPrefab;    // 실제 가방 프리팹
    public float damage;         // 공격력
    public BagType type;         // 투사체 속성
    // 추가로 필요한 속성들 (관통 횟수, 트랩 수명 등)
}
