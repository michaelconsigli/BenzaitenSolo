using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Face : Align
{
    [SerializeField]
    public Agent targetReal;

    Agent agenteVirtual;

    void Start()
    {
        Inicio();
    }

    public void Inicio()
    {
        this.nameSteering = "Face";
        agenteVirtual = Agent.createBasicNewVirtualAgent(new Vector3(0,0,0), 0);
        target = agenteVirtual;
    }

    public override Steering GetSteering(Agent agent)
    {
        Vector3 direction = targetReal.Position - agent.Position;

        if (direction.magnitude > 0.001f)
        {
            agenteVirtual.Orientation = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
        }

        agenteVirtual.exteriorAngle = targetReal.exteriorAngle;
        agenteVirtual.interiorAngle = targetReal.interiorAngle;

        return base.GetSteering(agent);
    }

    public override void destroyVirtualAgent()
    {
        Destroy(agenteVirtual.gameObject);
    }
}
