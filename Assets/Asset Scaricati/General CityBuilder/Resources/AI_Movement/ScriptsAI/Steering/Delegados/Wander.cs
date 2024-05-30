using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wander : Face
{
    [SerializeField]
    float wanderOffset=6;

    [SerializeField]
    float wanderRadius=4;

    [SerializeField]
    public float wanderRate=45;

    private Vector3 CentroCirculo;
    private bool ocupado;
    Agent agenteVirtualWander;

    void Start()
    {
        this.nameSteering = "Wander";
        agenteVirtualWander = Agent.createBasicNewVirtualAgent(new Vector3(0,0,0), 0);
        targetReal = agenteVirtualWander;
        base.Inicio();
    }

    public override Steering GetSteering(Agent agent)
    {
        agenteVirtualWander.alturaEnEscena = agent.alturaEnEscena;

        if(!ocupado)
        {
            agenteVirtualWander.Orientation = Random.Range(-1f, 1f) * wanderRate + agent.Orientation;
            StartCoroutine("espera");
        }
        

        agenteVirtualWander.Position = agent.Position + wanderOffset * agent.OrientationToVector();
        CentroCirculo = agenteVirtualWander.Position;

        agenteVirtualWander.Position += wanderRadius * agenteVirtualWander.OrientationToVector();

        Steering steer = base.GetSteering(agent);

        steer.linear = agent.MaxAcceleration * agent.OrientationToVector();

        return steer;
    }

    /*void OnDrawGizmos()
    {
        if(VisualDebug._debug && target!=null)
        {
            Gizmos.color = Color.black;
            Gizmos.DrawWireSphere(CentroCirculo, wanderRadius);
        }
    }*/

    IEnumerator espera()
    {
        ocupado = true;
        yield return new WaitForSeconds(0.5f);
        ocupado = false;
    }

    public override void destroyVirtualAgent()
    {
        base.destroyVirtualAgent();
        Destroy(agenteVirtualWander.gameObject);
    }
}
