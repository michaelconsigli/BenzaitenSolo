using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Path : MonoBehaviour
{
    [SerializeField]
    public List<Vector3> nodos;
    private int pathDir = 1;

    public float radio = 0.15f;
    
    public int getPrimerTarget()
    {
        if (nodos.Count > 0) return 0;
        else return -1;
    }

    public int getParam(Vector3 posicion, int nodo)
    {
        if ((posicion - nodos[nodo]).magnitude < radio && (nodo < nodos.Count-1)) return nodo+pathDir;
        else return nodo;
    }

    public int getParamVigilancia(Vector3 posicion, int nodo)
    {
        if ((posicion - nodos[nodo]).magnitude < radio)
        {
            if(nodo + pathDir < nodos.Count) return nodo+pathDir;
            else
            {
                return 0;
            }
        } 
        else return nodo; 
    }

    public Vector3 getPosition(int nodo)
    {
        return nodos[nodo];
    }

    public float getRadioActual(int nodo)
    {
        return radio;
    }

    void OnDrawGizmos()
    {
        if(VisualDebug._debug && VisualDebug.nodos)
        {
            Gizmos.color = Color.red;
            for(int i = 0; i < nodos.Count - 1; i++)
            {
                Gizmos.DrawLine(nodos[i], nodos[i+1]);
            }
            Gizmos.color = new Color (1,0.5f,0,0.3f);
            for(int i = 0; i < nodos.Count - 2; i++)
            {
                Gizmos.DrawLine(nodos[i], nodos[i+1]);
            }
        }
    }
}