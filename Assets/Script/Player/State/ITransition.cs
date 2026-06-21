using UnityEngine;

public interface ITransition 
{
    bool CheckRule(float fTimeDelta);
    void OnTransition();
    public ushort NextState {get;}
}
