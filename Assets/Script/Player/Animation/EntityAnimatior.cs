using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class EntityAnimator
{
    int _iLayerNumber = 0;
    float _fDuration = 0.15f;
    public Animator _animator {get; private set;}
    public NetworkAnimator _networkAnimator {get; private set;}
    
    
    private Dictionary<ushort, string> anims = new();
    
    public NetworkVariable<ushort> _state => animState;
    NetworkVariable<ushort> animState;

    public EntityAnimator(Animator animator , NetworkAnimator networkAnimator, 
    AnimData animData, NetworkVariable<ushort> State)
    {
        _animator = animator;
        _networkAnimator = networkAnimator;
        _iLayerNumber = animData.iLayerNumber;
        _fDuration = animData.fDuration;
        
        animState = State;
        animState.OnValueChanged += (cur , next) => {
             _animator.CrossFade(anims[next], _fDuration, _iLayerNumber);
             Debug.Log("크로스 페이드! " + anims[next]);
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
}
