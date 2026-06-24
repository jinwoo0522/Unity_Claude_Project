using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class EntityAnimator
{

    public Animator _animator {get; private set;}
    public NetworkAnimator _networkAnimator {get; private set;}
    public NetworkVariable<ushort> _state => animState;

    private Dictionary<ushort, string> anims = new();
    
    

    NetworkVariable<ushort> animState;
    
    private bool isActvieLayer;
    int iLayerNumber = 0;
    float fDuration = 0.15f;
    float fLayerWeight = 0f;
    float fWeightLerpSpeed = 1f;

    public EntityAnimator(Animator animator , NetworkAnimator networkAnimator, 
    AnimData animData, NetworkVariable<ushort> State)
    {
        _animator = animator;
        _networkAnimator = networkAnimator;
        
        iLayerNumber = animData.iLayerNumber;
        fDuration = animData.fDuration;
        fWeightLerpSpeed = animData.fWeightLerpSpeed;

        animState = State;
        animState.OnValueChanged += (cur , next) => {
            if(ActiveLayer(next) == false)
                return;
             _animator.CrossFade(anims[next], fDuration, iLayerNumber);
        };

        LinkAnim(animData);
    }
    
    void LinkAnim(AnimData animData)
    {
        foreach (var entry in animData.animEntries)
        {
            if (!anims.TryAdd(entry.key, entry.clipName))
            {
                Debug.LogWarning($"중복된 key: {entry.key}");
            }
        }
    }

    bool ActiveLayer(ushort state)
    {
         if(state != 0) // NONE이면 레이어를 비활성화시킴
            return isActvieLayer = true;
         else
            return isActvieLayer = false;
    }

    void LerpLayerWeight(float fTimeDelta)
    {
        int iTargetWeight = isActvieLayer ? 1 : 0;
         _animator.SetLayerWeight(iLayerNumber, fLayerWeight = Mathf.MoveTowards(fLayerWeight , iTargetWeight , fTimeDelta * fWeightLerpSpeed) );

        if(fLayerWeight >= 1)
        {
            isActvieLayer = false; 
        }
    }

    // 현재 레이어의 클립이 끝까지 재생됐는지 (서버에서 Animator 시간 기준으로 판정)
    public bool IsCurrentStateFinished()
    {
        var info = _animator.GetCurrentAnimatorStateInfo(iLayerNumber);
        return info.IsName(anims[animState.Value])              // 현재 상태 클립이 실제 재생 중이고
            && _animator.IsInTransition(iLayerNumber) == false  // 크로스페이드가 끝났고
            && info.normalizedTime >= 1f;                       // 끝까지 재생됨
    }

    public void AnimUpdate(float fTimeDelta)
    {
        LerpLayerWeight(fTimeDelta);
    }
}
