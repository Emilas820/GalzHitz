using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UnitStatusItem : MonoBehaviour
{
    [Header("UI 연결 (프리팹 안의 요소들을 연결하세요)")]
    public Image faceIcon;         // 얼굴
    public Image weaponIcon;       // 무기(가방)
    public TextMeshProUGUI nameText; // 이름
    public Image hpFillImage;      // 체력 게이지 (Filled Type)

    // 대상 유닛 정보 (이벤트 해제용)
    private UnitStats targetStats;

    public void Setup(BattleUnit unit)
    {
        targetStats = unit.stats;

        // 1. 텍스트 설정
        if (nameText != null) 
            nameText.text = targetStats.unitName;
        
        // 2. 얼굴 이미지 설정
        if (targetStats.sourceData != null && faceIcon != null)
            faceIcon.sprite = targetStats.sourceData.characterImage;

        // 3. 무기 이미지 설정
        if (weaponIcon != null && unit.playerScript != null && unit.playerScript.myBags.Count > 0)
        {
            if (unit.playerScript.myBags[0] != null)
                weaponIcon.sprite = unit.playerScript.myBags[0].bagIcon;
        }

        // 4. HP바 초기화 및 이벤트 연결
        UpdateHP(targetStats.currentHp, targetStats.maxHp);
        targetStats.onHpChanged += UpdateHP;

        // ★ 순서 변경 로직(SetAsLastSibling 등)은 모두 삭제했습니다!
        // 프리팹을 만들 때 이미 배치를 끝내놨기 때문입니다.
    }

    void UpdateHP(int current, int max)
    {
        if (hpFillImage != null)
        {
            // (float) 캐스팅 필수
            hpFillImage.fillAmount = (float)current / max;
        }
    }

    void OnDestroy()
    {
        if (targetStats != null)
            targetStats.onHpChanged -= UpdateHP;
    }
}