using System.Collections.Generic;
using UnityEngine;

public class StageSetup : MonoBehaviour
{
    [Header("스폰 위치 지정")]
    public Transform playerSpawnPoint;      // 아군 스폰 위치
    public List<Transform> enemySpawnPoints; // 적군 스폰 위치들 (여러 개)

    // GameManager가 이 정보를 가져가기 쉽게 싱글톤처럼 접근
    public static StageSetup Instance;

    void Awake()
    {
        Instance = this;
    }
}