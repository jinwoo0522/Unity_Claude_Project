using Unity.VisualScripting;
using UnityEngine;

public class Magician_UpperBody : Player_UpperBody
{
    [Header("스킬")]
    [SerializeField]
    private SkillType currentSkill;
    [SerializeField]
    private Transform rightHandBone;

    public override void NormalAttack()
    {
        if (SkillPool.Instance == null) return;
        Vector3 pos = rightHandBone != null ? rightHandBone.position : transform.position;
        SkillPool.Instance.Get(currentSkill, pos, transform.forward, this.GameObject());
    }
}
