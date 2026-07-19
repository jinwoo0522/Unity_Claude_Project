using System.Collections.Generic;
using UnityEngine;

// 풀 대상 프리팹의 리소스 로드 전담 — ObjectPoolManager가 소유하고 실행한다.
// 컨테이너는 참조로 넘겨받아 채운다(ref 불필요: 재할당이 아니라 Add만 하므로).
public class PoolResourceLoader
{
    // 네트워크(스킬) 프리팹 로드 — enum 순서와 로드 순서를 일치시킨다
    public void LoadNetworkPrefabs(List<GameObject> prefabs)
    {
        //0
        GameObject skill = Resources.Load<GameObject>("Prefabs/Skill/ElectricSkill");
        if (skill == null)
        {
            GameManager.Instance.DebugMessage<PoolResourceLoader>("ElectricSkill 스킬 NULL");
            return;
        }
        prefabs.Add(skill);
    }

    // 로컬(이펙트) 프리팹 로드 — enum 순서와 로드 순서를 일치시킨다
    public void LoadLocalPrefabs(List<GameObject> prefabs)
    {
        //0
        GameObject electricHit = Resources.Load<GameObject>("Prefabs/Effect/Magician/ElectronicHitEffect");
        if (electricHit == null)
        {
            GameManager.Instance.DebugMessage<PoolResourceLoader>("ElectricHit 이펙트 NULL");
            return;
        }
        prefabs.Add(electricHit);

        //1
        GameObject electricPJ= Resources.Load<GameObject>("Prefabs/Effect/Magician/ElectronicProjectileEffect");
        if (electricPJ == null)
        {
            GameManager.Instance.DebugMessage<PoolResourceLoader>("ElectronicProjectileEffect 이펙트 NULL");
            return;
        }
        prefabs.Add(electricPJ);
    }
}
