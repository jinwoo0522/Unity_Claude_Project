# Code Review — Agents_1 브랜치 (Golem_Player 리팩토링)

리뷰 날짜: 2026-06-28  
검토 범위: PLAN.md Step 1~6 전체 (스크립트 · AnimData · GolemAnimator · Golem_Player.prefab)

---

## 에셋 참조 검증 (unity-scanner)

| 에셋 | 실제 guid (.meta) | 프리팹/컨트롤러 참조 | 결과 |
|---|---|---|---|
| AnimData_Golem.asset | `983e2d9711ca40d3ac6096f7b763348c` | Golem_Player 컴포넌트 animData | ✅ 일치 |
| AnimData_UpperGolem.asset | `9a1d3916458242a9802cddc021ac3dc2` | Golem_Player 컴포넌트 upperAnimData | ✅ 일치 |
| Golem_UpperBody_Mask.mask | `1cd46f546ca900948bd250207b01e92e` | GolemAnimator UpperBody m_Mask | ✅ 일치 |
| AnimData.cs (스크립트) | `06443740ab07aa94a95000a86d8fcdbb` | AnimData_Golem · AnimData_UpperGolem m_Script | ✅ 일치 |

---

## PLAN 대비 구현 달성 요약

| Step | 내용 | 결과 |
|---|---|---|
| Step 1 | IState.AddTransition · EntityState.AddTransition · StateMachine.AddTransition 추가 | ✅ 완료 |
| Step 2 | 공통 상태에서 Elf 전용 전환 분리 · Elf_Player 외부 재등록 | ✅ 완료 (이슈 #1 참조) |
| Step 3 | Golem_Player.cs 작성 (공통 상태 재사용, 스킬 전환 미등록) | ✅ 완료 |
| Step 4 | AnimData_Golem · AnimData_UpperGolem 에셋 생성 | ✅ 완료 |
| Step 5 | GolemAnimator Base+Upper 2레이어 재편, 상태명/clipName 정합 | ✅ 완료 |
| Step 6 | Golem_Player.prefab Missing Script 제거, 컴포넌트 복구, AnimData 참조 연결 | ✅ 완료 |

---

## 발견 이슈

### [MEDIUM] #1 · Elf_Player.cs:19 — 상체 상태머신 초기 전환에 잘못된 enum 타입

```csharp
// 현재 (잘못된 타입)
_upperStateMachine.TransitionTo((ushort)ENTITY.StateType.IDLE);

// 올바른 타입
_upperStateMachine.TransitionTo((ushort)ELF.UpperStateType.IDLE);
```

**원인:** `_upperStateMachine.CreateState`는 `(ushort)ELF.UpperStateType.IDLE`(=1)로 상태를 등록했으나, `TransitionTo`는 `ENTITY.StateType.IDLE`(=1)로 호출.  
**현재 영향:** 두 enum 값이 모두 `1`로 동일하여 런타임 동작은 정상.  
**잠재 위험:** `ELF.UpperStateType`이나 `ENTITY.StateType`의 IDLE 값이 변경될 경우 상체 상태머신 초기화 실패 (KeyNotFoundException).

---

### [LOW] #2 · Golem_Player.cs:17-20 — 불필요한 Update() 오버라이드

```csharp
// 현재: base.Update()만 호출하는 빈 오버라이드
protected override void Update()
{
    base.Update();
}
```

`Player.Update()`를 그대로 상속하면 동일하므로 이 메서드는 삭제 가능.  
기능 영향 없음.

---

### [LOW] #3 · PlayerHitState.cs:32-33 — 매 프레임 Debug.Log 잔류

```csharp
protected override void UpdateState(float fTimedelta, ushort curState)
{
    Debug.Log("히트 중"); // 매 프레임 호출
}

public override void Exit()
{
    Debug.Log("히트 끝");
    _damagable._isHit = false;
}
```

피격 상태가 유지되는 동안 매 프레임 `"히트 중"` 로그가 출력됨 — 성능 저하 및 로그 오염. 테스트 완료 후 제거 필요.

---

### [INFO] #4 · Assets/Script/Player/Magician_UpperBody.cs — PLAN 범위 외 미추적 파일 발견

git 상태 기준 `??` (미추적) 파일. PLAN.md 및 TASK.md에 언급 없음.

1. **컴파일 위험**: `Player_UpperBody` 부모 클래스가 프로젝트 어디에도 존재하지 않음 → 현재 컴파일 오류 발생 가능성.
2. **코드 규칙 위반**: `rightHandBone != null ?` null 체크 포함 — CLAUDE.md "null 체크 코드 작성 금지" 규칙 위반.

이 파일의 의도 및 처리 방향 확인 필요.

---

## 상세 검토 메모

### GolemAnimator Walk BlendTree
- 2D Freeform Directional, 5개 모션: 중앙(0,0)=Idle 클립, (0,1)=Walk Forward, (0,-1)=Walk Back, (-1,0)=Walk Left, (1,0)=Walk Right.
- PLAN 명세("Walk Forward/Back/Left/Right")에 중앙 Idle이 추가된 구성으로, 이동 입력 0 지점에서 Idle 포즈를 자연스럽게 블렌드하는 표준 패턴. 의도적 설계로 판단.

### GolemAnimator UpperBody DefaultWeight=0
- EntityAnimatior가 `LerpLayerWeight()`에서 매 프레임 `SetLayerWeight`를 동적 관리.
- `_upperState`가 0(NONE)이면 weight→0, 1(IDLE) 이상이면 weight→1로 보간.
- 컨트롤러 설정 `m_DefaultWeight: 0`은 코드와 일치하며 의도에 부합.

### AnimData key-clipName ↔ 애니메이터 상태명 일치 확인

| AnimData_Golem (layer 0) | key | clipName | 애니메이터 상태 |
|---|---|---|---|
| IDLE | 1 | Idle | `Idle` ✅ |
| WALK | 2 | Walk | `Walk` ✅ |
| JUMP | 4 | Jump | `Jump` ✅ |
| RUN | 64 | Run | `Run` ✅ |
| LAND | 128 | Land | `Land` ✅ |
| AIRBORNE | 256 | Airborne | `Airborne` ✅ |

| AnimData_UpperGolem (layer 1) | key | clipName | 애니메이터 상태 |
|---|---|---|---|
| NONE | 0 | None | `None` ✅ |
| IDLE | 1 | Idle | `Idle` ✅ |
| HIT | 2 | Hit | `Hit` ✅ |

### Golem_Player.prefab Missing Script 확인
프리팹 전체에서 `m_Script: {fileID: 0}` 패턴 없음 → Missing Script 0개 ✅

---

## 총평

핵심 기능(상태머신 분리·외부 전환 주입·AnimData·애니메이터·프리팹 복구)은 PLAN 명세에 맞게 정상 구현됨.  
실제 런타임 버그는 없으나, #1(enum 타입 불일치)은 향후 enum 리팩토링 시 버그로 전환될 수 있어 수정 권장.  
#3(Debug.Log 잔류)은 QA 전 제거 필요. #4(Magician_UpperBody.cs)는 컴파일 오류 가능성 있으므로 확인 필요.
