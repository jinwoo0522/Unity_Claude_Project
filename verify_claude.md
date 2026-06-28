## 계획 검증 결과

### 요구사항 대조 (TASK.md → PLAN.md)

- ✅ **하체 움직임 리팩토링** : 공통 PlayerIdle/Walk/Run/Jump/Land + EntityAirborne 상태 재사용, ENTITY enum 기준으로 등록
- ✅ **상체 Idle/Hit 구현** : PlayerUpperIdleState + PlayerHitState + AnyToHit_Player(AnyTransition) 등록
- ✅ **스킬 미리팩토링** : TASK 명시 — 스킬 전환 및 스킬 상태 등록 금지 계획에 반영됨
- ✅ **AnimData 2종 작성** : AnimData_Magician(layer 0) / AnimData_UpperMagician(layer 1) 생성 계획 포함
- ✅ **프리팹 연결** : Magicain_Player.prefab에 컴포넌트 부착 및 animData/upperAnimData 할당 계획 포함
- ✅ **레거시 코드 비침범** : 신규 파일 추가 + 프리팹/데이터 수정만으로 범위 제한

---

### 이슈 목록

- **1. [낮음]** `AnimData.cs`가 두 경로에 존재
  - `Resources/Data/AnimData.cs` (guid: `d0119fad...`)
  - `Resources/Data/AnimData/AnimData.cs` (guid: `06443740...`)
  - PLAN.md는 `06443740ab07aa94a95000a86d8fcdbb`를 사용하며, 이는 기존 AnimData_Golem.asset의 `m_Script` guid와 일치함 → 정합성 있음. 구현 시 혼동 방지를 위해 주의 필요.

- **2. [낮음]** TASK.md의 프리팹 경로 오기재
  - TASK.md 2번째 줄: `Golem_Player.prefab : 매지션 프리펩` — 실제 Magician 프리팹은 `Magicain_Player.prefab`
  - PLAN.md는 실제 파일 탐색으로 올바르게 `Magicain_Player.prefab`을 참조 → PLAN.md 무관, TASK.md 자체 오류

- **3. [낮음]** `StateToIdle_Player` 상체 복귀 전환 미명시
  - HIT → IDLE 복귀 전환 클래스가 존재하나(`StateToIdle_Player.cs`), Golem_Player.cs도 `CreateUpperState()`에서 명시적으로 등록하지 않음 → `PlayerHitState` 내부 처리로 판단됨. 이상 없음.

---

### 확인 필요 항목

없음

---

### 최종 판정

**통과**

모든 참조 파일/클래스/guid가 실제 존재하며, ENTITY enum 값과 AnimData 매핑이 일치하고, Golem_Player 패턴을 올바르게 따르고 있습니다.
