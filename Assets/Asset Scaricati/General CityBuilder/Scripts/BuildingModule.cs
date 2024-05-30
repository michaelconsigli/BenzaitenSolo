using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Decorations;

// ************************
// *** Modulo Buildings ***
// ************************

public class BuildingModule
{
    private CityBuilder cityBuilder;
    private CityCellModule moduloCityCells;
    private Vector3Module moduloVectores;

    public GameObject buildingsContainer;
    public GameObject plantsContainer;

    public BuildingModule(CityBuilder builder){
        cityBuilder = builder;
        moduloCityCells = builder.moduloCityCell;
        moduloVectores = Vector3Module.GetInstance();
    }

    public void EdificarCiudad(float houseHeight){
        buildingsContainer = new GameObject("Edificaciones");
        buildingsContainer.transform.parent = cityBuilder.city.transform;

        plantsContainer = new GameObject("Vegetación");
        plantsContainer.transform.parent = cityBuilder.city.transform;

        List<CityCell> cityCellsAEdificar = moduloCityCells.cityCellsEdificables;
        List<CityCell> cityCellsSinCasas = moduloCityCells.cityCellsNoEdificables;
        int numCasa = 0;
        int numPlant = 0;
        int numBC = 0;

        // Establecemos el factor de densidad de vegetación
        float densityFactor = 0f;

        if(cityBuilder.plantDensity == Densidad.Baja){
            densityFactor = 0.05f;
        }else if(cityBuilder.plantDensity == Densidad.Media){
            densityFactor = 0.15f;
        }else{
            densityFactor = 1f;
        }

        foreach(CityCell currentCityCell in cityCellsAEdificar){
            // Construimos todas las casas del city cell actual
            foreach(BuildingLot currentLot in currentCityCell.getLots()){
                List<Nodo> contorno = currentLot.getNodosContorno();
                int indexModelo = (int)Random.Range(0f, cityBuilder.houseModels.Count);
                adjustaEdificio(houseHeight, contorno, cityBuilder.houseModels[indexModelo], numCasa);
                numCasa++;
            }

            // Plantamos toda la vegetación posible dentro del city cell actual
            List<GameObject> createdPlants = new List<GameObject>();
            List<float> createdPlantsRadius = new List<float>();

            float xR;
            float xL;
            float zU;
            float zD;

            getCityCellSquare(currentCityCell.getNodosContorno(), out xL, out xR, out zD, out zU);

            int numErrors = 0;
            float alturaNodo = currentCityCell.getNodosContorno()[0].getPosicion().y;

            while(numErrors < 250*densityFactor){ // 250
                float randNum = Random.Range(0f, 1f);
                Decoracion currentPlant = selectPlant(randNum);

                float factorAltura = Random.Range(1f, 2.25f);
                float originalScale = currentPlant.modelo.transform.localScale.x;
                float newScale = originalScale * factorAltura;
                float newRadius = (newScale * currentPlant.radio) / originalScale;
                Vector3 modifyScale = currentPlant.modelo.transform.localScale * factorAltura;
                currentPlant = new Decoracion(currentPlant, newRadius);

                float testX = Random.Range(xL, xR);
                float testZ = Random.Range(zD, zU);
                Vector3 testPos = new Vector3(testX, alturaNodo-0.05f, testZ);
                Vector3 refPos = new Vector3(xR+1f, alturaNodo, testZ);

                // Si el punto generado está dentro del city cell, continua. Si no, error cometido.
                if(isDentroCityCell(testPos, refPos, currentCityCell.getNodosContorno(), numPlant)){
                    // Si el punto generado no molesta a ningún edificio construido, continua. Si no, error cometido.
                    if(!isCollidingWithHouses(testPos, currentPlant.radio, currentCityCell.getNodosContorno())){
                        // Si el punto generado no molesta a ninguna vegetación creada, continua. Si no, error cometido.
                        if(!isCollidingWithOtherPlants(testPos, currentPlant.radio, createdPlants, createdPlantsRadius)){
                            // Generamos una instancia de la vegetación escogida
                            GameObject newPlant = cityBuilder.instanciarPrefabConScale(currentPlant.modelo, testPos, modifyScale, plantsContainer);
                            newPlant.name = "Planta " + numPlant;
                            numPlant++;

                            createdPlants.Add(newPlant);
                            createdPlantsRadius.Add(currentPlant.radio);
                        }else{
                            numErrors++;
                        }
                    }else{
                        numErrors++;
                    }
                }else{
                    numErrors++;
                }
            }
        }

        foreach(CityCell currentCityCell in cityCellsSinCasas){
            // Creamos toda la decoración posible dentro del city cell actual
            List<GameObject> createdDecorations = new List<GameObject>();
            List<float> createdDecorationsRadius = new List<float>();

            float xR;
            float xL;
            float zU;
            float zD;

            getCityCellSquare(currentCityCell.getNodosContorno(), out xL, out xR, out zD, out zU);

            int numErrors = 0;
            float alturaNodo = currentCityCell.getNodosContorno()[0].getPosicion().y;

            bool edificioCarColocado = false;

            while(!edificioCarColocado && numErrors < 100){ // 100
                float randNum = Random.Range(0f, 1f);
                Decoracion currentBC = selectBC(randNum);

                float testX = Random.Range(xL, xR);
                float testZ = Random.Range(zD, zU);
                Vector3 testPos = new Vector3(testX, alturaNodo, testZ);
                Vector3 refPos = new Vector3(xR+1f, alturaNodo, testZ);

                // Si el punto generado está dentro del city cell, continua. Si no, error cometido.
                if(isDentroCityCell(testPos, refPos, currentCityCell.getNodosContorno(), numBC)){
                    // Si el punto generado está muy cerca de la carretera, continua. Si no, error cometido.
                    if(!isCollidingWithHouses(testPos, currentBC.radio, currentCityCell.getNodosContorno())){
                        // Generamos una instancia de el edificio característico escogido
                        GameObject newBC = cityBuilder.instanciarPrefabConPos(currentBC.modelo, testPos, buildingsContainer);
                        newBC.name = "Edificio Característico " + numBC;
                        numBC++;

                        createdDecorations.Add(newBC);
                        createdDecorationsRadius.Add(currentBC.radio);

                        edificioCarColocado = true;
                    }else{
                        numErrors++;
                    }
                }else{
                    numErrors++;
                }
            }

            numErrors = 0;

            while(numErrors < 500*densityFactor){ // 500
                float randNum = Random.Range(0f, 1f);
                Decoracion currentPlant = selectPlant(randNum);

                float factorAltura = Random.Range(1f, 2.25f);
                float originalScale = currentPlant.modelo.transform.localScale.x;
                float newScale = originalScale * factorAltura;
                float newRadius = (newScale * currentPlant.radio) / originalScale;
                Vector3 modifyScale = currentPlant.modelo.transform.localScale * factorAltura;
                currentPlant = new Decoracion(currentPlant, newRadius);

                float testX = Random.Range(xL, xR);
                float testZ = Random.Range(zD, zU);
                Vector3 testPos = new Vector3(testX, alturaNodo-0.05f, testZ);
                Vector3 refPos = new Vector3(xR+1f, alturaNodo, testZ);

                // Si el punto generado está dentro del city cell, continua. Si no, error cometido.
                if(isDentroCityCell(testPos, refPos, currentCityCell.getNodosContorno(), numPlant)){
                    // Si el punto generado está muy cerca de la carretera, continua. Si no, error cometido.
                    if(!isCollidingWithHouses(testPos, (currentPlant.radio-cityBuilder.HouseDepth+0.3f), currentCityCell.getNodosContorno())){
                        // Si el punto generado no molesta a ninguna vegetación creada, continua. Si no, error cometido.
                        if(!isCollidingWithOtherPlants(testPos, (currentPlant.radio), createdDecorations, createdDecorationsRadius)){
                            // Generamos una instancia de la vegetación escogida
                            GameObject newPlant = cityBuilder.instanciarPrefabConScale(currentPlant.modelo, (testPos), modifyScale, plantsContainer);
                            newPlant.name = "Planta " + numPlant;
                            numPlant++;

                            createdDecorations.Add(newPlant);
                            createdDecorationsRadius.Add(currentPlant.radio);
                        }else{
                            numErrors++;
                        }
                    }else{
                        numErrors++;
                    }
                }else{
                    numErrors++;
                }
            }
        }
    }

