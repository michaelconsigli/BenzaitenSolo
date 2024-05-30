using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrive : SteeringBehaviour
{

    // Declara las variables que necesites para este SteeringBehaviour
    [SerializeField]
    public Agent target;

    [Range(0.01f, 60f)]
    [SerializeField]
    float tiempo = 1f;

    
    void Start()
    {
        this.nameSteering = "Arrive";
    }


    public override Steering GetSteering(Agent agent)
    {
        Steering steer = new Steering();
        Vector3 direccion = target.Position - agent.Position;
        Vector3 linear;
        if (direccion.magnitude > target.arrivalRadius)
        {
            direccion /= direccion.magnitude;
            direccion *= agent.MaxSpeed;
            direccion = direccion - agent.Velocity;
            direccion /= tiempo;
            if(direccion.magnitude > target.MaxAcceleration)
            {
                direccion /= direccion.magnitude;
                direccion *= agent.MaxAcceleration;
            }
            linear = direccion;
        }
        else if (direccion.magnitude > target.interiorRadius && direccion.magnitude < target.arrivalRadius)
        {
            direccion *= (agent.MaxSpeed / target.arrivalRadius); // longitud

            direccion = direccion - agent.Velocity;
            direccion /= tiempo;
            if(direccion.magnitude > agent.MaxAcceleration)
            {
                direccion /= direccion.magnitude;
                direccion *= agent.MaxAcceleration;
            }
            linear = direccion;
        }
        else
        {
            linear = new Vector3(0,0,0);
        }
        steer.linear = linear;
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