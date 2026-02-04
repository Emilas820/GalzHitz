using UnityEngine;

public class UnitStats : MonoBehaviour
{
    // 실제 게임 도중 변하는 값들
    [Header("현재 상태")]
    public string unitName;
    public int currentHp;
    public int maxHp;
    public float atk;
    public float spd;
    public float def;

    // 죽었는지 확인
    public bool IsDead => currentHp <= 0;

    // ★ 핵심: 데이터 파일(CharacterData)을 받아서 내 능력치로 세팅하는 함수
    public void Setup(Characters data)
    {
        this.unitName = data.characterName;
        this.maxHp = data.maxHp;
        this.currentHp = data.maxHp; // 시작할 땐 풀피
        this.atk = data.atk;
        this.def = data.def;

        this.spd = Random.Range(10, 20); // (임시 테스트용 랜덤 속도)

        Debug.Log($"[{unitName}] 스탯 세팅 완료! HP: {currentHp}");
    }

    // 데미지 받는 함수 (나중에 쓸 예정)
    public void TakeDamage(int damage)
    {
        // 방어력 계산 공식 (예: 방어력만큼 뎀감)
        int finalDamage = Mathf.Max(1, (int)(damage - def));
        currentHp -= finalDamage;

        Debug.Log($"{unitName}이(가) {finalDamage}의 피해를 입음! 남은 체력: {currentHp}");

        if (currentHp <= 0) Die();
    }

    void Die()
    {
        Debug.Log($"{unitName} 사망!");
        // BattleUnit에 사망 알림 보내기 등 처리
        GetComponent<BattleUnit>().isDead = true;
    }
}