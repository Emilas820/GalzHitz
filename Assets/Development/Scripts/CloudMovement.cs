using UnityEngine;

public class DistanceMover : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;   // 이동 속도
    [SerializeField] private float maxDistance = 10f; // 이동할 최대 거리
    
    private Vector3 _initialPosition; // 시작 위치

    void Start()
    {
        // 시작 시 현재 위치 저장
        _initialPosition = transform.position;
    }

    void Update()
    {
        // 1. 현재 이동한 거리 계산 (시작점과 현재점 사이)
        float currentDistance = Vector3.Distance(_initialPosition, transform.position);

        // 2. 최대 거리에 도달했는지 확인
        if (currentDistance < maxDistance)
        {
            // 아직 도달 전이면 x+ 방향으로 이동
            transform.Translate(Vector3.right * (moveSpeed * Time.deltaTime));
        }
        else
        {
            // 3. 범위를 벗어나면 즉시 초기 위치로 복귀
            ResetPosition();
        }
    }

    private void ResetPosition()
    {
        transform.position = _initialPosition;
    }
}