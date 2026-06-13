# PLAN.md

## 1. 작업 개요
- 목표: 본인/타인 분리형 HP UI 재구성 + 서버 권위적 마나 시스템·마나 UI 구현
- 브랜치: Agents_1
- 작업 디렉토리: C:/Unity/Golem_VS_Magician_Agents_1

## 2. 명세 요약
TASK.md에서 추출한 요구사항. 각 항목: 무엇을 / 어떻게 / 완료 기준.

- **HP UI 분리**: 머리 위 HP UI를 본인에겐 숨기고 타 플레이어에겐 표시 / 머리 위 월드 Canvas를 owner 로컬에서만 비활성, 타 인스턴스는 유지 / 본인 화면에 안 보이고 상대 머리 위엔 보임
- **본인 HP HUD**: 본인 HP를 화면 좌측 하단에 클라이언트 이름 + HP바로 표시 / 씬의 기존 화면 Canvas에 좌측 하단 HUD(이름·HP바) 추가 / 스폰 시 owner의 Stat HP가 좌측 하단 바에 동기화 표시
- **스폰 시 UI 연결**: Player_NetworkSpawn에서 스폰 시 UI 연결 / OnNetworkSpawn(owner)에서 화면 HUD 탐색·바인딩 및 머리 위 Canvas 숨김 처리
- **마나 스탯**: 플레이어 Stat에 마나 네트워크 변수 / Stat에 NetworkVariable<float>(현재 마나·최대 마나) 추가, 서버 쓰기·전체 읽기 / 마나가 서버에서만 변경되고 전 클라에 복제됨
- **마나 데이터**: Player_Data에 최대 마나·마나 재생량 추가 / `fMaxMana`, `fManaRegen` 필드 추가 / Golem 100/2, Magician 200/3로 초기화됨
- **마나 재생**: 초당 마나 재생량만큼 차오름 / Stat의 서버 전용 Update에서 `fManaRegen * deltaTime`씩 증가, 최대치 클램프 / 시간이 지나면 마나가 최대치까지 회복
- **스킬 마나 소모**: 골렘 기본공격 제외 모든 스킬이 마나 소모 / SkillData에 `fManaCost` 추가, 스킬 발동 시 서버에서 소모 / 마나 부족 시 발동 불가
- **마나 부족 차단**: 소모량보다 마나가 적으면 스킬 사용 불가 / 서버 권위 검증, 부족 시 애니·쿨타임·투사체 모두 미발생 / 부족 상태에서 스킬 키 입력해도 아무 일도 일어나지 않음
- **마나 UI**: 좌측 하단 HP바 밑에 파란색 마나바 / 화면 HUD에 마나 Slider(파란 Fill) 추가, owner Stat 마나와 동기화 / 마나 변화가 좌측 하단 파란 바에 실시간 반영
- **서버 권위**: 모든 데이터 서버 권위적 / 마나 검증·소모·재생은 서버에서만, 클라 입력값 불신 / 클라가 위조한 요청으로 마나 우회 불가

### 마나 소모량 매핑 (확정)
| 액션 | SkillData 자산 | SkillType/경로 | 마나 |
|---|---|---|---|
| 마법사 기본공격 | ElectricSkillData | SkillType.Electric (pool) | 5 |
| 마법사 Q | Magician_Q_Skill | SkillType.IceExplosion (pool) | 30 |
| 마법사 좌클릭 | Magician_Mouse_Skill | 비풀(애니+마나만) | 20 |
| 골렘 Q | Golem_Q_Skill | 비풀(애니+마나만) | 20 |
| 골렘 좌클릭 | Golem_Mouse_Skill | SkillType.FireExplosion (pool) | 30 |
| 골렘 기본공격 | (없음) | 근접, 마나 소모 없음 | 0 |

## 3. 영향 범위

### 수정할 스크립트
- `Assets/Resources/Data/PlayerData/Player_Data.cs` — 마나 필드 추가
- `Assets/Script/Stat.cs` — 마나 NetworkVariable·재생·소모·HUD 바인딩
- `Assets/Script/Player/Player_NetworkSpawn.cs` — owner 머리 Canvas 숨김·화면 HUD 바인딩
- `Assets/Resources/Data/SkillData/SkillData.cs` — `fManaCost` 추가
- `Assets/Script/Player/Player_Skill.cs` — 마나 게이트(owner 사전 + 서버 권위 소모), 비풀 스킬 공용 서버 헬퍼
- `Assets/Script/Player/Golem_Skill.cs` — OnBuff(Q) 마나+애니메이션 발동 구현
- `Assets/Script/Player/Magician_Skill.cs` — OnAttack_Skill(좌클릭) 마나+애니메이션 발동 구현
- `Assets/Script/Player/Player_UpperBody.cs` — 기본공격 마나 게이트 훅(기본 0)
- `Assets/Script/Player/Magician_UpperBody.cs` — 기본공격(Electric) 마나 5 소모

