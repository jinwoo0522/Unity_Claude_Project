# Golem UpperAttack Combo Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 골렘의 `UpperAttack`을 1타, 2타, 3타 콤보로 분기하고 각 구간 내 추가 클릭이 있을 때만 다음 타로 이어지게 만든다.

**Architecture:** 공통 상체 레이어 제어는 `Player_UpperBody`에 유지하고, 골렘 전용 시간 구간 기반 콤보 상태는 `Golem_UpperBody`에 배치한다. 테스트 가능한 시간 판정 로직은 순수 메서드로 분리해서 EditMode 테스트로 검증하고, 최종 검증은 `unity-cli` 재컴파일과 테스트 실행으로 마무리한다.

**Tech Stack:** Unity 6, C#, Animator UpperBody Layer, Unity Test Framework, unity-cli

---

### Task 1: 공통 상체 제어 확장

**Files:**
- Modify: `Assets/Script/Player_UpperBody.cs`

- [ ] **Step 1: 공격 상태를 자식이 제어할 수 있는 지점 추가**

```csharp
protected bool IsUpperLayerActive => anim != null && anim.GetLayerWeight(UpperBodyLayer) > 0f;
protected Animator CachedAnimator => anim;

protected bool TryStartUpperAttack()
{
    if (IsHit) return false;
    if (IsUpperLayerActive) return false;

    StartUpper("UpperAttack", UpperState.Attacking);
    return true;
}
```

- [ ] **Step 2: 공격 종료와 현재 상태 조회를 자식이 재사용 가능하게 정리**

```csharp
protected bool IsPlayingUpperState(string stateName, AnimatorStateInfo info)
{
    return info.IsName(stateName);
}

protected void BeginUpperFadeOut()
{
    fAccLerpTime = Mathf.Min(fAccLerpTime, 1f);
}
```

- [ ] **Step 3: 기본 공격 진입 경로를 새 확장 지점으로 연결**

```csharp
void OnAttack()
{
    if (TryStartUpperAttack() == false) return;
    OnUpperAttackStarted();
}
```

### Task 2: 골렘 콤보 로직 구현

**Files:**
- Modify: `Assets/Script/Golem_UpperBody.cs`

- [ ] **Step 1: 시간 구간 상수와 콤보 상태 필드 추가**

```csharp
private const float FirstAttackEndTime = 1.15f;
private const float SecondAttackEndTime = 1.55f;

private bool isComboActive;
private bool hasQueuedNextAttack;
private bool hasResolvedCurrentStage;
private int currentComboStage;
```

- [ ] **Step 2: 첫 공격 시작과 추가 클릭 기록 구현**

```csharp
public override void NormalAttack()
{
    if (IsComboActive() == false)
    {
        if (TryStartUpperAttack() == false) return;
        StartCombo();
        return;
    }

    hasQueuedNextAttack = true;
}
```

- [ ] **Step 3: 매 프레임 현재 재생 시간으로 단계 분기 처리**

```csharp
protected override void OnUpperBodyUpdated()
{
    if (isComboActive == false) return;

    AnimatorStateInfo info = CachedAnimator.GetCurrentAnimatorStateInfo(UpperBodyLayerIndex);
    if (info.IsName("UpperAttack") == false)
    {
        ResetCombo();
        return;
    }

    float currentTime = GetCurrentAttackTime(info);
    ResolveComboByTime(currentTime, info.length);
}
```

- [ ] **Step 4: 단계 종료 시 다음 타 진행 또는 종료 처리**

```csharp
private void ResolveComboByTime(float currentTime, float clipLength)
{
    if (currentComboStage == 1)
        ResolveStage(currentTime, FirstAttackEndTime, 2);
    else if (currentComboStage == 2)
        ResolveStage(currentTime, SecondAttackEndTime, 3);
    else if (currentComboStage == 3 && currentTime >= clipLength)
        ResetCombo();
}
```

### Task 3: EditMode 테스트와 Unity 검증

**Files:**
- Create: `Assets/Tests/Editor/GolemUpperBodyComboLogicTests.cs`
- Create: `Assets/Tests/Editor/Tests.Editor.asmdef`

- [ ] **Step 1: 시간 구간 판정 순수 로직 테스트 작성**

```csharp
[Test]
public void FirstStage_WithoutQueuedInput_ReturnsEndCombo()
{
    ComboDecision decision = GolemComboLogic.ResolveStage(1, 1.15f, false, 2.0f);
    Assert.AreEqual(ComboDecision.EndCombo, decision);
}
```

- [ ] **Step 2: EditMode 테스트를 실행해 RED 확인**

```bash
unity-cli test --filter GolemUpperBodyComboLogicTests
```

Expected: `ResolveStage` 또는 테스트 대상이 없어 실패

- [ ] **Step 3: 최소 구현 후 동일 테스트 재실행**

```bash
unity-cli test --filter GolemUpperBodyComboLogicTests
```

Expected: PASS

- [ ] **Step 4: 전체 스크립트 재컴파일과 콘솔 확인**

```bash
unity-cli editor refresh --compile
unity-cli console --type error
```

Expected: compile 성공, error 없음
