using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using ConectionType;
using Unity.Collections;
using System;

// ************************
// *** Modulo CityCells ***
// ************************

public class CityCellModule
{
    private CityBuilder cityBuilder;
    private Vector3Module moduloVectores;

    private List<CityCell> cityCells;
    public GameObject cityCellContainer;

    public List<CityCell> cityCellsEdificables;
    public List<CityCell> cityCellsNoEdificables;

    public CityCellModule(CityBuilder CBuilder){
        cityBuilder = CBuilder;
        moduloVectores = Vector3Module.GetInstance();
    }

    public void DetectExteriorRoads(){
        cityBuilder.acceptedNodes = cityBuilder.acceptedNodes.OrderByDescending(n => n.getPosicion().x).ThenByDescending(n => n.getPosicion().y).ToList();

        Nodo initialNode = cityBuilder.acceptedNodes[0];

        Nodo currentNode = cityBuilder.acceptedNodes[0];
        Nodo initialDownDirNode = new Nodo(currentNode.getPosicion() - Vector3.forward);

        moduloVectores.RigthAndLeftVectors(cityBuilder.acceptedNodes[0], initialDownDirNode);
        Nodo nextNode = currentNode.getConexion(moduloVectores.vectoresR[0]);
        currentNode.setTipoConexion(nextNode, TipoConexion.Externa);
        nextNode.setTipoConexion(currentNode, TipoConexion.Externa);

        currentNode = nextNode;
        Nodo previousNode = cityBuilder.acceptedNodes[0];

        while(currentNode != initialNode){
            moduloVectores.RigthAndLeftVectors(currentNode, previousNode, true);

            if(moduloVectores.vectoresI.Count > 0){
                nextNode = currentNode.getConexion(moduloVectores.vectoresI[moduloVectores.vectoresI.Count - 1]);
            }else{
                nextNode = currentNode.getConexion(moduloVectores.vectoresR[0]);
            }

            if(moduloVectores.vectoresR.Count == 0 && moduloVectores.vectoresI.Count == 0){ // Caso de error
                return;
            }

            if(currentNode.getTipoConexion(nextNode) == TipoConexion.Interna){
                currentNode.setTipoConexion(nextNode, TipoConexion.Externa);
                nextNode.setTipoConexion(currentNode, TipoConexion.Externa);
            }else{
                currentNode.setTipoConexion(nextNode, TipoConexion.Filamento);
                nextNode.setTipoConexion(currentNode, TipoConexion.Filamento);
            }

            previousNode = currentNode;
            currentNode = nextNode;
        }
    }

    private List<Nodo> createFilamentSpanningTree(Nodo initialNode, Nodo filamentConection){
        // Inicializamos SpanningTree
        List<Nodo> spanningTree = new List<Nodo>();
        spanningTree.Add(initialNode);

        // Inicializamos la pila de candidatos al SpanningTree
        Queue<Nodo> candidatesQueue = new Queue<Nodo>();
        foreach(Nodo c in initialNode.getConexiones()){
            if(c != filamentConection){
                candidatesQueue.Enqueue(c);
            }
        }

        // Vamos recorriendo el SpanningTree
        while(candidatesQueue.Count > 0){
            Nodo currentCandidate = candidatesQueue.Dequeue();

            if(!spanningTree.Contains(currentCandidate)){
                spanningTree.Add(currentCandidate);

                foreach(Nodo c in currentCandidate.getConexiones()){
                    candidatesQueue.Enqueue(c);
                }
            }
        }

        return spanningTree;
    }

    private void RemoveFromAcceptedNodes(List<Nodo> deleteNodes){
        foreach(Nodo n in deleteNodes){
            while(n.getConexiones().Count > 0){
                Nodo currentConection = n.getConexiones()[0];
                n.removeConexion(currentConection);
                currentConection.removeConexion(n);
            }

            cityBuilder.acceptedNodes.Remove(n);
        }
    }

    public void EliminateFilaments(){
        Nodo initialNode = cityBuilder.acceptedNodes[0];
        
        Nodo currentNode = cityBuilder.acceptedNodes[0];
        Nodo initialDownDirNode = new Nodo(currentNode.getPosicion() - Vector3.forward);

        moduloVectores.RigthAndLeftVectors(cityBuilder.acceptedNodes[0], initialDownDirNode);
        Nodo nextNode = currentNode.getConexion(moduloVectores.vectoresR[0]);

        currentNode = nextNode;
        Nodo previousNode = cityBuilder.acceptedNodes[0];

        bool endRemove = false;
        bool recentUpdate = false;

        while(!endRemove){
            if(recentUpdate || currentNode != initialNode){ // Es un nodo no visitado
                if(recentUpdate){
                    recentUpdate = false;
                }
                
                if(currentNode.getIndexFirstFilament() == -1){ // No hay filamento
                    moduloVectores.RigthAndLeftVectors(currentNode, previousNode, true);

                    if(moduloVectores.vectoresI.Count > 0){
                        nextNode = currentNode.getConexion(moduloVectores.vectoresI[moduloVectores.vectoresI.Count - 1]);
                    }else{
                        nextNode = currentNode.getConexion(moduloVectores.vectoresR[0]);
                    }

                    if(moduloVectores.vectoresR.Count == 0 && moduloVectores.vectoresI.Count == 0){ // Caso de error
                        return;
                    }

                    previousNode = currentNode;
                    currentNode = nextNode;
                }else{
                    Nodo otherFilamentBound = currentNode.getConexiones()[currentNode.getIndexFirstFilament()];

                    List<Nodo> spanningTree1 = createFilamentSpanningTree(currentNode, otherFilamentBound);
                    List<Nodo> spanningTree2 = createFilamentSpanningTree(otherFilamentBound, currentNode);

                    if(spanningTree1.Count > spanningTree2.Count){
                        RemoveFromAcceptedNodes(spanningTree2);
                    }else{
                        if(spanningTree1.Contains(initialNode)){
                            initialNode = otherFilamentBound;
                            recentUpdate = true;
                        }

                        RemoveFromAcceptedNodes(spanningTree1);
                        previousNode = currentNode;
                        currentNode = otherFilamentBound;
                    }
                }
            }else{
                endRemove = true;
            }
        }
    }

