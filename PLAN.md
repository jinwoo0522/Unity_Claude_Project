# PLAN.md

## 1. 작업 개요
- 목표: `Golem_Player`를 `Elf_Player`의 상태머신·기능 분리 구조를 참고해 기본 움직임(하체 이동 + 상체 Idle/Hit) 중심으로 리팩토링한다.
- 브랜치: `Agents_1`
- 작업 디렉토리: `C:\Unity\Golem_VS_Magician_Agents_1`

## 2. 명세 요약
TASK.md 핵심 요구사항:

- **Golem_Player 기본 움직임 리팩토링**: 무엇=Elf_Player처럼 상태머신/기능을 분리한 `Golem_Player`를 만든다 / 어떻게=`Player`를 상속하고 공통 상태를 재사용 / 완료 기준=골렘이 Idle·Walk·Run·Jump·Land·Airborne 하체 상태로 동작.
- **애니메이터 상태 전부 가공**: 무엇=`GolemAnimator`를 Elf식 base+upper 레이어 구조로 재편 / 어떻게=상태 이름을 AnimData clipName과 일치시키고 골렘 클립을 연결 / 완료 기준=`EntityAnimator.CrossFade(clipName, layer)`로 정상 재생.
- **AnimData 작성**: 무엇=`AnimData_Golem`(하체, layer 0) + `AnimData_UpperGolem`(상체, layer 1) ScriptableObject 생성 / 완료 기준=key↔clipName 매핑이 등록 상태/애니메이터 상태와 일치.
- **상체 움직임(Idle/Hit)**: 무엇=상체 상태머신에 Idle·Hit 등록 / 어떻게=공통 `PlayerUpperIdleState`·`PlayerHitState` 재사용 + `AnyToHit_Player` / 완료 기준=피격 시 상체 Hit 재생. (스킬/공격 상태는 범위 외 — 스킬 미리팩토링)
- **착지 상태**: 무엇=Land 상태는 만들되 클립은 공석 / 완료 기준=상태 존재, 클립 없음(에러 없이 통과).
- **전환 외부 등록 리팩토링(사용자 지시)**: 무엇=공통 상태에 박혀 있는 Elf 전용 스킬 전환을 상태 밖에서 캐릭터별로 주입하도록 구조 변경 / 완료 기준=Golem이 ELF 스킬 상태로 전환을 시도하지 않으며, Elf 동작은 기존과 동일.

## 3. 영향 범위

### 수정할 파일 (전환 외부 등록 리팩토링 — 사용자 승인된 레거시 변경)
- `Assets/Script/Player/State/IState.cs` — `AddTransition(ITransition)` 선언 추가
- `Assets/Script/Player/State/EntityState.cs` — `AddTransition` 구현 추가
- `Assets/Script/Player/State/StateMachine.cs` — `AddTransition(ushort tag, ITransition)` 추가
- `Assets/Script/Player/State/PlayerState/PlayerIdleState.cs` — `StateToMouseAttack_Elf`, `StateToQSkill_Elf` 전환 제거(공통 `IdleToWalk_Player`만 유지)
- `Assets/Script/Player/State/PlayerState/PlayerWalkState.cs` — Elf 스킬 전환 2개 제거(공통 `WalkToIdle_Player`/`WalkToRun_Player` 유지)
- `Assets/Script/Player/State/PlayerState/PlayerRunState.cs` — Elf 스킬 전환 2개 제거(공통 `RunToWalk_Player`/`RunToIdle_Player` 유지)
- `Assets/Script/Player/State/PlayerUpperState/PlayerUpperIdleState.cs` — `IdleToAttackStart_Elf` 전환 제거
- `Assets/Script/Player/Elf_Player.cs` — 제거된 Elf 전환들을 `_stateMachine.AddTransition(...)` / `_upperStateMachine.AddTransition(...)`로 외부 재등록 (Elf 동작 보존)

### 새로 생성할 파일
- `Assets/Script/Player/Golem_Player.cs` — `Player` 상속, 하체/상체 상태 등록
- `Assets/Resources/Data/AnimData/AnimData_Golem.asset` — 하체 AnimData (layer 0)
- `Assets/Resources/Data/AnimData/AnimData_UpperGolem.asset` — 상체 AnimData (layer 1)

### 수정할 에셋 (구현 단계서 자동 처리 — 사용자 승인됨)
- `Assets/Animator/GolemAnimator.controller` — base+upper 레이어 구조로 재편
- `Assets/Prefabs/Player/Golem_Player.prefab` — Missing Script 6개 제거, 정상 컴포넌트 복구, AnimData 참조 연결

### 건드리지 않을 파일/시스템
- `Assets/Script/Player/State/ElfState/*`, `ElfUpperState/*` (Elf 전용 상태·전환 클래스 — 그대로 유지)
- `Assets/Prefabs/Player/Elf_Player.prefab`, `Assets/Animator/ElfAnimator.controller`, `AnimData_Elf*.asset`
- 스킬 시스템(`Assets/Script/Skill`) — 명세상 미리팩토링
- `Assets/Reimport All` 실행 절대 금지

