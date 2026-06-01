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
            NormalAttack_ServerRpc(
                rightHandBone != null ? rightHandBone.position : transform.position , 
                GameManager.Instance.cameraManager.Get_Camera(CameraManager.CameraTag.MAIN).transform.forward);
    }
    

    [ServerRpc]
    public override void NormalAttack_ServerRpc(Vector3 pos = default, Vector3 dir = default)
    {
        if (GameManager.Instance.skillPool == null)
        {
            GameManager.Instance.DebugMessage<Magician_UpperBody>("스킬 풀 NULL");
            return;
        } 

        GameManager.Instance.skillPool.
        UseSkill(currentSkill, pos, dir, GetComponent<NetworkObject>().OwnerClientId);
    }
}
