# PLAN.md

## 1. 작업 개요
- 목표: 골렘의 근접 공격(무기 트리거 콜라이더 → 서버 권위 데미지/넉백) 시스템 구현
- 브랜치: `Agents_1`
- 작업 디렉토리: `C:/Unity/Golem_VS_Magician_Agents_1`

## 2. 명세 요약 (TASK.md)
- **콜라이더**: 골렘 손 뼈에 부착된 무기(DoubleAxe)에 IsTrigger 콜라이더 추가, 무기보다 약간 크게. / 완료: 공격 시 무기 영역에 트리거 반응.
- **활성화 타이밍**: 공격 애니메이션 시작 시 AnimationEvent로 콜라이더 ON, 종료 시 OFF. / 완료: 스윙 구간에만 콜라이더 활성.
- **충돌 판정**: 대상 = "Player" 태그 중 자기 자신 제외. 공격 1회당 동일 객체 최초 1회만 처리(히트 목록). 히트 목록은 콜라이더 활성 시점에 초기화. / 완료: 한 번 휘둘러 같은 적에게 데미지 1회.
- **데미지(서버 권위)**: 클라가 OnTriggerEnter 감지 → ServerRpc 호출. 서버가 피격 대상 Stat의 HP를 공격자 Stat의 공격력만큼 차감. HP는 NetworkVariable로 동기화. / 완료: 피격 시 모든 클라에서 HP 슬라이더 감소.
- **넉백**: 충돌 방향(공격자→피격자)으로 적용, 강도는 Inspector 노출.
  - ⚠️ **TASK.md 명세에서 의도적 이탈(사용자 승인)**: TASK는 "피격 클라가 Rigidbody에 force 적용 + ClientRpc 전달"이나, 현 프로젝트는 **CharacterController + 서버 권위 NetworkTransform**(Rigidbody 없음, 클라 force가 서버에 덮어써짐). 따라서 **서버 권위 방식**으로 구현: 서버가 피격 대상의 CharacterController에 넉백 속도를 부여해 감쇠 이동시킨다. ClientRpc/Rigidbody 추가 없음. / 완료: 피격 시 모든 클라에서 대상이 충돌 반대 방향으로 밀려남.

## 3. 영향 범위

### 신규 생성 파일
- `Assets/Script/Player/Golem_AttackHitbox.cs` — 무기 트리거 감지 + 히트 목록 관리(클라), 콜라이더 ON/OFF.

### 수정 파일 (스크립트)
- `Assets/Script/AnimEventRelay.cs` — 공격 시작/종료 이벤트 중계 메서드 추가.
- `Assets/Script/Player/Player_UpperBody.cs` — 히트박스 ON/OFF용 virtual 훅 추가.
- `Assets/Script/Player/Golem_UpperBody.cs` — 훅 override, 넉백 강도 필드, 데미지/넉백 ServerRpc.
- `Assets/Script/Player/Player_Move.cs` — 서버 권위 넉백 속도 적용 메서드 + 이동 처리에 반영.

### 수정 대상 (에셋 — 모두 사전 승인 후 변경)
- `Assets/Prefebs/Player/DoubleAxe.prefab` — IsTrigger BoxCollider + 트리거 감지용 Kinematic Rigidbody + `Golem_AttackHitbox` 추가(콜라이더 기본 비활성).
- `Assets/Prefebs/Player/Golem_Player.prefab` — `SKM_Golem (2)`(Animator 보유 객체)에 `AnimEventRelay` 추가, `Golem_UpperBody`의 히트박스 참조·넉백 강도 인스펙터 연결.
- `Assets/Animation/GolemAni/Standing Melee Attack Horizontal.anim` (guid `6153120c223df1449a2311beb5f37647`) — AnimationEvent 2개 추가.

### 건드리지 않을 것
- `Magicain_Player.prefab`, `Magician_UpperBody.cs`, `SkillProjectile.cs` 등 매지션/스킬 계열 (참고만, 변경 없음).
- 기존 이동/점프/카메라/HP 슬라이더 로직.
- `Reimport All` 절대 실행 금지.

## 4. 구현 단계

### Step 1. Player_Move — 서버 권위 넉백
- 작업 내용:
  - `[SerializeField] private float fKnockbackDecay = 5f;` (감쇠율) 추가.
  - `protected Vector3 vKnockback;` 필드 추가.
  - `public void ApplyKnockback(Vector3 dir, float strength)` — **서버에서만 호출**, `vKnockback = dir.normalized * strength;` (dir의 Y는 0으로).
  - `PlayerMove()`에서 `vMoveDir`에 `vKnockback` 합산 후 `cct.Move`, 이후 `vKnockback = Vector3.MoveTowards(vKnockback, Vector3.zero, fKnockbackDecay * Time.deltaTime);`로 감쇠.
