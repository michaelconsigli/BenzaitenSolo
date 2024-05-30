using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using ConectionType;
using Decorations;

namespace Decorations
{
    [System.Serializable]
    public struct Decoracion
    {
        public GameObject modelo;
        public float radio;
        public float probabilidad;

        public Decoracion(Decoracion d, float r){
            modelo = d.modelo;
            radio = r;
            probabilidad = d.probabilidad;
        }

        public Decoracion(float p, Decoracion d){
            modelo = d.modelo;
            radio = d.radio;
            probabilidad = p;
        }
    }

    public enum Densidad
    {
        Baja,
        Media,
        Alta
    }
}

#pragma warning disable 0414 // private field assigned but not used.

public enum Topologia
{
    Raster,
    Branching,
    Radial_Raster,
    Radial_Branching
}

[ExecuteInEditMode]
public class CityBuilder : MonoBehaviour
{
    [Tooltip("Punto donde empieza la creación de la ciudad")]
    [SerializeField]
    private Transform puntoOrigen;

    [Tooltip("Indica si se quiere desplegar el menú de configuración completo o no")]
    [SerializeField]
    private bool modoCompleto = false;

    [Header("Parámetros caminos")]
    [Tooltip("Topología de la ciudad")]
    [SerializeField]
    private Topologia topology;

    [Space(15)]

    [Tooltip("Longitud de las calles principales")]
    [SerializeField]
    private int longPR = 8;
    [Tooltip("Longitud de las calles secundarias")]
    [SerializeField]
    private int longSR = 4; // Menor que longPR
    [Tooltip("Anchura de las calles")]
    [Range(0.5f, 1.5f)]
    [SerializeField]
    public float anchuraCarretera = 1.5f;
    [Tooltip("Probabilidad de generar una calle principal alternativa")]
    [Range(0.0f, 1.0f)]
    [SerializeField]
    private float probBanchingPR = 0.10f;
    [Tooltip("Probabilidad de generar una calle secundaria alternativa")]
    [Range(0.0f, 1.0f)]
    [SerializeField]
    private float probBanchingSR = 0.9f;
    [Tooltip("Máximo número de calles principales a generar")]
    [Range(25, 175)]
    [SerializeField]
    private int segmentLimitPR = 100;
    [Tooltip("Máximo número de calles secundarias a generar por cada nodo secundario inicial")]
    [Range(25, 175)]
    [SerializeField]
    private int segmentLimitSR = 50;
    [Tooltip("Material con el que se dibujarán las carreteras")]
    [SerializeField]
    private Material RoadMaterial;

    [Space(15)]

    [ConditionalHide("modoCompleto")]
    [Tooltip("Longitud para considerar posibles uniones o alargamientos de calles")]
    [SerializeField]
    private float snapLength = 4.0f; // Hasta longSR
    [ConditionalHide("modoCompleto")]
    [Tooltip("Longitud para realizar una unión en lugar de una intersección en alguno de sus extremos")]
    [SerializeField]
    private float vertexRadio = 2.0f; // Hasta longSR / 2
    [ConditionalHide("modoCompleto")]
    [Tooltip("Ángulo mínimo de separación entre calles al realizar una unión")]
    [Range(35, 50)]
    [SerializeField]
    private float minAngle = 35.0f; // Hasta 50º

    [Space(15)]

    [ConditionalHide("modoCompleto")]
    [Tooltip("Número de círculos para una ciudad Radial")]
    [SerializeField]
    private int numCircles = 4;
    [ConditionalHide("modoCompleto")]
    [Tooltip("Cuanto detalle queremos que tengan los círculos")]
    [Range(1, 36)]
    [SerializeField]
    private int numSegsCircle = 12; // Al menos 4
    [ConditionalHide("modoCompleto")]
    [Tooltip("Distancia entre cada par de círculos")]
    [SerializeField]
    private float circlesDistance = 35.0f;
    [ConditionalHide("modoCompleto")]
    [Tooltip("Número de calles principales que atraviesan la ciudad Radial")]
    [SerializeField]
    private int numPRBranches = 3;

    [Header("Parámetros edificación")]

    [Tooltip("Largo de las casa a construir")]
    [Range(1.5f, 5f)]
    [SerializeField]
    private float HouseLength = 1.5f;
    [Tooltip("Largo de las casa a construir")]
    [Range(1.5f, 5f)]
    public float HouseDepth = 1.5f;
    [Tooltip("Altura de las casa a construir")]
    [Range(0.75f, 1.5f)]
    [SerializeField]
    private float HouseHeight = 1f;
    [Tooltip("Modelos de casas para edificar")]
    [SerializeField]
    public List<GameObject> houseModels;

    [Header("Parámetros decoración")]

    [Tooltip("Modelos de arbolado, con su radio de ocupación y probabilidad de aparición")]
    public List<Decoracion> arbolado;
    [Tooltip("Densidad de vegetación")]
    [SerializeField]
    public Densidad plantDensity;

    [Space(15)]

    [Tooltip("Modelos de edificios característicos, con su radio de ocupación y probabilidad de aparición")]
    public List<Decoracion> edificiosCaracteristicos;

    [Header("Opciones Personajes")]

    [Tooltip("Quiere que se genere IA en la ciudad")]
    [SerializeField]
    public bool AI = true;

    [Tooltip("Densidad de población")]
    [SerializeField]
    public Densidad populationDensity;

    [Space(5)]

    [Tooltip("Quiere que se generen pájaros con IA en la ciudad")]
    [SerializeField]
    public bool birds = true;

    [Space(15)]

    [Tooltip("Crear un personaje jugable y una cámara libre por defecto")]
    [SerializeField]
    public bool defaultPlayerSys = true;



    [HideInInspector]
    public bool isCityCreated;

    [HideInInspector]
    public GameObject city;

    [HideInInspector]
    public GameObject prefab;

    [HideInInspector]
    public Material lineMaterial;

    private float minAnguloGiro = 87.0f; // Menor que maxAnguloGiro
    private float maxAnguloGiro = 93.0f;

    private Queue<Nodo> evaluationQueue;
    private Queue<Nodo> evaluationQueueSR;
    public List<Nodo> acceptedNodes;

    private Vector3Module moduloVectores;
    [HideInInspector]
    public CityCellModule moduloCityCell;
    private BuildingModule moduloEdificacion;
    private AIModule moduloIA;

    void Start(){
        isCityCreated = false;
        prefab = (GameObject)Resources.Load("Prefabs/Nodo", typeof(GameObject));
        lineMaterial = (Material)Resources.Load("Materials/Rojo", typeof(Material));
    }

