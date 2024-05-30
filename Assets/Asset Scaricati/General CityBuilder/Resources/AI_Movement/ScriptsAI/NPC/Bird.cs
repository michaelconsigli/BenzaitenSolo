using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bird : AgentNPC
{
    public Path grafoAves;

    private PathFollowing pathFollowingSteering;

    public override void Start()
    {
        List<SteeringBehaviour> steerings = new List<SteeringBehaviour>();

        pathFollowingSteering = gameObject.AddComponent<PathFollowing>() as PathFollowing;
        pathFollowingSteering.camino = grafoAves;
        pathFollowingSteering.targetActual = 0;
        pathFollowingSteering.vigilancia = true;
        steerings.Add(pathFollowingSteering);

        Wander wanderSteering = gameObject.AddComponent<Wander>() as Wander;
        wanderSteering.wanderRate = 25f;
        steerings.Add(wanderSteering);

        GameObject[] pajaros = GameObject.FindGameObjectsWithTag("Pajaro");
        List<Agent> pajaroAgents = new List<Agent>();

        foreach(GameObject p in pajaros){
            pajaroAgents.Add(p.GetComponent<Agent>());
        }

        pajaroAgents.Remove(GetComponent<Agent>());

        Separation separationSteering = gameObject.AddComponent<Separation>() as Separation;
        separationSteering.listaAgentes = pajaroAgents;
        separationSteering.threshold = 10f;
        separationSteering.coeficienteDecay = 0.001f;
        steerings.Add(separationSteering);
        Cohesion cohesionSteering = gameObject.AddComponent<Cohesion>() as Cohesion;
        cohesionSteering.listaAgentes = pajaroAgents;
        cohesionSteering.threshold = 20f;
        steerings.Add(cohesionSteering);
        Allignment alligmentSteering = gameObject.AddComponent<Allignment>() as Allignment;
        alligmentSteering.listaAgentes = pajaroAgents;
        alligmentSteering.threshold = 20f;
        steerings.Add(alligmentSteering);

        LookWhereYouGoing lookSteering = gameObject.AddComponent<LookWhereYouGoing>() as LookWhereYouGoing;
        steerings.Add(lookSteering);

        GameObject[] depredador = GameObject.FindGameObjectsWithTag("Depredador");
        Flee fleeSteering = gameObject.AddComponent<Flee>() as Flee;
        fleeSteering.target = depredador[0].GetComponent<Agent>();
        fleeSteering.distanciaMin = 0.4f;
        steerings.Add(fleeSteering);

        List<float> pesosSteerings = new List<float>();
        pesosSteerings.Add(10f);

        pesosSteerings.Add(5f);

        pesosSteerings.Add(15f);
        pesosSteerings.Add(10f);
        pesosSteerings.Add(7f);

        pesosSteerings.Add(20f);

        pesosSteerings.Add(4f);

        arbitro.iniciarArbitroPonderado(steerings, pesosSteerings);

        // Parametrización del agente
        MaxSpeed = 0.8f;
        MaxSpeedDefault = 0.8f;
        MaxRotation = 270f;
        MaxAcceleration = 2.5f;
        MaxAccelerationDefault = 2.5f;
        MaxAngularAcc = 360f;
        anchura = 0.05f;
        interiorRadius = 0.5f;
        factorDesaceleracion = 0.8f;

        base.Start();
    }

    public override void Update()
    {
        // En cada frame se actualiza el movimiento
        ApplySteering(Time.deltaTime);
    }
}
