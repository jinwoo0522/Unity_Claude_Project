## Unity Projects Instructions

- Always use `unity-cli` for all Unity Editor interactions.
- Installed at: `C:/Users/kim05/AppData/Local/unity-cli/unity-cli`


### Key Commands

| Command | Description |
|---------|-------------|
| `unity-cli editor refresh --compile` | Recompile scripts |
| `unity-cli console --type error` | Check console errors |
| `unity-cli editor play` | Enter play mode |
| `unity-cli editor stop` | Exit play mode |
| `unity-cli status` | Check editor state |

## 코드/에디터 변경 규칙

- 코드 및 에디터 설정 변경 전 승인받고 할 것
- `Assets/Reimport All` 절대 금지 — Unity 크래시 유발, 승인 여부와 무관하게 실행 불가

## 코드 작성 규칙

- 외부에서 Inspector로 입력받아야 하는 필드는 `public` 대신 `[SerializeField]` + `private` 또는 `protected` 사용
- 스크립트 내부에서 `public` 변수 사용 금지 (은닉화 원칙 준수)

## 에셋 참조 규칙

- 애니메이션, 프리팹, 마스크 등 에셋을 참조할 때는 반드시 현재 프로젝트의 실제 파일과 guid를 확인 후 작업
- 대화 초반에 읽은 정보를 그대로 쓰지 말 것 - 사용자가 에셋을 교체/삭제/이동했을 수 있음
- Animator controller에 guid를 넣기 전 반드시 해당 .meta 파일로 guid 검증

