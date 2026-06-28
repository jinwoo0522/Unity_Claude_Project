using UnityEngine;

public interface IState
{
    public void Enter();
    public void Exit();
    public void Update(float fTimedelta , ushort curState);
    public ushort Check_Transition(float fTimedelta, ushort curState);
    public void Create();
    public void Reset();
    // 상태 생성 후 캐릭터별로 전환을 외부에서 주입하기 위한 메서드
    public void AddTransition(ITransition trans);
}
