using UnityEngine;
using System.Collections;


[System.Serializable]
public class Steering
{
    public float angular;
    public Vector3 linear;

    public Steering()
    {
        angular = 0.0f;
        linear = Vector3.zero;
    }
}