    private void checkParameters(){
        // Comprobación parámetros
        if(longPR < 5){
            longPR = 5;
        }else if((topology == Topologia.Radial_Raster || topology == Topologia.Radial_Branching) && longPR >= (circlesDistance/2)){
            longPR = (int)circlesDistance/2;
        }

        if(longSR < 1){
            longSR = 1;
        }else if(longSR >= longPR){
            longSR = longPR - 4;
        }

        if(snapLength < 0.0f){
            snapLength = 0.0f;
        }else if(snapLength > longSR){
            snapLength = longSR;
        }

        if(vertexRadio < 0.0f){
            vertexRadio = 0.0f;
        }else if(vertexRadio > (longSR/2)){
            vertexRadio = (longSR/2);
        }

        if(numCircles < 1){
            numCircles = 1;
        }

        if(numSegsCircle < 4){
            numSegsCircle = 4;
        }

        if(circlesDistance < 30.0f){
            circlesDistance = 30.0f;
        }

        if(numPRBranches < 1){
            numPRBranches = 1;
        }else if(numPRBranches > numSegsCircle){
            numPRBranches = numSegsCircle;
        }

        for(int i = 0 ; i < arbolado.Count ; i++){
            if(arbolado[i].radio <= 0f){
                arbolado[i] = new Decoracion(arbolado[i], 0.5f);
            }
        }

        for(int i = 0 ; i < edificiosCaracteristicos.Count ; i++){
            if(edificiosCaracteristicos[i].radio <= 0f){
                edificiosCaracteristicos[i] = new Decoracion(arbolado[i], 1.5f);
            }
        }
    }

    void Update(){
        checkParameters();
    }

    public void DestroyCity(){
        if (Application.isEditor){
            DestroyImmediate(city);
        }else{
            Destroy(city);
        }

        isCityCreated = false;
    }

    public void RestartCity(){
        if (Application.isEditor){
            DestroyImmediate(city);
        }else{
            Destroy(city);
        }

        city = new GameObject("Ciudad");
        city.transform.position = transform.position;
        city.transform.rotation = transform.rotation;

        evaluationQueue = new Queue<Nodo>();
        evaluationQueueSR = new Queue<Nodo>();
        acceptedNodes = new List<Nodo>();
    }

    private void RestartCityObjects(){
        if (Application.isEditor){
            DestroyImmediate(city);
        }else{
            Destroy(city);
        }

        city = new GameObject("Ciudad");
        city.transform.position = transform.position;
        city.transform.rotation = transform.rotation;
    }

    public void BuildCity(){
        moduloVectores = Vector3Module.GetInstance();
        moduloCityCell = new CityCellModule(this);
        moduloEdificacion = new BuildingModule(this);
        moduloIA = new AIModule(this);
        
        if(!isCityCreated){
            isCityCreated = true;
            city = new GameObject("Ciudad");
            city.transform.position = transform.position;
            city.transform.rotation = transform.rotation;

            evaluationQueue = new Queue<Nodo>();
            evaluationQueueSR = new Queue<Nodo>();
            acceptedNodes = new List<Nodo>();
        }else{
            RestartCity();
        }

        if(topology == Topologia.Raster){
            minAnguloGiro = 87.0f;
            maxAnguloGiro = 93.0f;
            GeneratePR();
        }else if(topology == Topologia.Branching){
            minAnguloGiro = 70.0f;
            maxAnguloGiro = 110.0f;
            GeneratePR();
        }else{
            if(topology == Topologia.Radial_Raster){
                minAnguloGiro = 87.0f;
                maxAnguloGiro = 93.0f;
            }else{
                minAnguloGiro = 70.0f;
                maxAnguloGiro = 110.0f;
            }

            GenerateRadialPR();
        }

        GenerateSR();

        RemoveConectionsByAngle();
        RemoveAisolatedPoints();
        ReducirNodos();
        DeleteInvalidStreets();
        DeleteNodesLess2Streets();
        
        if(acceptedNodes.Count > 1){
            moduloCityCell.DetectExteriorRoads();
            moduloCityCell.EliminateFilaments();
        }
    }

    private void GeneratePR(){
        // Add an initial road segment to branch out from
        Nodo nodoInicial = new Nodo(puntoOrigen.position);
        acceptedNodes.Add(nodoInicial);

        Nodo newNodo1 = new Nodo(nodoInicial.getPosicion() + new Vector3(5,0,0));
        newNodo1.addConexion(nodoInicial);

        evaluationQueue.Enqueue(newNodo1);

        Nodo newNodo2 = new Nodo(nodoInicial.getPosicion() + new Vector3(-5,0,0));
        newNodo2.addConexion(nodoInicial);

        evaluationQueue.Enqueue(newNodo2);

        
        int i = 0;
        
        // Loop until we reach a limit or can't build any more roads
        while (i < segmentLimitPR && evaluationQueue.Count != 0)
        {
            // Take the oldest node in the queue
            Nodo currentNode = evaluationQueue.Dequeue();
            
            // Check if it's acceptable and modify it as needed
            int caso;
            bool nodeAccepted = CheckLocalConstraints(out caso, currentNode);
            
            if (nodeAccepted && caso != 2)
            {
                // Add this segment to the actual road network
                acceptedNodes.Add(currentNode);
                currentNode.getConexiones()[0].addConexion(currentNode);
                
                if(caso == 0){
                    GeneratePossibleNodes(currentNode);
                }
            }

            i++;
        }
    }

