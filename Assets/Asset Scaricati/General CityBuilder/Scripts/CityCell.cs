using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CityCell
{
    private int id;

    private List<Nodo> nodosContorno; // Lista de nodos para versiones primerizas
    private List<NodoCopia> nodosContornoReal; // Lista de nodos para versión final
    private List<BuildingLot> lots;
    private GameObject cityCellContainer;
    private Material lineMaterial;
    private GameObject cityCellObject;

    public CityCell(int ID, List<Nodo> nodos, GameObject container, Material drawMaterial){
        id = ID;
        nodosContorno = nodos;
        nodosContornoReal = new List<NodoCopia>();
        lots = new List<BuildingLot>();
        cityCellContainer = container;
        lineMaterial = drawMaterial;
        cityCellObject = null;
    }

    public int getID(){
        return id;
    }

    public List<NodoCopia> getNodosContorno(){
        return nodosContornoReal;
    }

    public void setNodosContornoReal(List<NodoCopia> nodos){
        nodosContornoReal = nodos;
    }

    public List<BuildingLot> getLots(){
        return lots;
    }

    public void Draw(){
        GameObject nodos = new GameObject("City Cell " + id);
        nodos.transform.parent = cityCellContainer.transform;
        cityCellObject = nodos;

        int numNodo = 0;

        foreach (Nodo nodo in nodosContornoReal)
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
                    lineRenderer.material = lineMaterial;
                    lineRenderer.startWidth = 0.2f;
                    lineRenderer.SetPosition(0, nodo.getPosicion());
                    lineRenderer.endWidth = 0.2f;
                    lineRenderer.SetPosition(1, conexion.getPosicion());

                    nodo.setConexionVisitada(conexion);
                }
            }

            nodo.resetVisitados();
        }
    }

    public void CreateBuildingLots(float lengthBuilding, float depthBuilding){
        lots = new List<BuildingLot>();

        Vector3Module moduloVectores = Vector3Module.GetInstance();

        NodoCopia currentNode;
        NodoCopia nextNode;

        int i = 0;

        while(i < nodosContornoReal.Count){
            // Asiganamos los nodos de interés
            if(i == (nodosContornoReal.Count - 1)){
                currentNode = nodosContornoReal[i];
                nextNode = nodosContornoReal[0];
            }else{
                currentNode = nodosContornoReal[i];
                nextNode = nodosContornoReal[i+1];
            }

            // Creamos el Area total para los Building Lots
            Nodo n1 = new Nodo(currentNode);
            Nodo n2 = new Nodo(nextNode);

            Vector3 dir1 = currentNode.getPosicion() - currentNode.getNodoOriginal().getPosicion();
            dir1 = moduloVectores.normVector(dir1) * depthBuilding;
            Vector3 dir2 = nextNode.getPosicion() - nextNode.getNodoOriginal().getPosicion();
            dir2 = moduloVectores.normVector(dir2) * depthBuilding;
            Nodo n3 = new Nodo((currentNode.getPosicion() + dir1));
            Nodo n4 = new Nodo((nextNode.getPosicion() + dir2));

            // Comprobamos si el camino actual es más grande de lo normal para dividirlo
            float distance = moduloVectores.calculateDistance(n1.getPosicion(), n2.getPosicion());
            int numLots = (int)(distance/lengthBuilding);

            if(numLots == 1){
                List<Nodo> nodosCurrentLot = new List<Nodo>();
                n1.addConexion(n2);
                n2.addConexion(n1);

                n3.addConexion(n4);
                n4.addConexion(n3);
                n3.addConexion(n1);
                n1.addConexion(n3);
                n4.addConexion(n2);
                n2.addConexion(n4);

                nodosCurrentLot.Add(n1);
                nodosCurrentLot.Add(n2);
                nodosCurrentLot.Add(n3);
                nodosCurrentLot.Add(n4);

                BuildingLot currentLot = new BuildingLot(nodosCurrentLot, cityCellObject);
                lots.Add(currentLot);
            }else{
                List<Vector3> nodosExtraAbajo = new List<Vector3>();
                List<Vector3> nodosExtraArriba = new List<Vector3>();

                float LengthAbajo = distance;
                float LengthArriba = moduloVectores.calculateDistance(n3.getPosicion(), n4.getPosicion());

                Vector3 dirAbajo = n2.getPosicion() - n1.getPosicion();
                dirAbajo = moduloVectores.normVector(dirAbajo);
                Vector3 dirArriba = n4.getPosicion() - n3.getPosicion();
                dirArriba = moduloVectores.normVector(dirArriba);

                float maxLengthBuilding = lengthBuilding * 1.9f;
                float doubleMinLength = lengthBuilding * 2f;
                float limitLength = lengthBuilding + maxLengthBuilding;

                float distanciaRecorrida = 0f;
                numLots = 0;
                
                while(distance >= doubleMinLength){
                    float stepLength = 0f;

                    if(distance < limitLength){
                        stepLength = Random.Range(lengthBuilding, (distance - lengthBuilding));
                    }else{
                        stepLength = Random.Range(lengthBuilding, maxLengthBuilding);
                    }

                    float stepLengthArriba = LengthArriba * ((stepLength + distanciaRecorrida) / LengthAbajo);

                    Vector3 extraAbajo = n1.getPosicion() + (dirAbajo * (stepLength + distanciaRecorrida));
                    Vector3 extraArriba = n3.getPosicion() + (dirArriba * stepLengthArriba);

                    nodosExtraAbajo.Add(extraAbajo);
                    nodosExtraArriba.Add(extraArriba);

                    distance = distance - stepLength;
                    distanciaRecorrida = distanciaRecorrida + stepLength;
                    numLots++;
                }

                numLots++;

                for(int k = 0 ; k < numLots ; k++){
                    Nodo corner1 = null;
                    Nodo corner2 = null;
                    Nodo corner3 = null;
                    Nodo corner4 = null;

                    if(k == 0){
                        corner1 = n1;
                        corner2 = new Nodo(nodosExtraAbajo[k]);
                        corner3 = n3;
                        corner4 = new Nodo(nodosExtraArriba[k]);
                    }else if(k == (numLots-1)){
                        corner1 = new Nodo(nodosExtraAbajo[k-1]);
                        corner2 = n2;
                        corner3 = new Nodo(nodosExtraArriba[k-1]);
                        corner4 = n4;
                    }else{
                        corner1 = new Nodo(nodosExtraAbajo[k-1]);
                        corner2 = new Nodo(nodosExtraAbajo[k]);
                        corner3 = new Nodo(nodosExtraArriba[k-1]);
                        corner4 = new Nodo(nodosExtraArriba[k]);
                    }

                    List<Nodo> nodosCurrentLotDiv = new List<Nodo>();
                    corner1.addConexion(corner2);
                    corner2.addConexion(corner1);

                    corner3.addConexion(corner4);
                    corner4.addConexion(corner3);
                    corner3.addConexion(corner1);
                    corner1.addConexion(corner3);
                    corner4.addConexion(corner2);
                    corner2.addConexion(corner4);

                    nodosCurrentLotDiv.Add(corner1);
                    nodosCurrentLotDiv.Add(corner2);
                    nodosCurrentLotDiv.Add(corner3);
                    nodosCurrentLotDiv.Add(corner4);

                    BuildingLot currentLotDiv = new BuildingLot(nodosCurrentLotDiv, cityCellObject);
                    lots.Add(currentLotDiv);
                }
            }

            i++;
        }
    }

    public void DrawBuildingLots(){
        GameObject buildingLotsContainer = new GameObject("BuildingLots");
        buildingLotsContainer.transform.parent = cityCellObject.transform;
        buildingLotsContainer.tag = "BuildingLot";
        int id = 0;

        foreach(BuildingLot b in lots){
            b.Draw(id, buildingLotsContainer);
            id++;
        }
    }
}
