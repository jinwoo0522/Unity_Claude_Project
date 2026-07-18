using UnityEngine;
using Unity.Netcode;
using UnityEngine.Pool;
public class PooledHandler : INetworkPrefabInstanceHandler
{

    ObjectPool<ISkill> SkillPool;
    private ISkill SkillPrefeb;

    public PooledHandler(ISkill _SkillPrefeb , int min_Size , int max_Size)
    {
        Debug.Log("핸들러 생성자 들어옴!");
        SkillPrefeb = _SkillPrefeb;
        SkillPool = new ObjectPool<ISkill>(
            CreateNetworkObject,
            Active,
            Release,
            DestoryObject,
            false,
            min_Size,
            max_Size);
    }

    private ISkill CreateNetworkObject() // 생성
    {
        ISkill skill = (ISkill)GameObject.Instantiate((Component)SkillPrefeb);
        skill.Handler = this;             // 스킬이 자기 핸들러를 들고 스스로 반납할 수 있도록
        return skill;
    }
    private void DestoryObject(ISkill obj) => obj.Destroy();
    private void Active(ISkill obj) => obj.Active();
    private void Release(ISkill obj) => obj.Release();

    // 스킬 본인이 스스로를 풀에 반납할 때 호출
    public void Return(ISkill skill) => SkillPool.Release(skill);

    public void Destroy(NetworkObject networkObject)
    {
        SkillPool.Release(networkObject.gameObject.GetComponent<ISkill>());
    }
    public NetworkObject Instantiate(ulong ownerClientId, Vector3 position, Quaternion rotation)
    {
        ISkill obj = SkillPool.Get();
        Component comp = (Component)obj;
        comp.transform.SetPositionAndRotation(position, rotation);

        NetworkObject nbj = comp.GetComponent<NetworkObject>();
        if(nbj == null)
            GameManager.Instance.DebugMessage<PooledHandler>("네트워크 오브젝트 없음");
        return nbj;
    }

    public void Prewarm(int count)
    {
        var temp = new ISkill[count];
        for (int i = 0; i < count; i++)
            temp[i] = SkillPool.Get();        // 생성 + 활성화
        for (int i = 0; i < count; i++)
            SkillPool.Release(temp[i]);       // 비활성화하고 풀에 반납
    }
}
