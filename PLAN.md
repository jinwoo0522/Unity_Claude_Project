# PLAN.md

## 1. 작업 개요
- 목표: 두 플레이어(Golem / Magician)의 Animator를 자식 객체에서 **최상위(루트) 객체로 이동**하고, 자식 Animator를 전제로 하던 코드·이벤트·참조를 모두 정합화한다.
- 브랜치: `Agents_1`
- 작업 디렉토리: `C:/Unity/Golem_VS_Magician_Agents_1`

## 2. 명세 요약 (TASK.md 기준)
- **무엇**: Animator를 플레이어 루트 객체로 올린다 / **어떻게**: 두 프리팹(`Golem_Player`, `Magicain_Player`)의 자식에 있는 Animator를 루트로 이동 / **완료 기준**: 루트에 Animator가 존재하고 애니메이션이 정상 재생됨.
- **무엇**: `GetComponentInChildren<Animator>()`로 받아오던 코드를 전부 수정 / **어떻게**: 3개 스크립트의 호출을 `GetComponent<Animator>()`로 변경 / **완료 기준**: 모든 스크립트가 루트 Animator를 직접 참조.
- **무엇**: Animator가 부모로 갔을 때 발생할 문제 파악·해결 / **어떻게**: NetworkAnimator 참조, 애니메이션 이벤트 수신 객체, 루트모션, 이벤트 이름 충돌을 정리 / **완료 기준**: 이동·공격·스킬·피격·트레일 이펙트가 멀티플레이에서 정상 동작.

## 3. 영향 범위

### 조사로 확인된 현재 구조 (대화 초반 정보 아님, 실제 파일 기준)
- 두 모델 모두 **Humanoid**(`animationType: 3`) → 본 매핑 리타게팅이므로 Animator를 루트로 올려도 클립 경로 바인딩이 깨지지 않음. (핵심 리스크 완화)
- `Golem_Player.prefab` (guid `8447b0bd7afe13f4496fbd91d2ffa9d3`)
  - 루트 `Golem_Player`: NetworkObject·NetworkTransform·**NetworkAnimator**·PlayerInput·Golem_Move·Golem_UpperBody·Golem_Skill·Stat·CharacterController·Player_NetworkSpawn
  - 자식 `SKM_Golem (2)`(중첩 프리팹): **Animator**(controller `31eb3f4b07d5ad14ea7f434127841dd5`, avatar `SKM_Golem`, applyRootMotion=1)·Golem_TrailEmitting·AnimEventRelay
- `Magicain_Player.prefab` (guid `fde9f399b6c99fa4dad54f521a5b15e0`)
  - 루트 `Magicain_Player`: NetworkObject·NetworkTransform·**NetworkAnimator**·PlayerInput·Magician_Move·Magician_UpperBody·Magician_Skill·Stat·CharacterController·Player_NetworkSpawn
  - 자식 `Magician_RIO_Unity_forAnim`: **Animator**(controller `a60e8d6e97a02d2428eb73ed0d04ab77`, applyRootMotion=0)·AnimEventRelay
- 애니메이션 이벤트 함수명: `OnFireSkill`(Magician 공격 클립), `OnGolemAttackStart`/`OnGolemAttackEnd`(Golem 공격 클립), `Start_EmitTrail_ClientRpc`/`Stop_EmitTrail_ClientRpc`(Golem 클립).
- **위 guid·controller·avatar는 구현 시점에 .meta로 반드시 재검증할 것** (사용자가 교체했을 수 있음).

### 수정할 파일 (코드)
- `Assets/Script/Player/Player_Move.cs` — `GetComponentInChildren<Animator>()` → `GetComponent<Animator>()`
- `Assets/Script/Player/Player_UpperBody.cs` — 동일 변경 + `OnFireSkill()` → `FireSkill()` 개명
- `Assets/Script/Player/Player_Skill.cs` — 동일 변경
- `Assets/Script/Player/Magician_UpperBody.cs` — `override OnFireSkill()` → `override FireSkill()`
- `Assets/Script/AnimEventRelay.cs` — `playerUpper?.OnFireSkill()` 호출을 `playerUpper?.FireSkill()`로 변경 (자체 이벤트 핸들러 `OnFireSkill()` 이름은 **유지**)

### 수정할 파일 (프리팹 / 에디터, unity-cli)
- `Assets/Prefebs/Player/Golem_Player.prefab`
- `Assets/Prefebs/Player/Magicain_Player.prefab`

### 새로 생성할 파일
- 없음

### 건드리지 않을 것 (명시)
- `Assets/Resources/Monster/Stone_Golem/...` (중첩 프리팹 원본 SKM_Golem — 몬스터에서도 사용. **원본 수정 금지**, 변경은 `Golem_Player` 인스턴스 오버라이드로만)
- 애니메이터 컨트롤러·클립·아바타 에셋 자체
- `Golem_AttackHitbox`(별도 자식, `GetComponentInParent` 사용 — 영향 없음)
- 위 목록 외 모든 스크립트·씬·에셋

## 4. 구현 단계

