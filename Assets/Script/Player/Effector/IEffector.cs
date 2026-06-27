using UnityEngine;

public interface IEffector
{
    void PlayEffect(int iEffectNumber, Vector3 vPos = default);
    void PlayTrail(int iEffectNumber);
    void StopEffect(int iEffectNumber);
    void StopTrail(int iEffectNumber);
}