    public void DetectCityCells(){
        cityCellContainer = new GameObject("City Cells");
        cityCellContainer.transform.parent = cityBuilder.city.transform;

        int ID = 0; // ID de city cell

        List<NodoCopia> grafoCopia = copyGraph();

        cityCells = new List<CityCell>();
        List<Nodo> currentCityCellNodes = new List<Nodo>();
        List<Nodo> reverseCityCellNodes = new List<Nodo>();
        List<NodoCopia> currentNodosContorno = new List<NodoCopia>();

        Nodo initialNode = grafoCopia[0];

        Nodo currentNode = grafoCopia[0];
        Nodo initialDownDirNode = new Nodo(currentNode.getPosicion() - Vector3.forward);

        moduloVectores.RigthAndLeftVectors(grafoCopia[0], initialDownDirNode);
        Nodo nextNode = currentNode.getConexion(moduloVectores.vectoresR[0]);
        currentNode.setConexionVisitada(nextNode);
        Nodo copyNode = new Nodo(currentNode);
        currentCityCellNodes.Add(copyNode);
        reverseCityCellNodes.Add(currentNode);

        currentNode = nextNode;
        Nodo previousNode = grafoCopia[0];

        bool internoDetectado = false; // Indica si hemos encontrado un city cell interno (se desactiva cuando ya no hay repeticiones)
        bool recentInternalDetection = false;
        bool movDerecha = true; // Indica si el siguiente movimiento ha sido con giro hacia la derecha o izquierda

        while(grafoCopia.Count > 0){
            int createdCityCells = 0; // Solo para internos

            int errorCount = grafoCopia.Count * 2;
            int currentCount = 0;
            while(currentNode != initialNode){
                if(currentCount > errorCount){
                    throw new Exception($"Configuración generada errónea.");
                }else{
                    currentCount++;
                }

                if(reverseCityCellNodes.Contains(currentNode) && !internoDetectado){ // Se ha detectado city cell interno
                    createdCityCells++;
                    internoDetectado = true;
                    recentInternalDetection = true;
                    List<Nodo> internalCityCellNodes = new List<Nodo>();
                    List<NodoCopia> currentNodosContornoInternal = new List<NodoCopia>();

                    int indexInicial = reverseCityCellNodes.IndexOf(currentNode);

                    for(int i = indexInicial ; i < reverseCityCellNodes.Count ; i++){
                        if(i == indexInicial){
                            copyNode = new Nodo(reverseCityCellNodes[i]);
                            internalCityCellNodes.Add(copyNode);
                        }else{
                            Vector3 dirPathx = reverseCityCellNodes[i-1].getPosicion() - reverseCityCellNodes[i].getPosicion();
                            Vector3 dirPathxRef = -dirPathx;
                            Vector3 nextDirPathx = Vector3.zero;

                            if(i == (reverseCityCellNodes.Count - 1)){
                                nextDirPathx = reverseCityCellNodes[indexInicial].getPosicion() - reverseCityCellNodes[i].getPosicion();
                            }else{
                                nextDirPathx = reverseCityCellNodes[i+1].getPosicion() - reverseCityCellNodes[i].getPosicion();
                            }

                            if(!moduloVectores.oppositeDirections(dirPathx, nextDirPathx)){
                                Vector3 dirContInternalx = moduloVectores.normVector(dirPathx) +  moduloVectores.normVector(nextDirPathx); //dirPathx.normalized + nextDirPathx.normalized;
                                
                                Vector3 v = moduloVectores.getPerpendicularVector(dirPathxRef);

                                v = -v;
                    
                                float longitudCx = cityBuilder.anchuraCarretera;

                                if(Vector3.Dot(nextDirPathx, v) >= 0){
                                    dirContInternalx = -dirContInternalx;
                                }else if(Vector3.Angle(dirPathx, nextDirPathx) < 150f){
                                    float incrementFactor = 1f;

                                    if(Vector3.Angle(dirPathx, nextDirPathx) >= 80f){
                                        incrementFactor = (0.5f * 0.4f) / (180f/Vector3.Angle(dirPathx, nextDirPathx));
                                    }else if(Vector3.Angle(dirPathx, nextDirPathx) >= 35f){
                                        incrementFactor = (0.25f * 5f) / (180f/Vector3.Angle(dirPathx, nextDirPathx));
                                    }else{
                                        incrementFactor = (0.10f * 50f) / (180f/Vector3.Angle(dirPathx, nextDirPathx));
                                    }

                                    longitudCx = longitudCx + longitudCx * incrementFactor;
                                }

                                NodoCopia currentContornoInternalx = new NodoCopia(reverseCityCellNodes[i]);
                                Vector3 contPositionInternalx = reverseCityCellNodes[i].getPosicion() + (moduloVectores.normVector(dirContInternalx) * longitudCx); //dirContInternalx.normalized
                                currentContornoInternalx.setPosicion(contPositionInternalx);
                                currentContornoInternalx.resetConexiones();

                                // Enlazamos al contorno previo (si hay)
                                if(currentNodosContornoInternal.Count > 0){
                                    currentContornoInternalx.addConexion(currentNodosContornoInternal[currentNodosContornoInternal.Count - 1]);
                                    currentNodosContornoInternal[currentNodosContornoInternal.Count - 1].addConexion(currentContornoInternalx);
                                }

                                // Añadimos el nuevo nodo de contorno
                                currentNodosContornoInternal.Add(currentContornoInternalx);
                            }

                            copyNode = new Nodo(reverseCityCellNodes[i]);
                            Nodo previousInternalCopyNode = internalCityCellNodes[internalCityCellNodes.Count - 1];
                            copyNode.addConexion(previousInternalCopyNode);
                            previousInternalCopyNode.addConexion(copyNode);
                            internalCityCellNodes.Add(copyNode);

                            if(i == (indexInicial + 1)){
                                reverseCityCellNodes[i].setConexionVisitada(reverseCityCellNodes[i-1]);
                            }else if(reverseCityCellNodes[i].getConexiones().Count == 2 && reverseCityCellNodes[i].getNumVisitados() > 0){
                                reverseCityCellNodes[i].setConexionVisitada(reverseCityCellNodes[i-1]);
                            }
                        }
                    }

                    internalCityCellNodes[0].addConexion(internalCityCellNodes[internalCityCellNodes.Count-1]);
                    internalCityCellNodes[internalCityCellNodes.Count-1].addConexion(internalCityCellNodes[0]);

                    reverseCityCellNodes[indexInicial].setConexionVisitada(reverseCityCellNodes[reverseCityCellNodes.Count-1]);

                    Vector3 dirPath = reverseCityCellNodes[reverseCityCellNodes.Count-1].getPosicion() - reverseCityCellNodes[indexInicial].getPosicion();
                    Vector3 dirPathRef = -dirPath;
                    Vector3 nextDirPath = reverseCityCellNodes[indexInicial+1].getPosicion() - reverseCityCellNodes[indexInicial].getPosicion();

                    if(!moduloVectores.oppositeDirections(dirPath, nextDirPath)){
                        Vector3 dirContInternal = moduloVectores.normVector(dirPath) + moduloVectores.normVector(nextDirPath); //dirPath.normalized + nextDirPath.normalized;

                        Vector3 v = moduloVectores.getPerpendicularVector(dirPathRef);

                        v = -v;

                        float longitudCInternal = cityBuilder.anchuraCarretera;
            
                        if(Vector3.Dot(nextDirPath, v) >= 0){
                            dirContInternal = -dirContInternal;
                        }else if(Vector3.Angle(dirPath, nextDirPath) < 150f){
                            float incrementFactor = 1f;

                            if(Vector3.Angle(dirPath, nextDirPath) >= 80f){
                                incrementFactor = (0.5f * 0.4f) / (180f/Vector3.Angle(dirPath, nextDirPath));
                            }else if(Vector3.Angle(dirPath, nextDirPath) >= 35f){
                                incrementFactor = (0.25f * 5f) / (180f/Vector3.Angle(dirPath, nextDirPath));
                            }else{
                                incrementFactor = (0.10f * 50f) / (180f/Vector3.Angle(dirPath, nextDirPath));
                            }

                            longitudCInternal = longitudCInternal + longitudCInternal * incrementFactor;
                        }

                        NodoCopia currentContornoInternalInit = new NodoCopia(reverseCityCellNodes[indexInicial]);
                        Vector3 contPositionInternal = reverseCityCellNodes[indexInicial].getPosicion() + (moduloVectores.normVector(dirContInternal) * longitudCInternal); //dirContInternal.normalized
                        currentContornoInternalInit.setPosicion(contPositionInternal);
                        currentContornoInternalInit.resetConexiones();

                        // Enlazamos al contorno previo y al siguiente del inicial
                        currentContornoInternalInit.addConexion(currentNodosContornoInternal[currentNodosContornoInternal.Count - 1]);
                        currentNodosContornoInternal[currentNodosContornoInternal.Count - 1].addConexion(currentContornoInternalInit);
                        currentContornoInternalInit.addConexion(currentNodosContornoInternal[0]);
                        currentNodosContornoInternal[0].addConexion(currentContornoInternalInit);

                        // Añadimos el nuevo nodo de contorno
                        currentNodosContornoInternal.Add(currentContornoInternalInit);
                    }else{
                        Vector3 dirContInternal = reverseCityCellNodes[reverseCityCellNodes.Count-1].getPosicion() - reverseCityCellNodes[indexInicial].getPosicion();
                        dirContInternal = -dirContInternal;
                        dirContInternal = new Vector3(-dirContInternal.z, dirContInternal.y, dirContInternal.x);
                        dirContInternal = -dirContInternal;

                        NodoCopia currentContornoInternalInit = new NodoCopia(reverseCityCellNodes[indexInicial]);
                        Vector3 contPositionInternal = reverseCityCellNodes[indexInicial].getPosicion() + (dirContInternal.normalized * 0.5f);
                        currentContornoInternalInit.setPosicion(contPositionInternal);
                        currentContornoInternalInit.resetConexiones();

                        // Enlazamos al contorno previo y al siguiente del inicial
                        currentContornoInternalInit.addConexion(currentNodosContornoInternal[currentNodosContornoInternal.Count - 1]);
                        currentNodosContornoInternal[currentNodosContornoInternal.Count - 1].addConexion(currentContornoInternalInit);
                        currentContornoInternalInit.addConexion(currentNodosContornoInternal[0]);
                        currentNodosContornoInternal[0].addConexion(currentContornoInternalInit);

                        // Añadimos el nuevo nodo de contorno
                        currentNodosContornoInternal.Add(currentContornoInternalInit);
                    }

                    CityCell internalCityCell = new CityCell(ID, internalCityCellNodes, cityCellContainer, cityBuilder.lineMaterial);
                    ID++;
                    internalCityCell.setNodosContornoReal(currentNodosContornoInternal);
                    cityCells.Add(internalCityCell);
                }else if(internoDetectado){
                    internoDetectado = false;
                }

                moduloVectores.RigthAndLeftVectors(currentNode, previousNode, true);

                if(moduloVectores.vectoresR.Count > 0){
                    nextNode = currentNode.getConexion(moduloVectores.vectoresR[moduloVectores.vectoresR.Count - 1]);
                    movDerecha = true;
                }else{
                    nextNode = currentNode.getConexion(moduloVectores.vectoresI[0]);
                    movDerecha = false;
                }

                // Creamos el nodo contorno actual
                Vector3 dir1 = nextNode.getPosicion() - currentNode.getPosicion();
                Vector3 dir2 = previousNode.getPosicion() - currentNode.getPosicion();

                if(!moduloVectores.oppositeDirections(dir1,dir2)){
                    Vector3 dirCont = moduloVectores.normVector(dir1) + moduloVectores.normVector(dir2); //dir1.normalized + dir2.normalized;
                    float longitudC = cityBuilder.anchuraCarretera;
                    
                    if(!movDerecha){
                        dirCont = -dirCont;
                    }else if(Vector3.Angle(dir1, dir2) < 150f){
                        float incrementFactor = 1f;

                        if(Vector3.Angle(dir1, dir2) >= 80f){
                            incrementFactor = (0.5f * 0.4f) / (180f/Vector3.Angle(dir1, dir2));
                        }else if(Vector3.Angle(dir1, dir2) >= 35f){
                            incrementFactor = (0.25f * 5f) / (180f/Vector3.Angle(dir1, dir2));
                        }else{
                            incrementFactor = (0.10f * 50f) / (180f/Vector3.Angle(dir1, dir2));
                        }

                        longitudC = longitudC + longitudC * incrementFactor;
                    }

                    NodoCopia currentContorno = new NodoCopia(currentNode);
                    Vector3 contPosition = currentNode.getPosicion() + (moduloVectores.normVector(dirCont) * longitudC); //dirCont.normalized
                    currentContorno.setPosicion(contPosition);
                    currentContorno.resetConexiones();

                    // Enlazamos al contorno previo (si hay)
                    if(currentNodosContorno.Count > 0){
                        currentContorno.addConexion(currentNodosContorno[currentNodosContorno.Count - 1]);
                        currentNodosContorno[currentNodosContorno.Count - 1].addConexion(currentContorno);
                    }

                    // Añadimos el nuevo nodo de contorno
                    currentNodosContorno.Add(currentContorno);
                }
                
                if(internoDetectado){
                    currentNode.setConexionVisitada(previousNode);
                }else if(currentNode.getConexiones().Count == 2 && currentNode.getNumVisitados() > 0){
                    currentNode.setConexionVisitada(nextNode);
                }

                if(!internoDetectado){
                    copyNode = new Nodo(currentNode);
                    Nodo previousCopyNode = null;

                    if(!recentInternalDetection){
                        previousCopyNode = currentCityCellNodes[currentCityCellNodes.Count - 1];
                    }else{
                        bool foundPC = false;
                        int index = 0;

                        while(!foundPC){
                            if(currentCityCellNodes[index].getPosicion() == previousNode.getPosicion()){
                                previousCopyNode = currentCityCellNodes[index];
                                foundPC = true;
                            }

                            index++;
                        }

                        recentInternalDetection = false;
                    }

                    copyNode.addConexion(previousCopyNode);
                    previousCopyNode.addConexion(copyNode);
                    currentCityCellNodes.Add(copyNode);
                    reverseCityCellNodes.Add(currentNode);
                }else{
                    bool foundC = false;
                    bool foundP = false;
                    int index = 0;
                    Nodo previousCopyNode = null;

                    while(!foundC || !foundP){
                        if(currentCityCellNodes[index].getPosicion() == currentNode.getPosicion()){
                            copyNode = currentCityCellNodes[index];
                            foundC = true;
                        }else if(currentCityCellNodes[index].getPosicion() == previousNode.getPosicion()){
                            previousCopyNode = currentCityCellNodes[index];
                            foundP = true;
                        }

                        index++;
                    }

                    copyNode.addConexion(previousCopyNode);
                    previousCopyNode.addConexion(copyNode);
                }

                previousNode = currentNode;
                currentNode = nextNode;
            }

            if(previousNode != currentNode && !internoDetectado){
                Nodo initialCopyNode = currentCityCellNodes[0];
                Nodo finalCopyNode = currentCityCellNodes[currentCityCellNodes.Count - 1];
                initialCopyNode.addConexion(finalCopyNode);
                finalCopyNode.addConexion(initialCopyNode);

                if(!initialNode.isVisitadaConexion(previousNode)){ // Check eliminaciones al revés
                    bool endCheck = false;
                    reverseCityCellNodes.Reverse();
                    int i = 0;

                    while(!endCheck && i < reverseCityCellNodes.Count){
                        if(i == 0){
                            if(!reverseCityCellNodes[reverseCityCellNodes.Count - 1].isVisitadaConexion(reverseCityCellNodes[0]) && reverseCityCellNodes[reverseCityCellNodes.Count - 1].getConexiones().Count  == 2 && reverseCityCellNodes[reverseCityCellNodes.Count - 1].getNumVisitados() > 0){
                                reverseCityCellNodes[reverseCityCellNodes.Count - 1].setConexionVisitada(reverseCityCellNodes[0]);
                            }else{
                                endCheck = true;
                            }
                        }else{
                            if(!reverseCityCellNodes[i - 1].isVisitadaConexion(reverseCityCellNodes[i]) && reverseCityCellNodes[i - 1].getConexiones().Count  == 2 && reverseCityCellNodes[i - 1].getNumVisitados() > 0){
                                reverseCityCellNodes[i - 1].setConexionVisitada(reverseCityCellNodes[i]);
                            }else{
                                endCheck = true;
                            }
                        }

                        i++;
                    }

                    reverseCityCellNodes.Reverse();
                }
            }

            // Creamos el nodo contorno inicial
            Vector3 dir1Init = currentCityCellNodes[1].getPosicion() - currentNode.getPosicion();
            Vector3 dir2Init = previousNode.getPosicion() - currentNode.getPosicion();
            Vector3 dirContInit = moduloVectores.normVector(dir1Init) + moduloVectores.normVector(dir2Init); //dir1Init + dir2Init;

            if(moduloVectores.oppositeDirections(dir1Init,dir2Init)){
                dirContInit = previousNode.getPosicion() - currentNode.getPosicion();
                dirContInit = -dirContInit;
                dirContInit = new Vector3(-dirContInit.z, dirContInit.y, dirContInit.x);
                dirContInit = -dirContInit;
            }

            float longitudCInit = cityBuilder.anchuraCarretera;

            if(Vector3.Angle(dir1Init, dir2Init) < 150f){
                float incrementFactor = 1f;

                if(Vector3.Angle(dir1Init, dir2Init) >= 80f){
                    incrementFactor = (0.5f * 0.4f) / (180f/Vector3.Angle(dir1Init, dir2Init));
                }else if(Vector3.Angle(dir1Init, dir2Init) >= 35f){
                    incrementFactor = (0.25f * 5f) / (180f/Vector3.Angle(dir1Init, dir2Init));
                }else{
                    incrementFactor = (0.10f * 50f) / (180f/Vector3.Angle(dir1Init, dir2Init));
                }

                longitudCInit = longitudCInit + longitudCInit * incrementFactor;
            }

            NodoCopia currentContornoInit = new NodoCopia(currentNode);
            Vector3 contPositionInit = currentNode.getPosicion() + (moduloVectores.normVector(dirContInit) * longitudCInit); //dirContInit.normalized
            currentContornoInit.setPosicion(contPositionInit);
            currentContornoInit.resetConexiones();

            // Enlazamos al contorno previo y al siguiente del inicial
            currentContornoInit.addConexion(currentNodosContorno[currentNodosContorno.Count - 1]);
            currentNodosContorno[currentNodosContorno.Count - 1].addConexion(currentContornoInit);
            currentContornoInit.addConexion(currentNodosContorno[0]);
            currentNodosContorno[0].addConexion(currentContornoInit);

            // Añadimos el nuevo nodo de contorno
            currentNodosContorno.Add(currentContornoInit);

            List<Nodo> emptyList = new List<Nodo>();
            CityCell currentCityCell = new CityCell(ID, emptyList, cityCellContainer, cityBuilder.lineMaterial);
            ID++;
            currentCityCell.setNodosContornoReal(currentNodosContorno);
            cityCells.Add(currentCityCell);

            // Eliminamos los caminos (marcados como visitados) y nodos que no van a participar en nuevos city cells
            HashSet<Nodo> hashSet = new HashSet<Nodo>(reverseCityCellNodes);
            foreach(Nodo n in hashSet){
                if(n.getNumVisitados() == n.getConexiones().Count){
                    n.resetVisitados();
                    grafoCopia.Remove((NodoCopia)n);
                }else{
                    int i = 0;

                    while(i < n.getConexiones().Count){
                        if(n.isVisitadaConexion(n.getConexiones()[i])){
                            n.removeConexion(n.getConexiones()[i]);
                        }else{
                            i++;
                        }
                    }

                    if(n.getConexiones().Count == 1){
                        Nodo currentElimination = n;
                        Nodo auxElimination = n.getConexiones()[0];

                        n.removeConexion(auxElimination);
                        auxElimination.removeConexion(n);

                        grafoCopia.Remove((NodoCopia)n);

                        bool endEliminations = false;
                        while(!endEliminations){
                            if(auxElimination.getConexiones().Count == 1){
                                currentElimination = auxElimination;
                                auxElimination = currentElimination.getConexiones()[0];

                                currentElimination.removeConexion(auxElimination);
                                auxElimination.removeConexion(currentElimination);

                                grafoCopia.Remove((NodoCopia)currentElimination);
                            }else{
                                if(auxElimination.getConexiones().Count == 0){
                                    grafoCopia.Remove((NodoCopia)auxElimination);
                                }

                                endEliminations = true;
                            }
                        }
                    }
                }
            }

            // Eliminamos todos los posibles filamentos residuales generados tras el borrado del city cell actual
            bool erasedFilaments = true;

            if(grafoCopia.Count == 0){
                erasedFilaments = false;
            }

            int errorIndex = grafoCopia.Count;
            int currentErrorIndex = 0;

            while(erasedFilaments){
                // Comprobamos si se da caso de error por bucle infinito
                currentErrorIndex++;

                if(currentErrorIndex > errorIndex){
                    throw new Exception($"Configuración generada errónea.");
                }

                // Detectamos los filamentos posibles
                HashSet<Nodo> nodosRecorridos = new HashSet<Nodo>();

                Nodo initialNodeR = grafoCopia[0];

                Nodo currentNodeR = grafoCopia[0];
                nodosRecorridos.Add(currentNodeR);
                Nodo initialDownDirNodeR = new Nodo(currentNodeR.getPosicion() - Vector3.forward);

                moduloVectores.RigthAndLeftVectors(grafoCopia[0], initialDownDirNodeR);
                Nodo nextNodeR = currentNodeR.getConexion(moduloVectores.vectoresR[0]);
                currentNodeR.setTipoConexion(nextNodeR, TipoConexion.Externa);
                nextNodeR.setTipoConexion(currentNodeR, TipoConexion.Externa);

                currentNodeR = nextNodeR;
                Nodo previousNodeR = grafoCopia[0];

                errorCount = grafoCopia.Count * 2;
                currentCount = 0;

                while(currentNodeR != initialNodeR){
                    if(currentCount > errorCount){
                        throw new Exception($"Configuración generada errónea.");
                    }else{
                        currentCount++;
                    }

                    nodosRecorridos.Add(currentNodeR);
                    moduloVectores.RigthAndLeftVectors(currentNodeR, previousNodeR, true);

                    if(moduloVectores.vectoresR.Count == 0 && moduloVectores.vectoresI.Count == 0){ // Caso de error
                        throw new Exception($"Nodo sin conexiones.");
                    }

                    if(moduloVectores.vectoresI.Count > 0){
                        nextNodeR = currentNodeR.getConexion(moduloVectores.vectoresI[moduloVectores.vectoresI.Count - 1]);
                    }else{
                        nextNodeR = currentNodeR.getConexion(moduloVectores.vectoresR[0]);
                    }

                    if(currentNodeR.getTipoConexion(nextNodeR) == TipoConexion.Interna){
                        currentNodeR.setTipoConexion(nextNodeR, TipoConexion.Externa);
                        nextNodeR.setTipoConexion(currentNodeR, TipoConexion.Externa);
                    }else{
                        currentNodeR.setTipoConexion(nextNodeR, TipoConexion.Filamento);
                        nextNodeR.setTipoConexion(currentNodeR, TipoConexion.Filamento);
                    }

                    previousNodeR = currentNodeR;
                    currentNodeR = nextNodeR;
                }

                // Eliminamos los filamentos
                erasedFilaments = false;
                foreach(Nodo n in nodosRecorridos){
                    int j = 0;
                    while(j < n.getConexiones().Count){
                        if(n.getTipoConexion(n.getConexiones()[j]) == TipoConexion.Externa){
                            n.setTipoConexion(n.getConexiones()[j], TipoConexion.Interna);
                            j++;
                        }else if(n.getTipoConexion(n.getConexiones()[j]) == TipoConexion.Filamento){
                            n.getConexiones()[j].removeConexion(n);
                            n.removeConexion(n.getConexiones()[j]);

                            if(!erasedFilaments){
                                erasedFilaments = true;
                            }
                        }else{
                            j++;
                        }
                    }

                    if(n.getConexiones().Count == 0){
                        grafoCopia.Remove((NodoCopia)n);
                    }
                }
            }

            // Reseteamos todo lo necesario para buscar el siguiente city cell
            reverseCityCellNodes = new List<Nodo>();

            currentCityCellNodes = new List<Nodo>();

            currentNodosContorno = new List<NodoCopia>();

            if(grafoCopia.Count > 0){
                initialNode = grafoCopia[0];

                currentNode = grafoCopia[0];
                initialDownDirNode = new Nodo(currentNode.getPosicion() - Vector3.forward);

                moduloVectores.RigthAndLeftVectors(grafoCopia[0], initialDownDirNode);
                nextNode = currentNode.getConexion(moduloVectores.vectoresR[0]);
                currentNode.setConexionVisitada(nextNode);
                copyNode = new Nodo(currentNode);
                currentCityCellNodes.Add(copyNode);
                reverseCityCellNodes.Add(currentNode);

                currentNode = nextNode;
                previousNode = grafoCopia[0];
            }
        }

        DrawCityCells();
    }

