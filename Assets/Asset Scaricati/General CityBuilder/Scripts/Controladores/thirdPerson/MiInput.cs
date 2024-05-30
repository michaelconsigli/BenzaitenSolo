using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MiInput
{
    public string ejeHorizontal = "Horizontal";
    public string ejeVertical = "Vertical";
    public KeyCode jump=KeyCode.Space;

    [HideInInspector]
    public float hor, ver;
    public bool saltar;
    public void GetInput()
    {
        hor = Input.GetAxis(ejeHorizontal);
        ver = Input.GetAxis(ejeVertical);
        saltar =Input.GetKey(jump);
    }
}
