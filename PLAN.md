# PLAN.md

## 1. 작업 개요
- 목표: (1) 공중에서 무한 재점프되는 버그를 "착지 시점에만 게이트 해제"로 수정하고,
  (2) 골렘 기본공격·스킬 피격 시 서버 권위로 피격 애니메이션 + 체력 감소가 함께 발생하도록 플레이어 측 피격 함수를 구현한다.
- 브랜치: `Agents_1`
- 작업 디렉토리: `C:/Unity/Golem_VS_Magician_Agents_1`

## 2. 명세 요약 (TASK.md)

### 작업 1 — 점프
- **무엇을**: 점프가 바닥에 닿지 않아도 계속 수행되는 버그 수정.
- **어떻게**: 점프 게이트를 "타이머(`fJumpDelay`)"가 아닌 "착지(`cct.isGrounded`)" 시점에 해제. 검증은 서버에서 수행(서버 권위).
- **완료 기준**: 점프는 1회 수행 후 **바닥에 닿을 때까지** 다시 수행되지 않는다.

### 작업 2 — 피격 애니메이션
- **무엇을**: 골렘 기본공격에 맞거나 스킬에 맞았을 때, 현재 HP 프로퍼티만 직접 감소시키는 부분을 플레이어 측 **피격 함수**로 대체.
- **어떻게**: 피격 함수가 (a) 피격 애니메이션 재생 + (b) 체력 감소를 함께 처리. 공격 중 피격 시 공격이 종료되고 즉시 피격 애니로 전환. 피격 중에는 공격 불가.
- **완료 기준**: 두 데미지 경로(골렘 기본공격/스킬) 모두 피격 애니 + 체력 감소가 동시에 발생하며, 피격 중 공격 입력이 차단된다.

## 3. 근본 원인 / 현황 (조사 결과)

### 작업 1
- `Player_Move.JumpDelay()`(`Player_Move.cs:155-161`)가 `fJumpDelay` 시간이 지나면 **착지 여부와 무관하게** `isJumpPending = false`로 리셋 → 공중에서 게이트가 풀려 재점프됨.
- 게이트 `isJumpPending`은 현재 `protected bool` 평범한 필드로, 오너→서버 단방향(`SubmitJumpPending_ServerRpc`)으로만 전달됨. 착지 판정은 서버(`PlayerMove`)에만 있으므로 서버가 게이트를 풀고 이를 오너에 동기화해야 한다.
- `Player_Move`를 상속하는 `Magician_Move`/`Golem_Move`는 **빈 클래스**라 점프 필드/메서드 오버라이드 없음 → 필드 변경 안전.

### 작업 2
- 피격 애니 인프라는 `Player_UpperBody`에 **이미 존재**: `UpperHit` 상태, `IsHit` 프로퍼티(`Player_UpperBody.cs:9`), `OnAttack`의 `if(IsHit) return`(`:72`), 상부 레이어 가중치 동기화(`net_AnimWeight` NetworkVariable). Magician/Golem 애니메이터 둘 다 `UpperHit`/`UpperAttack` 상태 + `UpperBody` 레이어 보유(확인됨).
- 그러나 실제 데미지 경로는 피격 애니와 무관하게 HP만 직접 감소(둘 다 **서버 실행**):
  - 골렘 기본공격: `Golem_UpperBody.ReportHit_ServerRpc` → `targetStat.pHp = -pDamage`(`Golem_UpperBody.cs:27`).
  - 스킬: `SkillProjectile.OnHit` → `targetStat.pHp = -fDamage`(`SkillProjectile.cs:87`).
- HP는 `Stat.pHp`(서버 write NetworkVariable)로 관리됨(`Stat.cs:12-26`). 기존 디버그용 `TakeHit()`(`Player_UpperBody.cs:77-82`)은 오너 ServerRpc 경로라 서버에서 호출 불가 → 서버 권위 경로를 신규 추가해야 함.

## 4. 영향 범위

수정할 파일:
- `Assets/Script/Player/Player_Move.cs` — 점프 게이트를 서버 권위 NetworkVariable로 전환, 착지 기준 리셋.
- `Assets/Script/Player/Player_UpperBody.cs` — 서버 권위 피격 함수 `TakeHit(float)` 신규 추가, 기존 디버그 `TakeHit()` 정리.
- `Assets/Script/Player/Golem_UpperBody.cs` — 기본공격 데미지 적용을 `TakeHit(float)` 호출로 변경.
- `Assets/Script/Skill/SkillProjectile.cs` — 스킬 데미지 적용을 `TakeHit(float)` 호출로 변경.

새로 생성할 파일: 없음.

건드리지 않을 파일/시스템 (명시):
- `Stat.cs` — HP/데미지 관리 구조 유지(`pHp` setter를 통한 감소만 사용).
- `Magician_Move.cs`/`Golem_Move.cs` — 빈 상속 클래스, 변경 없음.
- 애니메이터 컨트롤러/프리팹/Inspector 설정 — `UpperHit`·`Jump`·`IsGrounded` 등 이미 존재, 코드만 수정. `Reimport All` 금지.
- 골렘 히트박스(`Golem_AttackHitbox.cs`)/넉백 로직 — 정상, 유지.
- 발사체 이동/풀링/디스폰 로직(스킬) — 본 작업 범위 밖.