    private void GenerateRadialPR(){
        // Add an initial road segment to branch out from
        Nodo nodoInicial = new Nodo(puntoOrigen.position);
        acceptedNodes.Add(nodoInicial);

        for(int i = 0 ; i < numCircles ; i++){
            float currentCircleDistance = circlesDistance * (i+1);

            Nodo startCirclePoint = new Nodo(nodoInicial.getPosicion() + new Vector3(0,0,currentCircleDistance));
            acceptedNodes.Add(startCirclePoint);

            Nodo previousPoint = startCirclePoint;

            for(int j = 1 ; j < numSegsCircle ; j++){
                float pointAngle = 2.0f * Mathf.PI / (float) numSegsCircle * j;
                Nodo nextPoint = new Nodo(nodoInicial.getPosicion() + new Vector3(Mathf.Sin(pointAngle) * currentCircleDistance,0,Mathf.Cos(pointAngle) * currentCircleDistance));
                nextPoint.addConexion(previousPoint);
                acceptedNodes.Add(nextPoint);
                previousPoint.addConexion(nextPoint);
                previousPoint = nextPoint;
            }

            startCirclePoint.addConexion(previousPoint);
            previousPoint.addConexion(startCirclePoint);
        }

        List<int> currentDirNodes = getRadialDirections(numSegsCircle);
        List<int> previousDirNodes = new List<int>();
        List<Vector3> directions = new List<Vector3>();

        for(int p = 0 ; p < currentDirNodes.Count ; p++){
            previousDirNodes.Add(currentDirNodes[0]); //Inicialización solo
        }

        for(int k = 0 ; k < numCircles ; k++){
            for(int p = 0 ; p < currentDirNodes.Count ; p++){
                Nodo nodo = acceptedNodes[currentDirNodes[p]];
                Vector3 currentDir;
                Nodo previousPoint;
                
                if(k == 0){
                    previousPoint = nodoInicial;
                    currentDir = moduloVectores.CalculateDirection(nodoInicial, nodo);
                    directions.Add(currentDir);
                }else{
                    previousPoint = acceptedNodes[previousDirNodes[p]];
                    currentDir = directions[p];
                }

                int numRoads = (int) (circlesDistance / longPR);

                if((circlesDistance % longPR) == 0){
                    numRoads--;
                }

                for(int i = 0 ; i < numRoads ; i++){
                    Nodo newNode = new Nodo(previousPoint.getPosicion() + currentDir * longPR);

                    newNode.addConexion(previousPoint);
                    previousPoint.addConexion(newNode);
                    acceptedNodes.Add(newNode);

                    if(GenerateRandomProb() <= 0.5f){
                        Vector3 newDir = Quaternion.AngleAxis(GenerateAngleRotation(), Vector3.up) * moduloVectores.CalculateDirection(newNode.getConexiones()[0], newNode);
                        Nodo newInitialSR = new Nodo(newNode.getPosicion() + newDir * longSR);
                        newInitialSR.addConexion(newNode);
                        evaluationQueueSR.Enqueue(newInitialSR);

                        if(GenerateRandomProb() <= 0.5f){
                            newDir = Quaternion.AngleAxis(-GenerateAngleRotation(), Vector3.up) * moduloVectores.CalculateDirection(newNode.getConexiones()[0], newNode);
                            Nodo newInitialSR2 = new Nodo(newNode.getPosicion() + newDir * longSR);
                            newInitialSR2.addConexion(newNode);
                            evaluationQueueSR.Enqueue(newInitialSR2);
                        }
                    }

                    previousPoint = newNode;
                }

                nodo.addConexion(previousPoint);
                previousPoint.addConexion(nodo);
                
                if(k < (numCircles - 1)){
                    previousDirNodes[p] = currentDirNodes[p];
                    currentDirNodes[p] = currentDirNodes[p] + numSegsCircle;
                }else{
                    Nodo newNode = new Nodo(nodo.getPosicion() + currentDir * longPR);

                    newNode.addConexion(nodo);
                    nodo.addConexion(newNode);
                    acceptedNodes.Add(newNode);
                }
            }
        }

        for(int i = 1; i <= (numSegsCircle*(numCircles-1)) ; i++){
            Nodo currentNode = acceptedNodes[i];

            if(currentNode.getConexiones().Count == 2 && GenerateRandomProb() <= probBanchingSR){
                Vector3 dirSum = moduloVectores.CalculateDirection(currentNode, currentNode.getConexiones()[0]) + moduloVectores.CalculateDirection(currentNode, currentNode.getConexiones()[1]);
                Vector3 newDir = Quaternion.AngleAxis(50, Vector3.up) * dirSum; 
                Nodo newInitialSR = new Nodo(currentNode.getPosicion() + newDir * longSR);
                newInitialSR.addConexion(currentNode);
                evaluationQueueSR.Enqueue(newInitialSR);
            }
        }
    }

    private List<int> getRadialDirections(int totalCandidateNodes){
        List<int> dirNodes = new List<int>();
        List<int> posibleNodes = new List<int>();

        for(int i = 1 ; i <= totalCandidateNodes ; i++){
            posibleNodes.Add(i);
        }

        int restCandidates = numPRBranches;

        while(restCandidates > 0){
            System.Random random = new System.Random();
            int randPos = random.Next(posibleNodes.Count);
            int selectedNode = posibleNodes[randPos];
            dirNodes.Add(selectedNode);
            posibleNodes.Remove(selectedNode);
            restCandidates--;
        }

        return dirNodes;
    }

    private void GenerateSR(){
        while(evaluationQueueSR.Count != 0){
            Nodo currentSRNodo = evaluationQueueSR.Dequeue();
            int i = 0;
            Queue<Nodo> evaluationQueueCurrentSR = new Queue<Nodo>();
            evaluationQueueCurrentSR.Enqueue(currentSRNodo);
            while (i < segmentLimitSR && evaluationQueueCurrentSR.Count != 0)
            {
                Nodo currentNode = evaluationQueueCurrentSR.Dequeue();
                
                int caso;
                bool nodeAccepted = CheckLocalConstraints(out caso, currentNode);
                
                if (nodeAccepted && caso != 2)
                {
                    acceptedNodes.Add(currentNode);
                    currentNode.getConexiones()[0].addConexion(currentNode);
                    
                    if(caso == 0){
                        GeneratePossibleNodesSR(currentNode, evaluationQueueCurrentSR);
                    }
                }

                i++;
            }
        }
    }

    private bool CheckLocalConstraints(out int caso, Nodo nodo)
    {
        caso = 0;

        int foundIntersection = isIntersecting(nodo);

        if(foundIntersection != 0){
            if(foundIntersection == -1){
                return false;
            }else{
                caso = foundIntersection;
                return true;
            }
        }else{
            foreach(Nodo currentNode in acceptedNodes){
                if(currentNode != nodo && currentNode != nodo.getConexiones()[0] && Vector3.Distance(nodo.getPosicion(), currentNode.getPosicion()) <= snapLength && !hayIntersecion(nodo.getConexiones()[0].getPosicion(), currentNode.getPosicion())){
                    if(isAllowedUnion(currentNode, nodo.getConexiones()[0])){
                        caso = 2;
                        currentNode.addConexion(nodo.getConexiones()[0]);
                        nodo.getConexiones()[0].addConexion(currentNode);
                        return true;
                    }else{
                        return false;
                    }
                }
            }

            if(false && topology != Topologia.Radial_Raster && topology != Topologia.Radial_Branching){ // Parece que da problemas en cualquier topología
                Vector3 snapPosition = nodo.getPosicion() + moduloVectores.CalculateDirection(nodo.getConexiones()[0], nodo) * (snapLength * 2);
                Nodo snapNode = new Nodo(snapPosition);
                snapNode.addConexion(nodo.getConexiones()[0]);

                foundIntersection = isIntersecting(snapNode);

                if(foundIntersection != 0){
                    if(foundIntersection == -1){
                        return false;
                    }else{
                        nodo = snapNode;
                        caso = foundIntersection;
                    }
                }
            }

            return true;
        }
    }

