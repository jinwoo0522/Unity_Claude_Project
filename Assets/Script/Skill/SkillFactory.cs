using Unity.Netcode;
using UnityEngine;

public class SkillFactory
{
    // 스킬을 스폰·초기화하여 반환
    public Skill Create(NetworkObjectType type, Vector3 vPos, Vector3 vDir, ulong clientID, GameObject owner)
    {
        Skill skill = GameManager.Instance.objectPoolManager.Get<Skill>(type);

        // Init이 위치/회전을 확정 → 그 뒤 스폰해야 스폰 스냅샷에 반영돼 클라가 올바른 위치에서 인스턴스화한다.
        skill.Init(type, vPos, vDir, clientID, owner);
        skill.GetComponent<NetworkObject>().SpawnWithOwnership(clientID);

        return skill;
    }
}
