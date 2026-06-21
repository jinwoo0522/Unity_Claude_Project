using System;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

public abstract class EntityState : IState
{

    protected List<ITransition> TransitionList = new();

    public abstract void Create();
    public abstract void Enter();
    public abstract void Exit();
    public void Update(float fTimedelta, ushort curState)
    {
        UpdateState(fTimedelta , curState);
    }

    protected abstract void UpdateState(float fTimedelta , ushort curState);

    public ushort Check_Transition(float fTimedelta)
    {
        foreach(var tarns in TransitionList)
        {
            if(tarns.CheckRule(fTimedelta) == false) continue;

            tarns.OnTransition();

            return tarns.NextState;
        }
        
        return 0;
    }
}
