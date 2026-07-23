using UnityEngine;

// 스킬 모듈 인터페이스 — 기본(빈) 구현을 제공하므로 구현체는 필요한 것만 override 한다
public interface ISkillModule
{
    void Bind(Skill skill) { }
    void Enter() { }
    void ServerTick(float fTimeDelta) { }
    void ClientTick(float fTimeDelta) { }
    void CollisionEnter(Collider collider) { }
    void CollisionStay(Collider collider) { }
    void Exit() { }
}
