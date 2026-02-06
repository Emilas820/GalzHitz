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

    // 싱글톤 준비운동
    void Awake()
    {
        if(Instance == null) Instance = this;
    }

    // 스테이트 머신 시작
    void Start()
    {
        SetState(TurnState.Init);
    }

    // =========================================================
    // 0. 유닛 생성
    // =========================================================
    // ★ 유닛 생성 전용 함수 (저번에 만든 로직 활용)
    void SpawnUnit(Characters data, Transform spawnPoint, Team team)
    {
        // 프리팹 생성
        GameObject obj = Instantiate(data.prefab, spawnPoint.position, Quaternion.identity);
        
        // 컴포넌트 세팅
        BattleUnit unit = obj.GetComponent<BattleUnit>();
        UnitStats stats = obj.GetComponent<UnitStats>();
        
        if (stats != null) stats.Setup(data);
        if (unit != null) unit.myTeam = team;

        // 적군이면 AI 두뇌 장착!
        if (team == Team.Enemy)
        {
            AIController ai = obj.AddComponent<AIController>();
            unit.aiBrain = ai;
            // ai.errorRange = 3.0f; // 필요시 난이도 조절
        }

        allUnits.Add(unit);
    }

    // =========================================================
    // 1. 상태 관리 (State Machine)
    // =========================================================
    public void SetState(TurnState newState)
    {
        if (currentState == newState) return; // 동일 상태인 경우 무시

        Debug.Log($"상태 변경: {currentState} -> {newState}");  // 확인

        currentState = newState;    // 상태 지정
        
        switch (currentState)
        {
            case TurnState.Init:
                StartCoroutine(SetupBattle());
                break;
            case TurnState.TurnStart:
                StartCoroutine(TurnStartProcess());
                break;
            case TurnState.PlayerAiming:
                // UI 켜기 등의 로직이 필요하다면 여기에 작성
                Debug.Log(">> 플레이어 조준 시작 (조이스틱 활성화)");
                break;
            case TurnState.EnemyAiming:
                break;
            case TurnState.Firing:
                // 발사 및 궤적 확인 (조작 불가)
                break;
            case TurnState.Resolution:
                StartCoroutine(ResolutionProcess());
                break;
            case TurnState.GameOver:
                // 게임 종료
                Debug.Log("게임 종료!");
                break;
        }
    }

    // =========================================================
    // 2. 주요 흐름 코루틴
    // =========================================================

    IEnumerator SetupBattle()
    {
        allUnits.Clear();

        // 1. 데이터 가져오기 (배달부와 표지판 찾기)
        GlobalDataManager globalData = GlobalDataManager.Instance;
        StageSetup mapInfo = StageSetup.Instance;

        // 예외 처리: 데이터가 없으면 테스트 모드로 간주하거나 중단
        if (globalData == null || mapInfo == null)
        {
            Debug.LogError("글로벌 데이터나 스테이지 셋업이 없습니다! (테스트 씬인가요?)");
            // 테스트를 위해 기존 Find 로직을 남겨두거나 yield break;
            yield break; 
        }

        // 2. 플레이어 생성 (아군)
        if (globalData.userCharacter != null)
        {
            SpawnUnit(globalData.userCharacter, mapInfo.playerSpawnPoint, Team.Player);
            BattleUnit playerUnit = allUnits[allUnits.Count - 1];   // 가방을 쥐어줌
        }
        else
        {
            Debug.LogError("선택된 플레이어 캐릭터가 없습니다!");
        }

        // 3. 적군 생성 (수정됨: enemies -> enemySpawns)
        StageData stageInfo = globalData.currentStage;
        if (stageInfo != null)
        {
            // ★ 수정 포인트 1: 리스트 이름 변경 (enemies -> enemySpawns)
            for (int i = 0; i < stageInfo.enemySpawns.Count; i++)
            {
                // 데이터 하나 꺼내기 (캐릭터 + 커스텀 가방 정보가 들어있음)
                EnemySpawnInfo info = stageInfo.enemySpawns[i];
                
                // 스폰 위치 잡기
                Transform spawnPos = mapInfo.enemySpawnPoints[i % mapInfo.enemySpawnPoints.Count];
                
                // ★ 수정 포인트 2: 그냥 리스트 요소가 아니라, .character를 꺼내서 생성
                SpawnUnit(info.character, spawnPos, Team.Enemy);
                
                // 생성된 적 유닛 가져오기 (방금 만들어서 리스트 맨 뒤에 있음)
                BattleUnit enemyUnit = allUnits[allUnits.Count - 1];

                // ★ 수정 포인트 3: 가방 쥐어주기 로직 추가
                enemyUnit.playerScript.myBags.Clear(); // 기존 가방 비우기

                if (info.customBag != null)
                {
                    // 스테이지 설정에 '전용 가방'이 있으면 그거 장착
                    enemyUnit.playerScript.myBags.Add(info.customBag);
                }
                else if (info.character.defaultBag != null)
                {
                    // 없으면 캐릭터의 '기본 가방' 장착
                    enemyUnit.playerScript.myBags.Add(info.character.defaultBag);
                }
            }
        }

        yield return null; // 1프레임 대기

        // 4. 턴 순서 정렬 (기존 로직)
        allUnits.Sort((a, b) => {
            if (a.stats == null || b.stats == null) return 0;
            if (a.stats.spd > b.stats.spd) return -1;
            else if (a.stats.spd < b.stats.spd) return 1;
            return 0;
        });

        // 정렬 로그
        string log = "턴 순서: ";
        foreach (var unit in allUnits) log += $"{unit.name}(SPD:{unit.stats.spd}) -> ";
        Debug.Log(log);

        yield return new WaitForSeconds(1f);
        StartNextTurn(); 
    }

    

    // [TurnStart] 카메라 이동 및 상태 분기
    IEnumerator TurnStartProcess()
    {
        Debug.Log($"====== [ 턴 시작: {currentActor.name} ] ======");

        if (CameraManager.Instance != null)
            CameraManager.Instance.SetTarget(currentActor.transform);

        yield return new WaitForSeconds(1.0f);

        // ★ 수정된 로직: 팀이 아니라 "AI 여부"로 판단
        if (currentActor.IsAI)
        {
            // AI 두뇌 가동
            SetState(TurnState.EnemyAiming);
            currentActor.aiBrain.StartAITurn(); 
        }
        else
        {
            // 사람 입력 대기
            SetState(TurnState.PlayerAiming);
        }
    }

    // [Resolution] 결과 확인 후 다음 턴으로
    IEnumerator ResolutionProcess()
    {
        yield return new WaitForSeconds(1.5f); // 폭발 연출 대기

        // 승패 조건 체크 (나중에 추가)
        // if (CheckGameEnd()) SetState(TurnState.GameOver);
        // else
        
        StartNextTurn(); // 다음 사람 호출
    }

    // =========================================================
    // 3. 턴 순환 엔진 (가장 중요!)
    // =========================================================
    public void StartNextTurn()
    {
        turnIndex++;
        
        // 리스트 끝에 도달하면 처음(0번)으로 돌아감
        if (turnIndex >= allUnits.Count) turnIndex = 0;

        currentActor = allUnits[turnIndex];

        // 만약 죽은 유닛이면 건너뛰기 (재귀 호출)
        if (currentActor.isDead)
        {
            StartNextTurn();
            return;
        }

        // 상태를 'TurnStart'로 바꾸면 위의 TurnStartProcess가 실행됨
        SetState(TurnState.TurnStart);
    }


    // 외부(Bag.cs)에서 호출하는 턴 종료 신호
    public void OnActionComplete()
    {
        if (currentState == TurnState.Firing || currentState == TurnState.EnemyAiming)
        {
            SetState(TurnState.Resolution);
        }
    }

    // =========================================================
    // 4. 입력 처리 (수정됨)
    // =========================================================

    // 조이스틱이 드래그 중일 때 계속 호출함
    public void UpdateInput(float angleRatio, float powerRatio)
    {
        if (currentState != TurnState.PlayerAiming) return;
        
        // player 변수 대신 currentActor를 사용해야 순서대로 조작 가능
        if (currentActor != null && currentActor.playerScript != null)
        {
            currentActor.playerScript.UpdateAim(angleRatio, powerRatio);
        }
    }

    public void RequestThrow()
    {
        if (currentState != TurnState.PlayerAiming) return;
        if (currentActor == null || currentActor.playerScript == null) return;

        SetState(TurnState.Firing); 
        currentActor.playerScript.Throw();
    }
}