using System;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine
{
    private Dictionary<ushort , IState> States = new(); // 선택된 상태만 감지하는 상태
    private List<ITransition> AnyTransition= new(); // 항상 감지하는 상태

    private ushort CurState = 0;

    public void State_Update(float fTimeDelta)
    {
        if(ChangeAnyState(fTimeDelta) == true) return;
        if(ChangeState(fTimeDelta) == true) return;

        States[CurState].Update(fTimeDelta, CurState);
    }

    public void CreateState(ushort tag , IState state)
    {
        if(States.TryAdd(tag , state) == false) 
            Debug.LogError("State 추가 Null 발생");

          States[tag].Create(); // 트랜지션 생성 
    }
    public void CreateAnyTransition(ITransition trans)
    {
        if(trans == null) 
            Debug.LogError("AnyTransition 추가 Null 발생");

        AnyTransition.Add(trans);
    }

    public void TransitionTo(ushort next)
    {
        if(CurState == next) return;

        if(CurState != 0) States[CurState].Exit();
        CurState = next;
        States[next].Reset();
        States[next].Enter();

    }

    private bool ChangeState(float fTimeDelta)
    {
        ushort nextState = States[CurState].Check_Transition(fTimeDelta, CurState);
        if(nextState != 0)
        {
            TransitionTo(nextState);
            return true;
        }
        return false;
    }
    private bool ChangeAnyState(float fTimeDelta)
    {
        ushort nextState = Check_AnyTransition(fTimeDelta);
        
        if(nextState != 0)
        {
            TransitionTo(nextState);
            return true;
        }
        return false;
    }

    public ushort Check_AnyTransition(float fTimedelta)
    {
        foreach(var trans in AnyTransition)
        {
            if(trans.CheckRule(fTimedelta) == false) continue; 
            if(trans.NextState == CurState) continue; // 바꿀 상태가 같은 상태이면 안 바꿈

            trans.OnTransition();

            return trans.NextState;
        }

        return 0;
    }

}
