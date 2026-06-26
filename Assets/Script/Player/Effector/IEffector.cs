using UnityEngine;

public interface IEffector
{
    void PlayEffect(int iEffectNumber);
    void PlayTrail(int iEffectNumber);
    void StopEffect(int iEffectNumber);
    void StopTrail(int iEffectNumber);
}
