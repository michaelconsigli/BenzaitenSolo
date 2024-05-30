using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingLot
{
    private List<Nodo> nodosContorno;
    private GameObject lotContainer;

    public BuildingLot(List<Nodo> nodos, GameObject container){
        nodosContorno = nodos;
        lotContainer = container;
    }

    public List<Nodo> getNodosContorno(){
        return nodosContorno;
    }

    public void Draw(int id, GameObject container){
        GameObject nodos = new GameObject("Building Lot " + id);
        nodos.transform.parent = container.transform;

        int numNodo = 0;

        foreach (Nodo nodo in nodosContorno)
        {
            GameObject instanciaNodo = new GameObject();
            instanciaNodo.transform.position = nodo.getPosicion();
            instanciaNodo.transform.parent = nodos.transform;
            instanciaNodo.name = "Nodo " + numNodo;
            numNodo++;

            GameObject conexiones = new GameObject("Conexiones");
            conexiones.transform.parent = instanciaNodo.transform;

            int numConexion = 0;

            foreach (Nodo conexion in nodo.getConexiones()){
                if(!nodo.isVisitadaConexion(conexion)){
                    numConexion++;
                    GameObject line = new GameObject("Linea Calle " + numConexion);
                    line.transform.parent = conexiones.transform;
                    line.transform.position = nodo.getPosicion();

                    var lineRenderer = line.AddComponent<LineRenderer>();
                    lineRenderer.material = (Material)Resources.Load("Materials/Azul", typeof(Material));;
                    lineRenderer.startWidth = 0.1f;
                    lineRenderer.SetPosition(0, nodo.getPosicion());
                    lineRenderer.endWidth = 0.1f;
                    lineRenderer.SetPosition(1, conexion.getPosicion());

                    nodo.setConexionVisitada(conexion);
                }
            }

            nodo.resetVisitados();
        }
    }
}