    private Decoracion selectPlant(float randNum){
        float accumulatedProb = 0f;

        foreach(Decoracion d in cityBuilder.arbolado){
            if(randNum <= (accumulatedProb + d.probabilidad)){
                return d;
            }else{
                accumulatedProb = accumulatedProb + d.probabilidad;
            }
        }

        return cityBuilder.arbolado[0];
    }

    private Decoracion selectBC(float randNum){
        float accumulatedProb = 0f;

        foreach(Decoracion d in cityBuilder.edificiosCaracteristicos){
            if(randNum <= (accumulatedProb + d.probabilidad)){
                return d;
            }else{
                accumulatedProb = accumulatedProb + d.probabilidad;
            }
        }

        return cityBuilder.edificiosCaracteristicos[0];
    }

    private void getCityCellSquare(List<NodoCopia> contorno, out float xMin, out float xMax, out float zMin, out float zMax){
        float xR = -Mathf.Infinity;
        float xL = Mathf.Infinity;
        float zU = -Mathf.Infinity;
        float zD = Mathf.Infinity;

        foreach(Nodo n in contorno){
            Vector3 currentPos = n.getPosicion();

            if(currentPos.x > xR){
                xR = currentPos.x;
            }

            if(currentPos.x < xL){
                xL = currentPos.x;
            }

            if(currentPos.z > zU){
                zU = currentPos.z;
            }

            if(currentPos.z < zD){
                zD = currentPos.z;
            }
        }

        xMin = xL;
        xMax = xR;
        zMin = zD;
        zMax = zU;
    }

