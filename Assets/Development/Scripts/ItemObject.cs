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