    private void DrawCityCells(){
        Debug.Log("Se han detectado " + cityCells.Count + " city cells");

        foreach(CityCell c in cityCells){
            c.Draw();
        }
    }

    private List<NodoCopia> copyGraph(){
        List<NodoCopia> grafoCopia = new List<NodoCopia>();

        // Inicializamos la pila de nodos a expandir
        Queue<NodoCopia> nodesQueue = new Queue<NodoCopia>();

        NodoCopia copyNode = new NodoCopia(cityBuilder.acceptedNodes[0]);
        grafoCopia.Add(copyNode);

        foreach(Nodo c in cityBuilder.acceptedNodes[0].getConexiones()){
            NodoCopia auxCopyNode = new NodoCopia(c);
            copyNode.addConexion(auxCopyNode);
            auxCopyNode.addConexion(copyNode);
            grafoCopia.Add(auxCopyNode);

            nodesQueue.Enqueue(auxCopyNode);
        }

        // Vamos expandiendo el grafo
        while(nodesQueue.Count > 0){
            NodoCopia currentExpansionNode = nodesQueue.Dequeue();

            if(currentExpansionNode.getNodoOriginal().getConexiones().Count > currentExpansionNode.getConexiones().Count){
                foreach(Nodo c in currentExpansionNode.getNodoOriginal().getConexiones()){
                    NodoCopia detectedCopy = null;
                    if(!repeatedCopyNode(grafoCopia, c, out detectedCopy)){
                        NodoCopia auxCopyNode = new NodoCopia(c);
                        currentExpansionNode.addConexion(auxCopyNode);
                        auxCopyNode.addConexion(currentExpansionNode);
                        grafoCopia.Add(auxCopyNode);

                        nodesQueue.Enqueue(auxCopyNode);
                    }else{
                        currentExpansionNode.addConexion(detectedCopy);
                        detectedCopy.addConexion(currentExpansionNode);
                    }
                }
            }
        }

        grafoCopia = grafoCopia.OrderByDescending(n => n.getPosicion().x).ThenByDescending(n => n.getPosicion().y).ToList();

        return grafoCopia;
    }

