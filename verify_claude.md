## 계획 검증 결과

### 요구사항 대조 (TASK.md → PLAN.md)

- [✅] **공중 띄우기** : StatusEffect_Airborne 컴포넌트 + ConsumePendingLaunch → PlayerMove verticalVelocity 주입 (Step 2, 4, 6)
- [✅] **점프와 동일 판정** : JumpRoutine 패턴 그대로 재사용 (WaitUntil 이륙→착지), net_isAirborne NetworkVariable (Step 2)
- [✅] **공중 중 점프 불가** : PlayerJump()에 IsAirborne 게이트 추가 (Step 4)
- [✅] **공중 중 스킬 불가** : CheckCanUseSkill에 IsAirborne 게이트 추가 (Step 5)
- [✅] **피격 순간 슬로우** : StatusEffect_Slow 컴포넌트 + SpeedMultiplier → PlayerMove 속도 곱산 (Step 3, 4, 6)
- [✅] **상태이상 모듈화** : StatusEffect_Airborne.cs, StatusEffect_Slow.cs 독립 컴포넌트, Apply() 함수 호출 발동 (Step 2, 3)

---

### 이슈 목록

**1. [높음] `Jump_ServerRpc()`에 공중 상태 게이트 누락**

계획 Step 4는 `PlayerJump()`(클라이언트)에만 `IsAirborne` 체크를 추가한다. 그런데 기존 코드에서 `net_isJumpPending`은 `PlayerJump()`와 `Jump_ServerRpc()` 양쪽 모두에서 검사된다(서버 권위적 설계의 핵심). `IsAirborne` 게이트가 `PlayerJump()`에만 있으면 서버는 공중 점프 여부를 검증하지 않아 RPC를 위조한 클라이언트가 공중 중 점프할 수 있다. CLAUDE.md 규칙 "클라이언트 입력값을 신뢰하지 말 것" 위반.
→ `Jump_ServerRpc()`에도 `if (airborne.IsAirborne) return;` 추가 필요.

**2. [중간] `AnimManage()`에서 `net_isAirborne` 반영 미명시**

현재 `AnimManage()`는 `anim.SetBool("IsGrounded", cct.isGrounded && !net_isJumpPending.Value)`로 계산한다. 점프의 경우 `net_isJumpPending`이 `Jump_ServerRpc()` 진입 즉시 true가 되어 이륙 전 `cct.isGrounded` 플리커를 차단한다. 공중 띄움 시 `net_isAirborne`는 `Apply()` 즉시 true가 되지만, `AnimManage()`가 이를 참조하지 않으면 `cct.isGrounded`가 true인 1~2프레임 동안 IsGrounded 파라미터가 잠깐 true로 플리커한다. "점프와 동일한 판정" 요건 불완전.
→ `anim.SetBool("IsGrounded", cct.isGrounded && !net_isJumpPending.Value && !airborne.IsAirborne)`으로 수정 명시 필요 (Player_Move 수정 목록에 추가).

---

### 확인 필요 항목

없음.

---

### 최종 판정

**보류**

보류 사유:
- 이슈 1: `Jump_ServerRpc()` 서버 측 공중 게이트 누락 — CLAUDE.md 서버 권위적 규칙 위반
- 이슈 2: `AnimManage()` net_isAirborne 반영 미명시 — "점프와 동일 판정" 요건 불완전