## 5. 구현 단계

### Step 1. 점프 게이트를 서버 권위 NetworkVariable로 전환
- 작업 내용 (`Player_Move.cs`):
  - `protected bool isJumpPending = false;`(`:20`)를 제거하고 서버 write NetworkVariable로 교체:
    ```csharp
    protected NetworkVariable<bool> net_isJumpPending = new NetworkVariable<bool>(
        false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    ```
  - `AnimManage()`의 `anim.SetBool("IsGrounded", cct.isGrounded && !isJumpPending);`(`:134`)을 `!net_isJumpPending.Value`로 변경.
- 완료 기준: 컴파일 성공, `IsGrounded` 애니 파라미터가 기존과 동일하게 동작.
- 예상 리스크: 없음(필드 의미 동일, 동기화 방향만 서버→전체로 확장).

### Step 2. 점프 입력→서버 요청, 서버 검증·적용·착지 리셋
- 작업 내용 (`Player_Move.cs`):
  - `PlayerJump()`(`:137-153`)를 오너 입력 처리로 단순화 — 게이트 검사 후 애니 트리거(기존 유지) + 서버 요청. 기존 `Debug.Log` 정리:
    ```csharp
    protected virtual void PlayerJump()
    {
        if (playerUpper != null && playerUpper.IsHit) return;
        if (net_isJumpPending.Value) return;       // 착지 전 재점프 차단
        net_anim.SetTrigger("Jump");               // 오너 즉시 애니(반응성 유지)
        Jump_ServerRpc();                          // 서버 검증·적용 요청
    }
    ```
  - 신규 `Jump_ServerRpc` — 서버 권위 검증(바닥일 때만 수락):
    ```csharp
    [ServerRpc]
    void Jump_ServerRpc()
    {
        if (net_isJumpPending.Value) return;
        if (!cct.isGrounded) return;               // 서버 검증: 클라 입력 불신
        net_isJumpPending.Value = true;
        StartCoroutine(JumpRoutine());
    }
    ```
  - `JumpDelay()`(`:155-161`)를 `JumpRoutine()`으로 교체 — 착지 시점에만 게이트 해제:
    ```csharp
    IEnumerator JumpRoutine()
    {
        yield return new WaitForSeconds(playerData.fJumpDelay);
        verticalVelocity = playerData.fJumpAmount;          // 서버에서 직접 적용
        yield return new WaitUntil(() => !cct.isGrounded);  // 이륙 대기
        yield return new WaitUntil(() => cct.isGrounded);   // 착지 대기
        net_isJumpPending.Value = false;                    // 착지 시점에만 해제
    }
    ```
  - 더 이상 쓰이지 않는 `SubmitVelocity_ServerRpc`(`:169-173`), `SubmitJumpPending_ServerRpc`(`:181-185`) 제거.
- 완료 기준: 점프 1회 후 착지 전까지 재점프 불가(공중 무한 점프 해소). 점프 애니/상승/하강/착지 정상.
- 예상 리스크:
  - 점프 후 `cct.isGrounded`가 끝내 false가 되지 않으면(약한 점프량·경사면) 코루틴이 착지 대기에 머물러 게이트가 안 풀릴 수 있음 → `fJumpAmount`가 실제 상승을 만드는지 플레이 검증으로 확인.
  - 오너가 바닥인데 서버가 순간적으로 공중으로 보는 지연 상황에서 애니만 재생되고 점프가 거절될 수 있음(희귀, 시각적 블립). 게이트가 공중 전 구간 true라 재점프 자체는 차단됨.

### Step 3. 서버 권위 피격 함수 `TakeHit(float)` 추가
- 작업 내용 (`Player_UpperBody.cs`):
  - 신규 서버 전용 피격 함수 — 체력 감소 + 피격 애니(공격 강제 종료):
    ```csharp
    // 서버 전용 — 피격: 체력 감소 + 피격 애니메이션(진행 중 공격 클립 대체)
    public void TakeHit(float damage)
    {
        if (!IsServer) return;

        GetComponent<Stat>().pHp = -damage;          // 체력 감소(서버 권위)

        state = UpperState.Hit;
        net_AccLerpTime.Value = 1f;
        net_AnimWeight.Value = 1f;                    // 상부 레이어 활성(전 클라 동기화)
        anim.Play("UpperHit", UpperBodyLayer, 0f);    // 서버 즉시 재생
        PlayHitAnim_ClientRpc();                       // 오너 포함 전 클라 동기화
    }

    [ClientRpc]
    void PlayHitAnim_ClientRpc()
    {
        state = UpperState.Hit;          // 오너 IsHit 게이트 즉시 true
        if (IsServer) return;            // 호스트는 이미 재생
        anim.Play("UpperHit", UpperBodyLayer, 0f);
    }
    ```
  - 기존 디버그 `[ContextMenu] TakeHit()`(`:77-82`)는 오너 ServerRpc 경로라 서버 호출과 충돌 → 제거(또는 `void DebugTakeHit() => TakeHit(10f);` 형태의 단순 디버그 훅으로 대체).