    public bool repeatedCopyNode(List<NodoCopia> grafoCopia, Nodo conexion, out NodoCopia detectedCopy){
        foreach(NodoCopia n in grafoCopia){
            if(n.getNodoOriginal() == conexion){
                detectedCopy = n;
                return true;
            }
        }

        detectedCopy = null;
        return false;
    }

    // Generación de Building Lots

    public void GenerarBuildingLots(float houseLength, float houseWidth){
        cityCellsEdificables = new List<CityCell>();
        cityCellsNoEdificables = new List<CityCell>();

        float acceptedLength = houseWidth + cityBuilder.anchuraCarretera;
        float anchuraAceptada = houseLength + cityBuilder.anchuraCarretera * 2f;

        int num1 = 0;
        int num2 = 0;
        int num3 = 0;
        int numC = 0;

        var watch = System.Diagnostics.Stopwatch.StartNew();

        //foreach(CityCell currentCityCell in cityCells) -> Da mejores tiempos la versión secuencial vs paralelismo
        //Parallel.ForEach(cityCells, currentCityCell =>
        foreach(CityCell currentCityCell in cityCells)
        {
            List<NodoCopia> nodosContorno = currentCityCell.getNodosContorno();

            NodoCopia currentNode;
            NodoCopia previousNode;
            NodoCopia nextNode;

            int i = 0;
            bool edificable = true;

            while(edificable && i < nodosContorno.Count){
                // Asiganamos los nodos de interés
                if(i == 0){
                    currentNode = nodosContorno[i];
                    previousNode = nodosContorno[nodosContorno.Count - 1];
                    nextNode = nodosContorno[i+1];
                }else if(i == (nodosContorno.Count - 1)){
                    currentNode = nodosContorno[i];
                    previousNode = nodosContorno[i-1];
                    nextNode = nodosContorno[0];
                }else{
                    currentNode = nodosContorno[i];
                    previousNode = nodosContorno[i-1];
                    nextNode = nodosContorno[i+1];
                }

                // Comprobamos primera condición (camino suficientemente largo)
                Vector3 currentCamino = nextNode.getNodoOriginal().getPosicion() - currentNode.getNodoOriginal().getPosicion();
                if(currentCamino.magnitude < anchuraAceptada){
                    edificable = false;
                    num1++;
                }else{
                    // Comprobamos segunda condición (angulo entre caminos suficientemente grande)
                    Vector3 previousCamino = previousNode.getNodoOriginal().getPosicion() - currentNode.getNodoOriginal().getPosicion();
                    Vector3 previousCaminoReverse = currentNode.getNodoOriginal().getPosicion() - previousNode.getNodoOriginal().getPosicion();

                    bool giroDerecha = true;

                    if(!moduloVectores.isToTheRigth(previousCaminoReverse, currentCamino)){
                        giroDerecha = false;
                    }

                    if(giroDerecha && Vector3.Angle(currentCamino, previousCamino) < 50f){
                        edificable = false;
                        num2++;
                    }else{
                        // Intentamos comprobar un caso especial (City cells de 4 nodos)
                        if(nodosContorno.Count == 4){
                            Vector3 centro = (nodosContorno[0].getPosicion() + nodosContorno[1].getPosicion() + nodosContorno[2].getPosicion() + nodosContorno[3].getPosicion()) / 4;

                            int k = 0;

                            while(edificable && k < 4){
                                Vector3 P = centro;
                                Vector3 Q = nodosContorno[k].getPosicion();
                                Vector3 R = Vector3.zero;

                                if(k == 3){
                                    R = nodosContorno[0].getPosicion();
                                }else{
                                    R = nodosContorno[k+1].getPosicion();
                                }

                                Vector3 Pproy = moduloVectores.calculateProjection(P,Q,R);

                                if(moduloVectores.calculateDistance(P,Pproy) < acceptedLength*0.5f){
                                    edificable=false;
                                    numC++;
                                }

                                k++;
                            }
                        }

                        if(edificable){
                            // Comprobamos tercera condición (distancia de un nodo al resto de caminos suficientemente grande)
                            NodoCopia auxCurrentNode;
                            NodoCopia auxNextNode;

                            int j = 0;

                            while(edificable && j < nodosContorno.Count){
                                // Asiganamos los nodos de interés
                                if(j == (nodosContorno.Count - 1)){
                                    auxCurrentNode = nodosContorno[j];
                                    auxNextNode = nodosContorno[0];
                                }else{
                                    auxCurrentNode = nodosContorno[j];
                                    auxNextNode = nodosContorno[j+1];
                                }

                                // Comprobamos si es un posible camino a analizar
                                if(auxCurrentNode != currentNode && auxNextNode != currentNode){
                                    Vector3 P = currentNode.getNodoOriginal().getPosicion();
                                    Vector3 Q = auxCurrentNode.getNodoOriginal().getPosicion();
                                    Vector3 R = auxNextNode.getNodoOriginal().getPosicion();

                                    Vector3 Pproy = moduloVectores.calculateProjection(P,Q,R);

                                    // Comprobamos que la proyección está dentro del camino real
                                    if(moduloVectores.calculateDistance(Q,R)==moduloVectores.calculateDistance(Q,Pproy)+moduloVectores.calculateDistance(Pproy,R)){
                                        // Si el punto de proyección está cerca de uno de los extremos del camino o tiene accepted length, aceptado. Si no, rechaza
                                        //if(moduloVectores.calculateDistance(Q, Pproy) >= 1.25f && moduloVectores.calculateDistance(R, Pproy) >= 1.25f){
                                            if(giroDerecha){
                                                if(moduloVectores.calculateDistance(Q, Pproy) >= 1.25f && moduloVectores.calculateDistance(R, Pproy) >= 1.25f){
                                                    if(moduloVectores.calculateDistance(P,Pproy)<acceptedLength*2.3f){
                                                        edificable=false;
                                                        num3++;
                                                    }
                                                }
                                            }else{
                                                if(moduloVectores.calculateDistance(P,Pproy)<acceptedLength*3.5f){
                                                    edificable=false;
                                                    num3++;
                                                }
                                            }
                                        //}else{
                                            /*Linea lineDebug = new Linea(P, Pproy);
                                            lineDebug.DrawLineSinCity(cityBuilder.ExteriorRoad, 0.3f);
                                            Linea lineDebugb = new Linea(Q, R);
                                            lineDebugb.DrawLineSinCity(cityBuilder.ExteriorRoad, 0.3f);*/
                                        //}
                                    }
                                }

                                j++;
                            }
                        }
                    }
                }

                i++;
            }

            if(edificable){
                cityCellsEdificables.Add(currentCityCell);
                currentCityCell.CreateBuildingLots(houseLength, houseWidth);
            }else{
                cityCellsNoEdificables.Add(currentCityCell);
            }
        }//);

        watch.Stop();
        Debug.Log("Tiempo tardado: " + watch.ElapsedMilliseconds + " ms");

        Debug.Log("Se pueden edificar " + cityCellsEdificables.Count + " de los " + cityCells.Count + " creados");
        Debug.Log("Se han reducido " + num1 + " por la primera condición");
        Debug.Log("Se han reducido " + num2 + " por la segunda condición");
        Debug.Log("Se han reducido " + num3 + " por la tercera condición");
        Debug.Log("Se han reducido " + numC + " por la condición extra para cuadriláteros");

        foreach(CityCell c in cityCellsEdificables){
            c.DrawBuildingLots();
        }
    }
}