    private void ReducirNodos(){
        int p = 0;
        int numAcceptedNodes = acceptedNodes.Count;

        while(p < numAcceptedNodes){
            Nodo currentNode = acceptedNodes[p];
            bool hayBorrado = true;

            for(int c = 0 ; c < currentNode.getConexiones().Count ; c++){
                hayBorrado = true;

                while(hayBorrado){
                    if(c >= currentNode.getConexiones().Count){  // Error en el que a veces si da este caso: c == currentNode.getConexiones().Count
                        break;
                    }

                    Nodo currentConexion = currentNode.getConexiones()[c];

                    currentConexion.removeConexion(currentNode);

                    if(currentConexion.getConexiones().Count == 1 && sameDirection(currentNode, currentConexion)){
                        currentConexion.getConexiones()[0].removeConexion(currentConexion);
                        currentNode.removeConexion(currentConexion);

                        currentNode.addConexion(currentConexion.getConexiones()[0]);
                        currentConexion.getConexiones()[0].addConexion(currentNode);

                        currentConexion.removeConexion(currentConexion.getConexiones()[0]);
                        acceptedNodes.Remove(currentConexion);
                        numAcceptedNodes--;
                    }else{
                        currentConexion.addConexion(currentNode);
                        hayBorrado = false;
                    }
                }
            }

            p++;
        }
    }

    private bool hayIntersecion(Vector3 start, Vector3 end){
        foreach(Nodo currentNode in acceptedNodes){
            if(currentNode.getPosicion() != start && currentNode.getPosicion() != end){
                foreach(Nodo currentConexion in currentNode.getConexiones()){
                    if(currentConexion.getPosicion() != start && currentConexion.getPosicion() != end){
                        if(moduloVectores.checkIntersection(start, end, currentNode.getPosicion(), currentConexion.getPosicion())){
                            return true;
                        }
                    }
                }
            }
        }

        return false;
    }

    private int isIntersecting(Nodo nodo){
        float nearestDistance = -1.0f; // Inicialización
        Vector3 intersection = new Vector3(0,0,0);
        Nodo n = new Nodo(intersection);
        Nodo nc = new Nodo(intersection);

        foreach(Nodo currentNode in acceptedNodes){
            if(currentNode != nodo && currentNode != nodo.getConexiones()[0]){
                foreach(Nodo currentConexion in currentNode.getConexiones()){
                    if(currentConexion != nodo && currentConexion != nodo.getConexiones()[0]){
                        if(moduloVectores.checkIntersection(nodo.getConexiones()[0].getPosicion(), nodo.getPosicion(), currentNode.getPosicion(), currentConexion.getPosicion())){
                            Vector3 foundIntersection = moduloVectores.CalculateIntersectionPoint(nodo.getConexiones()[0], nodo, currentNode, currentConexion);

                            if(Mathf.Abs(foundIntersection.x) == Mathf.Infinity || Mathf.Abs(foundIntersection.z) == Mathf.Infinity){
                                continue;
                            }

                            if(nearestDistance == -1 || Vector3.Distance(nodo.getConexiones()[0].getPosicion(), foundIntersection) < nearestDistance){
                                nearestDistance = Vector3.Distance(nodo.getConexiones()[0].getPosicion(), foundIntersection);
                                intersection = foundIntersection;
                                n = currentNode;
                                nc = currentConexion;
                            }
                        }
                    }
                }
            }
        }

        if(nearestDistance != -1){
            if(Vector3.Distance(intersection, n.getPosicion()) <= vertexRadio && !hayIntersecion(nodo.getConexiones()[0].getPosicion(), n.getPosicion())){
                if(isAllowedUnion(n, nodo.getConexiones()[0])){
                    n.addConexion(nodo.getConexiones()[0]);
                    nodo.getConexiones()[0].addConexion(n);
                    return 2;
                }else{
                    return -1;
                }
            }else if(Vector3.Distance(intersection, nc.getPosicion()) <= vertexRadio && !hayIntersecion(nodo.getConexiones()[0].getPosicion(), nc.getPosicion())){
                if(isAllowedUnion(nc, nodo.getConexiones()[0])){
                    nc.addConexion(nodo.getConexiones()[0]);
                    nodo.getConexiones()[0].addConexion(nc);
                    return 2;
                }else{
                    return -1;
                }
            }else{
                nodo.setPosicion(intersection);

                divideSegmento(nodo, n, nc);

                return 1;
            }
        }else{
            return 0;
        }
    }

    private bool isAllowedUnion(Nodo originNode, Nodo newPossibleConection){
        Vector3 dirPossibleConection = newPossibleConection.getPosicion() - originNode.getPosicion();

        foreach(Nodo n in originNode.getConexiones()){
            Vector3 dirCurrentConection = n.getPosicion() - originNode.getPosicion();

            if(Vector3.Angle(dirPossibleConection, dirCurrentConection) < minAngle){
                return false;
            }
        }

        return true;
    }

    private void divideSegmento(Nodo nodo, Nodo nodoInter1, Nodo nodoInter2){
        nodoInter1.removeConexion(nodoInter2);
        nodoInter2.removeConexion(nodoInter1);

        nodo.addConexion(nodoInter1);
        nodoInter1.addConexion(nodo);

        nodo.addConexion(nodoInter2);
        nodoInter2.addConexion(nodo);
    }

    private bool sameDirection(Nodo nodo, Nodo nodoInter){
        if(moduloVectores.CalculateDirection(nodo, nodoInter) == moduloVectores.CalculateDirection(nodo, nodoInter.getConexiones()[0])){
            return true;
        }else{
            return false;
        }
    }

    private bool sameDirectionConections(Nodo nodo){
        if(moduloVectores.CalculateDirection(nodo.getConexiones()[0], nodo) == moduloVectores.CalculateDirection(nodo.getConexiones()[0], nodo.getConexiones()[1])){
            return true;
        }else{
            return false;
        }
    }

