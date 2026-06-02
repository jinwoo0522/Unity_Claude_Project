# PLAN.md

## 1. 작업 개요
- 목표: 클라이언트에서 스킬 풀링 객체가 (1) 디스폰되지 않아 active가 false로 안 되는 문제와, (2) 재사용 시 히트 이펙트가 즉시 출력되는 문제를 수정한다.
- 브랜치: `Agents_1`
- 작업 디렉토리: `C:/Unity/Golem_VS_Magician_Agents_1`

## 2. 명세 요약 (TASK.md)
- **무엇을**: 멀티플레이 스킬 풀링 버그 수정 / **완료 기준**: 호스트·클라 모두 동일하게 동작
  - 호스트는 정상, 클라에서 스킬 풀링이 `active=false`가 되지 않음 → 풀로 반환되어 비활성화되어야 함.
  - 호스트가 스킬 사용 후 클라가 스킬 사용 시, 객체의 히트 이펙트가 즉시 출력됨 → 재사용 시 비행 이펙트로 정상 시작되어야 함.
  - 풀링 시스템은 각 클라마다 이펙트를 풀링해야 함 → 이미 프로세스별 `SkillPool` 1개로 충족됨(구조 변경 불필요).

## 3. 근본 원인 (조사 결과)
- `SkillProjectile.Init()`은 `SkillPool.UseSkill()`(`Assets/Script/Skill/SkillPool.cs:69`)에서 **서버에서만** 호출됨. 클라 프로세스의 풀링 인스턴스는 `Init()`이 호출되지 않아 `Data == null`, `hitParticle == null` 상태.
- 그 결과 클라에서:
  - `Skill.OnEnable()`(`Skill.cs:24`)·`SkillProjectile.OnEnable()`(`SkillProjectile.cs:35`)의 `if (Data == null) return;` 가드 때문에 재사용 시 시각 상태 리셋(`projectileEffect` 켜기 / `hitEffect` 끄기 / `bHitShown=false` / `hitParticle.Clear()`)이 실행되지 않음 → 버그 ②.
  - 클라가 시전한 스킬은 owner=클라인데 그 클라에서 `hitParticle == null` → `CheckDespawn()`(`SkillProjectile.cs:99`)이 막혀 `DespawnSkill_ServerRpc`가 안 나감 → 풀 반환/비활성화 안 됨 → 버그 ①.
- 핵심: 리셋·디스폰 로직은 `Data` 값이 필요 없고 `hitParticle`(직렬화된 `hitEffect` 자식에서 로컬로 획득 가능)만 있으면 됨. 따라서 **네트워크 동기화 없이** `hitParticle`을 모든 피어에서 캐싱하고 `Data` 가드를 제거하는 최소 수정으로 해결 가능.

## 4. 영향 범위
- 수정할 파일:
  - `Assets/Script/Skill/SkillProjectile.cs` — `Awake`에서 `hitParticle` 캐싱, `OnEnable`의 `Data==null` 가드 제거, `Init`에서 중복된 `hitParticle` 획득 라인 제거.
  - `Assets/Script/Skill/Skill.cs` — `OnEnable`의 `Data==null` 가드 제거.
- 새로 생성할 파일: 없음.
- 건드리지 않을 파일/시스템 (명시):
  - `SkillPool.cs`, `PooledHandler.cs` (풀 구조·핸들러 정상).
  - `Magician_UpperBody.cs`, `GameManager.cs` (시전/생성 흐름 정상, 프로세스별 풀 정상).
  - `ElectricSkill.prefab` 및 모든 에디터/Inspector 설정 (코드만 수정, NetworkObject·NetworkTransform 정상 확인됨).
  - 발사체 이동/히트 판정(서버 권위) 로직.

## 5. 구현 단계

