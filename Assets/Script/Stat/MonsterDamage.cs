using UnityEngine;

// 몬스터 피격 반응 — 상·하체 분리가 없어 경직이 전신을 멈춘다
public class MonsterDamage : EntityDamage
{
    protected override void OnHit(IDamagable.DamageInfo damageInfo)
    {
        LookAtAttacker(damageInfo.Attacker);
    }

    // 공격자 쪽으로 yaw만 즉시 스냅한다
    // 타격 지점이 아니라 공격자를 기준으로 잡는 이유 — 발밑 장판은 타격 지점이 몸 안이라 방향이 사라진다
    private void LookAtAttacker(Transform attacker)
    {
        Vector3 vFlat = attacker.position - transform.position;
        vFlat.y = 0f;

        if(vFlat.sqrMagnitude < 0.0001f) return;

        transform.rotation = Quaternion.LookRotation(vFlat);
    }
}
