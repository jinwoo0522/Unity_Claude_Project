using UnityEngine;

public class GolemQSkillState : EntityState
{
    
    Golem_Player _player;
    EntityAnimator _aniController;
    IEntityMovement _move;
    Transform _bodyTransform;
    public GolemQSkillState(Golem_Player player , Transform BodyTransform)
    {
        _player = player;
        _aniController = player._aniController;
        _move = player._move;
        _bodyTransform = BodyTransform;
    }
    public override void Create()
    {
        TransitionList.Add(new StateToIdle_Player(_aniController));
    }

    public override void Enter()
    {
        _aniController._state.Value = (ushort)GOLEM.StateType.Q_SKILL;
        // 서버 권위 — 시전자 위치에 FireBuff 스폰
        GameManager.Instance.skillFactory.Create(
            NetworkObjectType.FIRE_BUFF, _bodyTransform.position , Vector2.zero, _player.OwnerClientId, _player.gameObject);
    }

    public override void Exit()
    {
    }

    protected override void UpdateState(float fTimedelta, ushort curState)
    {
        _move.Gravity();
    }

}
