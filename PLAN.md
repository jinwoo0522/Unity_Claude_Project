# PLAN.md

## 1. 작업 개요
- 목표: Golem 마우스 우클릭 스킬(FireExplosion)에 맞은 적을 공중으로 띄우고 슬로우를 거는 상태이상 시스템을 모듈화하여 추가한다.
- 브랜치: `Agents_1`
- 작업 디렉토리: `C:/Unity/Golem_VS_Magician_Agents_1`

## 2. 명세 요약
- **공중 띄움**: Golem 우클릭 스킬(FireExplosion)에 맞은 플레이어를 공중으로 높게 띄운다. / 상승 속도(힘)는 `SkillData` 필드로 입력. / 완료 기준: 피격 시 대상이 위로 솟구친다.
- **점프 동일 판정**: 공중에 떠 있는 동안은 점프했을 때와 동일하게 취급한다. / 기존 점프 게이트(`net_isJumpPending`) 흐름과 일치. / 완료 기준: 공중에 떠 있는 동안 `IsGrounded` 애니 판정·재점프 차단이 점프와 동일하게 동작.
- **공중 행동 제한**: 공중에 떠 있는 동안 점프 불가, 스킬 사용 불가. / `Player_Move.PlayerJump`·`Player_Skill.CheckCanUseSkill`에 공중 상태 게이트 추가. / 완료 기준: 공중에서 점프 키·스킬 키 입력이 무시됨.
- **슬로우**: 공중에 맞는 순간부터 이동 속도가 낮아진다. / 슬로우 배율과 지속시간(고정 시간, 예 2초)은 `SkillData` 필드로 입력. / 완료 기준: 피격 순간부터 지정 시간 동안 이동 속도가 배율만큼 감소했다가 복귀.
- **모듈화**: 슬로우와 공중 띄움을 **각각 별도의 상태이상 MonoBehaviour 컴포넌트**로 만들고, 함수 호출로 발동한다. / 완료 기준: 두 효과가 독립 컴포넌트로 분리되어 `Apply...()` 함수 호출만으로 발동.

## 3. 영향 범위

### 새로 생성할 파일
- `Assets/Script/Player/StatusEffect_Airborne.cs` — 공중 띄움 상태이상 컴포넌트 (NetworkBehaviour)
- `Assets/Script/Player/StatusEffect_Slow.cs` — 슬로우 상태이상 컴포넌트 (NetworkBehaviour)
- (위 2개 생성 시 Unity가 `.meta` guid 자동 생성)

### 수정할 파일 (코드)
- `Assets/Resources/Data/SkillData/SkillData.cs` — 띄움 힘·슬로우 배율·슬로우 지속시간 필드 3개 추가
- `Assets/Script/Skill/Skill.cs` — `OnHitEnemy`에서 데이터 값이 설정된 경우 상태이상 발동 함수 호출
- `Assets/Script/Player/Player_Move.cs` — 슬로우 배율 반영, 공중 띄움 속도 적용, 공중 중 점프 차단(`PlayerJump` + `Jump_ServerRpc` 양쪽), `AnimManage`의 IsGrounded 계산에 공중 상태 반영, 공중·착지 상태 연동
- `Assets/Script/Player/Player_Skill.cs` — `CheckCanUseSkill`에 공중 상태 게이트 추가

### 수정할 파일 (에디터 — 사용자 결정으로 사전 승인됨)
- `Assets/Prefebs/Player/Magicain_Player.prefab` — `StatusEffect_Airborne`, `StatusEffect_Slow` 컴포넌트 부착 (Golem 스킬의 피격 대상)
- `Assets/Prefebs/Player/Golem_Player.prefab` — `StatusEffect_Airborne`, `StatusEffect_Slow` 컴포넌트 부착 (**모든 플레이어가 컴포넌트를 보유**하도록 부착 — `Player_Skill.CheckCanUseSkill`의 공중 게이트가 기반 클래스에서 `GetComponent`로 컴포넌트를 참조하므로, 컴포넌트가 없으면 Golem 스킬 사용 시 NullRef 발생. Apply 미호출 시 비활성 상태이므로 동작에는 영향 없음)
- `Assets/Resources/Data/SkillData/Golem_Mouse_Skill.asset` — 추가된 띄움 힘·슬로우 배율·슬로우 지속시간 값 입력

