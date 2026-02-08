using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 스테이트 머신 - 턴 관리
public enum TurnState
{
    None,           // 0. 초기 진입
    Init,           // 1. 게임 초기화 (데이터 로드)
    TurnStart,      // 2. 턴 시작 연출 (카메라 이동, 바람 적용)
    PlayerAiming,   // 3. 플레이어 조준 (입력 대기)
    PlayerMoving,   // 3. 플레이어 이동
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
    public int currentRound = 1;    // 턴 카운트
    public BattleUnit currentActor;
    public TurnState currentState; // 스테이트 머신
    public List<BattleUnit> allUnits = new List<BattleUnit>(); // 전체 참가자 명단
    public int turnIndex = -1; // 현재 순번 (-1로 시작)

    public bool hasMovedThisTurn = false;       // 실제로 이동했는가?

    [Header("스킬 시스템")]
    public float slowMotionFactor = 0.2f; // 슬로우 모션 배율 (0.2 = 5배 느려짐)
    public float slowMotionDuration = 0.0f; // 현재 슬로우 모션 중인지 체크용

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
                // 플레이어 조준 턴 -> 조준 조이스틱 & 버튼 켜기
                Debug.Log(">> 플레이어 조준 시작");
                if (UIManager.Instance != null) UIManager.Instance.SetJoystickMode(true);
                break;
            case TurnState.PlayerMoving:
                // 플레이어 이동 턴 -> 이동 조이스틱 & 버튼 켜기
                if (UIManager.Instance != null) UIManager.Instance.SetJoystickMode(false);
                break;
            case TurnState.EnemyAiming:
                // 적 턴 -> UI 끄기
                if (UIManager.Instance != null) UIManager.Instance.DisablePlayerControls();
                break;
            case TurnState.Firing:
                // 발사 중: 스킬 UI만 남기고 나머지는 숨김
                if (UIManager.Instance != null) UIManager.Instance.SetFiringMode();
                break;
            case TurnState.Resolution:
                // 결과 판정 중 -> UI 끄기
                if (UIManager.Instance != null) UIManager.Instance.DisablePlayerControls();
                StartCoroutine(ResolutionProcess());
                break;
            case TurnState.GameOver:
                // 게임 종료 -> UI 끄기
                if (UIManager.Instance != null) UIManager.Instance.DisablePlayerControls();
                Debug.Log("게임 종료!");
                break;
        }
    }

    // =========================================================
    // 2. 주요 흐름 코루틴
    // =========================================================

    IEnumerator SetupBattle()
    {
        Debug.Log(">>> [1] 전투 설정 시작 (SetupBattle 진입)");
        allUnits.Clear();

        // 1. 데이터 가져오기 (배달부와 표지판 찾기)
        GlobalDataManager globalData = GlobalDataManager.Instance;
        StageSetup mapInfo = StageSetup.Instance;

        // ★ [수정] 데이터가 없거나(OR) 덱이 비어있을 때 예외 처리
        if (globalData == null || globalData.characterDeck.Count == 0)
        {
            Debug.LogError("글로벌 데이터나 캐릭터 덱이 없습니다!");
            
            // 테스트용 임시 데이터를 여기서 생성하거나, 그냥 종료
            yield break; 
        }

        // 2. 플레이어 생성 (아군)
        if (globalData != null && globalData.characterDeck.Count > 0)
        {
            // 리스트에 있는 만큼 반복 (최대 스폰 포인트 개수만큼)
            int count = Mathf.Min(globalData.characterDeck.Count, 1); 
            // ※ 일단 1명만 소환한다면 1, 여러명이면 spawnPoints 리스트 필요

            for (int i = 0; i < count; i++)
            {
                // A. 캐릭터 소환
                Characters charData = globalData.characterDeck[i];
                SpawnUnit(charData, mapInfo.playerSpawnPoint, Team.Player);
                
                // B. 방금 소환된 유닛 가져오기
                BattleUnit newUnit = allUnits[allUnits.Count - 1];

                // C. 짝이 되는 가방 찾아서 쥐어주기
                if (i < globalData.bagDeck.Count && globalData.bagDeck[i] != null)
                {
                    BagData bagToEquip = globalData.bagDeck[i];
                    
                    newUnit.playerScript.myBags.Clear(); // 기본 가방 비우고
                    newUnit.playerScript.myBags.Add(bagToEquip); // 선택한 가방 장착
                }
            }
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

        // 턴 순서 정렬
        SortUnitsBySpeed();

        // 정렬 로그
        string log = "턴 순서: ";
        foreach (var unit in allUnits) log += $"{unit.name}(SPD:{unit.stats.spd}) -> ";
        Debug.Log(log);

        // 유닛 배치가 끝났으니 UI 생성 요청
        if (UIManager.Instance != null)
        {
            UIManager.Instance.GenerateUnitStatusUI();
        }

        yield return new WaitForSeconds(1f);
        StartNextTurn(); 
    }

    // 정렬 함수 분리
    void SortUnitsBySpeed()
    {
        allUnits.Sort((a, b) => {
            if (a.stats == null || b.stats == null) return 0;
            // 내림차순 (SPD 높은 사람이 먼저)
            if (a.stats.spd > b.stats.spd) return -1;
            else if (a.stats.spd < b.stats.spd) return 1;
            return 0;
        });
        
        string log = $"[Round {currentRound} 순서] ";
        foreach (var unit in allUnits) log += $"{unit.name}({unit.stats.spd}) -> ";
        Debug.Log(log);
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

    // [추가] 살아있는 유닛 수를 세서 게임 종료 여부 판단
    // 리턴값: 0=계속, 1=플레이어 승, 2=적 승(플레이어 패)
    int CheckGameEnd()
    {
        int alivePlayers = 0;
        int aliveEnemies = 0;

        foreach (var unit in allUnits)
        {
            if (unit.isDead) continue; // 죽은 놈은 카운트 X

            if (unit.myTeam == Team.Player) alivePlayers++;
            else if (unit.myTeam == Team.Enemy) aliveEnemies++;
        }

        // 1. 내가 다 죽었으면 -> 패배
        if (alivePlayers == 0) return 2;
        
        // 2. 적이 다 죽었으면 -> 승리
        if (aliveEnemies == 0) return 1;

        // 3. 둘 다 살아있으면 -> 계속 진행
        return 0; 
    }

    // [Resolution] 결과 확인 후 다음 턴으로
    IEnumerator ResolutionProcess()
        {
            // 각종 연출 대기
            yield return new WaitForSeconds(3.0f);
            // 작동 중지 신호 받는 방식으로 차후 변경

            // 승패 판정 (심판의 역할)
            int checkResult = CheckGameEnd(); // 0:진행중, 1:승리, 2:패배

            if (checkResult != 0) 
            {
                // 게임 종료!
                SetState(TurnState.GameOver);
                
                bool isWin = (checkResult == 1);
                if (UIManager.Instance != null)
                {
                    Debug.Log("결과 보냅니당");
                    UIManager.Instance.ShowGameOver(isWin, currentRound);
                }
            }
            else
            {
                // 아직 안 끝났으면 다음 턴으로
                StartNextTurn();
            }
        }

    // =========================================================
    // 3. 턴 순환 엔진 (가장 중요!)
    // =========================================================
    public void StartNextTurn()
    {
        turnIndex++;
        
        // 리스트 끝에 도달하면 처음(0번)으로 돌아감
        if (turnIndex >= allUnits.Count)
        {
            currentRound++;
            turnIndex = 0;
            Debug.Log($"=== [ Round {currentRound} 시작 ] ===");

            SortUnitsBySpeed();
        }

        currentActor = allUnits[turnIndex];

        hasMovedThisTurn = false;
        // 플레이어라면 연료 리필 (Player.cs에 함수 추가 예정)
        if (currentActor.playerScript != null)
        {
            currentActor.playerScript.ResetTurnData(); 
        }

        // 만약 죽은 유닛이면 건너뛰기 (재귀 호출)
        if (currentActor.isDead)
        {
            StartNextTurn();
            return;
        }

        // ★ [추가] 턴 시작 시 버프 시간 감소
        // UnitStats에 OnTurnStart() 함수를 만들었으므로 호출
        if (currentActor.stats != null)
        {
            currentActor.stats.OnTurnStart();
        }

        // 상태를 'TurnStart'로 바꾸면 위의 TurnStartProcess가 실행됨
        SetState(TurnState.TurnStart);
    }

    // =========================================================
    // 턴 종료 (조건 3, 4)
    // =========================================================
    // "턴 종료" 버튼을 누르거나, 연료가 다 떨어졌을 때 호출
    public void EndTurnManual()
    {
        // 이동/조준 중일 때만 가능
        if (currentState == TurnState.PlayerMoving || currentState == TurnState.PlayerAiming)
        {
            Debug.Log("턴 종료 요청됨 (이동 종료)");

            // ★ [추가] 현재 조종 중인 캐릭터를 즉시 멈춰세움!
            if (currentActor != null && currentActor.playerScript != null)
            {
                currentActor.playerScript.StopMove();
            }

            // UI 비활성화
            if (UIManager.Instance != null) UIManager.Instance.DisablePlayerControls();

            // 바로 결과 페이즈 -> 다음 턴
            SetState(TurnState.Resolution);
        }
    }

    // 플레이어가 "나 움직였어!"라고 보고하는 함수
    public void ReportPlayerMovement()
    {
        if (!hasMovedThisTurn)
        {
            hasMovedThisTurn = true;
            Debug.Log("이동 시작! 이제 투척 모드로 못 돌아감.");
        }
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

    // 좌우반전
    public void UpdatePlayerDirection(float direction)
    {
        // 1. 현재 턴이 플레이어 조준 상태인지 확인
        if (currentState != TurnState.PlayerAiming) return;

        // 2. 현재 행동 중인 유닛(currentActor)에게 방향 전달
        if (currentActor != null && currentActor.playerScript != null)
        {
            currentActor.playerScript.SetDirection(direction);
        }
    }

    public void RequestThrow()
    {
        if (currentState != TurnState.PlayerAiming) return;
        if (currentActor == null || currentActor.playerScript == null) return;

        SetState(TurnState.Firing); 
        currentActor.playerScript.Throw();
    }

    // 이동 - 투척 모드 전환 함수 (버튼 클릭 시 호출)
    public void ToggleMoveMode()
    {
        // 내 턴이 아니면 무시
        if (currentState != TurnState.PlayerAiming && currentState != TurnState.PlayerMoving) return;

        // ★ [조건 3] 이미 움직이기 시작했다면, 투척 모드(Aiming)로 돌아갈 수 없음!
        if (hasMovedThisTurn && currentState == TurnState.PlayerMoving)
        {
            Debug.Log("이미 움직여서 투척 모드로 돌아갈 수 없습니다!");
            return;
        }

        // 조준 <-> 이동 상태 토글
        if (currentState == TurnState.PlayerAiming)
        {
            SetState(TurnState.PlayerMoving);
        }
        else
        {
            SetState(TurnState.PlayerAiming);
        }
    }

    // 이동 조이스틱에서 오는 신호 처리
    public void UpdateMoveInput(float xInput)
    {
        // 이동 모드일 때만 작동
        if (currentState != TurnState.PlayerMoving) return;

        if (currentActor != null && currentActor.playerScript != null)
        {
            currentActor.playerScript.Move(xInput);
        }
    }

    // =========================================================
    // ★ [추가] 시간 조절 (슬로우 모션)
    // =========================================================
    public void SetTimeScale(float scale)
    {
        Time.timeScale = scale;
        Time.fixedDeltaTime = 0.02f * Time.timeScale; // 물리 연산도 비율에 맞춰 조정
    }

    // =========================================================
    // ★ [추가] 스킬 조이스틱 입력 처리
    // =========================================================
    
    // 스킬 조이스틱이 "나 지금 켜져도 돼?" 라고 물어볼 때 사용
    public bool CanUseSkillJoystick()
    {
        // 발사 중(Firing)이고, 아직 터지지 않았을 때만 가능
        return currentState == TurnState.Firing;
    }

    // 스킬 발동 명령 (Joystick -> GameManager -> Bag)
    public void ActivateCurrentProjectileSkill()
    {
        // 현재 날아가고 있는 가방(Bag)을 찾아서 스킬 실행
        // 카메라가 보고 있는 대상을 가져오거나, 태그로 찾습니다.
        if (CameraManager.Instance != null && CameraManager.Instance.virtualCamera.Follow != null)
        {
            Transform target = CameraManager.Instance.virtualCamera.Follow;
            Bag activeBag = target.GetComponent<Bag>();
            
            if (activeBag != null)
            {
                activeBag.ActivateSkill(); // 가방의 스킬 발동!
                Debug.Log("GameManager: 스킬 발동 명령 전달!");
            }
        }
    }
}