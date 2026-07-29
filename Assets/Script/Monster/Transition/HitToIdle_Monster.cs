using UnityEngine;

// 피격 모션이 끝나거나, 경직 중 다시 맞으면 대기로 복귀
public class HitToIdle_Monster : ITransition
{
    public ushort NextState => (ushort)ENTITY.StateType.IDLE;

    private EntityAnimator _aniController;
    private IDamagable _damagable;

    private float _fReHitTime;
    private float _fAccTime;

    public HitToIdle_Monster(EntityAnimator aniController, IDamagable damagable, float fReHitTime)
    {
        _aniController = aniController;
        _damagable = damagable;
        _fReHitTime = fReHitTime;
    }

    public bool CheckRule(float fTimeDelta)
    {
        _fAccTime += fTimeDelta;

        // 재피격 — 피격 플래그가 살아 있으므로 IDLE에서 곧바로 HIT으로 되돌아온다
        // 상태 값이 실제로 바뀌어야 CrossFade가 다시 걸려 클립이 처음부터 재생된다
        if(_fAccTime >= _fReHitTime && _damagable._isHit == true)
            return true;

        return _aniController.IsCurrentStateFinished();
    }

    public void OnTransition()
    {
        _fAccTime = 0f;
    }
}
