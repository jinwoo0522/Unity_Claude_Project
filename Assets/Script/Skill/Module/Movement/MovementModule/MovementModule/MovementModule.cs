using UnityEngine;

public interface IMovementModule
{
    void Bind(Transform transform);
    void Enter();
    void Move(float fTimeDelta);
}
