using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFindingVillager : AgentNPC
{

    public Animator animator;
    public GameObject grafoCiudad;
    public int maxNodosRecorridos = 2;
    public float maxWaitTime = 10f;

    private int numNodosRecorridos = 0;
    private float timer = 0f;
    private PathFollowing pathFollowingSteering;
    private Path bestPath;

    public override void Start()
    {
        int index = (int)Random.Range(0f, grafoCiudad.transform.childCount);

        NodoObj possibleNode = grafoCiudad.transform.GetChild(index).GetComponent<NodoObj>();

        if(possibleNode.pos == nodoActual.pos){
            if(index < (grafoCiudad.transform.childCount-1)){
                index++;
            }else{
                index--;
            }
        }

        nodoObjetivo = grafoCiudad.transform.GetChild(index).GetComponent<NodoObj>();
        numNodosRecorridos = 0;
        timer = 0f;

        List<SteeringBehaviour> steerings = new List<SteeringBehaviour>();

        WallAvoidance wallSteering = gameObject.AddComponent<WallAvoidance>() as WallAvoidance;
        steerings.Add(wallSteering);
        CollisionAvoidance collisionSteering = gameObject.AddComponent<CollisionAvoidance>() as CollisionAvoidance;
        steerings.Add(collisionSteering);
        bestPath = gameObject.AddComponent<Path>() as Path;
        findBestPath();
        pathFollowingSteering = gameObject.AddComponent<PathFollowing>() as PathFollowing;
        pathFollowingSteering.camino = bestPath;
        pathFollowingSteering.targetActual = 0;
        pathFollowingSteering.vigilancia = false;
        steerings.Add(pathFollowingSteering);
        LookWhereYouGoing lookSteering = gameObject.AddComponent<LookWhereYouGoing>() as LookWhereYouGoing;
        steerings.Add(lookSteering);

        List<float> pesosSteerings = new List<float>();
        pesosSteerings.Add(80f);
        pesosSteerings.Add(4f);
        pesosSteerings.Add(5f);
        pesosSteerings.Add(5f);

        arbitro.iniciarArbitroPonderado(steerings, pesosSteerings);

        // Parametrización del agente
        MaxSpeed = 0.75f;
        MaxSpeedDefault = 0.75f;
        MaxRotation = 270f;
        MaxAcceleration = 2.5f;
        MaxAccelerationDefault = 2.5f;
        MaxAngularAcc = 360f;
        anchura = 0.15f;
        interiorRadius = 0.75f;
        factorDesaceleracion = 0.8f;

        base.Start();
    }

    public override void Update()
    {
        if(timer == 0f){
            Vector3Module vectorModule = Vector3Module.GetInstance();

            if(vectorModule.calculateDistance(Position, nodoObjetivo.pos) < interiorRadius){
                numNodosRecorridos++;

                if(numNodosRecorridos >= maxNodosRecorridos){
                    numNodosRecorridos = -1;
                    timer += Time.deltaTime;
                }else{
                    nodoActual = nodoObjetivo;

                    int index = (int)Random.Range(0f, grafoCiudad.transform.childCount);

                    NodoObj possibleNode = grafoCiudad.transform.GetChild(index).GetComponent<NodoObj>();

                    if(possibleNode.pos == nodoActual.pos){
                        if(index < (grafoCiudad.transform.childCount-1)){
                            index++;
                        }else{
                            index--;
                        }
                    }

                    nodoObjetivo = grafoCiudad.transform.GetChild(index).GetComponent<NodoObj>();
                    findBestPath();
                    pathFollowingSteering.camino = bestPath;
                    pathFollowingSteering.targetActual = 0;
                }
            }
        }else if(timer >= maxWaitTime){
            timer = 0f;
        }else{
            timer += Time.deltaTime;
        }

        // En cada frame se actualiza el movimiento
        ApplySteering(Time.deltaTime);

        if(Speed > 0f){
            animator.SetBool("Walk", true);
        }else{
            animator.SetBool("Walk", false);
        }
    }

    public struct NodeRecord{
        public NodoObj nodo;
        public NodoObj mejorConexion;
        public float costeAcumulado;

        public NodeRecord(NodoObj n, float cA, NodoObj mC=null){
            nodo = n;
            mejorConexion = mC;
            costeAcumulado = cA;
        }
    }

    private void findBestPath(){ // utiliza algoritmo de coste uniforme mediante g() == distancia camino
        List<NodeRecord> openNodes = new List<NodeRecord>();
        List<NodeRecord> closedNodes = new List<NodeRecord>();

        NodeRecord startNodeRecord = new NodeRecord(nodoActual, 0f);
        openNodes.Add(startNodeRecord);

        NodeRecord finalNodeRecord = new NodeRecord();

        bool firstIter = true;

        while(openNodes.Count > 0){
            NodeRecord currentNode = new NodeRecord();
            int indexCurrentNode = -1;

            float minimumCost = Mathf.Infinity;

            if(firstIter){
                currentNode = openNodes[0];
                indexCurrentNode = 0;

                firstIter = false;
            }else{
                for(int i = 0 ; i < openNodes.Count ; i++){
                    if(openNodes[i].costeAcumulado < minimumCost){
                        currentNode = openNodes[i];
                        minimumCost = openNodes[i].costeAcumulado;
                        indexCurrentNode = i;
                    }
                }
            }

            if(currentNode.nodo == nodoObjetivo){
                finalNodeRecord = currentNode;
                break;
            }
            
            foreach(NodoObj currentConex in currentNode.nodo.conex){
                NodeRecord endNodeRecord = new NodeRecord();
                float endNodeCost = currentNode.costeAcumulado + Vector3.Distance(currentNode.nodo.pos, currentConex.pos);

                bool isInClosed = false;

                foreach(NodeRecord nr in closedNodes){
                    if(nr.nodo == currentConex){
                        isInClosed = true;
                        break;
                    }
                }

                bool isInOpen = false;

                foreach(NodeRecord nr in openNodes){
                    if(nr.nodo == currentConex){
                        isInOpen = true;
                        endNodeRecord = nr;
                        break;
                    }
                }

                if(isInClosed){
                    continue;
                }else if(isInOpen){
                    if(endNodeRecord.costeAcumulado > endNodeCost){
                        endNodeRecord.costeAcumulado = endNodeCost;
                        endNodeRecord.mejorConexion = currentNode.nodo;
                    }
                }else{
                    endNodeRecord = new NodeRecord(currentConex, endNodeCost, currentNode.nodo);
                    openNodes.Add(endNodeRecord);
                }
            }

            openNodes.RemoveAt(indexCurrentNode);
            closedNodes.Add(currentNode);
        }

        List<Vector3> camino = new List<Vector3>();

        camino.Add(finalNodeRecord.nodo.pos);

        NodeRecord reversePathNode = new NodeRecord();

        foreach(NodeRecord nr in closedNodes){
            if(nr.nodo == finalNodeRecord.mejorConexion){
                reversePathNode = nr;
                break;
            }
        }

        while(reversePathNode.nodo != nodoActual){
            camino.Add(reversePathNode.nodo.pos);

            foreach(NodeRecord nr in closedNodes){
                if(nr.nodo == reversePathNode.mejorConexion){
                    reversePathNode = nr;
                    break;
                }
            }
        }

        camino.Add(reversePathNode.nodo.pos);

        camino.Reverse();

        bestPath.nodos = camino;
    }
}
