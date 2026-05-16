# Golem UpperAttack Combo Design

**목표:** 골렘의 `UpperAttack` 애니메이션을 1타, 2타, 3타 콤보로 분기하고, 각 구간 내 클릭 입력이 있었을 때만 다음 타로 진행되도록 만든다.

## 요구사항 정리

- 대상 캐릭터는 골렘이다.
- 공격은 `UpperAttack` 상태 하나를 기준으로 동작한다.
- 애니메이션은 현재 하나의 연속 클립이지만, 다음 시간 구간으로 나뉜다.
  - 1타: `0.0 ~ 1.15초`
  - 2타: `1.15 ~ 1.55초`
  - 3타: `1.55초 ~ 클립 종료`
- 각 구간 안에서 클릭 입력이 한 번이라도 들어오면 다음 타로 진행한다.
- 각 구간 안에서 클릭 입력이 없으면 해당 구간 종료 시 공격을 종료한다.
- 2타 역시 `UpperBody` 레이어에서 처리한다.
- 공격 중에도 이동은 가능해야 한다.

## 현재 구조

- [`Assets/Script/Player_UpperBody.cs`](/C:/Unity/My%20project/Assets/Script/Player_UpperBody.cs) 는 상체 레이어 weight 관리와 `UpperAttack`/`UpperHit` 재생을 공통 처리한다.
- 현재 공격 시작은 `OnAttack()`에서 `UpperAttack`을 `0f`부터 재생하는 단일 구조다.
- [`Assets/Script/Golem_UpperBody.cs`](/C:/Unity/My%20project/Assets/Script/Golem_UpperBody.cs) 는 아직 비어 있다.
- [`Assets/Animator/GolemAnimator.controller`](/C:/Unity/My%20project/Assets/Animator/GolemAnimator.controller) 는 `UpperBody` 레이어에 `UpperAttack` 상태 하나를 사용한다.

## 선택한 접근

`Animator` 상태를 1타/2타/3타로 쪼개지 않고, `Golem_UpperBody`가 `UpperAttack` 단일 클립의 현재 재생 시간을 해석해서 콤보를 제어한다.

이 접근을 선택한 이유는 다음과 같다.

- 현재 `Animator` 구조를 최소한만 건드릴 수 있다.
- 요구사항이 하나의 `UpperAttack` 클립 시간 구간 분기를 직접 지정하고 있다.
- 골렘 전용 로직을 `Golem_UpperBody`에 국한시켜 기존 공통 상체 구조를 유지할 수 있다.

## 설계

### 1. 책임 분리

- `Player_UpperBody`
  - 상체 레이어 weight 관리
  - `UpperAttack`, `UpperHit` 재생 시작
  - 현재 상체 상태 조회와 종료 페이드 처리
- `Golem_UpperBody`
  - 골렘 콤보 공격 입력 수집
  - 현재 콤보 단계 추적
  - 각 구간 종료 시점에서 다음 타 진행 여부 판정

### 2. 콤보 상태

`Golem_UpperBody`는 아래 상태를 가진다.

- 현재 콤보 활성 여부
- 현재 콤보 단계 (`1`, `2`, `3`)
- 현재 단계 구간 안에서 추가 클릭이 있었는지 여부
- 현재 단계 종료 판정을 이미 처리했는지 여부

이 상태는 "선입력 버퍼 없이 현재 구간 클릭만 유효"라는 규칙을 직접 표현한다.

### 3. 입력 처리 규칙

- 첫 클릭:
  - 공격이 비활성 상태면 `UpperAttack`을 `0초`부터 시작한다.
  - 콤보 단계를 `1`로 초기화한다.
- 공격 중 추가 클릭:
  - 현재 단계 구간 안에서 다음 타 진행 예약 플래그만 `true`로 기록한다.
  - 같은 단계에서 여러 번 클릭해도 추가 누적은 하지 않는다.

입력은 즉시 애니메이션을 점프시키지 않고, 각 단계 종료 시점에서만 소비한다.

### 4. 시간 판정 규칙

`UpperBody` 레이어의 `UpperAttack` 상태 재생 시간을 매 프레임 읽어서 다음 기준으로 판정한다.

- 현재 시간이 `1.15초`에 도달했을 때:
  - 예약 입력이 있으면 2타로 진행한다.
  - 예약 입력이 없으면 공격 종료 페이드를 시작한다.
- 현재 시간이 `1.55초`에 도달했을 때:
  - 예약 입력이 있으면 3타로 진행한다.
  - 예약 입력이 없으면 공격 종료 페이드를 시작한다.
- 3타 구간은 추가 입력 없이 클립 종료까지 재생한 뒤 종료한다.

각 단계에 진입할 때는 다음 단계 판정을 위해 예약 입력 플래그와 판정 완료 플래그를 초기화한다.

### 5. 애니메이션 진행 방식

- 공격 시작 시 `UpperAttack`을 `0f` normalized time에서 재생한다.
- 1타에서 2타, 2타에서 3타로 넘어갈 때는 상태를 다시 바꾸지 않는다.
- 대신 현재 재생 위치가 다음 구간에 도달했을 때 "계속 진행 허용"만 코드로 관리한다.
- 종료 조건이 충족되면 기존 상체 레이어 fade-out 흐름으로 복귀한다.

이 방식은 하나의 긴 공격 클립을 유지하면서도, 입력이 없을 경우 중간 구간 이후 상체 공격을 자연스럽게 꺼지게 만든다.

### 6. 이동과의 관계

- 이동 로직은 [`Assets/Script/Player_Move.cs`](/C:/Unity/My%20project/Assets/Script/Player_Move.cs) 의 기존 동작을 유지한다.
- 콤보 공격은 `UpperBody` 레이어에서만 처리하고, 하체 이동 애니메이션과 병행된다.
- 이번 작업 범위에서는 공격 중 이동 차단을 추가하지 않는다.
- 점프 차단 규칙도 이번 변경 범위에 포함하지 않는다.

## 예상 수정 파일

- 수정: `Assets/Script/Player_UpperBody.cs`
- 수정: `Assets/Script/Golem_UpperBody.cs`
- 확인 필요: `Assets/Animator/GolemAnimator.controller`

## 검증 계획

- 골렘으로 첫 클릭만 입력했을 때 1타 후 공격이 종료되는지 확인
- 1타 구간 내 추가 클릭 시 2타까지 진행하는지 확인
- 2타 구간 내 추가 클릭이 없으면 2타 후 종료되는지 확인
- 2타 구간 내 추가 클릭 시 3타까지 진행하는지 확인
- 공격 중 이동이 계속 가능한지 확인
- 다른 캐릭터 상체 공격 동작이 깨지지 않았는지 확인

## 범위 제외

- 별도 1타/2타/3타 Animator 상태 분리
- Animation Event 기반 분기
- 공격 판정, 데미지 계산, 히트박스 시스템 개편
- 점프/회피/피격 규칙 재설계