### 건드리지 않을 파일/시스템
- `SkillCastRange.cs`, `SkillRange.cs` — 데이터 기반 발동이므로 스킬 클래스 자체는 수정 불필요
- `Golem_Skill.cs`, `Magician_Skill.cs` — 스킬 발동 경로는 변경 없음
- 기존 넉백(`ApplyKnockback`) 로직 — 유지
- `DoubleAxe.prefab` — 이번 작업 대상 아님

## 4. 구현 단계

### Step 1. SkillData에 상태이상 수치 필드 추가
- 작업 내용: `SkillData.cs`에 `[SerializeField] private float _fLaunchForce = 0f;`(공중 띄움 상승 속도), `_fSlowMultiplier = 1f;`(슬로우 배율, 0~1), `_fSlowDuration = 0f;`(슬로우 지속시간 초) 추가 + 각각 public 게터. 기본값은 "효과 없음"으로 설정해 기존 스킬 동작을 보존(`fKnockback` 패턴과 동일).
- 완료 기준: 컴파일 통과, 기존 에셋에서 기본값이 효과를 발생시키지 않음.
- 예상 리스크: 없음(추가만 함).

### Step 2. 공중 띄움 상태이상 컴포넌트 생성 (StatusEffect_Airborne)
- 작업 내용: NetworkBehaviour로 작성. 서버 권위.
  - `net_isAirborne`(NetworkVariable<bool>, 읽기 Everyone / 쓰기 Server) — 공중 상태 공개. `IsAirborne` 게터. **공중 상태는 다른 클라·스킬 게이트(`Player_Skill.CheckCanUseSkill`)가 읽어야 하므로 반드시 NetworkVariable로 공개한다.**
  - `Apply(float launchForce)` — 서버 전용. 펜딩 상승속도를 기록하고 `net_isAirborne = true`로 설정한 뒤, **착지 감지 코루틴을 서버에서 시작**한다.
  - 착지 감지 코루틴은 기존 `Player_Move.JumpRoutine` 패턴을 그대로 따른다:
    `yield return new WaitUntil(() => !cct.isGrounded)`(이륙 대기) → `yield return new WaitUntil(() => cct.isGrounded)`(착지 대기) → `net_isAirborne = false`(착지 시점에만 해제). 이로써 "점프와 동일 판정"이 성립한다.
  - `Player_Move`가 소비할 펜딩 상승속도 노출(`ConsumePendingLaunch(out float force)` 형태) — Player_Move가 다음 FixedUpdate에서 1회 소비해 `verticalVelocity`에 주입한다(Step 4에서 결선).
- 완료 기준: 함수 호출 시 서버에서 `IsAirborne`가 true가 되고, 펜딩 상승속도를 Player_Move가 1회 소비하며, 착지 시점에 코루틴이 `net_isAirborne`를 false로 되돌린다.
- 예상 리스크: NetworkVariable 동기화 타이밍 — 서버에서만 쓰기하므로 권위 유지. 코루틴은 서버 전용(`IsServer` 가드)으로 실행.

### Step 3. 슬로우 상태이상 컴포넌트 생성 (StatusEffect_Slow)
- 작업 내용: NetworkBehaviour로 작성. 서버 권위.
  - `Apply(float multiplier, float duration)` — 서버 전용. 배율·남은시간 기록.
  - `SpeedMultiplier` 게터 — 비활성 시 1f, 활성 시 배율. 서버에서 시간 경과로 남은시간 차감 후 만료 시 1f 복귀.
  - **동기화 방식**: `Player_Move.PlayerMove()`는 `FixedUpdate`에서 `if(IsServer)` 가드 하에 **서버에서만 실행**되는 서버 권위적 이동이다(현재 코드 구조). 따라서 슬로우 배율은 이동을 계산하는 **서버에서만 읽히므로 `SpeedMultiplier`는 서버 로컬 `float` 값으로 충분**하다. 슬로우로 감소된 위치는 서버가 CCT로 이동시키고 NetworkTransform으로 전 클라에 전파되므로 별도 NetworkVariable이 불필요하다. (공중 상태처럼 다른 클라/게이트가 직접 읽어야 하는 값만 NetworkVariable로 공개 — Step 2 참조.)
- 완료 기준: 함수 호출 후 지정 시간 동안 `SpeedMultiplier`가 배율을 반환하고, 만료 후 1f로 복귀.
- 예상 리스크: 슬로우 중복 적용 — 단순히 최신 값으로 갱신(덮어쓰기)하여 처리.