- 완료 기준: 서버에서 `ApplyKnockback` 호출 시 대상이 해당 방향으로 밀렸다가 멈춤(컴파일 통과 + 임시 호출 테스트).
- 예상 리스크: 중력 처리(verticalVelocity)와 합산 순서 충돌 — Y축은 기존 중력 유지, 넉백은 XZ만.

### Step 2. Golem_AttackHitbox — 무기 트리거 감지(클라)
- 작업 내용: `Assets/Script/Player/Golem_AttackHitbox.cs` (MonoBehaviour, 무기에 부착).
  - `[SerializeField] private Collider hitCollider;`
  - `private Golem_UpperBody owner;`, `private NetworkObject ownerNetObj;`
  - `private readonly HashSet<ulong> hitSet = new();`
  - **초기화 (`Start()` — 기존 `AnimEventRelay`와 동일 패턴)**:
    - `owner = GetComponentInParent<Golem_UpperBody>();`
    - `ownerNetObj = owner != null ? owner.GetComponent<NetworkObject>() : null;`
    - (둘 다 인스턴스화 시점부터 유효한 컴포넌트 참조이며, 콜라이더 기본 비활성 → `OnTriggerEnter`가 `Start` 전 발생할 레이스 없음)
  - `public void EnableHitbox()` → `hitSet.Clear(); hitCollider.enabled = true;`
  - `public void DisableHitbox()` → `hitCollider.enabled = false;`
  - `OnTriggerEnter(Collider other)`:
    1. `if (owner == null || owner.IsOwner == false) return;` (공격자 소유 클라만 RPC 전송)
    2. `if (!other.CompareTag("Player")) return;`
    3. 대상 `NetworkObject tgt = other.GetComponent<NetworkObject>();` 없으면 return.
    4. `if (tgt.NetworkObjectId == ownerNetObj.NetworkObjectId) return;` (자기 자신 제외)
    5. `if (!hitSet.Add(tgt.NetworkObjectId)) return;` (중복 차단)
    6. `owner.ReportHit_ServerRpc(tgt.NetworkObjectId);`
- 완료 기준: 콜라이더 활성 중 적 진입 시 1회만 RPC 호출(로그로 확인).
- 예상 리스크: 트리거 콜백 미발생 — 무기에 **Kinematic Rigidbody** 필요(Step 5에서 부여). 콜라이더는 기본 비활성으로 시작.

### Step 3. 이벤트 중계 (AnimEventRelay + Player_UpperBody)
- 작업 내용:
  - `AnimEventRelay.cs`: `void OnGolemAttackStart() => playerUpper?.OnAttackHitboxOn();`, `void OnGolemAttackEnd() => playerUpper?.OnAttackHitboxOff();` 추가(기존 `OnFireSkill` 유지).
  - `Player_UpperBody.cs`: `virtual public void OnAttackHitboxOn() {}`, `virtual public void OnAttackHitboxOff() {}` 추가.
- 완료 기준: 컴파일 통과, 골렘 외 캐릭터 동작에 영향 없음(빈 가상 메서드).
- 예상 리스크: 메서드명이 AnimationEvent 함수명과 정확히 일치해야 함(대소문자 포함).

### Step 4. Golem_UpperBody — 훅 override + 데미지/넉백 RPC
- 작업 내용:
  - `[SerializeField] private Golem_AttackHitbox hitbox;`
  - `[SerializeField] private float fKnockbackStrength = 8f;` (TASK: 넉백 강도 인스펙터 노출)
  - `public override void OnAttackHitboxOn() => hitbox.EnableHitbox();`
  - `public override void OnAttackHitboxOff() => hitbox.DisableHitbox();`
  - `[ServerRpc] public void ReportHit_ServerRpc(ulong targetNetId)`:
    1. 서버 검증: `NetworkManager.SpawnManager.SpawnedObjects`에 id 존재 확인, 없으면 return.
    2. 대상 `NetworkObject` 획득, `if (tgt.NetworkObjectId == NetworkObjectId) return;` (자기 제외 재검증 — 클라 입력 불신).
    3. `Stat targetStat = tgt.GetComponent<Stat>();` 없으면 return.
    4. 데미지: `targetStat.pHp = -GetComponent<Stat>().pDamage;` (서버 전용 쓰기, NetworkVariable 동기화).
    5. 넉백 방향 = `(tgt.transform.position - transform.position)`의 XZ 정규화.
    6. `tgt.GetComponent<Player_Move>()?.ApplyKnockback(dir, fKnockbackStrength);` (서버 실행).
