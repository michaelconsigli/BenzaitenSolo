using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Seek : SteeringBehaviour
{

    // Declara las variables que necesites para este SteeringBehaviour
    [SerializeField]
    public Agent target;

    
    void Start()
    {
        this.nameSteering = "Seek";
    }

    public override Steering GetSteering(Agent agent)
    {
        Steering steer = new Steering();
        Vector3 direccion = target.Position - agent.Position;

        if(Vector3.Distance(target.Position, agent.Position) <= 0.1f){
            steer.linear = Vector3.zero;
        }else{
            direccion /= direccion.magnitude;
            direccion *= agent.MaxAcceleration;
            steer.linear = direccion;
        }

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
            Gizmos.color = new Color(0,1f,0,0.4f);
            Gizmos.DrawLine(GetComponent<Agent>().Position, target.Position);
        }
    }
}