    private void GeneratePossibleNodes(Nodo nodo)
    {
        // This is where you generate new proposed road nodes based on 'global constraints' - height, nearby roads, etc.
        // Para la direccion, supondremos que solo tiene un vecino es nodo introducido en la función
        Vector3 newDir;
        Nodo newNodo;

        if(GenerateRandomProb() <= probBanchingPR){
            newDir = Quaternion.AngleAxis(GenerateAngleRotation(), Vector3.up) * moduloVectores.CalculateDirection(nodo.getConexiones()[0], nodo);

            newNodo = new Nodo(nodo.getPosicion() + newDir * longPR);
            newNodo.addConexion(nodo);

            evaluationQueue.Enqueue(newNodo);
        }

        newDir = moduloVectores.CalculateDirection(nodo.getConexiones()[0], nodo);

        newNodo = new Nodo(nodo.getPosicion() + newDir * longPR);
        newNodo.addConexion(nodo);

        evaluationQueue.Enqueue(newNodo);


        if(GenerateRandomProb() <= 0.5f){
            newDir = Quaternion.AngleAxis(-GenerateAngleRotation(), Vector3.up) * moduloVectores.CalculateDirection(nodo.getConexiones()[0], nodo);
            Nodo newInitialSR = new Nodo(nodo.getPosicion() + newDir * longSR);
            newInitialSR.addConexion(nodo);

            evaluationQueueSR.Enqueue(newInitialSR);
        }
    }

    private void GeneratePossibleNodesSR(Nodo nodo, Queue<Nodo> evaluationQueueCurrentSR)
    {
        // Para la direccion, supondremos que solo tiene un vecino es nodo introducido en la función
        Vector3 newDir;
        Nodo newNodo;

        if(GenerateRandomProb() <= probBanchingSR){
            float angle = GenerateAngleRotation();

            if(GenerateRandomProb() <= 0.5f){
                angle = -angle;
            }

            newDir = Quaternion.AngleAxis(angle, Vector3.up) * moduloVectores.CalculateDirection(nodo.getConexiones()[0], nodo);

            newNodo = new Nodo(nodo.getPosicion() + newDir * longPR);
            newNodo.addConexion(nodo);

            evaluationQueueCurrentSR.Enqueue(newNodo);
        }

        newDir = moduloVectores.CalculateDirection(nodo.getConexiones()[0], nodo);

        newNodo = new Nodo(nodo.getPosicion() + newDir * longPR);
        newNodo.addConexion(nodo);

        evaluationQueueCurrentSR.Enqueue(newNodo);
    }

