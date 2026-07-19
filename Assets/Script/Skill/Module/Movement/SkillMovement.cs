using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SkillMovement : ISkillModule
{
    // 인스펙터에서 SubclassSelector로 이동 모듈을 부착 — 여러 이동 방식 조합 가능
    [SerializeReference, SubclassSelector] private List<IMovementModule> _movements = new List<IMovementModule>();
    [SerializeField] private bool _stopOnHit;   // 대상 진입 시 이동을 스스로 멈출지 — 관통 투사체는 false

    public bool Move {get; set;}

    public void Bind(Skill skill)
    {
        for (int i = 0; i < _movements.Count; ++i)
            _movements[i].Bind(skill.transform);
    }

    // 발동 — 각 이동 모듈 상태 초기화
    public void Enter()
    {
        Move = true;

        for (int i = 0; i < _movements.Count; ++i)
            _movements[i].Enter();
    }

    // 서버 권위 — 이동 연산은 서버에서만, 위치는 NetworkTransform으로 클라에 복제
    public void ServerTick(float fTimeDelta)
    {
        if(Move == false) return;

        for (int i = 0; i < _movements.Count; ++i)
            _movements[i].Move(fTimeDelta);
    }

    public void ClientTick(float fTimeDelta) { }

    public void Exit() { }

    // 대상 진입 시 — 옵션이 켜져 있으면 스스로 정지 (다른 모듈을 참조하지 않음)
    public void CollisionEnter(Collider collider)
    {
        if (_stopOnHit)
            Move = false;
    }

    public void CollisionStay(Collider collider)
    {
    }

}
