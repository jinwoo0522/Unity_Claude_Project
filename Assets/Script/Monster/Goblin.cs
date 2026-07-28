using UnityEngine;

public class Goblin : Entity
{

    // 상태 추가 , 고블린 상태는 idle, attack , hit , move, die , freeze , airbone , 끝 
    // 상태를 공유할수있는것들이 있을것같은데 die,  frozen, airbone 이거는 entity용으로 상태를 수정해서 공유가능하지않을까?
    // 상체 상태는 없고 하나의 상태머신만 가진다.

    // 이동 , 회전 : 고블린이 range 안에 가장 먼저 들어온 플레이어를 쫓아가며 일정 거리 이상 이동하면 다시 되돌아간다.
    // 공격 상태 : 플레이어가 일정 거리 안으로 들어오면 공격상태로 들어가고 다시 idle,
    // 플레이어와의 거리를 어떻게 판정할 것인가?
    // spherecast 사용해야하나? 아니면 콜라이더를 달고 충돌로검사? , 아니면 매니저로 검사?
    // 만약 range 이상 거리에서 공격을 받게 되면 그 플레이어를 쫓아가야하는데 해당 플레이어를 어떻게 받아오지?
    // ->hitevent에 심어야겟다 스킬이면 스킬을 시전한 사람 owner를 가져와야할텐데 흠
    
    [SerializeField] private Transform _spawnPoint;   // 복귀 지점 — 몬스터를 따라 움직이지 않는 외부 오브젝트를 연결한다

    public IHitter _hitter {get; private set;}
    public MonsterTargeter _targeter {get; private set;}

    public override ENTITY.Faction Faction => ENTITY.Faction.MONSTER;
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        _hitter = GetComponent<IHitter>();
        // 타겟 보관소 — 탐색은 IDLE 상태가 주기적으로 돌린다
        _targeter = new MonsterTargeter(transform, _spawnPoint);
        // 회전 소스는 타게터의 타겟 — 호출 시점은 회전이 허용되는 상태가 정한다
        _rotate = new MonsterRotate(transform, _targeter, _stat._data.fRotateSpeed);

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

       _stateMachine.CreateState((ushort)ENTITY.StateType.AIRBORNE, new EntityAirborneState(this));
       _stateMachine.CreateState((ushort)ENTITY.StateType.FROZEN, new EntityFrozenState(this));

       _stateMachine.CreateAnyTransition(new AnyToAirborne_Entity(_crowdController));
       _stateMachine.CreateAnyTransition(new AnyToFrozen_Entity(_crowdController));


    }

}
