using UnityEngine;
using System.Collections.Generic;
using System;

// 버프 정보를 저장할 클래스
[System.Serializable]
public class Buff
{
    public string name;     // 버프 이름 (디버깅용)
    public ItemType type;   // 스탯 종류
    public float value;     // 증가량
    public int remainingTurns; // 남은 턴 수
    public Sprite icon;     // 아이콘

    public Buff(ItemData data)
    {
        this.name = data.itemName;
        this.type = data.itemType;
        this.value = data.rateValue;
        this.remainingTurns = data.duration;
        this.icon = data.StatusIcon;
    }
}

public class UnitStats : MonoBehaviour
{
    [Header("기본 스탯 (변하지 않는 원본)")]
    public float baseAtk;
    public float baseDef;
    public float baseSpd;

    [Header("현재 상태 (버프 포함)")]
    public string unitName;
    public int currentHp;
    public int maxHp;
    public float atk;   // 실제 적용 중인 공격력
    public float spd;   // 실제 적용 중인 속도
    public float def;   // 실제 적용 중인 방어력

    // ★ 현재 적용 중인 버프 목록 리스트
    public List<Buff> activeBuffs = new List<Buff>();

    public Action<int, int> onHpChanged;
    public Characters sourceData;

    public bool IsDead => currentHp <= 0;

    public void Setup(Characters data)
    {
        this.sourceData = data;
        this.unitName = data.characterName;
        this.maxHp = data.maxHp;
        this.currentHp = data.maxHp;

        // ★ 원본 스탯 저장 (중요!)
        this.baseAtk = data.atk;
        this.baseDef = data.def;
        this.baseSpd = data.spd;

        // 초기 스탯 계산
        RecalculateStats();
    }

    // =========================================================
    // ★ 1. 아이템 효과 적용 (ItemObject가 호출)
    // =========================================================
    public void ApplyItemEffect(ItemData item)
    {
        Debug.Log($"[{unitName}] 아이템 효과 적용: {item.itemName}");

        // 1. 즉발 효과 (힐) - 버프 리스트에 안 들어감
        if (item.itemType == ItemType.Heal)
        {
            Heal((int)item.rateValue);
            return;
        }
        
        // 2. 가방 교체 - 영구 지속
        if (item.itemType == ItemType.BagChange)
        {
            Player player = GetComponent<Player>();
            if (player != null && item.rateBagData != null)
            {
                player.myBags.Clear();
                player.myBags.Add(item.rateBagData);
                Debug.Log("가방 교체 완료!");
            }
            return;
        }

        // 3. 지속형 버프 (공격, 방어, 속도) - 리스트에 추가
        Buff newBuff = new Buff(item);
        activeBuffs.Add(newBuff);

        // 스탯 다시 계산 (베이스 + 버프)
        RecalculateStats();
    }

    // =========================================================
    // ★ 2. 턴 시작 시 지속시간 감소 (GameManager가 호출)
    // =========================================================
    public void OnTurnStart()
    {
        // 리스트를 거꾸로 돌면서 만료된 버프 제거
        for (int i = activeBuffs.Count - 1; i >= 0; i--)
        {
            activeBuffs[i].remainingTurns--; // 턴 차감

            if (activeBuffs[i].remainingTurns <= 0)
            {
                Debug.Log($"[{unitName}] 버프 종료: {activeBuffs[i].name}");
                activeBuffs.RemoveAt(i); // 리스트에서 삭제
            }
        }

        // 버프가 빠졌으니 스탯 재계산
        RecalculateStats();
    }

    // =========================================================
    // ★ 3. 스탯 재계산 (핵심 로직)
    // =========================================================
    void RecalculateStats()
    {
        // 1. 원본 스탯으로 초기화
        atk = baseAtk;
        def = baseDef;
        spd = baseSpd;

        // 2. 모든 버프 다시 더하기
        foreach (var buff in activeBuffs)
        {
            switch (buff.type)
            {
                case ItemType.AtkBuff: atk += buff.value; break;
                case ItemType.DefBuff: def += buff.value; break;
                case ItemType.SpdBuff: spd += buff.value; break;
            }
        }
        
        Debug.Log($"[{unitName}] 스탯 갱신 -> ATK:{atk}, DEF:{def}, SPD:{spd}");
    }

    // 기타 데미지/회복 함수
    public void Heal(int amount)
    {
        currentHp = Mathf.Min(currentHp + amount, maxHp);
        if (onHpChanged != null) onHpChanged(currentHp, maxHp);
        Debug.Log($"[{unitName}] 체력 {amount} 회복");
    }

    public void TakeDamage(int damage)
    {
        int finalDamage = Mathf.Max(1, (int)(damage - def));
        currentHp -= finalDamage;
        if (onHpChanged != null) onHpChanged(currentHp, maxHp);

        Debug.Log($"{unitName}이(가) {finalDamage}의 피해를 입음!");
        if (currentHp <= 0) Die();
    }

    void Die()
    {
        Debug.Log($"{unitName} 사망!");
        GetComponent<BattleUnit>().isDead = true;
    }
}