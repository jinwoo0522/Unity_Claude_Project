## 계획 검증 결과

### 요구사항 대조 (TASK.md → PLAN.md)

- [✅] **HP UI 본인 숨김**: 머리 위 Canvas를 owner 로컬에서만 비활성 (Step 5)
- [✅] **타 플레이어 HP UI 표시**: 비owner 머리 위 hpSlider 바인딩 유지 명시 (Step 5, 건드리지 않을 것)
- [✅] **본인 화면 좌측 하단 HUD**: PlayerHUD.cs 신규 생성 + 이름·HP Slider 구성 (Step 4)
- [✅] **Player_NetworkSpawn 스폰 시 UI 연결**: OnNetworkSpawn owner 분기에서 바인딩 (Step 5)
- [✅] **마나 NetworkVariable**: Stat에 fMana/fMaxMana 추가, 쓰기 Server (Step 3)
- [✅] **Player_Data 마나 필드**: fMaxMana, fManaRegen 추가 (Step 1)
- [✅] **골렘 100/2, 마법사 200/3**: 에셋 배선 항목 (Step 9)
- [✅] **마나 재생 (서버 Update)**: Stat Update에서 fManaRegen * deltaTime (Step 3)
- [✅] **SkillData.fManaCost**: [SerializeField] private + public getter 추가 (Step 2)
- [✅] **마나 부족 스킬 차단**: owner 사전 게이트 + 서버 권위 TryConsumeMana (Step 6)
- [✅] **좌측 하단 마나바 (파란색)**: PlayerHUD에 Mana Slider, 파란 Fill (Step 4, Step 9)
- [✅] **서버 권위**: 마나 변경·소모·재생 전부 서버에서만 (Step 3, 6, 8)
- [✅] **소모량 5종 매핑**: 골렘Q=20, 골렘좌클릭=30, 마법사기본=5, 마법사Q=30, 마법사좌클릭=20 (매핑 표)
- [✅] **골렘 기본공격 마나 0**: Player_UpperBody 가상 메서드 기본값 0 (Step 8)

### 이슈 목록

- 1. [낮음] **Player_NetworkSpawn 코드 삽입 순서 미명시** — 현재 `OnNetworkSpawn()` owner 분기에 `if(PlayerCamera == null) return;` 같은 early return이 2개 있음(라인 22~33). PLAN Step 5가 추가하는 `canvas.gameObject.SetActive(false)`를 early return 이후에 배치하면 카메라 설정 실패 시 canvas 숨김이 누락됨. canvas 숨김은 early return 이전에, HUD 바인딩은 그 이후에 배치해야 함 — 명시 필요.

- 2. [낮음] **비풀 스킬 owner 측 쿨타임 로컬 체크 없음** — `CheckCanUseSkill`은 풀 SkillData를 전제하므로 비풀 스킬(Golem Q, 마법사 좌클릭)에서 사용 불가. PLAN Step 7의 "owner 사전 게이트"는 마나 체크만 가능하고 로컬 쿨타임 체크는 미명시. 기존 코드도 동일 구조(서버가 쿨타임 권위 보유)이므로 동작 정확성에는 영향 없음.

- 3. [낮음] **Magician_UpperBody.GetBasicAttackManaCost() override의 ElectricSkillData 접근 방법 미명시** — 기존 `currentSkill` SkillType으로 `GameManager.Instance.skillPool.GetSkillData(currentSkill)?.fManaCost ?? 0f`를 사용하면 별도 필드 추가 없이 구현 가능. 명시하지 않아 구현 Agent가 불필요하게 새 직렬화 필드를 추가할 수 있음.

### 확인 필요 항목

없음.

### 최종 판정

**통과**

낮음 심각도 이슈 3개 모두 구현 시 합리적 판단으로 처리 가능한 수준. 요구사항 커버리지, 경로·에셋 실존, CLAUDE.md 규칙 준수, 애니메이터 상태명 모두 이상 없음.
