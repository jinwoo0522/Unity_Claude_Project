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

        GameObject iceExplosion = Resources.Load<GameObject>("Prefabs/Skill/Ice_Explosion");
        if (skill == null)
        {
            GameManager.Instance.DebugMessage<PoolResourceLoader>("Ice_Explosion 스킬 NULL");
            return;
        }
        prefabs.Add(iceExplosion);

        GameObject Storm = Resources.Load<GameObject>("Prefabs/Skill/Storm");
        if (skill == null)
        {
            GameManager.Instance.DebugMessage<PoolResourceLoader>("Storm 스킬 NULL");
            return;
        }
        prefabs.Add(Storm);

        GameObject Fire_Explosion = Resources.Load<GameObject>("Prefabs/Skill/Fire_Explosion");
        if (skill == null)
        {
            GameManager.Instance.DebugMessage<PoolResourceLoader>("Fire_Explosion 스킬 NULL");
            return;
        }
        prefabs.Add(Fire_Explosion);

        GameObject Fire_Buff = Resources.Load<GameObject>("Prefabs/Skill/Fire_Buff");
        if (skill == null)
        {
            GameManager.Instance.DebugMessage<PoolResourceLoader>("Fire_Buff 스킬 NULL");
            return;
        }
        prefabs.Add(Fire_Buff);
    }

    // 로컬(이펙트) 프리팹 로드 — enum 순서와 로드 순서를 일치시킨다
    public void LoadLocalPrefabs(List<GameObject> prefabs)
    {
        //0
        GameObject electricHit = Resources.Load<GameObject>("Prefabs/Effect/Magician/Electronic/ElectronicHitEffect");
        if (electricHit == null)
        {
            GameManager.Instance.DebugMessage<PoolResourceLoader>("ElectricHit 이펙트 NULL");
            return;
        }
        prefabs.Add(electricHit);

        //1
        GameObject electricPJ= Resources.Load<GameObject>("Prefabs/Effect/Magician/Electronic/ElectronicProjectileEffect");
        if (electricPJ == null)
        {
            GameManager.Instance.DebugMessage<PoolResourceLoader>("ElectronicProjectileEffect 이펙트 NULL");
            return;
        }
        prefabs.Add(electricPJ);

        //2
        GameObject iceExplosion= Resources.Load<GameObject>("Prefabs/Effect/Magician/IceExplosion/IceExplosionEffect");
        if (iceExplosion == null)
        {
            GameManager.Instance.DebugMessage<PoolResourceLoader>("iceExplosion 이펙트 NULL");
            return;
        }
        prefabs.Add(iceExplosion);

        
        //3
        GameObject Storm_Effect = Resources.Load<GameObject>("Prefabs/Effect/Magician/Storm/Storm");
        if (Storm_Effect == null)
        {
            GameManager.Instance.DebugMessage<PoolResourceLoader>("Storm_Effect 이펙트 NULL");
            return;
        }
        prefabs.Add(Storm_Effect);

        //4
        GameObject Fire_Explosion_Effect = Resources.Load<GameObject>("Prefabs/Effect/Golem/Fire_Explosion");
        if (Fire_Explosion_Effect == null)
        {
            GameManager.Instance.DebugMessage<PoolResourceLoader>("Fire_Explosion 이펙트 NULL");
            return;
        }
        prefabs.Add(Fire_Explosion_Effect);

        GameObject Fire_Buff_Effect = Resources.Load<GameObject>("Prefabs/Effect/Golem/Fire_Buff");
        if (Fire_Buff_Effect == null)
        {
            GameManager.Instance.DebugMessage<PoolResourceLoader>("Fire_Explosion 이펙트 NULL");
            return;
        }
        prefabs.Add(Fire_Buff_Effect);

        GameObject Motion_Trail_Object = Resources.Load<GameObject>("Prefabs/MotionTrailObject/MotionTrailObject");
        if (Motion_Trail_Object == null)
        {
            GameManager.Instance.DebugMessage<PoolResourceLoader>("Motion_Trail_Object 이펙트 NULL");
            return;
        }
        prefabs.Add(Motion_Trail_Object);

        GameObject FrozenSmoke = Resources.Load<GameObject>("Prefabs/Effect/FrozenSmoke");
        if (FrozenSmoke == null)
        {
            GameManager.Instance.DebugMessage<PoolResourceLoader>("FrozenSmoke 이펙트 NULL");
            return;
        }
        prefabs.Add(FrozenSmoke);

        GameObject Hit_stone = Resources.Load<GameObject>("Prefabs/Effect/Goblin/Hit_stone");
        if (Hit_stone == null)
        {
            GameManager.Instance.DebugMessage<PoolResourceLoader>("Hit_stone 이펙트 NULL");
            return;
        }
        prefabs.Add(Hit_stone);
    }
}
