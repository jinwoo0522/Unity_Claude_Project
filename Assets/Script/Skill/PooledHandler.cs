using UnityEngine;
using Unity.Netcode;
using UnityEngine.Pool;
public class PooledHandler : INetworkPrefabInstanceHandler
{

    ObjectPool<Skill> SkillPool;
    private Skill SkillPrefeb;

    public PooledHandler(Skill _SkillPrefeb , int min_Size , int max_Size)
    {
        Debug.Log("핸들러 생성자 들어옴!");
        SkillPrefeb = _SkillPrefeb;
        SkillPool = new ObjectPool<Skill>(
            CreateNetworkObject,
            Active,
            Release,
            DestoryObject,
            false,
            min_Size,
            max_Size);
    }
    
    private Skill CreateNetworkObject() // 생성
    {
        return GameObject.Instantiate(SkillPrefeb);
    }
    private void DestoryObject(Skill obj) => obj.Destroy();
    private void Active(Skill obj) => obj.Active();
    private void Release(Skill obj) => obj.Release();
    
    public void Destroy(NetworkObject networkObject)
    {
        SkillPool.Release(networkObject.gameObject.GetComponent<Skill>());
    }
    public NetworkObject Instantiate(ulong ownerClientId, Vector3 position, Quaternion rotation)
    {
        Skill obj = SkillPool.Get();
        obj.transform.SetPositionAndRotation(position, rotation);

        NetworkObject nbj = obj.GetComponent<NetworkObject>();
        if(nbj == null)
            GameManager.Instance.DebugMessage<PooledHandler>("네트워크 오브젝트 없음");
        return nbj;
    }

    public void Prewarm(int count)
    {
        var temp = new Skill[count];
        for (int i = 0; i < count; i++)
            temp[i] = SkillPool.Get();        // 생성 + 활성화
        for (int i = 0; i < count; i++)
            SkillPool.Release(temp[i]);       // 비활성화하고 풀에 반납
    }
}
