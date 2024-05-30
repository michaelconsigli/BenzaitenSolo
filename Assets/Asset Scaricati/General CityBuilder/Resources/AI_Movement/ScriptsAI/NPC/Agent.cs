using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Agent : Bodi
{
    [SerializeField] public float distanciaDeVision = 21f;
    //[SerializeField] protected float distanciaDeDeteccion;
    [SerializeField] public float anguloVision = 120f;
    [SerializeField] public int numBigotesWallAvoidance = 3;
    [Tooltip("Radio interior de la IA")]
    [SerializeField] protected float _interiorRadius = 1f;

    [Tooltip("Radio de llegada de la IA")]
    [SerializeField] protected float _arrivalRadius = 3f;

    [Tooltip("Ángulo interior de la IA")]
    [SerializeField] protected float _interiorAngle = 3f; // ángulo sexagesimal -> Grados (0, 360)

    [Tooltip("Ángulo exterior de la IA")]
    [SerializeField] protected float _exteriorAngle = 20f; // ángulo sexagesimal.

    [SerializeField]
    private bool esVirtual = false;

    public virtual void Update()
    {
        if (_interiorRadius > _arrivalRadius) _interiorRadius = _arrivalRadius;
        if (_interiorAngle > _exteriorAngle) _interiorAngle = _exteriorAngle;
    }

    /*public virtual void OnDrawGizmos()
    {
        if(VisualDebug._debug)
        {
            if(!esVirtual || (esVirtual && VisualDebug.agentesVirtuales))
            {
                Gizmos.DrawLine(Position, Position + Velocity);
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(Position, _arrivalRadius);
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(Position, _interiorRadius);
                if(!esVirtual)
                {
                    Gizmos.color = new Color(1,0.7f,0.8f,1);
                    Gizmos.DrawWireSphere(Position, distanciaDeVision);
                }
                Gizmos.color = Color.magenta;
                Gizmos.DrawLine(Position, Bodi.OrientationToVector(-_interiorAngle+Heading())*3f + Position);
                Gizmos.DrawLine(Position, Bodi.OrientationToVector(_interiorAngle+Heading())*3f + Position);

                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(Position, Bodi.OrientationToVector(-_exteriorAngle+Heading())*3f + Position);
                Gizmos.DrawLine(Position, Bodi.OrientationToVector(_exteriorAngle+Heading())*3f + Position);
            }
        }
    }*/

    public float arrivalRadius
    {
        get { return _arrivalRadius; }
        set { _arrivalRadius = value; }
    }
    public float interiorRadius
    {
        get { return _interiorRadius; }
        set { _interiorRadius = value; }
    }
    public float exteriorAngle
    {
        get { return _exteriorAngle; }
        set { _exteriorAngle = value; }
    }
    public float interiorAngle
    {
        get { return _interiorAngle; }
        set { _interiorAngle = value; }
    }

    /*public void changeDebugging(){
        isDebugging = !isDebugging;
    }

    public void changeRendererState(){
        this.GetComponent<MeshRenderer>().enabled = !this.GetComponent<MeshRenderer>().enabled;
    }*/

    public bool isVirtual()
    {
        return esVirtual;
    }

    public void setVirtual(bool isVirtual)
    {
        esVirtual = isVirtual;
    }

    public static Agent createBasicNewVirtualAgent(Vector3 posicion, float orientacion){
        GameObject newVA = Instantiate((GameObject)Resources.Load("Prefabs/VirtualNPC", typeof(GameObject)));

        newVA.AddComponent<BoxCollider>();
        newVA.GetComponent<Collider>().isTrigger = true;
        newVA.tag = "AgenteVirtual";
        newVA.GetComponent<MeshRenderer>().enabled = VisualDebug.agentesVirtuales;
        Agent VA = newVA.AddComponent<Agent>();
        VA.esVirtual = true;

        VA.Position = posicion;
        VA.Orientation = orientacion;

        return VA;
    }
}