### Step 1. `hitParticle` 캐싱을 `Init` → `Awake`로 이동
- 작업 내용: `SkillProjectile`에 `Awake()` 추가하여 `hitParticle = hitEffect.GetComponent<ParticleSystem>();` 수행. `Init()` 내 동일 라인 제거. (`Awake`는 모든 피어·모든 인스턴스에서 1회 실행되며 `OnEnable`보다 먼저 실행되므로 리셋 시점에 항상 유효.)
- 완료 기준: 클라 인스턴스에서도 `hitParticle`이 non-null로 캐싱됨. 컴파일 성공.
- 예상 리스크: `hitEffect`가 비할당이면 null 발생 → 프리팹에 직렬화 할당 확인됨(`ElectricSkill.prefab:78`), 안전을 위해 리셋 시 null 가드 유지.

### Step 2. 클라에서도 리셋이 실행되도록 `Data==null` 가드 제거
- 작업 내용:
  - `Skill.OnEnable()`(`Skill.cs:24`): `if (Data == null) return;` 제거 → `bHitShown = false;`만 남김(항상 안전).
  - `SkillProjectile.OnEnable()`(`SkillProjectile.cs:35`): `if (Data == null) return;` 제거. `hitParticle?.Clear();` 형태로 null 안전 처리.
- 완료 기준: 서버·클라 모두 풀 재사용 시 `bHitShown=false`, `projectileEffect` 활성, `hitEffect` 비활성으로 리셋됨.
- 예상 리스크: 최초 풀 생성 시점(`PooledHandler.CreateNetworkObject`)에 `OnEnable`이 호출되어도 직렬화 자식 이펙트는 존재하므로 안전. `Awake`(Step1)가 선행되어 `hitParticle` 유효.

### Step 3. 주석 정합성 정리 (선택, 최소)
- 작업 내용: `Data==null` 가드 관련 기존 주석(`Skill.cs:23`, `SkillProjectile` OnEnable 주변)을 변경된 동작에 맞게 갱신.
- 완료 기준: 주석이 실제 동작과 일치.
- 예상 리스크: 없음. 로직 변경 없음.

## 6. 가정 및 제약
- 가정:
  - 각 프로세스(호스트/각 클라)는 자체 `GameManager`/`SkillPool`/`PooledHandler`를 가진다(프로세스별 풀링) — 코드상 확인됨.
  - NGO가 디스폰 시 등록된 `INetworkPrefabInstanceHandler.Destroy`를 모든 피어에서 호출하여 풀로 반환한다(호스트 시전 스킬이 정상 동작하는 사실로 검증됨).
  - `Awake`는 `OnEnable`보다 먼저, `hitEffect` 자식은 인스턴스화 시점부터 존재한다.
- 제약(CLAUDE.md):
  - 서버 권위 유지 — 이동/히트 판정은 서버에서만(`IsServer` 가드 유지). 클라는 시각/디스폰 트리거만.
  - 코드만 수정, 에디터/프리팹/`Reimport All` 변경 없음. 변경 전 사용자 승인 필요.
  - `public` 필드 신규 추가 금지(은닉화). (`projectileEffect`/`hitEffect`의 기존 `public`은 본 작업 범위 밖이므로 변경하지 않음.)

## 7. 검증 방법
- 빌드/컴파일: Unity 에디터에서 컴파일 에러 없음 확인.
- 플레이 검증(호스트 1 + 클라 1, 수동):
  1. 클라가 스킬 시전 → 클라 화면에서 발사체가 비행 이펙트로 시작되고, 히트 후 히트 이펙트 재생이 끝나면 객체가 비활성(`active=false`)되어 풀로 반환되는지 확인 (버그 ① 해소).
  2. 호스트 시전 → 이어서 클라 시전을 반복 → 재사용된 객체가 히트 이펙트를 즉시 띄우지 않고 비행 이펙트로 정상 시작하는지 확인 (버그 ② 해소).
  3. 호스트 시전이 기존처럼 정상 동작하는지(회귀 없음) 확인.
- 성공 기준: 위 1~3이 모두 통과하고, 호스트·클라에서 풀링 동작이 동일.