    private bool isDentroCityCell(Vector3 testPos, Vector3 refPos, List<NodoCopia> nodosContorno, int numPlant){ // No funciona bien !!!
        int numIntersections = 0;

        /*cityBuilder.instanciarPrefabConPos(cityBuilder.prefab, testPos);
        cityBuilder.instanciarPrefabConPos(cityBuilder.prefab, refPos);*/

        NodoCopia currentNode;
        NodoCopia nextNode;

        for(int j = 0; j < nodosContorno.Count ; j++){
            // Asiganamos los nodos de interés
            if(j == (nodosContorno.Count - 1)){
                currentNode = nodosContorno[j];
                nextNode = nodosContorno[0];
            }else{
                currentNode = nodosContorno[j];
                nextNode = nodosContorno[j+1];
            }

            if (moduloVectores.direction(currentNode.getPosicion(), testPos, nextNode.getPosicion()) == 0){
                if(moduloVectores.onSegment(currentNode.getPosicion(), testPos, nextNode.getPosicion())){
                    return true;
                }
            }

            // Comprobamos si hay intersección
            if(moduloVectores.checkIntersectionCityCells(testPos, refPos, currentNode.getPosicion(), nextNode.getPosicion())){
                numIntersections++;
            }
        }

        if(numIntersections % 2 == 1){
            return true;
        }else{
            return false;
        }
    }

    private bool isCollidingWithHouses(Vector3 testPos, float testRadius, List<NodoCopia> nodosContorno){
        bool colliding = false;

        NodoCopia auxCurrentNode;
        NodoCopia auxNextNode;

        int j = 0;

        while(!colliding && j < nodosContorno.Count){
            // Asiganamos los nodos de interés
            if(j == (nodosContorno.Count - 1)){
                auxCurrentNode = nodosContorno[j];
                auxNextNode = nodosContorno[0];
            }else{
                auxCurrentNode = nodosContorno[j];
                auxNextNode = nodosContorno[j+1];
            }

            Vector3 P = testPos;
            Vector3 Q = auxCurrentNode.getPosicion();
            Vector3 R = auxNextNode.getPosicion();

            Vector3 Pproy = moduloVectores.calculateProjection(P,Q,R);

            float rangoAceptacion = cityBuilder.HouseDepth;

            if(!(Pproy.x >= Mathf.Min(Q.x-rangoAceptacion, R.x-rangoAceptacion) && Pproy.x <= Mathf.Max(Q.x+rangoAceptacion, R.x+rangoAceptacion)) || !(Pproy.z >= Mathf.Min(Q.z-rangoAceptacion, R.z-rangoAceptacion) && Pproy.z <= Mathf.Max(Q.z+rangoAceptacion, R.z+rangoAceptacion))){
                if(moduloVectores.calculateDistance(Q,P) <= moduloVectores.calculateDistance(R,P)){
                    Pproy = Q;
                }else{
                    Pproy = R;
                }
            }

            if(moduloVectores.calculateDistance(P,Pproy) < (testRadius + cityBuilder.HouseDepth) * 1.25f){
                colliding=true;
            }

            j++;
        }

        return colliding;
    }

