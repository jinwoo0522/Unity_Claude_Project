---
name: unity-task-start
description: Start a Unity task workflow by reading the project root AGENTS.md and TASK.md first, restating requirements, confirming an implementation plan, waiting for user approval before code or Unity editor changes, and verifying with unity-cli after implementation. Use when the user explicitly wants to begin a Unity TASK workflow, asks to follow a standard Unity task-start process, or invokes /unity-task-start.
---

# Unity Task Start

Unity 프로젝트에서 새 TASK 작업을 시작할 때 이 스킬을 사용한다.

## 핵심 규칙

- 모든 응답은 한국어로 작성한다.
- Windows 환경 기준으로 작업한다.
- 작업 시작 전에 프로젝트 루트의 `AGENTS.md`와 `TASK.md`를 반드시 읽는다.
- 사용자 요청이 문서 지침보다 우선한다.
- 문서 지침이 충돌하면 더 구체적이고 현재 작업에 가까운 지침을 우선한다.
- 코드 변경 전 기존 구조와 관련 파일을 먼저 확인한다.
- 목적에 필요한 최소 변경만 수행한다.
- Unity Editor 관련 상호작용은 항상 `unity-cli`를 사용한다.
- 코드 변경 전, Unity 에디터 설정 변경 전에는 반드시 사용자 승인을 받는다.
- `Assets/Reimport All`은 절대 실행하지 않는다.

## 시작 절차

1. 프로젝트 루트의 `AGENTS.md`를 읽고 프로젝트 전용 규칙을 정리한다.
2. 프로젝트 루트의 `TASK.md`를 읽고 현재 구현 목표, 범위, 요구사항을 정리한다.
3. 관련 코드, 프리팹, 씬, Animator, ScriptableObject, 에셋 참조를 확인한다.
4. 에셋 참조가 작업에 포함되면 실제 파일과 `.meta`의 guid를 다시 검증한다.
5. 요구사항이 모호하거나 누락된 부분이 있으면 바로 구현하지 말고 아래 형식으로 재정리한다.
   - 현재 이해한 작업 목표
   - 모호한 부분
   - 빠진 것으로 보이는 항목
   - 제안하는 해석안
6. 재정리한 요구사항을 사용자에게 확인받는다.
7. 구현 범위가 확정되면 아래 형식으로 구현 계획을 작성한다.
   - 수정 대상 파일
   - 변경 목적
   - 구현 방식
   - 영향 범위
   - 검증 방법
8. 구현 계획을 사용자에게 최종 확인받는다.
9. 코드 수정 또는 Unity 설정 변경이 필요하면 작업 직전에 다시 승인 상태를 확인하고 진행한다.

## 구현 규칙

- 기존 구조를 먼저 존중하고 필요한 범위 안에서만 수정한다.
- Inspector 노출이 필요한 필드는 `public` 대신 `[SerializeField] private` 또는 `[SerializeField] protected`를 우선 사용한다.
- 내부 상태 표현용 `public` 변수 사용은 지양한다.
- 공통화, 재사용, 함수 분리를 우선 검토한다.
- 사용자의 승인 없이 독립 하위 작업을 임의로 서브 에이전트로 분할하지 않는다.

## Unity 검증 절차

구현 후 가능한 범위에서 아래 순서로 검증한다.

1. 스크립트 재컴파일
   - `C:/Users/kim05/AppData/Local/unity-cli/unity-cli editor refresh --compile`
2. 콘솔 에러 확인
   - `C:/Users/kim05/AppData/Local/unity-cli/unity-cli console --type error`
3. 필요 시 플레이 검증
   - `C:/Users/kim05/AppData/Local/unity-cli/unity-cli editor play`
   - `C:/Users/kim05/AppData/Local/unity-cli/unity-cli editor stop`

검증을 실행하지 못했다면 이유를 명확히 밝힌다.

## 응답 형식

작업 시작 시:

- 프로젝트 루트의 `AGENTS.md`와 `TASK.md`를 먼저 확인하겠습니다.
- 그다음 관련 코드와 에셋 구조를 보고 요구사항을 정리하겠습니다.

요구사항 재정리 시:

- 현재 이해한 작업 목표는 다음과 같습니다.
- 모호하거나 빠져 보이는 부분은 다음과 같습니다.
- 아래 해석으로 진행해도 되는지 확인 부탁드립니다.

구현 계획 제시 시:

- 구현 전 작업 계획을 정리했습니다.
- 수정 대상, 변경 방식, 영향 범위, 검증 방법 기준으로 진행하려고 합니다.
- 이 계획으로 진행해도 되는지 확인 부탁드립니다.

작업 완료 보고 시:

- 변경 사항: ...
- 영향 범위: ...
- 검증 결과: ...

버그 수정 작업이면 아래 형식을 우선한다.

- 원인: ...
- 수정 내용: ...
- 영향 범위: ...
- 검증 결과: ...