    private void RemoveAisolatedPoints(){
        // Inicializamos SpanningTree
        List<Nodo> spanningTree = new List<Nodo>();
        spanningTree.Add(acceptedNodes[0]);

        // Inicializamos la pila de candidatos al SpanningTree
        Queue<Nodo> candidatesQueue = new Queue<Nodo>();
        foreach(Nodo c in acceptedNodes[0].getConexiones()){
            candidatesQueue.Enqueue(c);
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

        // Eliminamos los nodos aislados
        int i = 0;
        while(i < acceptedNodes.Count){
            Nodo currentNode = acceptedNodes[i];
            if(!spanningTree.Contains(currentNode)){
                while(currentNode.getConexiones().Count > 0){
                    Nodo currentConection = currentNode.getConexiones()[0];
                    currentNode.removeConexion(currentConection);
                    currentConection.removeConexion(currentNode);
                }

                acceptedNodes.Remove(currentNode);
            }else{
                i++;
            }
        }
    }

    private bool isRadialPRoad(Nodo n1, Nodo n2){
        if(topology != Topologia.Radial_Raster && topology != Topologia.Radial_Branching){
            return false;
        }else{
            int index1 = acceptedNodes.IndexOf(n1);
            int index2 = acceptedNodes.IndexOf(n2);
            int mainPoints = 1 + (numSegsCircle * numCircles) + (numPRBranches * numCircles * ((int)(circlesDistance / longPR) - 1));

            if(index1 <= mainPoints && index2 <= mainPoints){
                return true;
            }else{
                return false;
            }
        }
    }

    private void RemoveConectionsByAngle(){
        foreach(Nodo currentNode in acceptedNodes){
            if(currentNode.getConexiones().Count > 1){
                int i = 0;
                while(i < currentNode.getConexiones().Count && currentNode.getConexiones().Count > 1){
                    Nodo conexion = currentNode.getConexiones()[i];
                    moduloVectores.RigthAndLeftVectors(currentNode, conexion);
                    Vector3 vectorRef = conexion.getPosicion() - currentNode.getPosicion();

                    if(moduloVectores.vectoresI.Count == 0){
                        if(!isRadialPRoad(currentNode, conexion) || Vector3.Angle(vectorRef, moduloVectores.vectoresR[0]) >= minAngle){
                            i++;
                        }else{
                            currentNode.removeConexion(conexion);
                            conexion.removeConexion(currentNode);
                        }
                    }else if(moduloVectores.vectoresR.Count == 0){
                        if(!isRadialPRoad(currentNode, conexion) || Vector3.Angle(vectorRef, moduloVectores.vectoresI[0]) >= minAngle){
                            i++;
                        }else{
                            currentNode.removeConexion(conexion);
                            conexion.removeConexion(currentNode);
                        }
                    }else{
                        if(!isRadialPRoad(currentNode, conexion) || Vector3.Angle(vectorRef, moduloVectores.vectoresR[0]) >= minAngle && Vector3.Angle(vectorRef, moduloVectores.vectoresI[0]) >= minAngle){
                            i++;
                        }else{
                            currentNode.removeConexion(conexion);
                            conexion.removeConexion(currentNode);
                        }
                    }
                }
            }
        }
    }

    public float GenerateRandomProb(){
        System.Random random = new System.Random();
        double val = (random.NextDouble() * (1.0f - 0.0f) + 0.0f);
        return (float)val;
    }

    private float GenerateAngleRotation(){
        System.Random random = new System.Random();
        double val = (random.NextDouble() * (maxAnguloGiro - minAnguloGiro) + minAnguloGiro);
        return (float)val;
    }

    private void DeleteInvalidStreets(){ // Elimina generaciones de calles inválidas
        foreach (Nodo nodo in acceptedNodes)
        {
            int i = 0;
            while(i < nodo.getConexiones().Count){
                if(acceptedNodes.Contains(nodo.getConexiones()[i])){
                    i++;
                }else{
                    nodo.removeConexion(nodo.getConexiones()[i]);
                }
            }
        }
    }

    private void DeleteNodesLess2Streets(){
        int i = 0;
        while(i < acceptedNodes.Count){
            Nodo currentNode = acceptedNodes[i];

            if(currentNode.getConexiones().Count >= 2){
                i++;
            }else{
                if(currentNode.getConexiones().Count == 0){
                    acceptedNodes.Remove(currentNode);
                }else{
                    Nodo nodoRecursiva = currentNode.getConexiones()[0];

                    currentNode.removeConexion(nodoRecursiva);
                    nodoRecursiva.removeConexion(currentNode);

                    acceptedNodes.Remove(currentNode);

                    int nodosEliminados = EliminacionRecursiva(nodoRecursiva);

                    i = i - nodosEliminados;
                }
            }
        }
    }

    private int EliminacionRecursiva(Nodo node){
        if(node.getConexiones().Count >= 2){
            return 0;
        }else{
            if(node.getConexiones().Count == 0){
                acceptedNodes.Remove(node);
                return 1;
            }else{
                Nodo nodoRecursiva = node.getConexiones()[0];

                node.removeConexion(nodoRecursiva);
                nodoRecursiva.removeConexion(node);

                acceptedNodes.Remove(node);

                return (1 + EliminacionRecursiva(nodoRecursiva));
            }
        }
    }

    public void RenderLines(){
        RestartCityObjects();
        
        List<Nodo> acceptedNodesCopy = new List<Nodo>(acceptedNodes);
        GameObject nodos = new GameObject("Nodos");
        nodos.transform.parent = city.transform;

        int numNodo = 0;

        foreach (Nodo nodo in acceptedNodesCopy)
        {
            GameObject instanciaNodo = Instantiate(prefab, nodo.getPosicion(), Quaternion.identity, nodos.transform);
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

    public void RenderRoads(){ // En este función es donde asociamos las componentes NodoObj
        RestartCityObjects();

        List<NodoObj> createdNodes = new List<NodoObj>();

        List<Nodo> acceptedNodesCopy = new List<Nodo>(acceptedNodes);
        GameObject nodos = new GameObject("Nodos");
        nodos.transform.parent = city.transform;

        int numNodo = 0;

        foreach (Nodo nodo in acceptedNodesCopy)
        {
            int indexNode = -1;
            int j = 0;

            while(indexNode == -1 && j < createdNodes.Count){
                if(createdNodes[j].pos == nodo.getPosicion()){
                    indexNode = j;
                }

                j++;
            }

            NodoObj currentNObj;
            GameObject instanciaNodo;

            if(indexNode == -1){
                instanciaNodo = new GameObject("X");
                instanciaNodo.transform.parent = nodos.transform;
                instanciaNodo.transform.position = nodo.getPosicion();
                instanciaNodo.name = "Nodo " + numNodo;

                currentNObj = instanciaNodo.AddComponent<NodoObj>();
                currentNObj.pos = nodo.getPosicion();
                currentNObj.conex = new List<NodoObj>();

                createdNodes.Add(currentNObj);

                numNodo++;
            }else{
                currentNObj = createdNodes[indexNode];
                instanciaNodo = currentNObj.gameObject;
            }

            if(!moduloVectores.noMore180Degrees(nodo)){
                createVertexCircle(nodo, instanciaNodo);
            }

            GameObject conexiones = new GameObject("Conexiones");
            conexiones.transform.parent = instanciaNodo.transform;

            int numConexion = 0;

            foreach (Nodo conexion in nodo.getConexiones()){
                indexNode = -1;
                j = 0;

                while(indexNode == -1 && j < createdNodes.Count){
                    if(createdNodes[j].pos == conexion.getPosicion()){
                        indexNode = j;
                    }

                    j++;
                }

                NodoObj currentCNObj;

                if(indexNode == -1){
                    GameObject instanciaNodoC;
                    instanciaNodoC = new GameObject("X");
                    instanciaNodoC.transform.parent = nodos.transform;
                    instanciaNodoC.transform.position = conexion.getPosicion();
                    instanciaNodoC.name = "Nodo " + numNodo;

                    currentCNObj = instanciaNodoC.AddComponent<NodoObj>();
                    currentCNObj.pos = conexion.getPosicion();
                    currentCNObj.conex = new List<NodoObj>();
                    currentNObj.conex.Add(currentCNObj);

                    createdNodes.Add(currentCNObj);

                    numNodo++;
                }else{
                    currentCNObj = createdNodes[indexNode];
                    currentNObj.conex.Add(currentCNObj);
                }

                if(!nodo.isVisitadaConexion(conexion)){
                    numConexion++;
                    GameObject line = new GameObject("Linea Calle " + numConexion);
                    line.transform.parent = conexiones.transform;
                    line.transform.position = nodo.getPosicion();

                    TipoConexion conectionType = nodo.getTipoConexion(conexion);

                    if(conectionType == TipoConexion.Interna){
                        drawRoad(nodo, conexion, line, 0);
                    }else if(conectionType == TipoConexion.Externa){
                        drawRoad(nodo, conexion, line, 1);
                    }else{
                        drawRoad(nodo, conexion, line, 2);
                    }

                    nodo.setConexionVisitada(conexion);
                }
            }

            nodo.resetVisitados();
        }
    }

    private void drawRoad(Nodo nodo, Nodo conexion, GameObject conectionsObject, int tipoRoad){
        float maxTexture = 20;
        float copyZ = 7;
        float copyX = 7;
        Vector3 esquina = new Vector3(-maxTexture, 0, -maxTexture);

        GameObject road = new GameObject("Road");
        road.transform.parent = conectionsObject.transform;
        road.AddComponent<MeshRenderer>();

        road.GetComponent<MeshRenderer>().material = RoadMaterial;

        MeshFilter mf = road.AddComponent<MeshFilter>();
        Mesh mesh = new Mesh();
        mf.mesh = mesh;
                
        Vector3[] vertices = new Vector3[4];

        Vector3 dirRoad = conexion.getPosicion() - nodo.getPosicion();
        dirRoad = dirRoad / dirRoad.magnitude;
        dirRoad = dirRoad * anchuraCarretera;

        Vector3 vP = moduloVectores.getPerpendicularVector(conexion.getPosicion() - nodo.getPosicion());
        vP = vP / vP.magnitude;
        vP = vP * anchuraCarretera;

        Vector3 vQ = moduloVectores.getPerpendicularVector(conexion.getPosicion() - nodo.getPosicion());
        vQ = vQ / vQ.magnitude;
        vQ = vQ * anchuraCarretera;
                
        vertices[0] = nodo.getPosicion() + vP;
        vertices[1] = nodo.getPosicion() - vP;
        vertices[2] = conexion.getPosicion() + vQ;
        vertices[3] = conexion.getPosicion() - vQ;
                
        mesh.vertices = vertices;
                
        int[] tri = new int[6];

        tri[0] = 0;
        tri[1] = 2;
        tri[2] = 1;
                
        tri[3] = 2;
        tri[4] = 3;
        tri[5] = 1;
                
        mesh.triangles = tri;
                
        Vector3[] normals = new Vector3[4];
                
        normals[0] = -Vector3.up;
        normals[1] = -Vector3.up;
        normals[2] = -Vector3.up;
        normals[3] = -Vector3.up;
                
        mesh.normals = normals;

        Vector2[] uvs = new Vector2[4];

        for(int i = 0 ; i < 4 ; i++){
            Vector3 aux = vertices[i] - esquina;
            uvs[i] = new Vector2(aux.x * copyX / (2 * maxTexture), aux.z * copyZ / (2 * maxTexture));
        }

        mesh.uv = uvs;
    }

    private void createVertexCircle(Nodo vertexNode, GameObject vertexObject){
        float maxTexture = 20;
        float copyZ = 7;
        float copyX = 7;
        Vector3 esquina = new Vector3(-maxTexture, 0, -maxTexture);

        Vector3 P = vertexNode.getPosicion();
        Vector3 R = moduloVectores.getPCirclePoint(P, anchuraCarretera);
        Vector3 S = moduloVectores.getQCirclePoint(P, anchuraCarretera);
        int circleSegs = moduloVectores.calculateCurrentCircleSegmentation(R, S);

        GameObject road = new GameObject("RoadCircle");
        road.transform.parent = vertexObject.transform;
        road.AddComponent<MeshRenderer>();
        road.GetComponent<MeshRenderer>().material = RoadMaterial;

        MeshFilter mf = road.AddComponent<MeshFilter>();
        Mesh mesh = new Mesh();
        mf.mesh = mesh;

        Vector3[] vertices = new Vector3[circleSegs+3];
        vertices[0] = P; // C
        vertices[1] = R; // P
        vertices[2] = S; // Q
        
        Vector3 disPQ = S - R;
        float distPerSeg = disPQ.magnitude / circleSegs;
        Vector3 vPQ = disPQ / disPQ.magnitude;

        for(int j = 0 ; j < circleSegs ; j++){
            Vector3 Qi = R + vPQ * (distPerSeg * (j+1));
            Vector3 vCQi = Qi - P;
            vCQi = vCQi / vCQi.magnitude;
            Qi = P + vCQi * anchuraCarretera;
            vertices[j+3] = Qi;
        }

        mesh.vertices = vertices;

        int[] tri = new int[(circleSegs+1)*3];

        for(int i = 0 ; i <= circleSegs ; i++){
            int j = 3 * i;

            if(i == 0){
                tri[j] = 0;
                tri[j+1] = 1;
                tri[j+2] = (i+3);
            }
            else if(i == circleSegs){
                tri[j] = 0;
                tri[j+1] = (i+2);
                tri[j+2] = 2;
            }else{
                tri[j] = 0;
                tri[j+1] = (i+2);
                tri[j+2] = (i+3);
            }
        }
        
        mesh.triangles = tri;

        Vector3 vectorRef = R - P;
        Vector3 vectorTest = S - P;
        bool isRight = moduloVectores.isToTheRigth(vectorRef, vectorTest);

        if(!isRight){
            mesh.triangles = mesh.triangles.Reverse().ToArray();
        }

        Vector3[] normals = new Vector3[circleSegs+3];

        for(int i = 0 ; i < (circleSegs+3) ; i++){
            normals[i] = -Vector3.up;
        }
                
        mesh.normals = normals;

        Vector2[] uvs = new Vector2[circleSegs+3];

        for(int i = 0 ; i < (circleSegs+3) ; i++){
            Vector3 aux = vertices[i] - esquina;
            uvs[i] = new Vector2(aux.x * copyX / (2 * maxTexture), aux.z * copyZ / (2 * maxTexture));
        }

        mesh.uv = uvs;
    }

    // Llamadas al modulo CityCell

    public void DetectCityCells(){
        moduloCityCell.DetectCityCells();
    }

    public void DestroyCityCells(){
        if (Application.isEditor){
            DestroyImmediate(moduloCityCell.cityCellContainer);
        }else{
            Destroy(moduloCityCell.cityCellContainer);
        }
    }

    public void GenerateBuildingLots(){
        moduloCityCell.GenerarBuildingLots(HouseLength, HouseDepth);
    }

    public void DestroyBuildingLots(){
        GameObject[] lots = GameObject.FindGameObjectsWithTag("BuildingLot");

        foreach(GameObject lot in lots){
            if (Application.isEditor){
                DestroyImmediate(lot);
            }else{
                Destroy(lot);
            }
        }
    }

    // Llamadas al modulo Building

    public void GenerateBuildings(){
        checkPlantsProbs();
        checkBCProbs();
        moduloCityCell.cityCellContainer.SetActive(false);
        moduloEdificacion.EdificarCiudad(HouseHeight);
    }

    private void checkPlantsProbs(){
        bool goodConfiguration = true;
        float totalSum = 0f;

        int i = 0;
        while(goodConfiguration && i < arbolado.Count){
            if(arbolado[i].probabilidad < 0f || arbolado[i].probabilidad > 1f){
                goodConfiguration = false;
            }else{
                totalSum = totalSum + arbolado[i].probabilidad;

                if(totalSum > 1f){
                    goodConfiguration = false;
                }
            }

            i++;
        }

        if(goodConfiguration && totalSum < 1f){
            goodConfiguration = false;
        }

        if(!goodConfiguration){
            float equiProb = 1f / arbolado.Count;
            for(int j = 0 ; j < arbolado.Count ; j++){
                arbolado[j] = new Decoracion(equiProb, arbolado[j]);
            }
            
            Debug.Log("WARNING: Se han modificado las probabilidades de arbolado porque se ha encontado un error en ellas");
        }
    }

    private void checkBCProbs(){
        bool goodConfiguration = true;
        float totalSum = 0f;

        int i = 0;
        while(goodConfiguration && i < edificiosCaracteristicos.Count){
            if(edificiosCaracteristicos[i].probabilidad < 0f || edificiosCaracteristicos[i].probabilidad > 1f){
                goodConfiguration = false;
            }else{
                totalSum = totalSum + edificiosCaracteristicos[i].probabilidad;

                if(totalSum > 1f){
                    goodConfiguration = false;
                }
            }

            i++;
        }

        if(goodConfiguration && totalSum < 1f){
            goodConfiguration = false;
        }

        if(!goodConfiguration){
            float equiProb = 1f / edificiosCaracteristicos.Count;
            for(int j = 0 ; j < edificiosCaracteristicos.Count ; j++){
                edificiosCaracteristicos[j] = new Decoracion(equiProb, edificiosCaracteristicos[j]);
            }
            
            Debug.Log("WARNING: Se han modificado las probabilidades de edificios característicos porque se ha encontado un error en ellas");
        }
    }

    public void DestroyBuildings(){
        moduloCityCell.cityCellContainer.SetActive(true);

        if (Application.isEditor){
            DestroyImmediate(moduloEdificacion.buildingsContainer);
            DestroyImmediate(moduloEdificacion.plantsContainer);
        }else{
            Destroy(moduloEdificacion.buildingsContainer);
            Destroy(moduloEdificacion.plantsContainer);
        }
    }

    private GameObject instanciarCiudad(GameObject prefab, Vector3 pos){
        GameObject ob = Instantiate(prefab, pos, Quaternion.identity);

        return ob;
    }

    public GameObject instanciarPrefab(GameObject prefab, GameObject parent=null){ // Función para instanciar prefabs (solo lo pueden hacer las clases MonoBehaviour)
        GameObject ob = null;

        if(parent == null){
            ob = Instantiate(prefab, Vector3.zero, Quaternion.identity);
        }else{
            ob = Instantiate(prefab, Vector3.zero, Quaternion.identity, parent.transform);
        }

        ob.transform.localScale = new Vector3(1,1,1);

        return ob;
    }

    public GameObject instanciarPrefabConPos(GameObject prefab, Vector3 pos, GameObject parent=null){ // Función para instanciar prefabs (solo lo pueden hacer las clases MonoBehaviour)
        GameObject ob = null;

        if(parent == null){
            ob = Instantiate(prefab, pos, Quaternion.identity);
        }else{
            ob = Instantiate(prefab, pos, Quaternion.identity, parent.transform);
        }

        ob.transform.rotation = Quaternion.Euler(0, Random.Range(-180f, 180f), 0);

        return ob;
    }

    public GameObject instanciarPrefabConScale(GameObject prefab, Vector3 pos, Vector3 scale, GameObject parent=null){
        GameObject ob = null;

        if(parent == null){
            ob = Instantiate(prefab, pos, Quaternion.identity);
        }else{
            ob = Instantiate(prefab, pos, Quaternion.identity, parent.transform);
        }

        ob.transform.rotation = Quaternion.Euler(0, Random.Range(-180f, 180f), 0);

        if(scale != Vector3.zero){
            ob.transform.localScale = scale;
        }

        return ob;
    }

    // Establecer el Default Player System
    public void SetUpDeafultPlayerSys(){
        if(city.transform.Find("IA") != null){
            int LayerOfInterest = LayerMask.NameToLayer("Obstaculos");

            SetLayerRecursively(moduloEdificacion.buildingsContainer.transform, LayerOfInterest);
            SetLayerRecursively(moduloEdificacion.plantsContainer.transform, LayerOfInterest);
        }

        Vector3 posCentral = getPosCentral();

        GameObject player = instanciarPrefabConPos((GameObject)Resources.Load("Prefabs/Player", typeof(GameObject)), posCentral);
        player.name = "Player";

        Vector3 posCentralCam = posCentral + new Vector3(0f, 7f, 0f);
        GameObject camLibre = instanciarPrefabConPos((GameObject)Resources.Load("Prefabs/LibreCam", typeof(GameObject)), posCentralCam);
        camLibre.name = "CamaraLibre";
        CamaraLibre camLibreConfig = camLibre.GetComponent<CamaraLibre>();
        GameObject camPlayer = player.transform.GetChild(0).gameObject;
        camLibreConfig.playerCam = camPlayer.GetComponent<Camera>();
        camLibreConfig.player = player.GetComponent<Controlador2>();
    }

    private Vector3 getPosCentral(){
        Vector3 posMedia = acceptedNodes[0].getPosicion();

        for(int i = 1 ; i < acceptedNodes.Count ; i++){
            posMedia = posMedia + acceptedNodes[i].getPosicion();
        }

        posMedia = posMedia / acceptedNodes.Count;

        Vector3 mejorPos = acceptedNodes[0].getPosicion();

        for(int i = 1 ; i < acceptedNodes.Count ; i++){
            if(moduloVectores.calculateDistance(acceptedNodes[i].getPosicion(), posMedia) < moduloVectores.calculateDistance(mejorPos, posMedia)){
                mejorPos = acceptedNodes[i].getPosicion();
            }
        }

        return mejorPos;
    }

    private void SetLayerRecursively(Transform root, int layer)
    {
        Stack<Transform> children = new Stack<Transform>();
        children.Push(root);

        while (children.Count > 0) {
            Transform currentTransform = children.Pop();
            currentTransform.gameObject.layer = layer;

            foreach (Transform child in currentTransform){
                children.Push(child);
            }
        }
    }

    private void SetTagRecursively(Transform root, string tag)
    {
        Stack<Transform> children = new Stack<Transform>();
        children.Push(root);

        while (children.Count > 0) {
            Transform currentTransform = children.Pop();
            currentTransform.gameObject.tag = tag;

            foreach (Transform child in currentTransform){
                children.Push(child);
            }
        }
    }

    // Llamadas al modulo AI

    public void GenerateAI(){
        int LayerOfInterest = LayerMask.NameToLayer("Obstaculos");

        SetLayerRecursively(moduloEdificacion.buildingsContainer.transform, LayerOfInterest);
        SetLayerRecursively(moduloEdificacion.plantsContainer.transform, LayerOfInterest);

        string TagOfInterest = "Pared";

        SetTagRecursively(moduloEdificacion.buildingsContainer.transform, TagOfInterest);
        SetTagRecursively(moduloEdificacion.plantsContainer.transform, TagOfInterest);

        moduloIA.createVillagers();

        if(birds){
            moduloIA.createBirds();
        }
    }

    public void DestroyAI(){
        if (Application.isEditor){
            DestroyImmediate(moduloIA.AIContainer);
        }else{
            Destroy(moduloIA.AIContainer);
        }
    }

    public Nodo getNodoCentral(){
        Vector3 posCentral = getPosCentral();

        foreach(Nodo n in acceptedNodes){
            if(n.getPosicion() == posCentral){
                return n;
            }
        }

        return acceptedNodes[0];
    }
}
