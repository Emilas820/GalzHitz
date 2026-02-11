using UnityEngine;

// [방법 2] 수동형 아이템 (가만히 있고, 플레이어가 집어감)
public class ItemObject : MonoBehaviour
{
    [Header("데이터")]
    public ItemData data; // 구조물이 꽂아준 데이터

    // 구조물이 생성 직후 호출함
    public void Setup(ItemData targetData)
    {
        this.data = targetData;
    }

    // ★ [수정] Trigger -> Collision으로 변경
    // Is Trigger를 껐으므로, 이제 "충돌(Collision)" 이벤트를 받아야 합니다.
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Collision2D에서는 .gameObject를 통해 접근해야 합니다.
        BattleUnit unit = collision.gameObject.GetComponent<BattleUnit>();

        // 유닛이 맞고 + 살아있고 + 플레이어 팀일 때만 획득
        if (unit != null && !unit.isDead && unit.myTeam == Team.Player)
        {
            Collect(unit);
        }
    }

    // 플레이어가(Player.cs) 호출할 함수
    public void Collect(BattleUnit unit)
    {
        if (data != null && unit.stats != null)
        {
            Debug.Log($"[아이템 획득] {unit.name} -> {data.itemName}");
            unit.stats.ApplyItemEffect(data); // 효과 적용
            
            // (선택) 획득 효과음 재생
            // AudioManager.Instance.PlaySfx("ItemGet");

            Destroy(gameObject); // 삭제
        }
    }
}