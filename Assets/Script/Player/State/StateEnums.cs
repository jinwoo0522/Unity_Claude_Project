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
        HIT = 1 << 4,
        DIE = 1 << 5,
        RUN = 1 << 6,
        LAND = 1 << 7,
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