- 완료 기준: 클라 RPC 수신 시 서버에서 HP 차감 + 넉백, 모든 클라 슬라이더/위치 동기화.
- 예상 리스크: `Stat.pHp` 세터는 델타 입력이므로 음수 전달이 정상(기존 `SkillProjectile` 동일 패턴 확인됨).

### Step 5. 무기 프리팹 구성 (DoubleAxe.prefab) — 승인 후 변경
- 작업 내용:
  - BoxCollider 추가, `Is Trigger = ON`, 크기 무기 메시보다 약간 크게.
  - Rigidbody 추가, `Is Kinematic = ON`, `Use Gravity = OFF` (트리거 감지 전용, 물리 이동 없음).
  - `Golem_AttackHitbox` 추가, `hitCollider`에 위 BoxCollider 연결, 콜라이더 컴포넌트 `enabled = false`(기본 비활성).
- 완료 기준: 골렘 프리팹에 무기가 트리거 콜라이더를 갖고 기본 비활성 상태로 인스턴스화.
- 예상 리스크: DoubleAxe는 `SKM_Golem (2)` 모델의 손 뼈에 중첩 → 프리팹 자체 수정이 골렘 인스턴스에 반영되는지 확인 필요(unity-cli `LoadPrefabContents`로 검증).

### Step 6. 애니메이션 이벤트 추가 (Standing Melee Attack Horizontal.anim)
- 작업 내용: 스윙 시작 프레임에 `OnGolemAttackStart`, 스윙 종료(혹은 상체 레이어 페이드 시작 `fAttackExitStart=0.7` 이전) 프레임에 `OnGolemAttackEnd` 이벤트 추가.
- 완료 기준: 공격 재생 시 콜라이더가 스윙 구간에만 활성/비활성.
- 예상 리스크: 이벤트 함수는 Animator와 같은 객체(`SKM_Golem (2)`)의 컴포넌트에서 검색됨 → Step 7로 `AnimEventRelay` 부착 필요.

### Step 7. 골렘 프리팹 배선 (Golem_Player.prefab) — 승인 후 변경
- 작업 내용:
  - `SKM_Golem (2)`(Animator 보유 객체)에 `AnimEventRelay` 컴포넌트 추가.
  - 루트 `Golem_UpperBody`의 `hitbox` 필드에 무기의 `Golem_AttackHitbox` 연결, `fKnockbackStrength` 값 설정.
- 완료 기준: 인스펙터 참조 누락 없음, 플레이모드에서 공격→데미지→넉백 전 과정 동작.
- 예상 리스크: 중첩 프리팹 객체에 컴포넌트 추가는 override로 남음 — 정상.

## 5. 가정 및 제약
- 넉백은 **서버 권위 CharacterController** 방식으로 구현(사용자 승인). Rigidbody 기반 클라 넉백/ClientNetworkTransform 전환은 하지 않음.
- 공격 입력(`OnAttack`)이 이미 `UpperAttack` 상태를 재생함(기존 `Player_UpperBody` 로직) — 이 흐름은 변경하지 않고 클립 이벤트로 히트박스만 제어.
- 충돌 감지는 TASK대로 **클라(공격자 소유)에서 수행 후 ServerRpc** — 단, 서버는 입력을 신뢰하지 않고 대상 존재/자기제외/Stat 유무를 재검증(CLAUDE.md 보안 원칙).
- `Stat.pHp` 세터는 델타 누적 방식(기존 동작 유지).
- 무기 = `DoubleAxe`(guid `7c7914412c682494dab87f3b00b75fa4`), 골렘 모델에만 사용됨.
- 코드 작성 규칙 준수: 외부 입력 필드는 `[SerializeField] + private/protected`, 스크립트 내 `public` 변수 금지.

## 6. 검증 방법
- **Step별**: 각 스크립트 수정 후 Unity 컴파일 에러 0 확인(unity-cli).
- **Step 1 단위**: 임시 호출 또는 `[ContextMenu]`로 `ApplyKnockback` 호출 시 대상이 밀리는지 확인.
- **Step 2~4 통합**: 호스트+클라 2개 접속 → 골렘으로 매지션 공격 →
  1. 콜라이더가 스윙 구간에만 활성,
  2. 1회 스윙당 데미지 1회(HP 슬라이더 1회 감소, 모든 클라 동기화),
  3. 피격자가 공격 반대 방향으로 넉백되어 양쪽 화면 위치 동기화.
- **자기/중복 검증**: 골렘이 자기 자신 미피격, 콜라이더 유지 중 같은 적 재진입 시 추가 데미지 없음.
- **최종 시나리오**: 골렘 공격 연타 시 매 공격마다 히트 목록 초기화되어 정상적으로 1회씩 데미지 적용.
