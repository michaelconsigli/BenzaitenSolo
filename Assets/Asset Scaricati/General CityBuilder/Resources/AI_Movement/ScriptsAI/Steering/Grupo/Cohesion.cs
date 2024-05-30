using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cohesion : Seek
{
    public List<Agent> listaAgentes;

    [SerializeField]
    public float threshold;

    Agent agenteVirtual;

    void Start()
    {
        foreach(GameObject objeto in GameObject.FindGameObjectsWithTag("Pajaro"))
        {
            if(objeto != this.gameObject) listaAgentes.Add(objeto.GetComponent<Agent>());
        }
        agenteVirtual = Agent.createBasicNewVirtualAgent(new Vector3(0,0,0), 0);
        agenteVirtual.interiorRadius = 0.001f;
        target = agenteVirtual;
    }

    public override Steering GetSteering(Agent agent)
    {
        agenteVirtual.alturaEnEscena = agent.alturaEnEscena;
        Vector3 centroDeMasas = new Vector3(0,0,0);

        int contador = 0;
        foreach(Agent target in listaAgentes)
        {
            Vector3 direccion = target.Position - agent.Position;
            float distancia = direccion.magnitude;

            if (distancia < threshold)
            {
                centroDeMasas += target.Position;
                contador++;
            }
        }

        if (contador>0){
            agenteVirtual.Position = centroDeMasas/contador;
            return base.GetSteering(agent);
        } 

        Steering steer = new Steering();
        steer.linear = new Vector3(0,0,0);
        steer.angular = 0;

        return steer;
    }

    public override void destroyVirtualAgent()
    {
        Destroy(agenteVirtual.gameObject);
    }

    /*void OnDrawGizmos()
    {
        if(VisualDebug._debug)
        {
            Gizmos.color = new Color(0,1f,0,0.4f);
            Gizmos.DrawWireSphere(GetComponent<Agent>().Position, threshold);
            
            Gizmos.color = new Color(0,7f,0.7f,0.7f);
            foreach(Agent target in listaAgentes)
            {
                Gizmos.DrawLine(GetComponent<Agent>().Position, target.Position);
            }
        }
    }*/
}
