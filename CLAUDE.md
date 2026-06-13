## Unity Projects Instructions

- Always use `unity-cli` for all Unity Editor interactions.
- Installed at: `C:/Users/kim05/AppData/Local/unity-cli/unity-cli`
- Always use `unity-scanner` for all Unity asset reference/search tasks (reduces token usage).
- Installed at: `C:/Users/kim05/AppData/Local/unity-scanner/unity-scanner.exe`

## 코드/에디터 변경 규칙

- 코드 및 에디터 설정 변경 전 승인받고 할 것
- `Assets/Reimport All` 절대 금지 — Unity 크래시 유발, 승인 여부와 무관하게 실행 불가

## 코드 작성 규칙

- 외부에서 Inspector로 입력받아야 하는 필드는 `public` 대신 `[SerializeField]` + `private` 또는 `protected` 사용
- 스크립트 내부에서 `public` 변수 사용 금지 (은닉화 원칙 준수)
- 게임 로직은 서버 권위적(Server-Authoritative)으로 설계할 것 — 클라이언트는 입력만 전송하고, 검증/판정은 반드시 서버에서 수행
- 보안을 고려하여 코드를 작성할 것 — 클라이언트 입력값을 신뢰하지 말고, 중요 데이터는 서버에서만 관리
- null 체크 코드 작성 금지 — 내부 코드와 프레임워크 보장을 신뢰하고, 시스템 경계(사용자 입력, 외부 API)에서만 검증할 것

## 에셋 참조 규칙

- 애니메이션, 프리팹, 마스크 등 에셋을 참조할 때는 반드시 현재 프로젝트의 실제 파일과 guid를 확인 후 작업
- 대화 초반에 읽은 정보를 그대로 쓰지 말 것 - 사용자가 에셋을 교체/삭제/이동했을 수 있음
- Animator controller에 guid를 넣기 전 반드시 해당 .meta 파일로 guid 검증
- 에셋 검색/참조 확인은 반드시 `unity-scanner`를 사용할 것 (Read/Grep 대신 사용하여 토큰 절약)

## 프로젝트 디렉토리 구조 (탐색 시 우선 참조)

파일 탐색은 아래 고정 경로를 직접 겨냥한다. `Glob "**/*.cs"`처럼 프로젝트 전체를 훑으면
`Library/PackageCache`·VFX 에셋이 수천 줄 딸려와 토큰을 낭비한다. 반드시 `Assets/` 하위로
한정하고, 종류별 고정 경로를 먼저 본다.

- 스크립트: `Assets/Script`
- 프리팹: `Assets/Prefabs`
- 데이터(ScriptableObject): `Assets/Resources/Data`
- 애니메이션 클립: `Assets/Animation`
- 애니메이터 컨트롤러: `Assets/Animator`
- 씬: `Assets/Scenes`

- 탐색은 **항상 `Assets/` 부터** 시작한다. 단, `TASK.md`·`PLAN.md`·`CLAUDE.md` 등 `.md`
  문서를 찾을 때만 예외(워크트리 루트 기준).
- 에셋·프리팹·참조(guid) 탐색은 raw Glob/Grep 대신 `unity-scanner`(`search`/`refs`/`read`)를
  적극 활용한다. Glob은 위 고정 경로로 좁혀서만 사용한다.
- 변경에 관련된 스크립트는 해당 부분을 전체 읽어 구조를 정확히 파악한다 (부분 읽기로 추측 금지).