## 4. 구현 단계

### Step 1. 전환 외부 등록 메커니즘 추가
- 작업 내용:
  - `IState`에 `void AddTransition(ITransition trans);` 추가.
  - `EntityState`에 `public void AddTransition(ITransition trans) => TransitionList.Add(trans);` 구현.
  - `StateMachine`에 `public void AddTransition(ushort tag, ITransition trans) => States[tag].AddTransition(trans);` 추가.
- 완료 기준: 컴파일 통과, 기존 `Create()` 내부 전환 등록과 외부 등록이 공존 가능.
- 예상 리스크: `States[tag]` 미존재 키 접근 시 예외 — 반드시 상태 등록 후 호출하도록 순서 보장.

### Step 2. 공통 상태에서 Elf 전용 전환 분리
- 작업 내용:
  - `PlayerIdleState`/`PlayerWalkState`/`PlayerRunState`의 `Create()`에서 `StateToMouseAttack_Elf`·`StateToQSkill_Elf` 제거(공통 이동 전환은 유지).
  - `PlayerUpperIdleState.Create()`에서 `IdleToAttackStart_Elf` 제거.
  - `Elf_Player`에서 상태 생성 직후, 제거한 전환들을 해당 상태 태그에 `AddTransition`으로 외부 재등록.
- 완료 기준: Elf 플레이 동작이 리팩토링 전과 동일(Walk/Run 중 마우스·Q 스킬 진입 정상), 컴파일 통과.
- 예상 리스크: Elf 동작 회귀 — Elf의 외부 재등록 누락 시 스킬 전환 불가. 등록 대상 상태(IDLE/WALK/RUN, upper IDLE)와 전환 생성자 인자(`_input`) 정확히 매칭.

### Step 3. Golem_Player.cs 작성
- 작업 내용: `Elf_Player`를 참고해 `Golem_Player : Player` 작성.
  - `CreateState()`: IDLE=`PlayerIdleState`, WALK=`PlayerWalkState`, RUN=`PlayerRunState`, JUMP=`PlayerJumpState`, LAND=`PlayerLandState`, AIRBORNE=`EntityAirborneState`. AnyTransition: `AnyToJump_Player`, `AnyToAirborne_Entity`.
  - `CreateUpperState()`: IDLE=`PlayerUpperIdleState`, HIT=`PlayerHitState`. AnyTransition: `AnyToHit_Player`.
  - `OnNetworkSpawn()`에서 IDLE 진입(`ENTITY.StateType.IDLE`, `ENTITY.UpperStateType.IDLE`).
  - 상태 태그는 기존 `ENTITY.StateType` / `ENTITY.UpperStateType` 재사용(GOLEM enum 신설 불필요 — 값 동일).
  - Golem은 Elf 스킬 전환을 외부 등록하지 않음(Step 2 효과로 ELF 상태 미참조).
- 완료 기준: 컴파일 통과, Golem이 스킬 입력으로 미등록 ELF 상태 전환을 시도하지 않음.
- 예상 리스크: 하체 Jump를 `ElfJumpState`(점프 딜레이 0.25s) 대신 `PlayerJumpState` 사용 — 골렘 점프 연출이 즉시 점프가 됨. (의도: 공통 재사용. 연출 차이 필요 시 후속 조정.)

### Step 4. AnimData 에셋 생성
- 작업 내용:
  - `AnimData_Golem.asset`: iLayerNumber=0, animEntries = {1:Idle, 2:Walk, 4:Jump, 64:Run, 128:Land, 256:Airborne}.
  - `AnimData_UpperGolem.asset`: iLayerNumber=1, animEntries = {0:None, 1:Idle, 2:Hit}.
  - `AnimData.cs` 스크립트 guid(`06443740ab07aa94a95000a86d8fcdbb`)를 m_Script로 참조(Elf asset과 동일).
- 완료 기준: 두 에셋이 Inspector에서 AnimData로 정상 인식, key↔clipName이 Step 5 애니메이터 상태 이름과 일치.
- 예상 리스크: clipName 문자열과 애니메이터 상태 이름 불일치 시 CrossFade 실패 → Step 5와 문자열 정확히 동기화.

