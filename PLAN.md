# PLAN.md

## 1. 작업 개요
- 목표: Tab 유지 시 표시되는 킬 스코어보드(이름·킬·데스·가한 데미지) UI와 서버 권위 ScoreManager 집계 시스템 구현
- 브랜치: Agents_1
- 작업 디렉토리: C:\Unity\Golem_VS_Magician_Agents_1

## 2. 명세 요약
- **점수판 표시**: Tab을 누르고 있는 동안만 화면 중앙 반투명 Panel 안에 플레이어별 UI 항목(이름/킬/데스/가한 데미지)을 표시. 무엇을: Tab hold로 Panel on/off / 어떻게: 클라 로컬 입력으로 Panel.SetActive 토글 / 완료 기준: Tab을 누르는 동안만 보이고 떼면 사라짐.
- **항목 구성**: 플레이어 1명당 항목 1개. UI 프리팹을 생성하고 위치를 판정. 무엇을: 항목 프리팹 + 위치 배치 로직 / 어떻게: 클라가 복제 데이터로 항목 인스턴스를 생성·배치 / 완료 기준: 접속/스폰한 플레이어 수만큼 항목이 생성됨.
- **ScoreManager(순수 클래스)**: 다른 Manager처럼 순수 클래스로 만들어 GameManager를 통해 접근. 데이터 구조체를 플레이어마다 관리하고 클라이언트 번호(ulong)로 식별. 무엇을: 서버 집계 로직 / 완료 기준: 데미지·킬·데스가 clientId별로 누적됨.
- **카운팅 시점**: 각 플레이어가 데미지를 줄 때 / 죽일 때 / 죽을 때를 모두 ScoreManager로 카운팅. 완료 기준: 세 이벤트가 정확한 clientId에 반영됨.
- **죽음 처리**: 죽음/죽임 판정이 없으므로, HP가 0이 되면 `Die()`를 부르고 카운팅만 수행(부활·연출 등은 범위 외). 완료 기준: HP 0 시 Die 호출 + 킬/데스 1회 집계.
- **정렬**: 각 항목은 킬 수에 따라 정렬. 킬 발생 시 리스트를 정렬하고 정렬된 순서로 항목 위치를 재배치. **절대 Update에서 위치를 구성하지 않음**. 완료 기준: 킬 변동 시점에만 정렬·재배치 발생.

## 3. 영향 범위

### 수정할 파일
- `Assets/Script/Manager/GameManager.cs` — `ScoreManager scoreManager {get; private set;}` 추가, `Start()`에서 생성(CameraManager/SkillPool과 동일 패턴).
- `Assets/Script/Player/Player_UpperBody.cs` — `TakeHit` 시그니처에 `ulong attackerClientId` 추가. 데미지 적용 직전 피격자 `Stat`에 마지막 공격자 기록 + ScoreManager에 가한 데미지 누적.
- `Assets/Script/Player/Golem_UpperBody.cs` — `ReportHit_ServerRpc`에서 `targetUpper.TakeHit(pDamage, OwnerClientId)` 로 공격자 전달.
- `Assets/Script/Skill/Skill.cs` — `OnHitEnemy`에서 `targetUpper.TakeHit(fDamage, ClinetID, isHitAni)` 로 공격자(스킬 소유 clientId) 전달.
- `Assets/Script/Stat.cs` — 마지막 공격자(clientId) 필드 + 세터 추가. `Die()`에서 ScoreManager에 킬(killer)·데스(victim) 1회 집계.
- `Assets/Script/NetworkUI/PlayerSpawner.cs` — `ChoicePlayer`(서버)에서 스폰 성공 직후 `ScoreManager.AddPlayer(ulong clientId)` 호출(단일 파라미터) + Scoreboard 복제 갱신.