### Step 4. Player_Move 연동 (슬로우·띄움·점프 차단·착지)
- 작업 내용: 모든 연동은 `PlayerMove()`가 실행되는 **서버(`if(IsServer)`)에서 수행**된다. 슬로우로 변경된 위치는 NetworkTransform으로 전 클라에 전파되므로 클라 측 추가 처리는 불필요하다.
  - `PlayerMove()`의 속도 계산에 `StatusEffect_Slow.SpeedMultiplier`를 곱한다(서버에서 읽음 — Step 3의 동기화 방식 참조).
  - 공중 띄움: `StatusEffect_Airborne.ConsumePendingLaunch(out float force)`로 펜딩 상승속도를 1회 소비해 `verticalVelocity`에 주입(상승). 기존 `verticalVelocity` 흐름·중력·넉백 처리를 그대로 따라 점프와 동일 경로로 처리한다.
  - 착지 해제: `net_isAirborne` false 전환은 **Step 2의 착지 감지 코루틴(StatusEffect_Airborne 내부)이 담당**한다(`WaitUntil(!cct.isGrounded)` → `WaitUntil(cct.isGrounded)`). Player_Move는 별도 착지 처리를 추가하지 않고, 코루틴이 사용할 `cct` 참조만 공유하면 된다.
  - `PlayerJump()`(클라 입력 진입점): `StatusEffect_Airborne.IsAirborne`면 점프 차단(공중 중 재점프 방지).
  - **`Jump_ServerRpc()`(서버 검증)에도 동일 게이트 추가**: `if (airborne.IsAirborne) return;` — 기존 `net_isJumpPending` 검사가 클라·서버 양쪽에 있는 것과 동일하게, 공중 점프도 **서버에서 반드시 재검증**한다. RPC를 위조한 클라이언트가 공중 중 점프하는 것을 막는다(CLAUDE.md 클라이언트 입력 불신·서버 권위 원칙).
  - **`AnimManage()`의 IsGrounded 계산에 공중 상태 반영**: 현재 `anim.SetBool("IsGrounded", cct.isGrounded && !net_isJumpPending.Value)`를 `anim.SetBool("IsGrounded", cct.isGrounded && !net_isJumpPending.Value && !airborne.IsAirborne)`로 수정. 띄움 직후 1~2프레임 동안 `cct.isGrounded`가 true로 남아 IsGrounded가 플리커하는 것을 차단해, 점프와 동일한 애니 판정을 보장한다.
- 완료 기준: 피격 시 위로 솟구치고, 공중에서 점프 불가(클라·서버 양쪽 차단), 띄움 직후 IsGrounded 플리커 없음, 착지 시점에 코루틴이 공중 상태를 해제하며 속도가 정상 복귀(슬로우 시간 남아 있으면 슬로우 유지).
- 예상 리스크: 띄움 상승속도 적용 지점과 기존 중력/넉백 처리의 간섭 — 기존 `verticalVelocity` 흐름을 따르고, 띄움은 점프와 동일 경로(상승값 1회 주입)로 처리해 간섭 최소화.

### Step 5. Player_Skill 공중 스킬 차단
- 작업 내용: `CheckCanUseSkill`에 `StatusEffect_Airborne.IsAirborne` 검사 추가 — 공중이면 null 반환(발동 거부). (기존 IsSkilling/IsAttacking/IsHit 게이트와 동일 위치)
- 완료 기준: 공중에 떠 있는 동안 Q·마우스 스킬 발동이 거부됨.
- 예상 리스크: 없음(게이트 1줄 추가).

### Step 6. Skill.OnHitEnemy에서 상태이상 발동
- 작업 내용: `OnHitEnemy`의 Player 피격 분기에서, `Data.fLaunchForce > 0f`면 대상의 `StatusEffect_Airborne.Apply(Data.fLaunchForce)` 호출, `Data.fSlowDuration > 0f`면 대상의 `StatusEffect_Slow.Apply(Data.fSlowMultiplier, Data.fSlowDuration)` 호출. (기존 `fKnockback > 0f` 가드 패턴과 동일하게 데이터 값으로 게이트)
- 완료 기준: FireExplosion만 띄움·슬로우 발동, 값이 없는 다른 스킬은 기존대로 동작.
- 예상 리스크: 같은 클라 자가 피격 방지(기존 OwnerClientId 비교)는 이미 상위에서 처리됨 — 그 분기 내부에서 호출하므로 안전.

