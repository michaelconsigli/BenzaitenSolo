using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Decorations;

public class AIModule
{
    private CityBuilder cityBuilder;
    public GameObject AIContainer;
    public GameObject camsController;

    public AIModule(CityBuilder cB){
        cityBuilder = cB;
    }

    public void createVillagers(){
        AIContainer = new GameObject("IA");
        AIContainer.transform.parent = cityBuilder.city.transform;

        camsController = new GameObject("IACamsController");
        camsController.transform.parent = AIContainer.transform;
        IACamsController controladorCamaras = camsController.AddComponent<IACamsController>();
        controladorCamaras.existsBirds = false;
        List<Camera> IACams = new List<Camera>();

        GameObject cityNodes = cityBuilder.city.transform.GetChild(0).gameObject;

        List<GameObject> villagersModels = new List<GameObject>();

        GameObject villagerModel1 = (GameObject)Resources.Load("Prefabs/chinoNuevoV2Adjust", typeof(GameObject));
        GameObject villagerModel2 = (GameObject)Resources.Load("Prefabs/chinoVNuevoRigged0Adjust", typeof(GameObject));

        villagersModels.Add(villagerModel1);
        villagersModels.Add(villagerModel2);

        List<RuntimeAnimatorController> animatorControlers = new List<RuntimeAnimatorController>();

        RuntimeAnimatorController aniController1 = (RuntimeAnimatorController)Resources.Load("Characters/RedAsianBoy/chino", typeof(RuntimeAnimatorController));
        RuntimeAnimatorController aniController2 = (RuntimeAnimatorController)Resources.Load("Characters/RedAsianBoy/chino 1", typeof(RuntimeAnimatorController));
        RuntimeAnimatorController aniController3 = (RuntimeAnimatorController)Resources.Load("Characters/RedAsianBoy/chino 2", typeof(RuntimeAnimatorController));
        RuntimeAnimatorController aniController4 = (RuntimeAnimatorController)Resources.Load("Characters/RedAsianBoy/chino 3", typeof(RuntimeAnimatorController));
        RuntimeAnimatorController aniController5 = (RuntimeAnimatorController)Resources.Load("Characters/RedAsianBoy/chino 4", typeof(RuntimeAnimatorController));

        animatorControlers.Add(aniController1);
        animatorControlers.Add(aniController2);
        animatorControlers.Add(aniController3);
        animatorControlers.Add(aniController4);
        animatorControlers.Add(aniController5);

        // Establecemos el factor de densidad de población
        float densityFactor = 0f;

        if(cityBuilder.populationDensity == Densidad.Baja){
            densityFactor = 0.25f;
        }else if(cityBuilder.populationDensity == Densidad.Media){
            densityFactor = 0.5f;
        }else{
            densityFactor = 0.75f;
        }

        Nodo nodoCentral = cityBuilder.getNodoCentral();

        int numVillager = 0;

        foreach(Nodo n in cityBuilder.acceptedNodes){
            if(n != nodoCentral){
                bool setVillager = true;
                float probSetVillager = cityBuilder.GenerateRandomProb();

                if(probSetVillager > densityFactor){
                    setVillager = false;
                }

                if(setVillager){
                    int modelIndex = (int)Random.Range(0f, villagersModels.Count);

                    GameObject currentVillager = cityBuilder.instanciarPrefabConPos(villagersModels[modelIndex], n.getPosicion(), AIContainer);
                    currentVillager.name = "Aldeano " + numVillager;
                    if(modelIndex == 0){
                        currentVillager.transform.localScale = new Vector3(0.1817f, 0.1817f, 0.1817f);
                    }else{
                        currentVillager.transform.localScale = new Vector3(0.1645f, 0.1645f, 0.1645f);
                    }
                    numVillager++;

                    Animator currentAnimator = currentVillager.GetComponent<Animator>();

                    int animControllerIndex = (int)Random.Range(0f, animatorControlers.Count);

                    currentAnimator.runtimeAnimatorController = animatorControlers[animControllerIndex];

                    // Insertamos cámara para el villager actual
                    GameObject currentCamObj = new GameObject("CamaraVillager");
                    currentCamObj.transform.parent = currentVillager.transform;
                    currentCamObj.transform.localPosition = new Vector3(0f, 2.51f, -2.71f);
                    currentCamObj.transform.localRotation = Quaternion.Euler(22.701f, 0f, 0f);
                    currentCamObj.transform.localScale = new Vector3(1f, 1f, 1f);

                    Camera currentCamera = currentCamObj.AddComponent<Camera>();
                    currentCamera.depth = 0;
                    currentCamera.rect = new Rect(0.79f, 0.75f, 0.2f, 0.22f);

                    if(IACams.Count > 0){
                        currentCamera.enabled = false;
                    }

                    IACams.Add(currentCamera);

                    float randProb = cityBuilder.GenerateRandomProb();

                    if(randProb < 0.9f){
                        SimpleVillager sV = currentVillager.AddComponent<SimpleVillager>() as SimpleVillager;
                        sV.animator = currentAnimator;
                        sV.grafoCiudad = cityNodes;
                        sV.maxNodosRecorridos = (int)Random.Range(5f, 15f);
                        sV.maxWaitTime = (int)Random.Range(5f, 15f);

                        NodoObj currentNodoObj = cityNodes.transform.GetChild(0).GetComponent<NodoObj>();
                        sV.alturaEnEscena = currentNodoObj.pos.y;
                        bool findNodo = false;
                        int i = 0;

                        while(!findNodo){
                            NodoObj nObj = cityNodes.transform.GetChild(i).GetComponent<NodoObj>();

                            if(nObj.pos == n.getPosicion()){
                                currentNodoObj = nObj;
                                findNodo = true;
                            }

                            i++;
                        }

                        sV.nodoActual = currentNodoObj;
                    }else{
                        PathFindingVillager pfV = currentVillager.AddComponent<PathFindingVillager>() as PathFindingVillager;
                        pfV.animator = currentAnimator;
                        pfV.grafoCiudad = cityNodes;
                        pfV.maxNodosRecorridos = (int)Random.Range(5f, 15f);
                        pfV.maxWaitTime = (int)Random.Range(5f, 15f);

                        NodoObj currentNodoObj = cityNodes.transform.GetChild(0).GetComponent<NodoObj>();
                        pfV.alturaEnEscena = currentNodoObj.pos.y;
                        bool findNodo = false;
                        int i = 0;

                        while(!findNodo){
                            NodoObj nObj = cityNodes.transform.GetChild(i).GetComponent<NodoObj>();

                            if(nObj.pos == n.getPosicion()){
                                currentNodoObj = nObj;
                                findNodo = true;
                            }

                            i++;
                        }

                        pfV.nodoActual = currentNodoObj;
                    }
                }
            }
        }

        controladorCamaras.villagersCams = IACams;
    }

