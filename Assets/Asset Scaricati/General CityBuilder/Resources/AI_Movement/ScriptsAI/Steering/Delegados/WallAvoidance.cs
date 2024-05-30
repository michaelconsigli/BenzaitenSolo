using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallAvoidance : Seek
{
    [Range(1,11)]
    [SerializeField]
    int numBigotes = 3;

    [Range(10,180)]
    [SerializeField]
    float anguloVision = 80f;

    [SerializeField]
    float minDistanciaChoque=3f;

    [SerializeField]
    float distanciaGrande=6f;

    [SerializeField]
    float distanciaCorta=2.8f;

    [SerializeField]
    public bool choque;

    public List<Vector3> bigotes = new List<Vector3>();

    Agent agenteVirtual;

    void Start()
    {
        this.nameSteering = "WallAvoidance";
        agenteVirtual = Agent.createBasicNewVirtualAgent(new Vector3(0,0,0), 0);
        agenteVirtual.alturaEnEscena = GetComponent<Agent>().alturaEnEscena;
        agenteVirtual.Position = new Vector3(0,0,0);
        anguloVision = GetComponent<Agent>().anguloVision;
        numBigotes = GetComponent<Agent>().numBigotesWallAvoidance;
        target = agenteVirtual;
        minDistanciaChoque = GetComponent<Agent>().anchura * 3f;
        distanciaGrande = GetComponent<Agent>().anchura * 6f;
        distanciaCorta = GetComponent<Agent>().anchura * 2.8f;
    }

    public override Steering GetSteering(Agent agent)
    {
        Steering steer = new Steering();

        bigotes = new List<Vector3>();
        Vector3 initDireccion = agent.Velocity;
        initDireccion /=  initDireccion.magnitude;

        if(numBigotes % 2 == 0){
            float distanceAngle = (anguloVision / 2) / ((numBigotes - 1) / 2);

            bigotes.Add(Quaternion.Euler(0, distanceAngle/2, 0) * initDireccion * distanciaGrande);
            bigotes.Add(Quaternion.Euler(0, -distanceAngle/2, 0) * initDireccion * distanciaGrande);   

            for(int i = 1 ; i <= (numBigotes - 2)/2 ; i += 1){
                bigotes.Add(Quaternion.Euler(0, distanceAngle/2 + distanceAngle * i, 0) * initDireccion * distanciaCorta);
                bigotes.Add(Quaternion.Euler(0, -distanceAngle/2 - distanceAngle * i, 0) * initDireccion * distanciaCorta);
            }
        }else{
            bigotes.Add(initDireccion*distanciaGrande);

            float distanceAngle = (anguloVision / 2) / ((numBigotes - 1) / 2);

            for(int i = 1 ; i <= (numBigotes - 1)/2 ; i += 1){
                bigotes.Add(Quaternion.Euler(0, distanceAngle * i, 0) * initDireccion * distanciaCorta);
                bigotes.Add(Quaternion.Euler(0, -distanceAngle * i, 0) * initDireccion * distanciaCorta);
            }
        }

        choque = false;

        float distanciaMasCorta = Mathf.Infinity;
        Vector3 posicionAgenteVirtual = new Vector3(0,0,0);

        foreach (Vector3 bigote in bigotes)
        {
            int layerPared = 1 << LayerMask.NameToLayer("Obstaculos");
            RaycastHit hit;
            if(Physics.Raycast(agent.Position, bigote / bigote.magnitude, out hit, bigote.magnitude, layerPared))
            {
                string str = hit.transform.gameObject.tag;
                
                if (str.Equals("Pared")){
                    agenteVirtual.Position = hit.point + hit.normal * minDistanciaChoque;
                    if (hit.distance < distanciaMasCorta)
                    {
                        posicionAgenteVirtual = hit.point + hit.normal * minDistanciaChoque;
                        distanciaMasCorta = hit.distance;
                    }
                    choque = true;
                }
            }
        }

        if (choque)
        {
            agenteVirtual.Position = posicionAgenteVirtual;
            agenteVirtual.arrivalRadius = agent.arrivalRadius;
            agenteVirtual.interiorRadius = agent.interiorRadius;
            return base.GetSteering(agent);
        }
        else
        {
            steer.linear = new Vector3(0,0,0);
            steer.angular = 0;
        }
        return steer;
    }

    public override void destroyVirtualAgent()
    {
        Destroy(agenteVirtual.gameObject);
    }

    void OnDrawGizmos()
    {
        if(VisualDebug._debug)
        {
            Gizmos.color = Color.red;
            foreach(Vector3 bigote in bigotes){
                Gizmos.DrawLine(GetComponent<Agent>().Position, GetComponent<Agent>().Position + bigote);
            }
        }
    }
}