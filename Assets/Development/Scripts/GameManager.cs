using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 스테이트 머신 - 턴 관리
public enum TurnState
{
    Init,           // 1. 게임 초기화 (데이터 로드)
    TurnStart,      // 2. 턴 시작 연출 (카메라 이동, 바람 적용)
    PlayerAiming,   // 3. 플레이어 조준 (입력 대기)
    EnemyAiming,    // 3. 적 AI 생각 중
    Firing,         // 4. 발사 및 궤적 확인 (조작 불가)
    Resolution,     // 5. 결과 판정 (데미지, 승패 체크)
    GameOver        // 6. 게임 종료
}

public class GameManager : MonoBehaviour
{

    [Header("싱글톤")]
    public static GameManager Instance;

    [Header("턴 관리")]
    public BattleUnit currentActor;
    public TurnState currentState; // 스테이트 머신
    public List<BattleUnit> allUnits = new List<BattleUnit>(); // 전체 참가자 명단
    public int turnIndex = -1; // 현재 순번 (-1로 시작)
    
    [Header("연결")]
    public Player player; // 플레이어 캐릭터

    void Start()
    {
        SetState(TurnState.Init);
    }

    void Awake()
    {
        if(Instance == null) Instance = this;
    }

    public void SetState(TurnState newState)
    {
        if (currentState == newState) return; // 동일 상태인 경우 무시

        Debug.Log($"상태 변경: {currentState} -> {newState}");  // 확인

        currentState = newState;    // 상태 지정
        
        switch (currentState)
        {
            case TurnState.Init:
                // 1. 게임 초기화 (데이터 로드)
                break;
            case TurnState.TurnStart:
                // 2. 턴 시작 연출 (카메라 이동, 바람 적용)
                break;
            case TurnState.PlayerAiming:
                // 3. 플레이어 조준 (입력 대기)
                break;
            case TurnState.EnemyAiming:
                // 3. 적 AI 생각 중
                break;
            case TurnState.Firing:
                // 4. 발사 및 궤적 확인 (조작 불가)
                break;
            case TurnState.Resolution:
                // 5. 결과 판정 (데미지, 승패 체크)
                break;
            case TurnState.GameOver:
                // 6. 게임 종료
                break;
        }
    }

    IEnumerator SetupBattle()
    {
        Debug.Log("전투 시작!");
        yield return new WaitForSeconds(1f);
        SetState(TurnState.PlayerAiming);
    }

    // 조이스틱이 드래그 중일 때 계속 호출함
    public void UpdateInput(float angleRatio, float powerRatio)
    {
        if (currentState != TurnState.PlayerAiming) return;   // 플레이어 턴이 아니라면 불가능
        if (player != null) // 플레이어 오브젝트가 있다면
        {
            // 플레이어에게 "조준해!" 명령
            player.UpdateAim(angleRatio, powerRatio);
        }
    }

    // 조이스틱을 놓았을 때 호출함
    public void RequestThrow()
    {
        if (currentState != TurnState.PlayerAiming) return;   // 플레이어 턴이 아니라면 불가능
        
        SetState(TurnState.Firing); // 던지는 순간 Action 상태로 전환
        player.Throw();
    }
}