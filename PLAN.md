# PLAN.md

## 1. 작업 개요
- 목표: 컴포넌트로 흩어진 상태이상을 `StatusEffect` 추상클래스 + 순수 C# 자식 클래스 구조로 통합하고, 단일 `Player_Status` 컴포넌트가 관리하도록 리팩토링한다. 더불어 Magician Q(Ice_Explosion)가 부여하는 신규 상태이상 "빙결"을 구현한다.
- 브랜치: `Agents_1`
- 작업 디렉토리: `C:/Unity/Golem_VS_Magician_Agents_1`

## 2. 명세 요약 (TASK.md 기준)
- **상태이상 구조 통합**: 무엇을 / 현재 `StatusEffect_Airborne`·`StatusEffect_Slow`가 각각 `NetworkBehaviour` 컴포넌트로 분리되어 있음 → 어떻게 / `StatusEffect` 추상클래스 아래 순수 C# 자식 클래스(Airborne, Slow, Freeze)로 재구성 → 완료기준 / 자식들은 `MonoBehaviour`/`NetworkBehaviour`를 상속하지 않는 순수 C# 클래스다.
- **단일 관리 컴포넌트**: 무엇을 / 여러 상태이상 객체를 모두 보유 → 어떻게 / 플레이어에 붙는 `Player_Status`(NetworkBehaviour) 컴포넌트가 자식 객체들을 생성·보유하고 외부에 부여(Apply)·조회 API를 노출 → 완료기준 / 외부 코드는 개별 상태이상 컴포넌트 대신 `Player_Status` 하나만 참조한다.
- **코루틴 위임**: 무엇을 / 순수 C# 클래스는 코루틴을 못 돌림 → 어떻게 / 코루틴/시간 처리는 `Player_Status`가 대신 수행 → 완료기준 / Airborne 착지 감지·Freeze/Slow 시간경과가 `Player_Status`에서 구동된다.
- **빙결(신규)**: 무엇을 / 2초간 완전 행동불가 상태이상 → 어떻게 / Magician Q(IceExplosion) 피격 시 부여, 공격·점프·이동 모두 차단하되 중력은 유지(공중 피격 시 언 채로 낙하) → 완료기준 / 빙결 2초 동안 이동/점프/공격 불가, 중력으로 착지, 2초 후 정상 복귀.

## 3. 영향 범위

### 수정할 파일
- `Assets/Script/Player/StatusEffect_Airborne.cs` — `NetworkBehaviour` → `StatusEffect` 상속 순수 C# 클래스로 전환
- `Assets/Script/Player/StatusEffect_Slow.cs` — 동일하게 순수 C# 클래스로 전환
- `Assets/Script/Player/Player_Move.cs` — 두 상태이상 참조를 `Player_Status` 단일 참조로 교체, 빙결 이동/점프 차단 추가
- `Assets/Script/Player/Player_Skill.cs` — `_airborne` 참조를 `Player_Status`로 교체, 빙결 시 스킬 차단 추가
- `Assets/Script/Player/Player_UpperBody.cs` — `Player_Status` 참조 추가, 빙결 시 기본공격 차단 추가
- `Assets/Script/Skill/Skill.cs` — `OnHitEnemy`의 Apply 호출을 `Player_Status` 경유로 변경, 빙결 부여 분기 추가
- `Assets/Resources/Data/SkillData/SkillData.cs` — 빙결 지속시간 필드 `_fFreezeDuration` 추가

