using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookWhereYouGoing : Align
{
    Agent agenteVirtual;

    void Start()
    {
        this.nameSteering = "LookWhereYouGoing";
        agenteVirtual = Agent.createBasicNewVirtualAgent(new Vector3(0,0,0), 0);
        target = agenteVirtual;
    }

    public override Steering GetSteering(Agent agent)
    {
        if (agent.Speed > 0.001f) agenteVirtual.Orientation = Mathf.Atan2(agent.Velocity.x, agent.Velocity.z) * Mathf.Rad2Deg;
        
        agenteVirtual.exteriorAngle = agent.exteriorAngle;
        agenteVirtual.interiorAngle = agent.interiorAngle;

        return base.GetSteering(agent);
    }

    public override void destroyVirtualAgent()
    {
        Destroy(agenteVirtual.gameObject);
    }
}
