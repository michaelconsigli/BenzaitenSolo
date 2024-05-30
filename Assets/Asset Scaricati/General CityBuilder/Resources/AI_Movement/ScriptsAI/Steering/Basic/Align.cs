using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Align : SteeringBehaviour
{

    // Declara las variables que necesites para este SteeringBehaviour
    [SerializeField]
    public Agent target;

    [Range(0.01f, 60f)]
    [SerializeField]
    float TimeToTarget = 1.0f;
    
    void Start()
    {
        this.nameSteering = "Align";
    }

    public override Steering GetSteering(Agent agent)
    {
        Steering steer = new Steering();
        steer.linear = new Vector3(0,0,0);

        // Calcula el steering.
        float newRotation = target.Orientation - agent.Orientation;

        if(newRotation > 180){
            newRotation = newRotation - 360;
        }else if(newRotation < -180){
            newRotation = newRotation + 360;
        }

        float rotationSize = Mathf.Abs(newRotation);

        if(rotationSize < target.interiorAngle){
            steer.angular = 0;
            return steer;
        }

        float targetRotation;

        if(rotationSize > target.exteriorAngle){
            targetRotation = agent.MaxRotation;
        }else{
            targetRotation = agent.MaxRotation * rotationSize / target.exteriorAngle;
        }

        targetRotation = targetRotation * newRotation / rotationSize;

        float accAngular = (targetRotation - agent.Rotation)/ TimeToTarget;

        float angularAcceleration = Mathf.Abs(accAngular);

        if(angularAcceleration > agent.MaxAngularAcc){
            accAngular /= angularAcceleration;
            accAngular *= agent.MaxAngularAcc;
        }
        steer.angular = accAngular;

        // Retornamos el resultado final.
        return steer;
    }

    public override void destroyVirtualAgent()
    {
        if (target.isVirtual())
            Destroy(target.gameObject);
    }
}