    public void createBirds(){
        IACamsController controladorCamaras = camsController.GetComponent<IACamsController>();
        controladorCamaras.existsBirds = true;

        Nodo nodoCentral = cityBuilder.getNodoCentral();
        float birdHeightOffset = 3.5f;

        Nodo extremoDerecho;
        Nodo extremoIzquierdo;
        Nodo extremoArriba;
        Nodo extremoAbajo;

        List<Nodo> nodosOrdenados = cityBuilder.acceptedNodes.OrderByDescending(n => n.getPosicion().x).ToList();
        extremoDerecho = nodosOrdenados[0];
        nodosOrdenados = cityBuilder.acceptedNodes.OrderBy(n => n.getPosicion().x).ToList();
        extremoIzquierdo = nodosOrdenados[0];
        nodosOrdenados = cityBuilder.acceptedNodes.OrderByDescending(n => n.getPosicion().z).ToList();
        extremoArriba = nodosOrdenados[0];
        nodosOrdenados = cityBuilder.acceptedNodes.OrderBy(n => n.getPosicion().z).ToList();
        extremoAbajo = nodosOrdenados[0];

        List<Vector3> nodosPath = new List<Vector3>();
        Vector3 posInitBirds = new Vector3(extremoDerecho.getPosicion().x, nodoCentral.getPosicion().y+birdHeightOffset, nodoCentral.getPosicion().z);
        nodosPath.Add(posInitBirds);
        nodosPath.Add(new Vector3(nodoCentral.getPosicion().x, nodoCentral.getPosicion().y+birdHeightOffset, extremoAbajo.getPosicion().z));
        nodosPath.Add(new Vector3(extremoIzquierdo.getPosicion().x, nodoCentral.getPosicion().y+birdHeightOffset, nodoCentral.getPosicion().z));
        nodosPath.Add(new Vector3(nodoCentral.getPosicion().x, nodoCentral.getPosicion().y+birdHeightOffset, extremoArriba.getPosicion().z));

        GameObject pathBirdsObj = new GameObject("PathBirds");
        pathBirdsObj.transform.parent = AIContainer.transform;
        Path pathBirds = pathBirdsObj.AddComponent<Path>() as Path;
        pathBirds.nodos = nodosPath;

        GameObject birdModel = (GameObject)Resources.Load("Prefabs/littleBird2_Adjusted", typeof(GameObject));

        List<RuntimeAnimatorController> animatorControlers = new List<RuntimeAnimatorController>();

        RuntimeAnimatorController aniController1 = (RuntimeAnimatorController)Resources.Load("Birds/littleBird2", typeof(RuntimeAnimatorController));
        RuntimeAnimatorController aniController2 = (RuntimeAnimatorController)Resources.Load("Birds/littleBird2 1", typeof(RuntimeAnimatorController));
        RuntimeAnimatorController aniController3 = (RuntimeAnimatorController)Resources.Load("Birds/littleBird2 2", typeof(RuntimeAnimatorController));
        RuntimeAnimatorController aniController4 = (RuntimeAnimatorController)Resources.Load("Birds/littleBird2 3", typeof(RuntimeAnimatorController));
        RuntimeAnimatorController aniController5 = (RuntimeAnimatorController)Resources.Load("Birds/littleBird2 4", typeof(RuntimeAnimatorController));

        animatorControlers.Add(aniController1);
        animatorControlers.Add(aniController2);
        animatorControlers.Add(aniController3);
        animatorControlers.Add(aniController4);
        animatorControlers.Add(aniController5);

        int numBirds = Mathf.Min((int)(cityBuilder.acceptedNodes.Count/2f), 40);
        float distBirdBase = 0.15f;
        float distBird = 0.15f;

        int numBird = 1;

        GameObject currentBird = cityBuilder.instanciarPrefabConPos(birdModel, extremoDerecho.getPosicion()+new Vector3(0, birdHeightOffset, 0), AIContainer);
        currentBird.name = "Pájaro " + numBird;
        currentBird.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);
        numBird++;