### 새로 생성할 파일
- `Assets/Script/Manager/ScoreManager.cs` — 순수 C# 클래스. `Dictionary<ulong, ScoreData>` 보관. 메서드: `AddPlayer(ulong clientId)`, `AddDamage(ulong attackerId, float amount)`, `RegisterKill(ulong killerId, ulong victimId)`. **`AddPlayer`는 단일 파라미터(clientId)만 받으며, 표시 이름은 ScoreManager 내부에서 `client : {clientId}` 패턴으로 생성**하여 ScoreData.name에 채운다(호출부는 이름을 전달하지 않음). `RemovePlayer`는 본 TASK 범위 밖(퇴장 처리 미요구)이므로 **이번 구현에서는 작성하지 않는다**(필요 시 후속). 변경 시 `event Action<ulong>`(변경된 clientId) 발생 → Scoreboard가 NetworkList 미러링에 사용. **서버에서만 호출**(GameManager는 네트워크 비인지이므로 호출 측에서 IsServer 가드).
- `Assets/Script/Manager/ScoreData.cs` — 데이터 구조체(`clientId`, `name`, `kills`, `deaths`, `damageDealt`). 순수 C# (서버 집계용).
- `Assets/Script/UI/ScoreEntry.cs` — NetworkList 전송용 `struct` (`INetworkSerializable`, `IEquatable<ScoreEntry>`). 필드: `clientId(ulong)`, `name(FixedString64Bytes)`, `kills/deaths(int)`, `damageDealt(float)`.
- `Assets/Script/UI/Scoreboard.cs` — 씬 단일 `NetworkBehaviour`. `NetworkList<ScoreEntry>`(서버 쓰기/전 클라 읽기) 보유. 서버: ScoreManager 이벤트 구독 → NetworkList upsert. 클라: `OnListChanged`에서 항목 생성/갱신 + **킬 기준 정렬 후 위치 재배치**. Tab hold 입력으로 Panel.SetActive 토글(로컬). **프리팹/Panel 참조는 모두 `[SerializeField] private`로 Inspector 직렬화 연결**: 반투명 Panel 루트(`[SerializeField] private GameObject _panel`), 항목이 들어갈 컨테이너 RectTransform(`[SerializeField] private Transform _entryContainer`), 항목 프리팹(`[SerializeField] private ScoreboardEntry _entryPrefab`). (Resources.Load 미사용 — 씬 직렬화로 누락 방지.)
- `Assets/Script/UI/ScoreboardEntry.cs` — 항목 프리팹용 `MonoBehaviour`. 이름/킬/데스/데미지 `TextMeshProUGUI` 4개에 값 세팅(`SetData`).

### 새로 생성할 에셋(구현 단계에서 Agent 제작)
- `Assets/Prefabs/UI/Scoreboard_Panel.prefab` — 반투명 Panel + 수직 정렬 컨테이너(RectTransform). `Scoreboard` 스크립트 부착 대상은 씬 오브젝트, Panel은 그 하위.
- `Assets/Prefabs/UI/Scoreboard_Entry.prefab` — 이름/킬/데스/데미지 텍스트 4개 + `ScoreboardEntry` 스크립트.
- 씬 `Assets/Scenes/Dungeon_Level_1.unity` — Scoreboard용 씬 NetworkObject 배치 및 NetworkManager의 NetworkPrefabs/씬 배치 등록(PlayerSpawner와 동일하게 씬 상주 네트워크 오브젝트로).

### 건드리지 않을 파일/시스템
- 상태이상(`StatusEffect*`), 카메라(`Camera/*`, `CameraManager`), 이동(`*_Move`), 스킬 데이터/풀의 기존 로직, `PlayerHUD`(개인 HUD)·`SkillCooldownUI`. 데미지 **계산식**(저항 등)은 변경하지 않고 기존 `pHp = -damage` 흐름 유지.

## 4. 구현 단계

### Step 1. 데이터 구조체 + ScoreManager(순수 클래스)
- 작업 내용: `ScoreData` 구조체와 `ScoreManager` 순수 클래스 작성. `AddPlayer(ulong clientId)`(단일 파라미터 — 내부에서 `client : {clientId}` 이름 생성 후 ScoreData 등록), `AddDamage(ulong attackerId, float amount)`, `RegisterKill(ulong killerId, ulong victimId)` 구현. (`RemovePlayer`는 TASK 범위 밖이므로 작성하지 않음.) 각 변경마다 `Changed?.Invoke(clientId)` 발생. 자기 자신/존재하지 않는 clientId 등 경계만 가드.
- 완료 기준: 컴파일 통과. 단위 흐름상 데미지 누적·킬/데스 증가가 Dictionary에 반영.
- 예상 리스크: GameManager가 네트워크 비인지 → 모든 호출이 서버 경로에서만 일어나도록 호출 측 IsServer 가드 필요.

### Step 2. GameManager에 ScoreManager 연결
- 작업 내용: `GameManager`에 `scoreManager` 프로퍼티 추가, `Start()`에서 생성(기존 cameraManager/skillPool과 동일).
- 완료 기준: `GameManager.Instance.scoreManager` 접근 가능, 컴파일 통과.
- 예상 리스크: 없음(기존 패턴 복제).

