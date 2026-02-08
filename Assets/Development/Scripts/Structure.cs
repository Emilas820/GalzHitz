using UnityEngine;
using UnityEngine.UI;

public class Structure : MonoBehaviour
{
    [Header("구조물 정보")]
    public string structureName = "Wooden Box";
    public int maxHp = 30;
    public int currentHp;

    [Header("보상")]
    [Tooltip("떨어뜨릴 아이템의 '데이터'를 연결하세요 (프리팹 X)")]
    public ItemData dropItemData; // ★ 변경: GameObject -> ItemData

    [Header("스테이터스 UI")]
    public GameObject StatusPanel;
    public Image hpFillImage;

    void Start()
    {
        currentHp = maxHp;
        StatusPanel.SetActive(false);
    }

    // Bag.cs에서 호출할 데미지 함수
    public void TakeDamage(float damage)
    {
        StatusPanel.SetActive(true);
        currentHp -= (int)damage;
        hpFillImage.fillAmount = (float)currentHp / maxHp;
        Debug.Log($"[구조물] {structureName} 피격! 남은 HP: {currentHp}");

        // (선택) 피격 시 흔들림이나 색상 변경 연출 추가 가능

        if (currentHp <= 0)
        {
            Break();
        }
    }

    void Break()
    {
        Debug.Log($"[구조물] {structureName} 파괴됨!");

        // ★ 아이템 생성 및 데이터 주입 로직
        if (dropItemData != null && dropItemData.itemPrefab != null)
        {
            // 1. 데이터에 연결된 껍데기(Prefab)를 생성
            GameObject itemObj = Instantiate(dropItemData.itemPrefab, transform.position, Quaternion.identity);
            
            // 2. 데이터 주입
            ItemObject itemScript = itemObj.GetComponent<ItemObject>();
            if (itemScript != null)
            {
                itemScript.Setup(dropItemData);
            }
        }

        Destroy(gameObject);
    }
}