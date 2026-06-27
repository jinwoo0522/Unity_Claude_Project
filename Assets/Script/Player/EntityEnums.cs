using System;
using UnityEngine;

namespace ENTITY
{
    [Flags]
    public enum StateType : ushort
    {
        NONE,
        IDLE = 1 << 0,
        WALK = 1 << 1,
        JUMP = 1 << 2,
        ATTACK = 1 << 3 ,
        DIE = 1 << 5,
        RUN = 1 << 6,
        LAND = 1 << 7,
        AIRBORNE = 1 << 8,
        FREZEE = 1 << 9,
    }

    public enum UpperStateType : ushort
    {
        NONE,
        IDLE = 1 << 0,
        HIT = 1 << 1
    }

    [Flags]
    public enum InputFlagType : ushort
    {
        NONE = 0,
        JUMP = 1 << 0,
        Q =  1 << 1,
        MOUSE_RIGHT = 1 << 2,
        MOUSE_LEFT = 1 << 3,
        SPRINT = 1 << 4,
        MOVE = 1 << 5,
    }
}

namespace ELF
{
    public enum StateType : ushort
    {
        MOUSE_SKILL = 1 << 10,
        Q_SKILL = 1 << 11,
    }
    public enum UpperStateType : ushort
    {
        NONE = 0,
        IDLE = 1 << 0,
        HIT = 1 << 1,
        ATTACK_START = 1 << 2,
        ATTACK_MIDDLE = 1 << 3,
        ATTACK_LAST = 1 << 4,
    }

    public enum ElfEffect
    {
        QSKILL,
        MOUSE_SKILL,
        WEAPON_PARTICLE,
        HIT_EFFECT,
    }
    public enum ElfTrail
    {
        WEAPON_TRAIL,
    }

            
}
