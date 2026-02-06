using UnityEngine;

public enum Team { Player, Enemy }

public class BattleUnit : MonoBehaviour
{
    [Header("팀 구분")]
    public Team myTeam;

    [Header("현재 스텟")]
    public UnitStats stats;

    [Header("참조")]
    public Player playerScript;


    [Header("AI")]
    // ★ AI 두뇌 (이게 붙어있으면 AI, 없으면 사람)
    public AIController aiBrain;
    // 편의 속성
    public bool IsAI => aiBrain != null;

    // 이 유닛이 죽었는지 확인하는 변수
    public bool isDead = false;

    void Awake()
    {
        stats = GetComponent<UnitStats>();
        // 내 몸체(Player) 찾기
        playerScript = GetComponent<Player>();
        // 내 두뇌(AI) 찾기 (없을 수도 있음)
        aiBrain = GetComponent<AIController>();
    }
}