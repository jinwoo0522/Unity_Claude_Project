# PLAN.md

## 1. 작업 개요
- 목표: Magician_Player를 Golem_Player/Elf_Player 패턴으로 리팩토링하여 **기본 움직임(하체) + 상체 Idle/Hit**만 상태머신으로 구동하고, AnimData 2종을 생성한 뒤 프리팹에 연결한다. (상체 공격·스킬은 구현하지 않음)
- 브랜치: `Agents_1`
- 작업 디렉토리: `C:\Unity\Golem_VS_Magician_Agents_1`

## 2. 명세 요약 (TASK.md 기준 + 사용자 확정 사항)
- **하체 움직임 리팩토링**: Magician_Player가 공통 상태(PlayerIdle/Walk/Run/Jump/Land + EntityAirborne)를 재사용하도록 상태머신을 구성한다. 완료 기준: Golem_Player와 동일한 방식으로 하체 상태/AnyTransition이 등록되고 컴파일된다.
- **상체 움직임**: 상체 상태머신에 Idle/Hit만 등록한다(공통 PlayerUpperIdleState/PlayerHitState 재사용). 완료 기준: 상체 IDLE/HIT가 등록되고 피격 반응(AnyToHit_Player)이 동작한다.
- **상체 공격(Start→Middle→Last) 구현하지 않음** (사용자 확정). MAGICIAN 전용 공격 상태/전환 클래스, MAGICIAN enum 생성 금지.
- **스킬(MouseSkill/QSkill) 리팩토링하지 않음** (TASK 명시). Magician_Player에 스킬 상태/전환 등록 금지.
- **AnimData 작성**: 하체용 `AnimData_Magician`, 상체용 `AnimData_UpperMagician` 에셋을 생성한다. 완료 기준: 구현되는 상태가 사용하는 클립이 모두 매핑되고, EntityAnimator가 정상 조회한다.
- **프리팹 연결**: `Magicain_Player.prefab`에 Magician_Player 컴포넌트를 부착하고 animData/upperAnimData 슬롯에 위 두 AnimData를 할당한다 (사용자 확정 — 파이프라인이 직접 수행).
- **레거시 코드 비침범**: 기존 공통/Elf/Golem 코드는 수정하지 않고 신규 파일 추가 + 프리팹/데이터 생성으로만 달성한다.

## 3. 영향 범위

### 새로 생성할 파일
- `Assets/Script/Player/Magician_Player.cs` — Player 상속 신규 스크립트
- `Assets/Resources/Data/AnimData/AnimData_Magician.asset` — 하체 AnimData (layer 0)
- `Assets/Resources/Data/AnimData/AnimData_UpperMagician.asset` — 상체 AnimData (layer 1)
- (위 3개 파일의 `.meta`는 Unity 임포트 시 자동 생성)

### 수정할 파일
- `Assets/Prefabs/Player/Magicain_Player.prefab` — Magician_Player 컴포넌트 부착 + animData/upperAnimData 참조 할당

### 건드리지 않을 파일/시스템 (명시적 제외)
- `EntityEnums.cs` (MAGICIAN enum 추가 안 함 — ENTITY enum만 재사용)
- 공통 상태/전환 (`PlayerState/`, `PlayerUpperState/`, `PlayerTransition/`, `PlayerUpperTransition/`, `Entity/State/`, `Entity/Transition/`)
- Elf/Golem 관련 모든 스크립트, 프리팹, AnimData
- `MagicianAnimator.controller` (상태 구조 이미 완성됨 — 수정 불필요)
- 스킬 관련(`Skill/`), AttackHitbox(프리팹에 부재, 공격 미구현이므로 불필요)

## 4. 구현 단계

### Step 1. Magician_Player.cs 작성
- 작업 내용: `Golem_Player.cs`를 템플릿으로, **스킬 전환을 제거한** 버전을 작성한다.
  - 클래스: `public class Magician_Player : Player`
  - `OnNetworkSpawn()`: `base.OnNetworkSpawn()` → `CreateState()` → `CreateUpperState()` → (IsServer일 때) 하체/상체 IDLE로 `TransitionTo`.
  - `_hitter` 필드 **불필요** (공격 미구현, AnyToHit_Player는 hitter를 받지 않음) — 추가하지 않는다.
  - `CreateState()` 등록 (모두 `(ushort)ENTITY.StateType.XXX` 사용):
    - IDLE → `new PlayerIdleState(this)`
    - WALK → `new PlayerWalkState(this)`
    - RUN → `new PlayerRunState(this)`
    - JUMP → `new PlayerJumpState(this, 0.1f)` (Golem과 동일 파라미터)
    - LAND → `new PlayerLandState(this)`
    - AIRBORNE → `new EntityAirborneState(this)`
    - AnyTransition: `new AnyToJump_Player(_input, _aniController, _jump)`, `new AnyToAirborne_Entity(_crowdController)`
    - **스킬 전환(StateToMouseAttack_*, StateToQSkill_*) 등록하지 않음**
  - `CreateUpperState()` 등록 (`(ushort)ENTITY.UpperStateType.XXX` 사용):
    - IDLE → `new PlayerUpperIdleState(this)`
    - HIT → `new PlayerHitState(this)`
    - AnyTransition: `new AnyToHit_Player(_upperAniController, _stat)`
    - **상체 공격 전환(IdleToAttackStart_*) 등록하지 않음**
