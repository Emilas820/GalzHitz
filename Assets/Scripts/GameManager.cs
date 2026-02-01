using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
    }

    // 1. 턴 지정 메소드

    // 2. 현재 턴 유저 각도, 파워 값 받기 + 값 받기 완료했다는 신호 쏘기
}