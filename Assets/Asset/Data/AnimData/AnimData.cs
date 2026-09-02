using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AnimData", menuName = "Scriptable Objects/AnimData")]


public class AnimData : ScriptableObject
{
    [Serializable]
    public struct AnimEntry
    {
        public ushort key;
        public string clipName;
    }

    [Header("레이어 번호")]
    public int iLayerNumber;

    [Header("애니메이션 보간 시간")]
    public float fDuration = 0.15f;

    [Header("레이어 보간 시간")]
    public float fWeightLerpSpeed = 1f;

    [Header("애니메이션 이름")]
    public List<AnimEntry> animEntries = new();
    
}
