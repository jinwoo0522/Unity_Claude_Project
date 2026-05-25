using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class Magician_UpperBody : Player_UpperBody
{
    [Header("스킬")]
    [SerializeField]
    private SkillType currentSkill;
    [SerializeField]
    private Transform rightHandBone;

    public override void OnFireSkill()
    {
        if(IsOwner == true)
            NormalAttack_ServerRpc();
    }
    

    [ServerRpc]
    public override void NormalAttack_ServerRpc()
    {
        if (GameManager.Instance.skillPool == null)
        {
            GameManager.Instance.DebugMessage<Magician_UpperBody>("스킬 풀 NULL");
            return;
        } 
        Vector3 pos = rightHandBone != null ? rightHandBone.position : transform.position;
        GameManager.Instance.skillPool.
        UseSkill(currentSkill, pos, transform.forward, GetComponent<NetworkObject>().OwnerClientId);
    }
}
