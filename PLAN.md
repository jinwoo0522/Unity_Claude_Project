# PLAN.md

## 1. 작업 개요
- 목표: Golem/Magician 점프 버그 수정 — 스페이스바 입력 시 점프 애니메이션만 잠깐 재생되고 실제 점프가 발생하지 않는 문제 해결
- 브랜치: `Agents_1`
- 작업 디렉토리: `C:\Unity\Golem_VS_Magician_Agents_1`

## 2. 명세 요약 (TASK.md)
- 무엇을: 점프 상태에 진입했다가 점프가 발생하지 않고 Idle로 되돌아가는 버그 수정
- 어떻게: 점프 입력 → 서버 검증 → 점프 실행 경로의 결함 제거
- 완료 기준: 스페이스바를 누르면 (지상에 있을 때) 매번 점프가 발생한다

## 3. 버그 원인 분석 (조사로 확인한 사실)

대상 코드: `Assets/Script/Player/Player_Move.cs` (Golem_Move/Magician_Move는 빈 파생 클래스 — 모든 로직이 이 파일에 있음)

1. **애니메이션과 물리의 분리**: `PlayerJump()`(147행)에서 오너가
   `net_anim.SetTrigger("Jump")`(152행)와 `Jump_ServerRpc()`(153행)를 따로 호출한다.
   - 프리팹의 NetworkAnimator는 **서버 권위형**(NGO 기본 `NetworkAnimator`, guid e8d0727d로 확인).
     NGO 소스(`NetworkAnimator.cs` SetTrigger → `SendServerAnimTriggerRpc`) 확인 결과,
     오너의 트리거는 **서버 검증과 무관하게 무조건** 서버 애니메이터에 적용되고 전 클라이언트에 전파된다.
   - 반면 실제 점프(수직 속도 적용)는 `Jump_ServerRpc`(156~163행)의 `!cct.isGrounded` 검증을 통과해야만 실행된다.
2. **서버 접지 판정의 불안정**: `Jump_ServerRpc`는 RPC 처리 시점(EarlyUpdate, 해당 프레임 FixedUpdate 이전)에
   직전 물리 스텝의 `CharacterController.isGrounded`를 읽는다. 이 값은 지상 정지 상태에서도 간헐적으로
   false가 되는 플리커가 있어 RPC가 자주 기각된다.
3. **증상 재현 메커니즘**: RPC가 기각되면 `net_isJumpPending`이 false로 남고, 서버 `AnimManage()`(144행)가
   `IsGrounded` 파라미터를 true로 유지한다. 애니메이터(GolemAnimator/MagicianAnimator 동일 구조)의
   Jump → Idle 전이는 **Exit Time 없이** `IsGrounded == true` 조건으로 즉시 발동하므로,
   Jump 상태에 진입하자마자 Idle로 복귀한다. → "점프 상태 진입 → 점프 없음 → Idle 복귀, 가끔 성공"과 일치.

## 4. 영향 범위
- 수정할 파일: `Assets/Script/Player/Player_Move.cs` **1개 파일만 수정**
- 새로 생성할 파일: 없음
- 건드리지 않을 것: 애니메이터 컨트롤러(GolemAnimator/MagicianAnimator), 프리팹(Golem_Player/Magicain_Player),
  Player_Data 에셋, InputSystem_Actions, 파생 클래스(Golem_Move/Magician_Move), 그 외 모든 씬·에셋

## 5. 구현 단계

### Step 1. 서버 접지 유예(grounded grace) 도입
- 작업 내용 (`Player_Move.cs`):
  1. 필드 추가:
     ```csharp
     private float fLastGroundedTime;                  // 서버 전용 — 마지막 접지 시각
     private const float GroundedGraceTime = 0.15f;    // 접지 판정 유예
     ```
  2. `PlayerMove()`의 `cct.Move(...)` 호출(123행) 직후에 기록:
     ```csharp
     if (cct.isGrounded) fLastGroundedTime = Time.time;
     ```
  3. `Jump_ServerRpc()`의 `if (!cct.isGrounded) return;`(160행)을 다음으로 교체:
     ```csharp
     if (Time.time - fLastGroundedTime > GroundedGraceTime) return; // 서버 검증: 클라 입력 불신
     ```
- 완료 기준: 지상 정지 상태에서 점프 RPC가 isGrounded 플리커로 기각되지 않는다.
  공중(이륙 0.15초 후~착지 전)에서는 여전히 기각된다. 공중 재점프는 `net_isJumpPending` 게이트가 그대로 차단한다.
- 예상 리스크: 이륙 직후 0.15초 내 재점프 입력 — `net_isJumpPending == true`가 먼저 차단하므로 발생 불가.

### Step 2. 점프 애니메이션 트리거를 서버 검증 이후로 이동
- 작업 내용 (`Player_Move.cs`):
  1. `PlayerJump()`에서 `net_anim.SetTrigger("Jump");`(152행) 제거.
  2. `Jump_ServerRpc()`에서 검증 통과 후 `net_isJumpPending.Value = true;` 다음 줄에
     `net_anim.SetTrigger("Jump");` 추가. (서버 권위형 NetworkAnimator의 정상 사용 경로 —
     서버가 트리거를 적용하고 전 클라이언트에 전파)
- 완료 기준: 점프 애니메이션은 서버가 점프를 승인한 경우에만 재생된다.
  기각 시 애니메이션 깜빡임(Jump 진입 후 즉시 Idle)이 사라진다.
- 예상 리스크: 오너 체감 반응성 — 기존 코드도 서버 권위형이라 오너 로컬 즉시 재생이 아니었으므로
  (NGO 소스 1786행 `if (!IsServerAuthoritative()) InternalSetTrigger` — 서버 권위 모드에서는 로컬 미적용) 체감 변화 없음.

## 6. 가정 및 제약
- 가정: 버그 원인은 정적 분석(코드 + NGO 패키지 소스 + 애니메이터 YAML)으로 특정한 것이며,
  런타임 로그로 재확인하지 않았다. 단, 두 수정은 원인이 「isGrounded 플리커 기각」이든
  「트리거/RPC 도착 프레임 차이」이든 모두 증상을 제거한다 (애니메이션이 물리 승인에 종속되므로).
- 제약 (CLAUDE.md):
  - 서버 권위적 설계 유지 — 클라이언트는 입력만 전송, 검증·속도 적용은 서버에서만 (기존 구조 그대로)
  - 새 필드는 `private`, Inspector 노출 불필요하므로 상수(const) 사용
  - null 체크 추가 금지, 기존 코드 스타일(한국어 주석) 유지
  - 애니메이터·프리팹·에셋 수정 금지 (코드 1개 파일만)

## 7. 검증 방법
- Step 1 완료 후: 컴파일 에러 없음 확인 (unity-cli로 에디터 컴파일 상태 확인).
- Step 2 완료 후: 동일 + 코드 리뷰로 트리거 호출 위치가 서버 검증 이후인지 확인.
- 최종 시나리오 (에디터 플레이 모드, host):
  1. 지상에서 스페이스바 연타 → 매번 점프 발생 (애니메이션 + 실제 상승)
  2. 공중에서 스페이스바 → 점프·애니메이션 모두 발생하지 않음 (착지 후 가능)
  3. 점프 중 Idle로 되돌아가는 깜빡임 없음 — 착지 시점에 Idle/Locomotion 복귀
  4. 가능하면 클라이언트 1명 추가 접속 후 1~3 반복 (양쪽 플레이어 Golem/Magician 동일 확인)