- 완료 기준: 서버에서 `TakeHit(damage)` 호출 시 전 클라에 `UpperHit` 재생, `Stat.pHp` 감소, 진행 중 공격 애니가 즉시 피격 애니로 대체됨.
- 예상 리스크:
  - 비공격 상태에서 피격 시 `net_AnimWeight`가 0→1로 바뀌며 `OnValueChanged`로 레이어 가중치 동기화(정상). 공격 중 피격 시 이미 1이라 변화 없음 → 가중치는 유지되고 클립만 교체되므로 정상.
  - `state`는 비동기 필드라 오너에서도 `ClientRpc`로 설정해야 `IsHit`가 즉시 true가 됨(공격 차단 보장). 위 코드에 반영.

### Step 4. 데미지 경로를 `TakeHit(float)` 호출로 변경
- 작업 내용:
  - `Golem_UpperBody.cs:24-27` — `Stat targetStat` 직접 감소를 피격 함수 호출로 교체:
    ```csharp
    Player_UpperBody targetUpper = tgt.GetComponent<Player_UpperBody>();
    if (targetUpper == null) return;
    targetUpper.TakeHit(GetComponent<Stat>().pDamage);
    ```
    (기존 `targetStat` 지역변수 제거. 넉백 적용 로직 `:29-31`은 그대로 유지.)
  - `SkillProjectile.cs:83-88` — 동일하게 교체:
    ```csharp
    Player_UpperBody targetUpper = output.collider.GetComponent<Player_UpperBody>();
    if (targetUpper != null) targetUpper.TakeHit(fDamage);
    ```
- 완료 기준: 골렘 기본공격/스킬 모두 `TakeHit`를 통해 체력 감소 + 피격 애니가 동시에 발생. 자기 발사체 자해 방지·태그 검사 등 기존 가드 유지.
- 예상 리스크: 대상에 `Player_UpperBody`가 없으면 호출 생략(`?.`/null 체크) → 비플레이어 대상 무피해. 현재 데미지 대상은 "Player" 태그 플레이어뿐이라 안전.

## 6. 가정 및 제약
- 가정:
  - Magician/Golem 프리팹 루트에 `Stat` + `Player_UpperBody`(파생) + `Player_Move`(파생) + `CharacterController` + `NetworkAnimator`가 함께 존재(코드 `GetComponent` 사용 전제, 기존 동작으로 검증됨).
  - 애니메이터의 `UpperBody`(레이어 1)에 `UpperHit`/`UpperAttack` 상태 존재(확인됨). `Jump` 트리거/`IsGrounded` bool 존재(확인됨).
  - `NetworkAnimator`는 현재 오너가 `SetTrigger("Jump")`를 호출해 정상 동작 중 → 본 계획은 점프 트리거를 **오너에 유지**하므로 권위 모드 변경 불필요.
  - `playerData.fJumpAmount`가 실제 상승(이륙)을 만든다(착지 대기 코루틴 전제).
- 제약(CLAUDE.md):
  - 서버 권위 유지 — 점프 검증(`cct.isGrounded`)·게이트 해제·체력 감소·피격 판정은 모두 서버에서 수행. 클라는 입력/요청과 시각 동기화만.
  - 은닉화 — 신규 필드는 `public` 금지. `net_isJumpPending`은 `protected`, ClientRpc는 `private`. `TakeHit(float)`는 외부(데미지 경로) 호출이 필요하므로 `public` 메서드로 노출(필드 아님).
  - 코드만 수정, 에디터/프리팹/`Reimport All` 변경 없음. 변경 전 사용자 승인.

## 7. 검증 방법
- 빌드/컴파일: Unity 에디터에서 컴파일 에러 없음 확인.
- 점프 검증(플레이모드):
  1. 지상에서 점프 → 1회 상승/하강/착지 후에만 재점프 가능. 공중에서 점프 입력 연타 시 추가 점프가 **발생하지 않음**(버그 해소).
  2. 점프 애니메이션이 기존처럼 즉시 재생되는지(반응성) 확인.
  3. 호스트/클라 양쪽에서 동일하게 동작하는지 확인.
- 피격 검증(호스트 1 + 클라 1):
  1. 골렘 기본공격으로 마법사 피격 → 피격 애니 재생 + HP 슬라이더 감소 동시 발생.
  2. 마법사가 공격(스킬 시전) 중 피격 → 공격 애니가 즉시 종료되고 피격 애니로 전환.
  3. 피격 중 공격 입력 → 공격이 수행되지 않음(차단). 피격 애니 종료(가중치 페이드아웃) 후 공격 재개 가능.
  4. 스킬 발사체로 대상 피격 시에도 1~3과 동일하게 피격 애니 + HP 감소 발생.
  5. 호스트/클라 모두에서 피격 애니가 동기화되어 보이는지 확인.
- 성공 기준: 위 점프 1~3, 피격 1~5가 모두 통과하고 회귀(기존 이동/공격/스킬 동작)가 없음.
