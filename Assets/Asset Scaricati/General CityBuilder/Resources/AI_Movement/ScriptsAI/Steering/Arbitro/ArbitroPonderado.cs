using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArbitroPonderado : SteeringBehaviour
{
    [System.Serializable]
    struct comportamiento
    {
        public SteeringBehaviour steer;
        [Range(0f, 100f)]
        public float peso;
    }

    [SerializeField]
    List<comportamiento> pesos;

    void Awake(){
        SteeringBehaviour[] steeringsPrevios = GetComponents<SteeringBehaviour>();

        if(steeringsPrevios.Length == 0){
            pesos = new List<comportamiento>();
        }
    }

    void Start()
    {
        this.nameSteering = "ArbitroPonderado";
    }

    public override Steering GetSteering(Agent agent)
    {
        Steering steer = new Steering();
        steer.linear = new Vector3(0,0,0);
        steer.angular = 0;
        foreach (comportamiento peso in pesos)
        {
            steer.linear += peso.steer.GetSteering(agent).linear * peso.peso;
            steer.angular += peso.steer.GetSteering(agent).angular * peso.peso;
        }

        return steer;
    }

    public void iniciarArbitroPonderado(List<SteeringBehaviour> steerings, List<float> pesosSteerings, List<SteeringBehaviour> steeringsPermanentesPausados=null){
        pesos = new List<comportamiento>();

        for(int i = 0 ; i < steerings.Count ; i++){
            comportamiento currentComp;
            currentComp.steer = steerings[i];
            currentComp.peso = pesosSteerings[i];

            pesos.Add(currentComp);
        }

        if(steeringsPermanentesPausados != null){
            foreach(SteeringBehaviour s in steeringsPermanentesPausados){
                int i = 0;
                bool eliminado = false;

                while(!eliminado && i < pesos.Count){
                    if(pesos[i].steer == s){
                        pesos.RemoveAt(i);
                        eliminado = true;
                    }else{
                        i++;
                    }
                }
            }
        }
    }

    public void addSteering(SteeringBehaviour steer, float peso){
        comportamiento currentComp;
        currentComp.steer = steer;
        currentComp.peso = peso;

        pesos.Add(currentComp);
    }

    public void setPesos(int indice, float nuevoPeso)
    {
        comportamiento aux = pesos[indice];
        aux.peso = nuevoPeso;
        pesos[indice] = aux;
    }

    public void limpiarPesos()
    {
        pesos.Clear();
    }
}
