using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleVillager : AgentNPC
{

    public Animator animator;
    public GameObject grafoCiudad;
    public int maxNodosRecorridos = 2;
    public float maxWaitTime = 10f;

    private int numNodosRecorridos = 0;
    private float timer = 0f;
    private Seek seekSteering;

    public override void Start()
    {
        int index = (int)Random.Range(0f, nodoActual.conex.Count);
        nodoObjetivo = nodoActual.conex[index];
        numNodosRecorridos = 0;
        timer = 0f;

        List<SteeringBehaviour> steerings = new List<SteeringBehaviour>();

        WallAvoidance wallSteering = gameObject.AddComponent<WallAvoidance>() as WallAvoidance;
        steerings.Add(wallSteering);
        CollisionAvoidance collisionSteering = gameObject.AddComponent<CollisionAvoidance>() as CollisionAvoidance;
        steerings.Add(collisionSteering);
        seekSteering = gameObject.AddComponent<Seek>() as Seek;
        seekSteering.target = createBasicNewVirtualAgent(nodoObjetivo.pos, 0);
        seekSteering.target.alturaEnEscena = alturaEnEscena;
        seekSteering.target.Position = nodoObjetivo.pos;
        steerings.Add(seekSteering);
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

            if(vectorModule.calculateDistance(Position, seekSteering.target.Position) < interiorRadius){
                numNodosRecorridos++;

                if(numNodosRecorridos >= maxNodosRecorridos){
                    numNodosRecorridos = -1;
                    timer += Time.deltaTime;
                }else{
                    NodoObj nodoPrevio = nodoActual;
                    nodoActual = nodoObjetivo;

                    int index = (int)Random.Range(0f, nodoActual.conex.Count);

                    if(nodoActual.conex[index].pos == nodoPrevio.pos){
                        if(index < (nodoActual.conex.Count-1)){
                            index++;
                        }else{
                            index--;
                        }
                    }

                    nodoObjetivo = nodoActual.conex[index];
                    seekSteering.target.Position = nodoObjetivo.pos;
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
}
