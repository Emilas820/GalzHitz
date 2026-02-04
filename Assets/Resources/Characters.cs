using UnityEngine;

[CreateAssetMenu(fileName = "New Character", menuName = "Game Data/Character Data")]
public class Characters : ScriptableObject
{
    [Header("기본 정보")]
    public string characterName;  // 캐릭터 이름
    public GameObject prefab;     // 캐릭터 프리팹 (외형)

    [Header("스탯")]
    public int maxHp;      // 최대 체력
    public float atk;      // 공격력 보정치
    public float def;      // 방어력
    public float spd;      // 이동 속도 (필요 시)

    [Header("전용 장비")]
    public BagData defaultBag; // 캐릭터와 매칭되는 가방 (전무)

    

}