### 새로 생성할 파일
- `Assets/Script/Player/StatusEffect.cs` — 상태이상 추상 기반(순수 C#)
- `Assets/Script/Player/StatusEffect_Freeze.cs` — 빙결 상태이상(순수 C#)
- `Assets/Script/Player/Player_Status.cs` — 상태이상 통합 관리 컴포넌트(NetworkBehaviour)

### 에디터/에셋 변경 (※ 사용자 승인 필요, CLAUDE.md 규칙)
- `Assets/Prefebs/Player/Golem_Player.prefab` — `StatusEffect_Airborne`·`StatusEffect_Slow` 컴포넌트 제거, `Player_Status` 컴포넌트 추가
- `Assets/Prefebs/Player/Magicain_Player.prefab` — 동일
- `Assets/Resources/Data/SkillData/Magician_Q_Skill.asset` — `_fFreezeDuration: 2` 설정

### 건드리지 않을 파일/시스템 (명시)
- `Assets/Script/Skill/SkillPool.cs`, `SkillType.cs` — SkillType↔SkillData 인덱스 매핑 유지(IceExplosion=index 1=Magician_Q_Skill)
- Golem 계열 스킬/데이터, 넉백(`ApplyKnockback`) 로직
- 기존 Airborne/Slow의 **동작(밸런스)** 자체는 변경하지 않음 — 구조만 이전

## 4. 구현 단계

### Step 1. `StatusEffect` 추상 기반 + 순수 C# 자식 전환
- 작업 내용:
  - `StatusEffect.cs` 생성: 순수 C# 추상클래스. `protected readonly Player_Status owner;` + 보호 생성자. 시간기반 효과용 `public virtual void Tick(float dt) {}` 빈 훅 제공.
  - `StatusEffect_Slow.cs`를 `StatusEffect` 상속 순수 C# 클래스로 전환. 기존 `_multiplier`/`_remainingTime`/`SpeedMultiplier`/`Apply(mult,dur)` 로직 유지. `Update` 대신 `Tick(dt)`에서 `_remainingTime` 감소(서버 전용 상태, 네트워크 불필요).
  - `StatusEffect_Airborne.cs`를 순수 C# 클래스로 전환. `_pendingLaunchForce`/`ConsumePendingLaunch` 로직 유지. `Apply(force)`는 `owner`에게 공중 상태 set + 착지 코루틴 시작을 요청. `IsAirborne`는 `owner`의 네트워크 상태를 조회.
- 완료 기준: 세 파일이 `MonoBehaviour`/`NetworkBehaviour`/`Unity.Netcode`를 상속/사용하지 않는 순수 C# 클래스로 컴파일된다.
- 예상 리스크: Airborne의 공중 bool은 owner 클라가 읽어야 하므로(점프 게이트) 네트워크 복제가 필요 → 상태 bool은 `Player_Status`의 NetworkVariable로 이전한다(Step 2).

### Step 2. `Player_Status` 통합 관리 컴포넌트 생성
- 작업 내용:
  - `Player_Status.cs` 생성: `NetworkBehaviour`. 세 순수 C# 객체(Airborne/Slow/Freeze)를 `OnNetworkSpawn`에서 생성·보유.
  - 클라 공개가 필요한 상태는 NetworkVariable로 보유: `net_isAirborne`, `net_isFrozen` (Everyone read / Server write). Slow 배율은 서버 전용이므로 네트워크 불필요.
  - 외부 조회 API 노출: `IsAirborne`, `IsFrozen`, `SpeedMultiplier`, `ConsumePendingLaunch(out float)`.
  - 외부 부여 API 노출(서버 전용 가드): `ApplyAirborne(force)`, `ApplySlow(mult,dur)`, `ApplyFreeze(duration)`.
  - 코루틴/시간 위임: 서버 `Update`에서 `slow.Tick(dt)`·`freeze.Tick(dt)` 호출, Freeze 만료 시 `net_isFrozen=false`. Airborne의 착지 감지 코루틴(`WaitUntil` 이륙→착지)은 `Player_Status`가 `StartCoroutine`으로 구동하고 종료 시 `net_isAirborne=false`.
- 완료 기준: `Player_Status` 하나로 세 상태이상의 부여·조회·시간처리가 동작한다.
- 예상 리스크: 기존 Airborne 코루틴의 `CharacterController.isGrounded` 의존 → `Player_Status`에서 `GetComponent<CharacterController>()`로 동일 참조 확보.

### Step 3. 신규 빙결(`StatusEffect_Freeze`) 구현
- 작업 내용:
  - `StatusEffect_Freeze.cs` 생성: 순수 C#. `_remainingTime` 보유, `Apply(duration)`로 설정 + `owner`에 빙결 상태 set 요청, `Tick(dt)`로 감소, 만료 시 해제 요청. `IsFrozen`은 `owner`의 net 상태 조회.
  - `Player_Status.ApplyFreeze`: 서버에서 `net_isFrozen=true` 후 freeze 객체에 지속시간 위임.
- 완료 기준: `ApplyFreeze(2)` 호출 시 2초간 `IsFrozen==true`, 이후 자동 해제.
- 예상 리스크: 공중 피격 시에도 중력이 유지되어야 함 → 빙결은 **수평 입력 이동만** 차단하고 중력 코드는 건드리지 않음(Step 4).

### Step 4. 행동 차단 연동(이동/점프/공격/스킬)
- 작업 내용:
  - `Player_Move.cs`: `_airborne`/`_slow` 필드를 `Player_Status _status` 단일 참조로 교체. 수평 입력 이동 분기(현 `!skill.IsSkilling`)에 `&& !_status.IsFrozen` 추가(중력·넉백 코드는 그대로 → 공중 피격 시 낙하). `PlayerJump`/`Jump_ServerRpc`에 `_status.IsFrozen` 차단 추가. 기존 `IsAirborne`/`SpeedMultiplier`/`ConsumePendingLaunch` 호출을 `_status` 경유로 변경.
  - `Player_Skill.cs`: `_airborne`를 `_status`로 교체. `CheckCanUseSkill`에 `if (_status.IsFrozen) return null;` 추가, 기존 `IsAirborne` 조회를 `_status`로 변경.
  - `Player_UpperBody.cs`: `Player_Status _status` 참조 추가, `OnAttack`에 `if (_status.IsFrozen) return;` 추가(기존 `IsHit`/`IsSkilling` 게이트와 동일 위치, owner 측).
- 완료 기준: 빙결 중 이동/점프/공격/스킬이 모두 차단되고, 중력은 유지된다.
- 예상 리스크: 서버 권위 — 이동/점프는 서버에서 재검증되므로 안전. 기본공격은 기존 구조상 owner 측 게이트(현행 패턴 유지), 서버측 공격 차단 강화는 본 작업 범위 외로 둔다.

### Step 5. 스킬 부여 경로 연결 + 데이터 필드
- 작업 내용:
  - `SkillData.cs`: `[SerializeField] private float _fFreezeDuration = 0f;` + `public float fFreezeDuration => _fFreezeDuration;` 추가(0이면 빙결 없음, 기존 패턴과 동일).
  - `Skill.cs`의 `OnHitEnemy`: 기존 `GetComponent<StatusEffect_Airborne>().Apply(...)`/`StatusEffect_Slow` 호출을 `output.GetComponent<Player_Status>().ApplyAirborne(...)`·`ApplySlow(...)`로 변경. `if (Data.fFreezeDuration > 0f) output.GetComponent<Player_Status>().ApplyFreeze(Data.fFreezeDuration);` 분기 추가.
- 완료 기준: IceExplosion 피격자가 빙결 부여를 받는다(데이터값 기반).
- 예상 리스크: `Player_Status` 컴포넌트가 프리팹에 없으면 `GetComponent` null → Step 6 프리팹 작업 선행 필수.

### Step 6. 프리팹/에셋 반영 (※ 승인 후 진행)
- 작업 내용:
  - Step 1~5 스크립트 작성 후 Unity 임포트(`unity-cli`)로 `Player_Status.cs` guid 생성·확인.
  - `Golem_Player.prefab`·`Magicain_Player.prefab`에서 `StatusEffect_Airborne`(guid d82227305984980468a08d962d1e03b2)·`StatusEffect_Slow`(guid ef749d2150b42004a873cd8b82619500) 컴포넌트 제거, `Player_Status` 컴포넌트 추가. (직렬화 참조가 아닌 런타임 `GetComponent` 사용이므로 다른 컴포넌트 참조 깨짐 없음 — 단, 제거 전 .meta guid 재검증)
  - `Magician_Q_Skill.asset`에 `_fFreezeDuration: 2` 추가.
- 완료 기준: 두 프리팹이 `Player_Status` 단일 컴포넌트를 가지며 누락 스크립트가 없다.
- 예상 리스크: 프리팹 YAML 수동 편집은 정확한 fileID/guid 필요 → `Assets/Reimport All` 금지, 개별 임포트만 사용.

## 5. 가정 및 제약
- 공중 bool·빙결 bool은 owner 클라의 입력 게이트(점프/공격)에서 읽혀야 하므로 `Player_Status`의 NetworkVariable(Everyone read/Server write)로 복제한다. Slow 배율은 서버 전용이라 비복제.
- 빙결은 **플레이어 입력 수평 이동만** 차단한다. 중력·넉백 등 외력은 차단하지 않는다(중력 유지 명세 충족, 넉백은 명세 외).
- 빙결은 고정 2초, 갱신은 덮어쓰기 방식(기존 Slow의 덮어쓰기 패턴과 동일). 스택/연장은 명세에 없으므로 구현하지 않음.
- IceExplosion=`SkillType` 인덱스 1=`Magician_Q_Skill.asset`, 광역(SkillRange) 피격이 공통 `OnHitEnemy`를 통과한다는 현행 구조를 전제한다.
- 상태이상 부여는 모두 서버에서만 수행(`IsServer` 가드 유지) — 클라이언트 입력 불신 원칙.
- 코드 작성 규칙 준수: `public` 필드 대신 `[SerializeField] private`, 은닉화, null 체크는 시스템 경계로 한정.

## 6. 검증 방법
- Step 1~5: `unity-cli`로 컴파일 에러 0 확인(순수 C# 클래스가 `NetworkBehaviour` 미상속으로 정상 컴파일).
- Step 6: 두 프리팹에 누락 스크립트(missing script) 경고가 없는지 확인.
- 최종 플레이모드 시나리오:
  1. Magician Q(IceExplosion)로 상대 적중 → 적이 2초간 이동/점프/공격/스킬 모두 불가.
  2. 공중에 있는 상대를 빙결 → 입력 불가 상태로 중력에 의해 바닥으로 낙하 후 정지.
  3. 2초 경과 후 상대가 정상 조작 복귀.
  4. 회귀 검증: 기존 Airborne(공중 띄움)·Slow(감속)가 종전과 동일하게 동작.
