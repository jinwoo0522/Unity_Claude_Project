using UnityEngine;

// 피격 경직 — 상·하체 분리가 없는 몬스터라 피격 모션이 끝날 때까지 이동·회전을 모두 막는다
public class MonsterHitState : EntityState
{
    private IDamagable _damagable;
    private IEntityMovement _move;
    private EntityAnimator _aniController;

    private const float _fReHitTime = 0.3f;   // 다음 경직으로 넘어가기까지의 최소 간격

    public MonsterHitState(Entity entity)
    {
        _damagable = entity._stat;
        _move = entity._move;
        _aniController = entity._aniController;
    }

    public override void Create()
    {
        TransitionList.Add(new HitToIdle_Monster(_aniController, _damagable, _fReHitTime));
    }

    public override void Enter()
    {
        _aniController._state.Value = (ushort)MONSTER.StateType.HIT;
        // 진입시킨 타격은 여기서 소비한다 — 남기면 곧바로 다음 경직으로 튕겨나간다
        _damagable._isHit = false;
        // 뒤로 밀리는 피격 모션의 이동량은 클립이 직접 만든다
        _aniController._animator.applyRootMotion = true;
    }

    public override void Exit()
    {
        _aniController._animator.applyRootMotion = false;
    }

    protected override void UpdateState(float fTimedelta, ushort curState)
    {
        _move.Gravity();
    }
}
