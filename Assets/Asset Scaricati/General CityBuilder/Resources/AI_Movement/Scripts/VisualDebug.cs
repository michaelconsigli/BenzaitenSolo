using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualDebug : MonoBehaviour
{
    public static bool _debug = false;
    public static bool nodos = false;
    public static bool agentesVirtuales = false;

    [SerializeField] public bool ActivarDebug = false;
    [SerializeField] public bool ActivarAgentesVirtualesEnInicio = false;
    [SerializeField] public bool ActivarNodosEnInicio = false;

    public void Start()
    {
        if(ActivarAgentesVirtualesEnInicio) activaragentesVirtuales();
        if(ActivarDebug) activarDebug();
    }

    public void Update(){
        if(ActivarDebug && !_debug){
             activarDebug();
        }else if(!ActivarDebug && _debug){
             activarDebug();
        }
    }

    public void activarDebug()
    {
        _debug = !_debug;
    }

    public void activaragentesVirtuales()
    {
        agentesVirtuales = !agentesVirtuales;
        foreach(GameObject n in GameObject.FindGameObjectsWithTag("AgenteVirtual"))
        {
            n.GetComponent<MeshRenderer>().enabled = agentesVirtuales;
        }
    }

    public void reiniciarDebugs()
    {
        _debug = false;
        nodos = false;
        agentesVirtuales = false;
    }
}
