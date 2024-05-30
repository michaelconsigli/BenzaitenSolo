using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class MiInput2
{   
    public string ejeHorizontal = "Mouse X";
    public string ejeVertical = "Mouse Y";
    public string ejeAvance = "Vertical";
    public KeyCode jump=KeyCode.Space;

    [HideInInspector]
    public float hor, ver, avanzar;
    [HideInInspector]
    public bool saltar;
    public void GetInput()
    {
        hor = Input.GetAxis(ejeHorizontal);
        ver = Input.GetAxis(ejeVertical);
        avanzar = Input.GetAxis(ejeAvance);
        saltar =Input.GetKey(jump);
    }
}
 
