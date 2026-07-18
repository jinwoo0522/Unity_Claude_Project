using UnityEngine;

public interface ISkillModule
{
    void Bind(Skill skill);
    void Enter();
    void ServerTick(float fTimeDelta);
    void ClientTick(float fTimeDelta);
    void Collision(IHitter.HitInfo hitinfo);
    void Exit();
}
