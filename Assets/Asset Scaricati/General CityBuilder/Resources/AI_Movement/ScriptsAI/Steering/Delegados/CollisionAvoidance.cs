using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionAvoidance : Seek
{
    float minBoxLength;
    Agent agenteVirtual;

    void Start()
    {
        this.nameSteering = "CollisionAvoidance";
        agenteVirtual = Agent.createBasicNewVirtualAgent(new Vector3(0,0,0), 0);
        agenteVirtual.alturaEnEscena = GetComponent<Agent>().alturaEnEscena;
        agenteVirtual.Position = new Vector3(0,0,0);
        target = agenteVirtual;
        minBoxLength = GetComponent<Agent>().anchura * 3;
    }

    public override Steering GetSteering(Agent agent)
    {
        Steering steer = new Steering();

        if(agent.Speed > 0f){
            float minIntersection = Mathf.Infinity;

            Agent closestAgent = null;
            Vector3 localPosClosestAgent = Vector3.forward;

            float boxLength = minBoxLength + (agent.Speed / agent.MaxSpeed) * minBoxLength;

            GameObject[] NPCs = GameObject.FindGameObjectsWithTag("NPC");
            GameObject[] Players = GameObject.FindGameObjectsWithTag("Player");
            List<Agent> obstaculosParaAnalizar = new List<Agent>();

            foreach(GameObject n in NPCs){
                obstaculosParaAnalizar.Add(n.GetComponent<Agent>());
            }

            obstaculosParaAnalizar.Remove(agent); // Quitamos el propio NPC

            filtrarObstaculos(obstaculosParaAnalizar, agent, boxLength); // Eliminamos los que están por delante y por detrás del Box

            float rotacionT = getRotacionLocal(agent);

            if(obstaculosParaAnalizar.Count > 0){
                float rotacion = getRotacionLocal(agent);

                foreach(Agent currentAgent in obstaculosParaAnalizar){
                    Vector3 localPos = ConvertLocalPos(agent, currentAgent, rotacion);

                    if(localPos.x >= 0){
                        float sumRadius = currentAgent.anchura + agent.anchura;

                        if(Mathf.Abs(localPos.z) < sumRadius){
                            float root = Mathf.Sqrt(Mathf.Pow(sumRadius, 2f) - Mathf.Pow(localPos.z, 2f));
                            float intersection = localPos.x - root;

                            if(intersection <= 0){
                                intersection = localPos.x + root;
                            }

                            if(intersection < minIntersection){
                                minIntersection = intersection;
                                closestAgent = currentAgent;
                                localPosClosestAgent = localPos;
                            }
                        }
                    }
                }

                if(closestAgent != null){
                    if(Players.Length > 0){
                        rotacion = getRotacionLocal(agent);

                        Vector3 localPos = ConvertLocalPos(agent, Players[0].transform.position, rotacion);

                        if(localPos.x >= 0){
                            float sumRadius = agent.anchura + agent.anchura;

                            if(Mathf.Abs(localPos.z) < sumRadius){
                                float root = Mathf.Sqrt(Mathf.Pow(sumRadius, 2f) - Mathf.Pow(localPos.z, 2f));
                                float intersection = localPos.x - root;

                                if(intersection <= 0){
                                    intersection = localPos.x + root;
                                }

                                if(intersection < minIntersection){
                                    minIntersection = intersection;
                                    closestAgent = agent;
                                    localPosClosestAgent = localPos;
                                }

                                localPosClosestAgent = localPos;
                            }
                        }
                    }

                    float x = localPosClosestAgent.x;
                    float z = localPosClosestAgent.z;

                    float factorX = 0.2f;
                    float factorZ = 1f + (boxLength - x) / boxLength;

                    Vector3 finalLocalPos = new Vector3((closestAgent.anchura - x) * factorX, 0, (closestAgent.anchura - z) * factorZ);

                    agenteVirtual.Position = ConvertWorldPos(agent, finalLocalPos, rotacion);
                    agenteVirtual.arrivalRadius = agent.arrivalRadius;
                    agenteVirtual.interiorRadius = agent.interiorRadius;
                    return base.GetSteering(agent);
                }else{
                    if(Players.Length > 0){
                        rotacion = getRotacionLocal(agent);

                        Vector3 localPos = ConvertLocalPos(agent, Players[0].transform.position, rotacion);

                        if(localPos.x >= 0){
                            float sumRadius = agent.anchura + agent.anchura;

                            if(Mathf.Abs(localPos.z) < sumRadius){
                                float root = Mathf.Sqrt(Mathf.Pow(sumRadius, 2f) - Mathf.Pow(localPos.z, 2f));
                                float intersection = localPos.x - root;

                                if(intersection <= 0){
                                    intersection = localPos.x + root;
                                }

                                localPosClosestAgent = localPos;

                                float x = localPosClosestAgent.x;
                                float z = localPosClosestAgent.z;

                                float factorX = 0.2f;
                                float factorZ = 1f + (boxLength - x) / boxLength;

                                Vector3 finalLocalPos = new Vector3((agent.anchura - x) * factorX, 0, (agent.anchura - z) * factorZ);

                                agenteVirtual.Position = ConvertWorldPos(agent, finalLocalPos, rotacion);
                                agenteVirtual.arrivalRadius = agent.arrivalRadius;
                                agenteVirtual.interiorRadius = agent.interiorRadius;
                                return base.GetSteering(agent);
                            }else{
                                steer.linear = Vector3.zero;
                            }
                        }else{
                            steer.linear = Vector3.zero;
                        }
                    }else{
                        steer.linear = Vector3.zero;
                    }
                }
            }else{
                if(Players.Length > 0){
                    float rotacion = getRotacionLocal(agent);

                    Vector3 localPos = ConvertLocalPos(agent, Players[0].transform.position, rotacion);

                    if(localPos.x >= 0){
                        float sumRadius = agent.anchura + agent.anchura;

                        if(Mathf.Abs(localPos.z) < sumRadius){
                            float root = Mathf.Sqrt(Mathf.Pow(sumRadius, 2f) - Mathf.Pow(localPos.z, 2f));
                            float intersection = localPos.x - root;

                            if(intersection <= 0){
                                intersection = localPos.x + root;
                            }

                            localPosClosestAgent = localPos;

                            float x = localPosClosestAgent.x;
                            float z = localPosClosestAgent.z;

                            float factorX = 0.2f;
                            float factorZ = 1f + (boxLength - x) / boxLength;

                            Vector3 finalLocalPos = new Vector3((agent.anchura - x) * factorX, 0, (agent.anchura - z) * factorZ);

                            agenteVirtual.Position = ConvertWorldPos(agent, finalLocalPos, rotacion);
                            agenteVirtual.arrivalRadius = agent.arrivalRadius;
                            agenteVirtual.interiorRadius = agent.interiorRadius;
                            return base.GetSteering(agent);
                        }else{
                            steer.linear = Vector3.zero;
                        }
                    }else{
                        steer.linear = Vector3.zero;
                    }
                }else{
                    steer.linear = Vector3.zero;
                }
            }
        }else{
            steer.linear = Vector3.zero;
        }

        steer.angular = 0f;

        return steer;
    }

    private float getRotacionLocal(Agent agent){
        Vector3 realDir = agent.OrientationToVector();
        Vector3 vRef = getPerpendicularVector(Vector3.right);

        if(isOppositeVectors(Vector3.right, realDir)){
            return 180f;
        }

        if(!isToTheRigth(vRef, realDir)){
            return Vector3.Angle(realDir, Vector3.right);
        }else{
            return 360f - Vector3.Angle(realDir, Vector3.right);
        }
    }

    private bool isOppositeVectors(Vector3 v1, Vector3 v2){
        if(v1 == -v2){
            return true;
        }else{
            return false;
        }
    }

    private Vector3 ConvertLocalPos(Agent agent, Agent npc, float rotacion){
        Vector3 realDir = npc.Position - agent.Position;

        Vector3 localDir = Quaternion.AngleAxis(-rotacion, Vector3.up) * realDir;

        return localDir;
    }

    private Vector3 ConvertLocalPos(Agent agent, Vector3 npc, float rotacion){
        Vector3 realDir = npc - agent.Position;

        Vector3 localDir = Quaternion.AngleAxis(-rotacion, Vector3.up) * realDir;

        return localDir;
    }

    private Vector3 ConvertWorldPos(Agent agent, Vector3 localPos, float rotacion){
        Vector3 realDir = Quaternion.AngleAxis(rotacion, Vector3.up) * localPos;

        return new Vector3(realDir.x + agent.Position.x, 0, realDir.z + agent.Position.z);
    }

    private void filtrarObstaculos(List<Agent> obstaculos, Agent agent, float boxLength){
        int i = 0;
        while(i < obstaculos.Count){
            Agent currentNPC = obstaculos[i];
            Vector3 vRef = agent.OrientationToVector();
            Vector3 vTest = currentNPC.Position - agent.Position;

            if(isToTheRigth(vRef, vTest)){
                Vector3 boxEnd = agent.Position + agent.OrientationToVector() * boxLength;
                vRef = (boxEnd + agent.OrientationToVector()) - boxEnd;
                vTest = currentNPC.Position - boxEnd;

                if(!isToTheRigth(vRef, vTest)){
                    i++;
                }else{
                    obstaculos.Remove(currentNPC);
                }
            }else{
                obstaculos.Remove(currentNPC);
            }
        }
    }

    private bool isToTheRigth(Vector3 vectorRef, Vector3 vectorTest){
        if(Vector3.Dot(vectorTest, vectorRef) >= 0){
            return true;
        }else{
            return false;
        }
    }

    private Vector3 getPerpendicularVector(Vector3 u){
        return new Vector3(-u.z, 0, u.x);
    }

    void OnDrawGizmos()
    {
        if(VisualDebug._debug)
        {
            Gizmos.color = Color.red;

            float boxLength = minBoxLength + (GetComponent<Agent>().Speed /  GetComponent<Agent>().MaxSpeed) * minBoxLength;
            Vector3 v1 = getPerpendicularVector(GetComponent<Agent>().OrientationToVector());
            Vector3 P1 = GetComponent<Agent>().Position + v1 * GetComponent<Agent>().anchura;
            Vector3 Q1 = GetComponent<Agent>().Position - v1 * GetComponent<Agent>().anchura;
            Vector3 P2 = P1 + GetComponent<Agent>().OrientationToVector() * boxLength;
            Vector3 Q2 = Q1 + GetComponent<Agent>().OrientationToVector() * boxLength;

            Gizmos.DrawLine(GetComponent<Agent>().Position, GetComponent<Agent>().Position + GetComponent<Agent>().OrientationToVector() * boxLength);
            Gizmos.DrawLine(P1, Q1);
            Gizmos.DrawLine(P2, Q2);
            Gizmos.DrawLine(P1, P2);
            Gizmos.DrawLine(Q1, Q2);
        }
    }
}