### Step 1. 코드 수정 (Animator 참조 + OnFireSkill 개명)
- 작업 내용:
  - `Player_Move.cs`, `Player_UpperBody.cs`, `Player_Skill.cs`의 `GetComponentInChildren<Animator>()` → `GetComponent<Animator>()`.
  - `Player_UpperBody.cs`의 `virtual public void OnFireSkill() => NormalAttack();` → `FireSkill()`로 개명.
  - `Magician_UpperBody.cs`의 `public override void OnFireSkill()` → `FireSkill()`로 개명.
  - `AnimEventRelay.cs`의 `playerUpper?.OnFireSkill();` → `playerUpper?.FireSkill();` (이벤트 진입점 메서드명 `OnFireSkill`은 그대로 둠 — 클립 이벤트가 이 이름을 호출).
- 완료 기준: 컴파일 에러 없음. `OnFireSkill`이라는 이름은 `AnimEventRelay`의 이벤트 핸들러 단 하나만 남음(루트에서 이중 호출 제거).
- 예상 리스크: `OnFireSkill`/`FireSkill` 추가 참조 없음 확인됨. 개명 누락 시 컴파일 에러로 즉시 드러남.

### Step 2. Golem_Player 프리팹 구조 변경 (unity-cli, 사전 승인)
- 작업 내용:
  - 루트 `Golem_Player`에 **Animator 추가** — controller `31eb3f4b...`, avatar `SKM_Golem`, **applyRootMotion = false**(코드의 수동 `cct.Move(deltaPosition)`와 충돌·이중이동 방지).
  - 자식 `SKM_Golem (2)`의 **Animator 제거**(인스턴스 오버라이드로 제거, 원본 프리팹 미수정).
  - `AnimEventRelay`·`Golem_TrailEmitting` 컴포넌트를 **자식 → 루트로 이동**(이벤트는 Animator가 있는 객체에서 발생하므로). `Golem_TrailEmitting`의 TrailRenderer 직렬화 참조(객체 참조)는 이동 후에도 유지되는지 확인.
  - 루트 **NetworkAnimator의 Animator 참조를 루트 Animator로 재지정**.
- 완료 기준: 루트에 Animator 1개만 존재, 자식에 Animator 없음, NetworkAnimator가 루트 Animator를 가리킴, 두 중계 컴포넌트가 루트에 있음.
- 예상 리스크: 중첩 프리팹 인스턴스에서 Animator 제거가 까다로울 수 있음 → 실패 시 자식 Animator를 비활성/컨트롤러 해제로 무력화하는 대안 검토. 변경 전 사용자 승인 필수.

### Step 3. Magicain_Player 프리팹 구조 변경 (unity-cli, 사전 승인)
- 작업 내용:
  - 루트 `Magicain_Player`에 **Animator 추가** — controller `a60e8d6e...`, avatar(forAnim), applyRootMotion=false(이미 0).
  - 자식 `Magician_RIO_Unity_forAnim`의 **Animator 제거**.
  - `AnimEventRelay`를 **자식 → 루트로 이동**.
  - 루트 **NetworkAnimator의 Animator 참조를 루트 Animator로 재지정**.
- 완료 기준: Step 2와 동일 기준(Magician은 TrailEmitting 없음).
- 예상 리스크: Step 2와 동일.

### Step 4. 플레이모드 검증
- 작업 내용: 호스트+클라이언트로 두 캐릭터 각각 실행해 동작 확인.
- 완료 기준: 아래 §6 검증 시나리오 전부 통과.
- 예상 리스크: 애니메이션 이벤트 이중 호출/누락, 트레일 미동작, 루트모션 위치 어긋남 — 각 시나리오로 조기 검출.

## 5. 가정 및 제약
- 두 모델은 Humanoid이며 아바타·컨트롤러 guid는 위에 기재한 값과 동일하다(구현 시 .meta 재검증).
- 서버 권위 설계 유지: 애니메이션 동기화는 NetworkAnimator, 위치는 NetworkTransform, 검증·판정은 서버에서만 수행. 클라 입력 불신 원칙 유지.
- 외부 입력 필드는 `[SerializeField] + private` 사용, 스크립트 내부 `public` 변수 신규 도입 금지(은닉화 유지).
- `Assets/Reimport All` 금지. 중첩 프리팹 원본(SKM_Golem) 미수정.
- 에디터/프리팹 변경은 **변경 전 사용자 승인** 후 unity-cli로만 수행.

## 6. 검증 방법
- Step 1 후: Unity 콘솔에 컴파일 에러 없음 확인.
- Step 2~3 후: 프리팹 인스펙터에서 루트 Animator 존재·자식 Animator 부재·NetworkAnimator 참조·중계 컴포넌트 위치 확인.
- 최종 플레이모드 시나리오 (호스트+클라 2인):
  1. 이동/스프린트/점프 애니메이션이 양쪽 클라에서 동기화된다.
  2. Magician 기본공격: `OnFireSkill`이 **한 번만** 발동(이중 공격 없음), 투사체 1회 생성.
  3. Golem 기본공격: `OnGolemAttackStart/End`로 히트박스 On/Off, `Start/Stop_EmitTrail`로 트레일 이펙트가 켜졌다 꺼진다.
  4. Q/우클릭 스킬: 스킬 레이어 재생·쿨타임·중복 차단 정상. Golem 마우스 스킬의 루트모션 위치 이동이 정상(이중 이동·드리프트 없음).
  5. 피격(`UpperHit`) 애니메이션과 체력 감소가 전 클라에 동기화된다.
