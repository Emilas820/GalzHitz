using UnityEngine;

public enum Team { Player, Enemy }

public class BattleUnit : MonoBehaviour
{
    [Header("팀 구분")]
    public Team myTeam;

    [Header("현재 스텟")]
    public UnitStats stats;

    [Header("참조")]
    public Player playerScript; // 플레이어라면 연결
    public Enemy enemyScript;

    // 이 유닛이 죽었는지 확인하는 변수
    public bool isDead = false;

    void Awake()
    {
        // 시작할 때 자동으로 찾아두기
        stats = GetComponent<UnitStats>();
    }
}