### Step 3. 데미지 경로에 공격자 clientId 전파 + 집계 호출
- 작업 내용: `Player_UpperBody.TakeHit(float, ulong attackerClientId, bool)` 로 시그니처 확장. 내부에서 (서버) ScoreManager.AddDamage(attacker, damage) 호출 + 피격자 Stat에 lastAttacker 기록 후 `pHp = -damage`. `Golem_UpperBody.ReportHit_ServerRpc`/`Skill.OnHitEnemy` 호출부 갱신.
- 완료 기준: 두 데미지 경로(기본공격·스킬)에서 공격자 ID가 전달되고 데미지가 누적됨. 컴파일 통과.
- 예상 리스크: TakeHit 호출부 누락 시 컴파일 에러 → grep으로 호출부 전수 확인(`TakeHit(`).

### Step 4. Stat.Die에서 킬/데스 집계
- 작업 내용: `Stat`에 lastAttacker(ulong) 필드 + 세터. `pHp` set이 0 이하로 Die 호출 시, `Die()`에서 ScoreManager.RegisterKill(lastAttacker, OwnerClientId) 1회. (서버 전용 경로) 중복 사망 방지 가드(이미 죽음 처리됨 플래그).
- 완료 기준: HP 0 시 killer 킬 +1, victim 데스 +1 정확히 1회.
- 예상 리스크: Die 다회 호출(연속 데미지) → 사망 플래그로 1회만 집계.

### Step 5. Scoreboard NetworkBehaviour(NetworkList 동기화)
- 작업 내용: `ScoreEntry` struct(INetworkSerializable) + `Scoreboard` 작성. 서버: OnNetworkSpawn에서 ScoreManager.Changed 구독 → 해당 clientId의 ScoreData를 NetworkList에 upsert. PlayerSpawner 스폰 시 `AddPlayer(clientId)`(단일 파라미터) 연동.
- 완료 기준: 서버 집계가 NetworkList를 통해 전 클라에 복제됨.
- 예상 리스크: NetworkList struct 직렬화 — IEquatable/INetworkSerializable 정확 구현 필요.

### Step 6. 클라 UI(항목 생성·정렬·위치 재배치, Tab 토글) + 프리팹/씬
- 작업 내용: `ScoreboardEntry`(항목 UI) 작성. `Scoreboard.OnListChanged`에서 clientId별 항목 인스턴스 생성/갱신, **킬 내림차순 정렬 후 위치 재배치**(Update 금지). Tab hold로 Panel.SetActive. Panel/Entry 프리팹 제작 및 씬 배치 후, **`Scoreboard`의 `[SerializeField]` 필드(`_panel`/`_entryContainer`/`_entryPrefab`)를 Inspector에서 직렬화 연결**.
- 완료 기준: 2인 접속·데미지/킬 발생 시 Tab UI가 갱신되고 킬 순으로 정렬·재배치됨.
- 예상 리스크: 항목 위치 재배치 방식(LayoutGroup 사용 시 sibling index 정렬로 충분 / 수동 배치 시 anchoredPosition 계산). LayoutGroup + sibling index 정렬을 우선.

## 5. 가정 및 제약
- 네트워크 프레임워크는 Unity Netcode for GameObjects(`Unity.Netcode`). 클라 식별자는 `OwnerClientId`(ulong).
- ScoreManager·NetworkList 쓰기는 **서버 권위**. 클라이언트는 복제 데이터 읽기와 로컬 UI 표시만 수행(클라 입력 불신 원칙).
- `Reimport All` 금지. 프리팹/씬 변경은 승인된 구현 단계에서 수행.
- 코드 규칙: 외부 입력 필드는 `[SerializeField] private`, 스크립트 내부 public 변수 금지, 시스템 경계 외 불필요 null 체크 금지(프로젝트 CLAUDE.md 준수).
- 데미지 집계는 명목 damage 값을 누적(저항·초과분 보정 없음, 최소 구현).
- ScoreData 식별/표시 이름은 기존 패턴 `client : {clientId}` 사용.

## 6. 검증 방법
- Step 1~4: Unity 컴파일 에러 0 (서버 로직 단위 흐름 확인).
- Step 5: Host + Client 2인 접속 후 서버에서 데미지/킬 발생 시 NetworkList가 양쪽에 동일하게 복제되는지 로그로 확인.
- Step 6 / 최종 시나리오:
  1. Host + Client 2인 접속, 각각 골렘/마법사 스폰 → Scoreboard에 2개 항목 생성.
  2. 기본공격·스킬로 상대에게 데미지 → Tab 유지 시 '가한 데미지' 증가 확인.
  3. 상대 HP 0 → killer 킬 +1, victim 데스 +1, 항목이 킬 순으로 재정렬·재배치.
  4. Tab을 떼면 Panel 사라짐, Update 중 위치 재구성이 없는지(정렬은 킬 변동 시에만) 확인.
