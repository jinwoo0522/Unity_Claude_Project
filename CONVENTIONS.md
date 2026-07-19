# Unity 프로젝트 코드 컨벤션

이 문서는 프로젝트 전반의 코드 스타일과 설계 규칙을 정리한다.
신규 코드는 이 컨벤션을 따르고, 기존 코드 수정 시에도 점진적으로 맞춰간다.
---

## 1. 네이밍 규칙

| 대상 | 규칙 | 예시 |
| --- | --- | --- |
| 클래스 / 구조체 | PascalCase | `Skill`, `StateMachine`, `GameManager` |
| 인터페이스 | `I` 접두사 + PascalCase | `ISkill`, `IState`, `IEntityMovement` |
| public 프로퍼티 | PascalCase, 헝가리안 미적용 | `Number`, `Type`, `Data` |
| `int` 지역/멤버 | `i` 접두사 | `iNum`, `iCount` |
| `float` 지역/멤버 | `f` 접두사 | `fTime`, `fSpeed` |
| `double` 지역/멤버 | `d` 접두사 | `dDouble` |
| `bool` 지역/멤버 | `is` 접두사 | `isBool`, `isActive` |
| `Vector2` / `Vector3` 지역/멤버 | `v` 접두사 | `vDir`, `vCustomDir` |
| 멤버 변수 | `_` 접두사 | `_iNum`, `_fTime`, `_isActive` |
| 클래스 타입 멤버 객체 | `_` + 소문자 시작 | `_hitter`, `_effector` |
| enum 값(태그) | 대문자만 사용 | `IDLE`, `WALK`, `ATTACK_START` |

- **식별자 스펠링이 틀리면 반드시 올바르게 수정한다.** (예: `Clinet` → `Client`, `Animatior` → `Animator`)
---

## 2. 접근 제어 / 은닉화

- **접근 지정자(`public` / `protected` / `private`)는 반드시 명시한다.** 생략해 기본값에 의존하지 않는다.
- **변수·함수 모두 접근 수준이 넓은 순서로 나열한다: `public` → `protected` → `private`.**
- **`[SerializeField]` 는 선언과 한 줄에 붙여 쓴다.** 어트리뷰트와 필드를 두 줄로 나누지 않는다.
  - O: `[SerializeField] private EntityEffector _effector;`
  - X: `[SerializeField]` 와 `private EntityEffector _effector;` 를 두 줄로 분리


## 3. 주석 / 언어

- 모든 주석·커밋 메시지·문서는 **한국어**로 작성한다.