    private bool isCollidingWithOtherPlants(Vector3 testPos, float testRadius, List<GameObject> createdPlants, List<float> createdPlantsRadius){
        bool colliding = false;

        int j = 0;
        while(!colliding && j < createdPlants.Count){
            Vector3 currentPos = createdPlants[j].transform.position;
            float currentRadius = createdPlantsRadius[j];

            Vector3 distanceVector = testPos - currentPos;
            float distance = distanceVector.magnitude;

            if(distance < (testRadius + currentRadius) * 1.5f){
                colliding = true;
            }

            j++;
        }

        return colliding;
    }

    private void adjustaEdificio(float altura, List<Nodo> contorno, GameObject modeloCasa, int id){
        // Inicializamos o declaramos las varibles necesarias
        GameObject ob = cityBuilder.instanciarPrefab(modeloCasa, buildingsContainer);
        ob.name = "Casa " + id;

        Mesh mesh, mesh2;
        Vector3[] pos, pos0;

        #if UNITY_EDITOR
            //Only do this in the editor
            MeshFilter mf = ob.GetComponent<MeshFilter>();            // A better way of getting the meshfilter using Generics
            Mesh meshCopy = Mesh.Instantiate(mf.sharedMesh) as Mesh;  // Make a deep copy
            mesh = mf.mesh = meshCopy;                                // Assign the copy to the meshes
        #else
            //do this in play mode
            mesh = ob.GetComponent<MeshFilter>().mesh;
        #endif

        mesh2 = new Mesh();
        pos = new Vector3[mesh.vertices.Length];
        pos0 = mesh.vertices;

        Vector3 P0 = contorno[1].getPosicion();
        Vector3 P1 = contorno[0].getPosicion();
        Vector3 P2 = contorno[3].getPosicion();
        Vector3 P3 = contorno[2].getPosicion();

        float signo = 1;

        Vector3 A = Vector3.zero;
        Vector3 B = P1 - P0;
        Vector3 C = P2 - P0;
        Vector3 D = P3 - P0;

        Vector3 v = new Vector3(-B.z, 0, B.x);

        if(Vector3.Dot(C,v) < 0){
            signo = -1;
        }

        for(int i=0 ; i < pos.Length ; i++){
            Vector3 H1 = A + (C-A) * pos0[i].y;
            Vector3 H2 = B + (D-B) * pos0[i].y;
            Vector3 H = H1 + (H2-H1) * pos0[i].x;
            pos[i] = new Vector3(H.x, -pos0[i].z*altura, H.z);
        }

        mesh2.vertices = pos;   
        
        if(signo == -1){
            int[] triangulos2 = new int[mesh.triangles.Length];

            for(int i = 0 ; i < triangulos2.Length ; i+=3){
                triangulos2[i] = mesh.triangles[i];
                triangulos2[i+1] = mesh.triangles[i+2];
                triangulos2[i+2] = mesh.triangles[i+1];
            }

            mesh2.triangles = triangulos2;
        }
        else{
            mesh2.triangles = mesh.triangles;
        }

        mesh2.normals = mesh.normals;
        mesh2.uv = mesh.uv;
        mesh2.RecalculateNormals();
        
        ob.GetComponent<MeshFilter>().mesh = mesh2;
        ob.transform.position = P0;
        ob.AddComponent<MeshCollider>();
    }
}
