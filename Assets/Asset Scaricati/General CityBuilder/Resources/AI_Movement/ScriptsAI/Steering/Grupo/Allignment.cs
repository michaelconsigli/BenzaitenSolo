using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Allignment : SteeringBehaviour
{
    public List<Agent> listaAgentes;

    [SerializeField]
    public float threshold;

    void Start()
    {
        foreach(GameObject objeto in GameObject.FindGameObjectsWithTag("Pajaro"))
        {
            if(objeto != this.gameObject) listaAgentes.Add(objeto.GetComponent<Agent>());
        }
    }

    public override Steering GetSteering(Agent agent)
    {
        Steering steer = new Steering();
        steer.linear = new Vector3(0,0,0);
        steer.angular = 0;

        int contador = 0;
        foreach(Agent target in listaAgentes)
        {
            Vector3 direccion = target.Position - agent.Position;
            float distancia = direccion.magnitude;

            if (distancia < threshold)
            {
                steer.angular += target.Orientation;
                contador++;
            }
        }

        if (contador>0){
            steer.angular /= contador;
            steer.angular -= agent.Orientation;
        } 
        return steer;
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
