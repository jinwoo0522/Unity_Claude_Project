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
- 컴파일/에러 확인은 허락 없이 바로 실행

## 에셋 참조 규칙

- 애니메이션, 프리팹, 마스크 등 에셋을 참조할 때는 반드시 현재 프로젝트의 실제 파일과 guid를 확인 후 작업
- 대화 초반에 읽은 정보를 그대로 쓰지 말 것 - 사용자가 에셋을 교체/삭제/이동했을 수 있음
- Animator controller에 guid를 넣기 전 반드시 해당 .meta 파일로 guid 검증

## Git 규칙

- 머지 전 반드시 `unity-cli menu "File/Save Project"` 로 Unity 씬 저장
- Develop 머지 시 반드시 `--no-ff` 옵션 사용 (훅 실행을 위해 필수)
- Develop 머지 후 반드시 `git push origin Develop` 으로 원격 푸시
- 각 작업 브랜치 완료 후 해당 브랜치도 `git push origin <브랜치명>` 으로 원격 푸시
