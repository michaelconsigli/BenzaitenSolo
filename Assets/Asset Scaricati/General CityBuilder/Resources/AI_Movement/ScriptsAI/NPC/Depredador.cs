using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Depredador : AgentNPC
{
    private float timer = 0f;

    protected override void Awake()
    {
        this.steer = new Steering();

        arbitro = gameObject.AddComponent<ArbitroPonderado>() as ArbitroPonderado;
    }

    public override void Start()
    {
        WanderFormation();

        MaxSpeed = 1.25f;
        MaxSpeedDefault = 1.25f;
        anchura = 3f;
        distanciaDeVision = 30f;
        MaxAcceleration = 5f;
        MaxAccelerationDefault = MaxAcceleration;
        MaxAngularAcc = 90f;
        anguloVision = 100f;

        base.Start();
    }

    public override void Update()
    {
        if(timer == 0f){
            Seek seekSteering = GetComponent<Seek>();

            if(Vector3.Distance(Position, seekSteering.target.Position) > 0.3f){
                // Actualizamos el punto de Seek
                GameObject[] peces = GameObject.FindGameObjectsWithTag("Pajaro");
                Vector3 centroMasas = Vector3.zero;

                foreach(GameObject p in peces){
                    centroMasas += p.GetComponent<Agent>().Position;
                }

                centroMasas = centroMasas / peces.Length;

                seekSteering.target.Position = centroMasas;
            }else{
                ArbitroPonderado arbitro = GetComponent<ArbitroPonderado>();
                arbitro.setPesos(0, 5);
                arbitro.setPesos(1, 0);
                timer += Time.deltaTime;
            }
        }else{
            if(timer >= 6f){
                timer = 0f;
                ArbitroPonderado arbitro = GetComponent<ArbitroPonderado>();
                arbitro.setPesos(0, 0);
                arbitro.setPesos(1, 5);
            }else{
                timer += Time.deltaTime;
            }
        }

        // En cada frame se actualiza el movimiento
        ApplySteering(Time.deltaTime);
    }

    public void WanderFormation()
    {
        List<SteeringBehaviour> steerings = new List<SteeringBehaviour>();
        List<float> pesos = new List<float>();

        Wander wanderSteering = gameObject.AddComponent<Wander>() as Wander;
        wanderSteering.wanderRate = 0f;
        steerings.Add(wanderSteering);
        LookWhereYouGoing lookWhereYouGoingSteering = gameObject.AddComponent<LookWhereYouGoing>() as LookWhereYouGoing;
        steerings.Add(lookWhereYouGoingSteering);

        pesos.Add(0f);
        pesos.Add(5.0f);

        GameObject[] peces = GameObject.FindGameObjectsWithTag("Pajaro");
        Vector3 centroMasas = Vector3.zero;

        foreach(GameObject p in peces){
            centroMasas += p.transform.position;
        }

        centroMasas = centroMasas / peces.Length;

        Seek seekSteering = gameObject.AddComponent<Seek>() as Seek;
        Agent centroMasasAgent = Agent.createBasicNewVirtualAgent(centroMasas, 0f);
        centroMasasAgent.alturaEnEscena = alturaEnEscena;
        seekSteering.target = centroMasasAgent;
        steerings.Add(seekSteering);

        pesos.Add(5f);

        arbitro.iniciarArbitroPonderado(steerings, pesos);
    }
}
