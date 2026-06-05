# PLAN.md

## 1. 작업 개요
- 목표: 스킬 쿨타임 오버레이가 **위→아래로 점진적으로 걷히도록** 수정한다(현재의 "검정↔하양 즉시 토글" 버그 해결).
- 브랜치: Agents_1
- 작업 디렉토리: C:/Unity/Golem_VS_Magician_Agents_1

## 2. 명세 요약 (TASK.md)
- 정사각형 쿨타임 박스가 처음엔 검정, 남은 쿨타임에 비례해 검은 영역이 줄어든다 / `fillAmount`로 표현 / 완료 기준: 쿨타임 진행 중 검은 비율이 줄어든다.
- 검정이 **위에서 아래로** 서서히 걷히며 원래색으로 복귀 / FillOrigin=Bottom + `fillAmount=1-ratio` / 완료 기준: 위쪽부터 원래색이 드러난다.
- 쿨타임 0초=완전 원래색, 절반 남으면 위 절반 원래색·아래 절반 검정 / 완료 기준: 절반 시점에 박스의 위/아래가 반반으로 보인다.

## 3. 근본 원인 분석 (정적 조사로 확정)
현재 코드·씬은 점진적 채움 구현이 **이미 완료**돼 있으나, 한 가지 누락으로 인해 즉시 토글로 동작한다.

- `Assets/Script/UI/SkillCooldownUI.cs`: `fillAmount = 1f - (경과/길이)`로 매 프레임 점진 감소 — **정상**.
- `Assets/Script/Player/Player_Skill.cs`: 서버 확정 후 `NotifyCooldown_ClientRpc` → owner만 `StartCooldown`, `OnNetworkSpawn`에서 `SetCooldownLength` 등록 — **정상**.
- 씬 `Dungeon_Level_1.unity`의 Overlay Image 2개:
  - `m_Type:3`(Filled) / `m_FillMethod:1`(Vertical) / `m_FillOrigin:0`(Bottom) / 검정 `a:0.75` — **정상**.
  - **`m_Sprite: {fileID: 0}` (Source Image = None) — 이것이 버그의 원인.**

**원인:** Unity `Image`는 `activeSprite == null`이면 `OnPopulateMesh`에서 Filled/`fillAmount` 로직을 건너뛰고 전체 사각형(base Graphic)으로 렌더한다. 따라서 `fillAmount`를 줄여도 화면에 반영되지 않아, 쿨타임 내내 풀 검정으로 있다가 `enabled=false`로 한 번에 사라진다(= 즉시 토글). Overlay에 스프라이트만 할당하면 `fillAmount`가 정상 동작한다.

## 4. 영향 범위

### 수정할 파일 (에디터 변경, unity-cli)
- `Assets/Scenes/Dungeon_Level_1.unity` — `SkillCooldownHUD` 하위 두 Overlay Image의 **Source Image에 단색 UI 스프라이트 할당**.
  - 대상 1: `SkillCooldownHUD/QSlot/Overlay` (Image, GameObject fileID 656490395 계열, `_overlays[0]`)
  - 대상 2: `SkillCooldownHUD/MouseSlot/Overlay` (Image fileID 319519833, `_overlays[1]`)

### 건드리지 않을 파일/시스템
- `SkillCooldownUI.cs` — 로직 정상, **코드 변경 없음**.
- `Player_Skill.cs` / `Golem_Skill.cs` / `Magician_Skill.cs` — 변경 없음.
- 서버 쿨타임 검증(`UseSkill_ServerRpc`, `_lastUseTimes`) — 변경 없음.
- QSlot/MouseSlot **배경** Image(Simple, 단색) — 스프라이트 없어도 단색 사각형으로 정상 렌더되므로 변경 없음.
- 오버레이의 Filled/Vertical/Bottom·색·앵커 설정 — 이미 올바르므로 변경 없음.

## 5. 구현 단계

### Step 1. 두 Overlay Image에 스프라이트 할당 + Image Type을 Filled로 고정
- 배경: 스프라이트를 할당하는 순간 Image Type이 **Filled→Sliced로 바뀌는** 현상이 확인됨(`MouseSlot/Overlay`가 m_Type:1로 변경됨). `fillAmount`는 **Filled 타입에서만** 동작하므로, 스프라이트만 넣고 Type을 방치하면 동일 증상이 재현된다. 또한 `QSlot/Overlay`는 아직 스프라이트가 None이다.
- 작업 내용: 에디터(unity-cli, 사전 승인)에서 **두 Overlay(`QSlot/Overlay` fileID 319519833, `MouseSlot/Overlay` fileID 656490395) 모두** 아래 값으로 맞춘다.
  - Source Image: 단색 UI 스프라이트 (빌트인 `Background`/`UISprite` 가능)
  - **Image Type: Filled** ← 스프라이트 할당 후 반드시 다시 Filled로
  - Fill Method: Vertical / Fill Origin: Bottom
  - Color: 검정, A 0.75 / 초기 컴포넌트 `enabled=0` 유지
- 완료 기준: 두 Overlay 모두 `m_Sprite`가 유효 값 **AND** `m_Type: 3`(Filled) `m_FillMethod: 1` `m_FillOrigin: 0`.
- 예상 리스크: Type을 Filled로 되돌리는 것을 누락하면 증상 재현. 두 칸 중 하나만 고치는 누락 주의.

### Step 2. 플레이모드 검증
- 작업 내용: unity-cli로 플레이모드 진입, owner 캐릭터로 Q·우클릭 사용.
- 완료 기준:
  - 사용 직후 박스가 검정으로 덮인다.
  - 쿨타임 진행에 따라 검정이 **위→아래로** 걷히며 원래색이 위부터 드러난다.
  - 절반 시점에 위 절반 원래색·아래 절반 검정.
  - 쿨타임 종료 시 오버레이 완전 소멸, 재사용 가능.
- 예상 리스크: 없음(설정값 검증 완료).

### (예비) Step 3. 그래도 즉시 토글이면 등록값 점검
- Step 2가 실패할 경우에만 수행. `SetCooldownLength`에 0이 들어가면(예: `fCooldown`이 0, 또는 `FindObjectOfType`가 HUD를 못 찾음) `ratio`가 즉시 1을 넘어 한 프레임 만에 사라진다.
- 점검: `OnNetworkSpawn`에서 `_cooldownUI`가 null이 아닌지, `qSkillData/mouseSkillData.fCooldown`이 양수인지 임시 로그로 확인.
- 완료 기준: `_lengths[0]`, `_lengths[1]`이 실제 쿨타임(양수)으로 등록됨을 확인.

## 6. 가정 및 제약
- 근본 원인은 "Overlay에 Source Image 없음" 단일 원인으로 확정했으며, Step 1만으로 해결될 것으로 본다(Step 3는 예비).
- `Assets/Reimport All` 금지. 에디터 변경은 unity-cli로 수행하며 사전 승인.
- 코드 변경 없음 — 최소 변경 원칙(karpathy).
- 슬롯 매핑 0=Q, 1=우클릭 고정. 쿨타임 값은 `SkillData.fCooldown`에서 가져옴(하드코딩 없음).

## 7. 검증 방법
- Step 1: 씬 저장 후 두 Overlay의 `m_Sprite`가 유효 값인지 확인.
- 최종 시나리오(플레이모드):
  1. Q·우클릭 사용 → 박스 검정 → 위→아래로 점진 복귀.
  2. 절반 시점 위/아래 반반.
  3. 종료 시 완전 소멸, 재사용 가능.
  4. 상대 플레이어 쿨타임은 내 화면에 보이지 않음.