### 새로 생성할 스크립트
- `Assets/Script/UI/PlayerHUD.cs` — 화면 좌측 하단 HUD 컨트롤러(이름·HP Slider·Mana Slider). MonoBehaviour, 씬 단일 오브젝트(SkillCooldownUI와 동일 패턴, owner가 FindAnyObjectByType로 탐색)

### 에디터/에셋 변경 (구현 단계에서 승인 후 진행 — CLAUDE.md 규칙)
- `Assets/Resources/Data/PlayerData/Golem_Data.asset` — fMaxMana=100, fManaRegen=2
- `Assets/Resources/Data/PlayerData/Magicain_Data.asset` — fMaxMana=200, fManaRegen=3
- SkillData 5종 .asset — fManaCost 설정(위 표)
- `Assets/Prefebs/Player/Golem_Player.prefab` — Golem_Skill에 Golem_Q_Skill 자산 참조 연결, 머리 Canvas 참조 확인
- `Assets/Prefebs/Player/Magicain_Player.prefab` — Magician_Skill에 Magician_Mouse_Skill 자산 참조 연결
- `Assets/Scenes/Dungeon_Level_1.unity` — 기존 Canvas 하위에 좌측 하단 HUD(이름 Text, HP Slider, Mana Slider[파란 Fill]) 추가 + PlayerHUD 컴포넌트·참조 배선

### 건드리지 않을 것 (명시)
- `Assets/Script/Skill/SkillPool.cs`, `SkillType.cs` — enum/풀 변경 없음(비풀 스킬은 풀 우회). prefab=None 자산을 풀에 넣지 않는다.
- `Assets/Resources/Data/Entity_Data.cs`, `Enemy_Data` — 마나는 Player_Data에만 추가(적 제외)
- 머리 위 Canvas의 HP Slider(타 플레이어용) 기존 바인딩 — 비owner 동작 유지
- VFX/이펙트, 카메라/시네머신 연결 로직

## 4. 구현 단계

### Step 1. 마나 데이터 필드 추가 (Player_Data)
- 작업 내용: `Player_Data.cs`에 `[Header("마나")] public float fMaxMana = 100f; public float fManaRegen = 2f;` 추가. 기존 Entity_Data/Player_Data가 public 필드 컨벤션이므로 데이터 컨테이너 일관성 위해 동일 컨벤션 사용(Stat이 `Stat_Data.fMaxHp` 방식으로 접근).
- 완료 기준: 컴파일 성공, Inspector에 마나 필드 노출.
- 예상 리스크: 낮음.

### Step 2. SkillData 마나 비용 필드 추가
- 작업 내용: `SkillData.cs`에 기존 패턴대로 `[SerializeField] private float _fManaCost = 0f; public float fManaCost => _fManaCost;` 추가.
- 완료 기준: 컴파일 성공, Inspector 노출.
- 예상 리스크: 낮음.

### Step 3. Stat에 마나 시스템 구현
- 작업 내용:
  - NetworkVariable<float> 현재 마나·최대 마나 추가(읽기 Everyone, 쓰기 Server) — HP 패턴 동일.
  - `public float pMana => fMana.Value;`, 마나 최대 getter 추가.
  - 서버 전용 `public bool TryConsumeMana(float cost)`: `!IsServer`면 false, 마나 부족이면 false(소모 없음), 충분하면 차감 후 true.
  - 서버 전용 `Update()`: `fMana.Value < max`일 때 `fManaRegen * Time.deltaTime` 증가, max 클램프.
  - `OnNetworkSpawn`(서버 분기): `Stat_Data as Player_Data`로 캐스팅(시스템 경계 검증 — Player_Data가 아니면 마나 초기화 스킵). fMaxMana·fManaRegen 읽어 초기화, 현재 마나=최대로 시작.
  - HUD 바인딩 지원: owner 화면 HUD에 HP·마나 초기값 반영 + NetworkVariable.OnValueChanged 구독을 위한 진입점 제공(`public void BindOwnerHUD(PlayerHUD hud)`). 비owner는 기존 머리 hpSlider 바인딩 유지.
