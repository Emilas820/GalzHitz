using UnityEngine;

public class Bag : MonoBehaviour
{
    private float finalDamage;
    private Team shooterTeam; // ★ 발사한 사람의 팀 정보
    public GameObject damageEffect;

    void Start()
    {
        Invoke("ForceDestroy", 7.0f);
    }

    // Setup 함수에 team 정보 추가
    public void Setup(float damageAmount, Team team) 
    {
        this.finalDamage = damageAmount;
        this.shooterTeam = team; // 팀 정보 저장
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.tag == "Ground")
        return;

        else
        {
            // 1. 부딪힌 대상의 BattleUnit(명찰)을 확인
        BattleUnit targetUnit = collision.gameObject.GetComponent<BattleUnit>();

        // 대상이 전투 유닛(캐릭터)이라면?
        if (targetUnit != null)
        {
            // 만약 발사한 팀과 피격자의 팀이 같다면? (아군 오폭)
            if (targetUnit.myTeam == this.shooterTeam)
            {
                Debug.Log("아군입니다! 데미지 무효.");
                // 여기서 return 하면 데미지 안 주고 끝냄 (폭발 이펙트는 나올 수 있음)
                GameManager.Instance.OnActionComplete();
                return;
            }
        }

        // 2. 적군이라면 데미지 처리
        UnitStats targetStats = collision.gameObject.GetComponent<UnitStats>();
        if (targetStats != null)
        {
            targetStats.TakeDamage((int)finalDamage);
        }

        GameObject ef = Instantiate(damageEffect, transform.position, Quaternion.identity);
        GameManager.Instance.OnActionComplete();
        Destroy(ef, 1f);
        Destroy(gameObject);
        }        
    }

    void ForceDestroy()
    {
        if (gameObject != null) // 이미 터졌으면 무시
        {
            if (GameManager.Instance != null) 
                GameManager.Instance.OnActionComplete();
            
            Destroy(gameObject);
        }
    }
}