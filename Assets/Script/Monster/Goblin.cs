using UnityEngine;

public class Goblin : Monster
{
    public override ENTITY.Faction Faction => ENTITY.Faction.MONSTER;
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // // 회전 소스는 타게터의 타겟 — 호출 시점은 회전이 허용되는 상태가 정한다
        // _rotate = new MonsterRotate(transform, _targeter, _stat._data.fRotateSpeed);

        CreateState();

        if(IsServer == false) return;
       // 시작 시 IDLE 시작
       _stateMachine.TransitionTo((ushort)ENTITY.StateType.IDLE);
    }

    protected override void Update()
    {
        base.Update();
    }

    void CreateState()
    {
       _stateMachine.CreateState((ushort)ENTITY.StateType.IDLE, new GoblinIdleState(this));
       _stateMachine.CreateState((ushort)ENTITY.StateType.WALK, new GoblinMoveState(this));
       _stateMachine.CreateState((ushort)ENTITY.StateType.ATTACK, new GoblinAttackState(this));

       _stateMachine.CreateState((ushort)ENTITY.StateType.AIRBORNE, new EntityAirborneState(this));
       _stateMachine.CreateState((ushort)ENTITY.StateType.LAND, new GoblinLandState(this));
       _stateMachine.CreateState((ushort)ENTITY.StateType.FROZEN, new EntityFrozenState(this));
       _stateMachine.CreateState((ushort)MONSTER.StateType.HIT, new MonsterHitState(this));

       // 등록 순서가 곧 우선순위 — CC가 피격 경직보다 앞선다
       _stateMachine.CreateAnyTransition(new AnyToAirborne_Entity(_crowdController));
       _stateMachine.CreateAnyTransition(new AnyToFrozen_Entity(_crowdController));
       _stateMachine.CreateAnyTransition(new AnyToHit_Monster(_crowdController, _stat));


    }

}
