using System;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

public abstract class EntityState : IState
{

    protected List<ITransition> TransitionList = new();
    protected List<(float , Action)> StateEvents = new();

    float fAccTime;

    public abstract void Create();
    // 캐릭터 클래스에서 캐릭터별 전환을 외부 주입할 때 사용
    public void AddTransition(ITransition trans) => TransitionList.Add(trans);
    public abstract void Enter();
    public abstract void Exit();
    public void Update(float fTimedelta, ushort curState)
    {
        UpdateState(fTimedelta , curState);
        CheckEvent(fTimedelta);
    }

    protected abstract void UpdateState(float fTimedelta , ushort curState);

    public ushort Check_Transition(float fTimedelta, ushort curState)
    {
        foreach(var trans in TransitionList)
        {
            if(trans.CheckRule(fTimedelta) == false) continue;
            if(trans.NextState == curState) continue; // 바꿀 상태가 같은 상태이면 안 바꿈

            trans.OnTransition();
            

            return trans.NextState;
        }
        
        return 0;
    }

    void CheckEvent(float fTimedelta)
    {
        float fPrev = fAccTime;
        fAccTime += fTimedelta;
        foreach(var Event in StateEvents)
        {
            if(Event.Item1 > fPrev && Event.Item1 <= fAccTime)
                Event.Item2.Invoke();
        }
    }
    public void Reset()
    {
        fAccTime = 0f; // 이벤트를 위한 누적 타임 초기화
    }
}
