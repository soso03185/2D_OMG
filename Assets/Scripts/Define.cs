using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Define
{
    public enum CreatureState
    {
        Idle,
        Moving,
        Skill,
        Dead,
    }

    public enum MoveDir
    {
        None,
        Up,
        Down,
        Left,
        Right,
    }
}