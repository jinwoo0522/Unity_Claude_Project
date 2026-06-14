// Magician 전용 스킬.
// 스테이트명·쿨타임은 Inspector의 qSkillData·mouseSkillData 에셋으로 주입.
// 추가 동작 없이 Player_Skill 베이스 로직만 사용.
using UnityEngine;

public class Magician_Skill : Player_Skill
{
    protected override void OnBuff()
    {
        if (!IsOwner) return;
        if(_stat._isDead == true) return;

        if (CheckCanUseSkill(SkillType.IceExplosion, 0) == null)
            return;

        Animation_Play_ServerRpc(SkillType.IceExplosion , 0);
        UseSkill_ServerRpc(SkillType.IceExplosion);
        SetCooldownLength(SkillType.IceExplosion, 0);
    }



    protected override void OnAttack_Skill()
    {
        if (!IsOwner) return;
        if(_stat._isDead == true) return;
        
        if (CheckCanUseSkill(SkillType.Earthquake, 1) == null)
            return;

        Debug.Log($"지진 스킬 사용");

        Animation_Play_ServerRpc(SkillType.Earthquake , 1);
        UseSkill_ServerRpc(SkillType.Earthquake);
        SetCooldownLength(SkillType.Earthquake, 1);
    }


}
