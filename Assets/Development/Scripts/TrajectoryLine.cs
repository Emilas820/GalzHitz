using System.Collections.Generic;
using UnityEngine;

public class TrajectoryLine : MonoBehaviour
{
    [Header("설정")]
    public GameObject dotPrefab;   // ★ 점으로 쓸 프리팹 (Sprite)
    public int maxSteps = 30;      // 최대 점 개수
    public float timeStep = 0.1f; // 점 간격 (시간 단위)

    [Header("페이드 아웃")]
    public float fadeHeight = -7.5f; // 이 높이에서 끊기

    // 점들을 재사용하기 위해 모아두는 리스트 (메모리 절약)
    private List<GameObject> dotPool = new List<GameObject>();
    private GameObject dotsParent; // 하이러키 정리용 부모

    void Awake()
    {
        // 점들을 담아둘 폴더(부모) 생성
        dotsParent = new GameObject("DotPool");
        dotsParent.transform.SetParent(transform);
    }

    public void DrawSimulatedPath(Vector2 startPos, Vector2 startVelocity, float drag, float gravityScale)
    {
        Vector2 currentPos = startPos;
        Vector2 currentVel = startVelocity;
        Vector2 gravity = Physics2D.gravity * gravityScale;
        // 시뮬레이션 돌리기
        for (int i = 0; i < maxSteps; i++)
        {
            // --- 물리 계산 ---
            if (drag > 0) currentVel *= Mathf.Clamp01(1f - drag * timeStep);
            currentVel += gravity * timeStep;
            Vector2 nextPos = currentPos + (currentVel * timeStep);

            // --- 높이 체크 (바닥 닿으면 그만 그리기) ---
            if (nextPos.y < fadeHeight)
            {
                // 남은 점들은 모두 끄고 종료
                HideDots(i); 
                return;
            }

            // --- 점(Dot) 표시하기 ---
            GameObject dot = GetDot(i); // i번째 점 가져오기 (없으면 만듦)
            dot.SetActive(true);
            dot.transform.position = currentPos;

            // (선택) 점의 회전을 진행 방향으로 맞추기 (화살표 모양일 때 유용)
            // float angle = Mathf.Atan2(currentVel.y, currentVel.x) * Mathf.Rad2Deg;
            // dot.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

            currentPos = nextPos;
        }
        
        // 다 그렸는데 점이 남으면 끄기
        HideDots(maxSteps);
    }

    // 오브젝트 풀링: 점이 부족하면 만들고, 있으면 재사용
    GameObject GetDot(int index)
    {
        // 필요한데 리스트에 없으면 새로 생성
        if (index >= dotPool.Count)
        {
            GameObject newDot = Instantiate(dotPrefab, dotsParent.transform);
            dotPool.Add(newDot);
            return newDot;
        }
        
        // 이미 있으면 그거 리턴
        return dotPool[index];
    }

    // index 이후의 모든 점을 안 보이게 끔
    void HideDots(int startIndex)
    {
        for (int i = startIndex; i < dotPool.Count; i++)
        {
            if (dotPool[i].activeSelf) 
                dotPool[i].SetActive(false);
        }
    }

    public void ClearPath()
    {
        HideDots(0); // 모든 점 끄기
    }
}