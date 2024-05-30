using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class PathFollowing : Seek
{
    [SerializeField]
    public Path camino;
    //[HideInInspector]
    public bool vigilancia;
    //[HideInInspector]
    public int targetActual;
    Agent agenteVirtual;
    void Start()
    {
        this.nameSteering = "PathFollowing";
        agenteVirtual = Agent.createBasicNewVirtualAgent(new Vector3(0,0,0), 0);
        agenteVirtual.alturaEnEscena = GetComponent<Agent>().alturaEnEscena;
        target = agenteVirtual;
        agenteVirtual.interiorRadius = GetComponent<Agent>().anchura * 0.75f;
    }
    public override Steering GetSteering(Agent agent)
    {
        agenteVirtual.alturaEnEscena = agent.alturaEnEscena;
        
        if(vigilancia) targetActual = camino.getParamVigilancia(agent.Position, targetActual);
        else targetActual = camino.getParam(agent.Position, targetActual);
        agenteVirtual.Position = camino.getPosition(targetActual);
        float radio = camino.getRadioActual(targetActual);
        agenteVirtual.arrivalRadius = radio;

        return base.GetSteering(agent);
    }

    public override void destroyVirtualAgent()
    {
        Destroy(agenteVirtual.gameObject);
        Destroy(GetComponent<Path>());
    }
}