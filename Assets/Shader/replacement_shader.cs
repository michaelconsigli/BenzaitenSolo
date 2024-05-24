using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteInEditMode]
public class Replacement_shader : MonoBehaviour
{
    public Shader ReplacementShader;
    public new Camera camera;
    void OnEnable()
    {
        if (ReplacementShader != null)
            camera.SetReplacementShader(ReplacementShader, "RenderType");
    }
    void OnDisable()
    {
        camera.ResetReplacementShader();
    }
}