- 완료 기준: 컴파일 에러 없이 빌드되고, 시그니처가 기존 공통 상태 생성자와 일치한다.
- 예상 리스크: 공통 상태 생성자 시그니처 변동 가능 → 구현 시 각 상태 파일을 다시 확인할 것.

### Step 2. AnimData 에셋 2종 생성
- 작업 내용: 기존 `AnimData_Golem.asset` / `AnimData_UpperGolem.asset` YAML 포맷을 그대로 따라 작성한다. `m_Script` guid는 AnimData 스크립트 guid `06443740ab07aa94a95000a86d8fcdbb` 사용.
  - `AnimData_Magician.asset` (iLayerNumber: 0, fDuration: 0.15, fWeightLerpSpeed: 1):
    - 1→Idle, 2→Walk, 4→Jump, 64→Run, 128→Land, 256→Airborne, 512→Freeze
    - (스킬 클립 MouseSkill/QSkill 제외 — 스킬 미리팩토링)
  - `AnimData_UpperMagician.asset` (iLayerNumber: 1, fDuration: 0.15, fWeightLerpSpeed: 2):
    - 0→None, 1→Idle, 2→Hit
    - (AttackStart/Middle/Last 제외 — 상체 공격 미구현. 코드가 구동하는 상태만 매핑)
- 완료 기준: 두 에셋이 MagicianAnimator의 실제 클립명(Idle/Walk/Jump/Run/Land/Airborne/Freeze, None/Idle/Hit)과 일치한다.
- 예상 리스크: 클립명 오타 시 CrossFade 실패 → MagicianAnimator 상태명과 대조 확인(이미 조사함: 상태명 일치 확인됨).

### Step 3. Unity 임포트로 .meta/guid 생성
- 작업 내용: `unity-cli`로 에셋 리프레시(임포트)를 트리거하여 신규 스크립트/에셋의 `.meta` guid를 생성·확정한다. (`Reimport All` 금지 — 단일 임포트/리프레시만)
- 완료 기준: `Magician_Player.cs.meta`, `AnimData_Magician.asset.meta`, `AnimData_UpperMagician.asset.meta`가 생성되고 각 guid를 읽을 수 있다.
- 예상 리스크: Unity 미기동 시 guid 미생성 → unity-cli로 에디터 상태 확인 후 진행.

### Step 4. 프리팹 연결 (Magicain_Player.prefab)
- 작업 내용:
  - `Magicain_Player.prefab` 루트 GameObject에 Magician_Player 컴포넌트를 부착한다.
  - 부착된 컴포넌트의 직렬화 필드 할당:
    - `animData` (Entity 상속 [SerializeField]) ← `AnimData_Magician.asset` guid
    - `upperAnimData` (Player [SerializeField]) ← `AnimData_UpperMagician.asset` guid
  - 가능하면 `unity-cli`로 컴포넌트 부착/참조 할당을 수행한다. unity-cli로 직렬화 필드 지정이 불가하면, Step 3에서 확정한 guid를 사용해 프리팹 YAML을 직접 편집한다 (기존 컴포넌트/계층 구조는 보존).
- 완료 기준: 프리팹에 Magician_Player가 존재하고 두 AnimData 참조가 채워져 있다. 프리팹 YAML이 깨지지 않는다.
- 예상 리스크: 프리팹 YAML 수동 편집 시 fileID 충돌/구조 손상 → 편집 전 백업, 편집 후 unity-scanner read로 검증.

## 5. 가정 및 제약
- Magician 프리팹에는 이미 Animator(MagicianAnimator), Player_Input, PlayerMovement, PlayerCameraRotate, CrowdController, EntityEffector, Stat, Player_NetworkSpawn, CharacterController, NetworkObject/NetworkAnimator(패키지)가 부착되어 있음(조사 확인). → Magician_Player의 `GetComponent<>` 의존성이 충족된다.
- 상체 공격·스킬은 이번 범위에서 제외(사용자/TASK 확정). 따라서 MAGICIAN 네임스페이스 enum, AttackHitbox(IHitter), 공격용 이펙트 enum은 생성하지 않는다.
- Magician 고유 하체 상태는 불필요 — 공통 ENTITY 상태 전면 재사용("중복되는 것을 사용" 충족).
- 점프 파라미터는 Golem 기준(0.1f)을 따른다.

## 6. 검증 방법
- Step 1: 프로젝트 컴파일(unity-cli 빌드/컴파일 확인) — Magician_Player.cs 에러 0.
- Step 2: `AnimData_Magician`/`AnimData_UpperMagician`의 clipName이 MagicianAnimator 상태명과 1:1 일치(unity-scanner read 대조).
- Step 3: 세 `.meta` 파일 guid 존재 확인.
- Step 4: `unity-scanner read Magicain_Player.prefab`로 Magician_Player 컴포넌트 및 animData/upperAnimData 참조 채워짐 확인.
- 전체 시나리오: 플레이모드 진입 시 서버에서 하체 IDLE/WALK/RUN/JUMP/LAND/AIRBORNE 전환과 상체 IDLE/HIT가 애니메이션과 함께 정상 동작(에러 로그 없음).
