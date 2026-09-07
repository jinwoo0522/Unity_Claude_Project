using UnityEngine;

public class Goblin : Monster
{
    public override ENTITY.Faction Faction => ENTITY.Faction.MONSTER;

    public RagdollController _rc {get; private set;}

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        _rc = GetComponent<RagdollController>();
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
       _stateMachine.CreateState((ushort)ENTITY.StateType.DIE, new MonsterDieState(this, _rc));

       // 등록 순서가 곧 우선순위 — 사망이 최우선, 그다음 CC, 마지막이 피격 경직
       _stateMachine.CreateAnyTransition(new AnyToDie_Monster(_stat, _damagable));
       _stateMachine.CreateAnyTransition(new AnyToAirborne_Entity(_crowdController));
       _stateMachine.CreateAnyTransition(new AnyToFrozen_Entity(_crowdController));
       _stateMachine.CreateAnyTransition(new AnyToHit_Monster(_crowdController, _damagable));


    }

}