        Animator currentAnimator = currentBird.GetComponent<Animator>();

        int animControllerIndex = (int)Random.Range(0f, animatorControlers.Count);

        currentAnimator.runtimeAnimatorController = animatorControlers[animControllerIndex];

        Bird birdAgent = currentBird.AddComponent<Bird>() as Bird;

        birdAgent.grafoAves = pathBirds;
        birdAgent.alturaEnEscena = posInitBirds.y;

        // Insertamos cámara para el villager actual
        GameObject currentCamObj = new GameObject("CamaraBirds");
        currentCamObj.transform.parent = currentBird.transform;
        currentCamObj.transform.localPosition = new Vector3(-0.45f, 10.18f, -21.27f);
        currentCamObj.transform.localRotation = Quaternion.Euler(20.642f, 0f, 0f);
        currentCamObj.transform.localScale = new Vector3(1f, 1f, 1f);

        Camera currentCamera = currentCamObj.AddComponent<Camera>();
        currentCamera.depth = 0;
        currentCamera.rect = new Rect(0.79f, 0.75f, 0.2f, 0.22f);
        currentCamera.nearClipPlane = 0.01f;
        controladorCamaras.birdsCam = currentCamera;

        controladorCamaras.villagersCams[0].enabled = false;

        for(int i = 0 ; i < (numBirds-1) ; i++){
            if(i % 8 == 0){
                currentBird = cityBuilder.instanciarPrefabConPos(birdModel, extremoDerecho.getPosicion()+new Vector3(distBird, birdHeightOffset, 0), AIContainer);
            }else if(i % 8 == 1){
                currentBird = cityBuilder.instanciarPrefabConPos(birdModel, extremoDerecho.getPosicion()+new Vector3(-distBird, birdHeightOffset , 0), AIContainer);
            }else if(i % 8 == 2){
                currentBird = cityBuilder.instanciarPrefabConPos(birdModel, extremoDerecho.getPosicion()+new Vector3(0, birdHeightOffset, distBird), AIContainer);
            }else if(i % 8 == 3){
                currentBird = cityBuilder.instanciarPrefabConPos(birdModel, extremoDerecho.getPosicion()+new Vector3(0, birdHeightOffset, -distBird), AIContainer);
            }else if(i % 8 == 4){
                currentBird = cityBuilder.instanciarPrefabConPos(birdModel, extremoDerecho.getPosicion()+new Vector3(distBird, birdHeightOffset, distBird), AIContainer);
            }else if(i % 8 == 5){
                currentBird = cityBuilder.instanciarPrefabConPos(birdModel, extremoDerecho.getPosicion()+new Vector3(-distBird, birdHeightOffset, distBird), AIContainer);
            }else if(i % 8 == 6){
                currentBird = cityBuilder.instanciarPrefabConPos(birdModel, extremoDerecho.getPosicion()+new Vector3(distBird, birdHeightOffset, -distBird), AIContainer);
            }else{
                currentBird = cityBuilder.instanciarPrefabConPos(birdModel, extremoDerecho.getPosicion()+new Vector3(-distBird, birdHeightOffset, -distBird), AIContainer);
                distBird = distBird + distBirdBase;
            }

            currentBird.name = "Pájaro " + numBird;
            currentBird.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);
            numBird++;

            currentAnimator = currentBird.GetComponent<Animator>();

            animControllerIndex = (int)Random.Range(0f, animatorControlers.Count);

            currentAnimator.runtimeAnimatorController = animatorControlers[animControllerIndex];

            birdAgent = currentBird.AddComponent<Bird>() as Bird;

            birdAgent.grafoAves = pathBirds;
            birdAgent.alturaEnEscena = posInitBirds.y;
        }

        GameObject depredadorModel = (GameObject)Resources.Load("Prefabs/Predator", typeof(GameObject));

        GameObject predator = cityBuilder.instanciarPrefabConPos(depredadorModel, nodoCentral.getPosicion()+new Vector3(0, birdHeightOffset, 0), AIContainer);
        predator.name = "Depredador";

        Depredador predatorAgent = predator.transform.GetChild(0).gameObject.GetComponent<Depredador>();
        predatorAgent.alturaEnEscena = posInitBirds.y;
    }
}