- 완료 기준: 서버에서 마나가 초당 재생, TryConsumeMana가 부족 시 false 반환.
- 예상 리스크: NetworkVariable 초기화 타이밍 — OnNetworkSpawn에서 초기값 세팅 후 OnValueChanged 구독.

### Step 4. PlayerHUD(화면 좌측 하단 HUD) 생성
- 작업 내용: `PlayerHUD.cs`(MonoBehaviour). 직렬화 필드: 이름 TMP Text, HP Slider, Mana Slider. 메서드: `SetName`, `SetHp(cur,max)`, `SetMana(cur,max)`. SkillCooldownUI와 동일하게 씬 단일 오브젝트로 존재, owner가 탐색.
- 완료 기준: 컴파일 성공, 메서드 호출 시 슬라이더/텍스트 갱신.
- 예상 리스크: 낮음(순수 로컬 UI).

### Step 5. Player_NetworkSpawn 스폰 시 UI 연결
- 작업 내용: owner 분기에서 — (1) 머리 위 Canvas를 로컬 비활성(본인 화면에서 숨김, 타 인스턴스 영향 없음), (2) `FindAnyObjectByType<PlayerHUD>()`로 화면 HUD 탐색, (3) `GetComponent<Stat>().BindOwnerHUD(hud)` 호출, (4) HUD 이름을 LocalClientId로 설정. 기존 카메라/시네머신 연결 로직은 유지. 서버 분기의 머리 이름 ClientRpc(타 플레이어 표시용)는 유지.
- 완료 기준: owner는 머리 위 UI가 안 보이고 좌측 하단 HUD에 이름·HP·마나 표시, 타 플레이어 머리 위엔 이름·HP 표시.
- 예상 리스크: 컴포넌트 간 OnNetworkSpawn 순서 — BindOwnerHUD가 초기값 세팅 + 구독을 모두 처리하므로 순서 무관하게 동작.

### Step 6. Player_Skill 마나 게이트(서버 권위) + 비풀 스킬 헬퍼
- 작업 내용:
  - `CheckCanUseSkill`에 owner 사전 게이트 추가: `Stat.pMana < data.fManaCost`면 null 반환(복제된 마나로 사전 차단).
  - 서버 권위 소모: 스킬 확정 지점(`Animation_Play_ServerRpc`)에서 `Stat.TryConsumeMana(data.fManaCost)` 호출, 실패 시 즉시 return(weight·anim·쿨타임·NotifyCooldown 모두 미발생). 마나는 서버에서 1회만 소모.
  - 투사체 게이트: 풀 스킬의 투사체 생성이 서버 마나 확정을 우회하지 못하도록 보장. 마법사 Q는 투사체 생성을 서버 마나 확정 이후 경로로 모으고, 골렘 좌클릭의 애니 이벤트 투사체는 스킬 확정(net_SkillWeight>0/활성 상태)일 때만 생성되도록 서버에서 검증. (클라가 보낸 마나·발동 요청 불신)
  - 비풀 스킬 공용 서버 헬퍼: 직렬화된 SkillData를 받아 마나 확정·쿨타임·애니 재생(Skill 레이어)·ClientRpc 동기화·NotifyCooldown을 수행하는 서버 메서드 추가(투사체 없음). 서버 인스턴스도 직렬화 SkillData를 보유하므로 마나/쿨타임/상태명을 서버에서 직접 읽어 클라 값 불신.
- 완료 기준: 마나 부족 시 풀 스킬의 애니·투사체·쿨타임 모두 미발생. 충분 시 정상 발동 + 정확히 비용만큼 소모.
- 예상 리스크: 기존 2개 RPC(UseSkill/Animation_Play) 흐름의 경합 — 서버 확정 지점 일원화로 해소.

### Step 7. 골렘 Q / 마법사 좌클릭 비풀 스킬 구현
- 작업 내용:
  - `Golem_Skill`: `[SerializeField] private SkillData _qSkillData;`(Golem_Q_Skill 연결). OnBuff(Q)를 owner 사전 게이트 후 서버 RPC 호출로 변경 → 서버에서 _qSkillData로 Step 6 비풀 헬퍼 실행(마나 20·`Standing Taunt Battlecry` 재생). 투사체 없음.
  - `Magician_Skill`: `[SerializeField] private SkillData _mouseSkillData;`(Magician_Mouse_Skill 연결). OnAttack_Skill(좌클릭)을 동일 패턴으로 구현(마나 20·`Standing 2H Magic Area Attack 01` 재생).