### Step 7. 에디터 결선 (프리팹·에셋) — unity-cli 사용
- 작업 내용:
  - `Magicain_Player.prefab`과 `Golem_Player.prefab` **양쪽 모두**에 `StatusEffect_Airborne`, `StatusEffect_Slow` 컴포넌트 부착(신규 스크립트 `.meta` guid로 검증 후 부착). 모든 플레이어가 컴포넌트를 보유해야 `Player_Skill.CheckCanUseSkill`의 `GetComponent` 참조에서 NullRef가 발생하지 않는다(Golem은 Apply 미호출로 비활성 유지).
  - `Golem_Mouse_Skill.asset`에 `_fLaunchForce`, `_fSlowMultiplier`, `_fSlowDuration` 값 입력(초기 튜닝값: 띄움 힘은 점프 상승값보다 크게, 슬로우 배율 약 0.5, 지속 2초).
- 완료 기준: 두 프리팹에 컴포넌트가 정상 부착되고 에셋 값이 반영됨.
- 예상 리스크: 신규 스크립트 컴파일 후에야 컴포넌트 부착 가능 — Step 1~6 완료 및 Unity 컴파일 성공 후 진행. `Assets/Reimport All` 절대 금지.

## 5. 가정 및 제약
- Golem 우클릭 스킬은 `SkillType.FireExplosion`이며, `Fire_Explosion.prefab`은 `SkillRange.cs`(guid `c0eeb74819d756a44a68c95a7db1ab11`) 컴포넌트를 사용한다(확인 완료).
- Golem 우클릭 스킬의 데이터 에셋은 `Golem_Mouse_Skill.asset`이다.
- 띄움·슬로우 효과가 실제로 발동하는 피격 대상은 상대 플레이어인 Magician(`Magicain_Player.prefab`)이다. 다만 `Player_Skill.CheckCanUseSkill`의 공중 게이트가 기반 클래스에서 `GetComponent<StatusEffect_Airborne>()`로 참조하므로, **두 플레이어 프리팹(Golem·Magician) 모두 컴포넌트를 보유**해야 NullRef가 발생하지 않는다. Golem은 컴포넌트만 보유하고 Apply가 호출되지 않아 비활성 상태로 유지된다(향후 양방향 PvP 확장과도 일치).
- 모든 판정(띄움·슬로우·차단)은 서버에서만 수행(서버 권위적). 상태 공개는 NetworkVariable로 전 클라 동기화.
- CLAUDE.md 규칙 준수: 외부 입력 필드는 `[SerializeField] private`, 스크립트 내부 `public` 변수 금지, 내부 경계 null 체크 금지(상태 컴포넌트는 **두 플레이어 프리팹 모두에 부착**하여 존재 보장 — 이 때문에 null 체크 없이 `GetComponent` 참조 가능).
- 띄움은 "점프와 동일 판정"이므로 별도 공중 물리를 새로 만들지 않고 기존 점프/중력 흐름을 재사용한다.

## 6. 검증 방법
- **Step 1·6**: Unity 컴파일 성공(에러 0). 다른 스킬(Electric/Ice)은 기본값으로 효과 미발생 확인.
- **Step 2·3**: 컴파일 성공 및 함수 호출 시 상태/배율 변화(에디터 인스펙터 또는 로그로 확인).
- **Step 4·5**: 플레이모드에서 Golem 우클릭으로 Magician 피격 시 — (1) 위로 솟구침, (2) 공중에서 점프 키 무시, (3) 공중에서 스킬 키 무시, (4) 피격 순간부터 이동 속도 감소, (5) 착지 후 공중 상태 해제·슬로우 시간 만료 시 속도 복귀.
- **Step 7**: 프리팹에 컴포넌트 부착·에셋 값 반영 확인.
- **전체 최종 시나리오**: 2인(호스트=Golem, 클라=Magician) 접속 → Golem 우클릭 → Magician이 공중으로 뜨고 슬로우 적용 → 공중 중 점프·스킬 불가 → 지정 시간 후 슬로우 해제. 모든 판정이 서버에서 일어나고 양 클라에 동일하게 보임.
