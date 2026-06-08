// Magician 전용 스킬.
// 스테이트명·쿨타임은 Inspector의 qSkillData·mouseSkillData 에셋으로 주입.
// 추가 동작 없이 Player_Skill 베이스 로직만 사용.
public class Magician_Skill : Player_Skill
{
    protected override void OnBuff()
    {
        if (!IsOwner) return;

        if (CheckCanUseSkill(SkillType.IceExplosion, 0) == null)
            return;

        UseSkill_ServerRpc(SkillType.IceExplosion);
        Animation_Play_ServerRpc(SkillType.IceExplosion , 0);
        SetCooldownLength(SkillType.IceExplosion, 0);
    }

    protected override void OnAttack_Skill()
    {
        if (!IsOwner) return;
    
        SetCooldownLength(SkillType.IceExplosion, 1);
    }
}
