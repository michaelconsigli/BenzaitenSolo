using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flee : SteeringBehaviour
{

    // Declara las variables que necesites para este SteeringBehaviour
    [SerializeField]
    public Agent target;
    
    [SerializeField]
    public float distanciaMin = 30f;

    void Start()
    {
        this.nameSteering = "Flee";
    }

    public override Steering GetSteering(Agent agent)
    {
        Steering steer = new Steering();
        Vector3 direccion = agent.Position - target.Position;
        if(direccion.magnitude < distanciaMin)
        {
            direccion /= direccion.magnitude;
            steer.linear = direccion * agent.MaxAcceleration;
        }
        else steer.linear = new Vector3(0,0,0);
        steer.angular = 0;
        return steer;
    }

    public override void destroyVirtualAgent()
    {
        if (target.isVirtual())
            Destroy(target.gameObject);
    }

    void OnDrawGizmos()
    {
        if(VisualDebug._debug && target!=null)
        {
            Gizmos.color = new Color(0.54f,0.27f,0.1f,1f);
            Gizmos.DrawLine(GetComponent<Agent>().Position, target.Position);
            Gizmos.DrawWireSphere(GetComponent<Agent>().Position, distanciaMin);
        }
    }
}