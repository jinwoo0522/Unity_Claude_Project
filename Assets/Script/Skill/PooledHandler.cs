using UnityEngine;
using Unity.Netcode;
using UnityEngine.Pool;
public class PooledHandler : INetworkPrefabInstanceHandler
{

    ObjectPool<GameObject> SkillPool;
    private GameObject SkillPrefeb;

    public PooledHandler(GameObject _SkillPrefeb , int min_Size , int max_Size)
    {
        Debug.Log("핸들러 생성자 들어옴!");
        SkillPrefeb = _SkillPrefeb;
        SkillPool = new ObjectPool<GameObject>(
            CreateNetworkObject,
            Active,
            Release,
            DestoryObject,
            false,
            min_Size,
            max_Size);
    }
    
    private GameObject CreateNetworkObject() // 생성
    {
        Debug.Log("스킬 프리펩 생성!");
        return GameObject.Instantiate(SkillPrefeb);
    }
    private void DestoryObject(GameObject obj) => GameObject.Destroy(obj);
    private void Active(GameObject obj) => obj.SetActive(true); 
    private void Release(GameObject obj) => obj.SetActive(false); 
    
    public void Destroy(NetworkObject networkObject)
    {
        SkillPool.Release(networkObject.gameObject);
    }
    public NetworkObject Instantiate(ulong ownerClientId, Vector3 position, Quaternion rotation)
    {
        GameObject obj = SkillPool.Get();
        obj.transform.SetPositionAndRotation(position, rotation);

        NetworkObject nbj = obj.GetComponent<NetworkObject>();
        if(nbj == null)
            GameManager.Instance.DebugMessage<PooledHandler>("네트워크 오브젝트 없음");
        return nbj;
    }

    public void Prewarm(int count)
    {
        var temp = new GameObject[count];
        for (int i = 0; i < count; i++)
            temp[i] = SkillPool.Get();        // 생성 + 활성화
        for (int i = 0; i < count; i++)
            SkillPool.Release(temp[i]);       // 비활성화하고 풀에 반납
    }
}