- 완료 기준: 두 액션이 마나 충분 시 애니 재생 + 마나 소모 + 쿨타임, 부족 시 아무 일도 없음.
- 예상 리스크: 애니메이터 컨트롤러 Skill 레이어에 해당 상태명이 없으면 애니는 미재생(마나/쿨타임은 동작). 6절에서 상태 존재 여부 검증 필요.

### Step 8. 마법사 기본공격 마나 소모
- 작업 내용: `Player_UpperBody`에 `protected virtual float GetBasicAttackManaCost() => 0f;` 추가, `OnAttack`에서 비용>0이고 owner 마나 부족이면 return(골렘은 0이라 무영향). `Magician_UpperBody`에서 GetBasicAttackManaCost를 Electric SkillData의 fManaCost로 override. 서버 권위 소모는 `NormalAttack_ServerRpc`에서 `GetSkillData(currentSkill).fManaCost`로 TryConsumeMana, 실패 시 투사체 미생성.
- 완료 기준: 마법사 기본공격이 마나 5 소모, 부족 시 투사체 미발생. 골렘 기본공격은 마나 무관.
- 예상 리스크: 기본공격 스윙 애니가 OnAttack에서 즉시 재생되므로, owner 사전 게이트로 부족 시 스윙도 차단.

### Step 9. 에셋·프리팹·씬 배선 (승인 후)
- 작업 내용: 2절 "에디터/에셋 변경" 항목 일괄 적용 — 데이터 자산 마나 값, SkillData 비용, 프리팹 SkillData 참조, 씬 좌측 하단 HUD(파란 마나바) 구성·PlayerHUD 배선.
- 완료 기준: 플레이모드에서 전체 시나리오(6절) 통과.
- 예상 리스크: 씬/프리팹 수동 배선 누락 — 6절 검증으로 확인.

## 5. 가정 및 제약
- Stat은 플레이어 전용 경로에서만 마나를 사용한다. Stat_Data가 Player_Data가 아니면(적 등) 마나 초기화를 스킵한다(경계 검증).
- 마나 비용은 전적으로 SkillData.fManaCost에 저장한다(TASK 명세). 비풀 스킬은 컴포넌트에 직렬화된 SkillData 참조로 비용을 서버에서 읽는다.
- SkillType enum과 SkillPool은 변경하지 않는다(미배정 prefab으로 풀 초기화가 깨지는 것 방지). 골렘 Q·마법사 좌클릭은 투사체 없이 애니+마나만 처리한다.
- 머리 위 UI 숨김은 owner 로컬 가시성 처리이며, 네트워크로 동기화되는 상태가 아니다(타 클라 인스턴스 영향 없음).
- 화면 HUD는 씬 단일 오브젝트로 기존 Canvas(SkillCooldownHUD 위치)에 추가하며, owner가 FindAnyObjectByType로 탐색한다.
- 데이터 ScriptableObject(Player_Data/Entity_Data)는 기존 public 필드 컨벤션을 따른다. CLAUDE.md의 public 변수 금지 규칙은 동작 스크립트 대상으로 해석한다.
- 마나는 스폰 시 최대치로 시작한다(명세에 초기값 명시 없음 — 최대 시작이 합리적 기본값).

## 6. 검증 방법
- **빌드**: 전 스크립트 컴파일 성공.
- **Step별**: 각 Step 완료 후 컴파일 + 관련 동작 단위 확인.
- **애니 상태 확인**: 골렘 `Standing Taunt Battlecry`, 마법사 `Standing 2H Magic Area Attack 01` 상태가 각 Animator Skill 레이어에 존재하는지 검증(없으면 애니만 미재생, 마나/쿨타임은 정상 — 리스크로 보고).
- **최종 플레이모드 시나리오(호스트+클라 2인)**:
  1. 본인 화면: 머리 위 HP UI 안 보임. 좌측 하단에 본인 이름·HP바·파란 마나바 표시.
  2. 상대 화면: 상대 머리 위에 이름·HP바 표시.
  3. 마나가 초당 재생량(골렘 2/마법사 3)만큼 차오름, 최대치(골렘 100/마법사 200)에서 멈춤.
  4. 각 스킬 사용 시 해당 마나가 정확히 소모(표 기준), 좌측 하단 마나바 감소.
  5. 마나 부족 시 해당 스킬 발동 불가(애니·투사체·쿨타임 모두 미발생).
  6. 골렘 기본공격은 마나 소모 없음.
  7. 피격으로 HP 감소 시 본인 좌측 하단 HP바와 상대 머리 위 HP바가 모두 갱신.
