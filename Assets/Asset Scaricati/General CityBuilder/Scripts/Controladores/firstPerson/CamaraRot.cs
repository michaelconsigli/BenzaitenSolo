using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamaraRot : MonoBehaviour
{
    public void giraCamara(){
        Vector3 R = transform.localRotation.eulerAngles;
        transform.localRotation = Quaternion.Euler(R.x, 0f, 0f);
    }
}
