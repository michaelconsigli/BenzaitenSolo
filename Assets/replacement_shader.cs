using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteInEditMode]
public class Replacement_shader : MonoBehaviour
{
    public Shader ReplacementShader;
    void OnEnable()
    {
        if (ReplacementShader != null)
            GetComponent<Camera>().SetReplacementShader(ReplacementShader, "RenderType");
    }
    void OnDisable()
    {
        GetComponent<Camera>().ResetReplacementShader();
    }
}