### Step 5. GolemAnimator 재편
- 작업 내용:
  - **Base 레이어(layer 0)**: 상태 이름 `Idle`, `Walk`, `Run`, `Jump`, `Land`, `Airborne` 생성. 기본 상태=`Idle`.
    - Idle ← `Standing Idle`
    - Walk ← `MoveX`/`MoveZ` 2D 블렌드트리(`Standing Walk Forward/Back/Left/Right`)
    - Run ← 블렌드트리(`Standing Run Forward/Back`)
    - Jump ← `Unarmed Jump Running`
    - Land ← 공석(모션 없음 — TASK 지시)
    - Airborne ← 공석 또는 Jump 모션 재사용
    - `MoveX`, `MoveZ` Float 파라미터 추가(Elf 애니메이터 파라미터 참고).
  - **Upper 레이어(layer 1)**: AvatarMask=`Golem_UpperBody_Mask.mask`. 상태 `None`(공석), `Idle`(공석/유휴), `Hit`←`Standing React Large From Right`. 기본=`None` 또는 `Idle`.
  - 상태 간 Animator 트랜지션/컨디션은 불필요(상태 전이는 코드의 CrossFade가 구동). 상태 존재와 이름·모션만 맞추면 됨.
- 완료 기준: 각 상태 이름이 AnimData clipName과 정확히 일치, 골렘 클립이 연결됨, 레이어 2개 구성.
- 예상 리스크: `.controller` YAML 수동/자동 편집 오류. 가능하면 unity-cli 활용, 클립 guid는 .meta로 검증. **Reimport All 금지.**

### Step 6. Golem_Player.prefab 복구
- 작업 내용:
  - Missing Script 컴포넌트 6개(guid ff056c88, f6568e20, 4e462607, 0792060d, 1e353317, 97987844) 제거.
  - 루트에 정상 컴포넌트 추가/복구(Elf_Player.prefab 루트 구성 diff 기준): `Golem_Player`, `Player_Input`, `PlayerMovement`, `PlayerCameraRotate`, `CrowdController`, `EntityEffector`, `NetworkAnimator`(EntityAnimator 필수), 기존 `Animator`/`CharacterController`/`Stat`/`Player_NetworkSpawn`/`NetworkObject`/`NetworkTransform` 유지.
  - `Animator.controller` = `GolemAnimator`.
  - `Golem_Player` 컴포넌트: `animData`=`AnimData_Golem`(guid), `upperAnimData`=`AnimData_UpperGolem`(guid) 연결.
- 완료 기준: 프리팹에 Missing Script 0개, Elf와 동등한 기능 컴포넌트 세트, AnimData 참조 연결됨.
- 예상 리스크: 프리팹 YAML 직접 편집 위험. 에셋 guid는 생성 후 .meta로 재확인하고 참조. AttackHitbox/PlayerHUDBinder는 공격/스킬·HUD 의존이므로 기본 움직임 범위에서 제외(필요 시 후속).

## 5. 가정 및 제약
- 상체 태그는 `ENTITY.UpperStateType`(IDLE=1, HIT=2)을 사용하며, 이는 `PlayerUpperIdleState`/`PlayerHitState`가 설정하는 값과 일치한다(GOLEM enum 불필요).
- 하체 상태는 공통 `Player*` 상태를 재사용하며, Elf 전용 전환만 외부로 분리한다(공통 이동 전환은 상태 내부 유지 — 최소 변경).
- **사용자 승인 전제**: (a) 공통 상태·Elf_Player 수정(전환 외부 등록), (b) 애니메이터/프리팹 자동 편집을 구현 단계에서 수행.
- `PlayerMovement`가 `IEntityMovement`와 `IJumpMovement`를 모두 구현한다고 전제(Player.cs가 둘 다 GetComponent로 획득).
- Land/Airborne 클립은 공석 허용(에러 없이 상태 통과).
- 스킬·상체 공격 상태는 본 작업 범위 외(스킬 미리팩토링).
- `Assets/Reimport All` 금지. 에셋 참조는 반드시 실제 .meta guid로 검증 후 사용.

## 6. 검증 방법
- **Step 1~3 (스크립트)**: Unity 컴파일 에러 0건. `Golem_Player`/수정 상태들이 정상 빌드.
- **Step 2 회귀**: Elf 플레이모드에서 Walk/Run 중 마우스 좌클릭·Q로 스킬 진입이 기존과 동일하게 동작(전환 외부 등록 후 회귀 없음).
- **Step 4 (Data)**: 두 AnimData 에셋의 key↔clipName이 애니메이터 상태 이름과 1:1 일치.
- **Step 5~6 (애니메이터/프리팹)**: 프리팹 Missing Script 0건, GolemAnimator 레이어 2개·상태 이름 정합.
- **전체 시나리오 (플레이모드)**:
  1. 골렘 스폰 → Idle 재생.
  2. 이동 입력 → Walk, 스프린트 → Run, MoveX/MoveZ 블렌드 동작.
  3. 점프 입력 → Jump → 착지 시 Land 상태 진입(클립 공석이라 시각효과 없이 통과) → Idle 복귀.
  4. 피격 → 상체 Hit 재생 후 상체 Idle 복귀.
  5. 골렘이 스킬 입력(마우스/Q)에 ELF 상태 전환을 시도하지 않음(KeyNotFound 미발생).
