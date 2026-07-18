using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SkillEffector : NetworkBehaviour, ISkillModule
{
    // 이펙트 발동 시점
    private enum EffectTiming { ENTER, COLLISION, TIMING }

    // 이펙트 하나의 구성 단위 — 인스펙터 리스트에서 항목별로 설정
    [System.Serializable]
    private struct EffectInfo
    {
        [SerializeField] private ParticleSystem _effect;       // 재생할 파티클
        [SerializeField] private EffectTiming   _timing;       // 발동 시점
        [SerializeField] private float          _fTriggerTime; // TIMING일 때 발동 시각(초)
        [SerializeField] private float          _fDelay;       // 발동까지 지연(초)

        public ParticleSystem Effect      => _effect;
        public EffectTiming   Timing      => _timing;
        public float          TriggerTime => _fTriggerTime;
        public float          Delay       => _fDelay;
    }

    [SerializeField] private List<EffectInfo> _effects = new List<EffectInfo>();

    private float _fElapsed;

    public void Bind(Skill skill) { }

    // 스킬 발동 — 경과 시간 초기화 및 모든 이펙트 정지 전파
    public void Enter()
    {
        _fElapsed = 0f;
    }

    // 서버 권위 — 시간 기반(ENTER/TIMING) 발동 판정 후 전 클라에 재생 전파
    public void ServerTick(float fTimeDelta)
    {
        float fPrev = _fElapsed;
        _fElapsed += fTimeDelta;

        for (int i = 0; i < _effects.Count; ++i)
        {
            EffectInfo info = _effects[i];

            // 시간 기반(ENTER/TIMING)만 처리 — COLLISION은 콜백에서
            if (info.Timing != EffectTiming.ENTER && info.Timing != EffectTiming.TIMING)
                continue;

            // ENTER는 TriggerTime이 0 → 딜레이만, TIMING은 지정 시각 + 딜레이
            float fTarget = info.TriggerTime + info.Delay;

            if (fTarget >= fPrev && fTarget < _fElapsed)
                Play_ClientRpc(i);
        }
    }

    public void ClientTick(float fTimeDelta) { }

    // 피격 시(서버 판정) — COLLISION 이펙트 재생 전파
    public void Collision(IHitter.HitInfo hitinfo)
    {
        if (!IsServer) return; // RPC는 서버에서만

        for (int i = 0; i < _effects.Count; ++i)
        {
            if (_effects[i].Timing == EffectTiming.COLLISION)
                Play_ClientRpc(i);
        }
    }

    // 스킬 종료 — 모든 이펙트 정지 전파
    public void Exit()
    {
        if (!IsServer) return; // RPC는 서버에서만
        StopAll();
    }

    // 각 클라에서 해당 파티클 재생
    [ClientRpc]
    private void Play_ClientRpc(int iIndex)
    {
        ParticleSystem ps = _effects[iIndex].Effect;
        ps.gameObject.SetActive(true);
        ps.Play();
    }

    // 각 클라에서 해당 파티클 정지
    [ClientRpc]
    private void Stop_ClientRpc(int iIndex)
    {
        _effects[iIndex].Effect.Stop();
    }

    // 모든 이펙트 정지 전파
    private void StopAll()
    {
        for (int i = 0; i < _effects.Count; ++i)
            Stop_ClientRpc(i);
    }
}
