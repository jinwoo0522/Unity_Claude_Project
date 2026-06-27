using UnityEngine;

public interface IState 
{    
    public void Enter();
    public void Exit();
    public void Update(float fTimedelta , ushort curState);
    public ushort Check_Transition(float fTimedelta, ushort curState);
    public void Create();
    public void Reset();
}
