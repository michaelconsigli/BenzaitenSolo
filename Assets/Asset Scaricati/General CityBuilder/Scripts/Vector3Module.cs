using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public sealed class Vector3Module
{
    private static Vector3Module _instance;

    public List<Vector3> vectoresR;
    public List<Vector3> vectoresI;
    public Vector3 vectorCirc1;
    public Vector3 vectorCirc2;

    private Vector3Module(){
        vectoresR = new List<Vector3>();
        vectoresI = new List<Vector3>();
    }

    public static Vector3Module GetInstance()
    {
        if (_instance == null)
        {
            _instance = new Vector3Module();
        }

        return _instance;
    }

    public Vector3 normVector(Vector3 v){
        if(v.magnitude < 1f){
            return (v * (1f / v.magnitude));
        }else{
            return (v / v.magnitude);
        }
    }

    public Vector3 getPerpendicularVector(Vector3 u){
        return new Vector3(-u.z, 0, u.x);
    }

    public bool isToTheRigth(Vector3 vectorRef, Vector3 vectorTest){
        Vector3 v = getPerpendicularVector(vectorRef);

        v = -v;

        if(Vector3.Dot(vectorTest, v) >= 0){
            return true;
        }else{
            return false;
        }
    }

    public float Vectors3Angle(Vector3 v1, Vector3 v2){
        if(oppositeDirections(v1, v2)){
            return 180.0f;
        }else{
            return Vector3.Angle(v1, v2);
        }
    }

    public void RigthAndLeftVectors(Nodo node, Nodo conexionRef, bool opposite=false){
        vectoresR = new List<Vector3>();
        vectoresI = new List<Vector3>();

        Vector3 vectorRef = conexionRef.getPosicion() - node.getPosicion();

        if(opposite){
            vectorRef = - vectorRef;
        }

        // Clasificar vectores
        foreach(Nodo c in node.getConexiones()){
            if(c != conexionRef){
                Vector3 currentVector = c.getPosicion() - node.getPosicion();

                RigthOrLeft(vectorRef, currentVector);
            }
        }

        // Ordenamos los vectores
        orderVectores(vectorRef);
    }

    public void RigthOrLeft(Vector3 vectorRef, Vector3 vectorTest){
        Vector3 v = getPerpendicularVector(vectorRef);

        v = -v;

        if(Vector3.Dot(vectorTest, v) >= 0){
            vectoresR.Add(vectorTest);
        }else{
            vectoresI.Add(vectorTest);
        }
    }

    public bool oppositeDirections(Vector3 v1, Vector3 v2){
        Vector3 vn1 = v1 / v1.magnitude;
        Vector3 vn2 = v2 / v2.magnitude;

        if(vn1 == (-vn2)){
            return true;
        }else{
            return false;
        }
    }

    public Vector3 getOppositeDirection(Vector3 v){
        Vector3 vn = v / v.magnitude;

        return (-vn);
    }

    public void orderVectores(Vector3 u){
        vectoresR = vectoresR.OrderBy(v => (Vectors3Angle(u, v))).ToList();

        vectoresI = vectoresI.OrderBy(v => (Vectors3Angle(u, v))).ToList();
    }

    public void showVectores(){ // Solo para temas de Debug
        Debug.Log("Derechos:");
        foreach(Vector3 v in vectoresR){
            Debug.Log(v);
        }

        Debug.Log("Izquierdos:");
        foreach(Vector3 v in vectoresI){
            Debug.Log(v);
        }
    }

    public bool isClockwiseAngle(Vector3 v1, Vector3 v2){
        Vector3 v = getPerpendicularVector(v1);

        v = -v;

        if(Vector3.Dot(v2, v) >= 0){
            return true;
        }else{
            return false;
        }
    }

    public bool primeroDerecha(){
        if(oppositeDirections(vectoresR[0], vectoresI[0]) || !isClockwiseAngle(vectoresR[0], vectoresI[0])){
            return true;
        }else{
            return false;
        }
    }

    public bool ultimoDerecha(){
        if(oppositeDirections(vectoresR[vectoresR.Count - 1], vectoresI[0]) || isClockwiseAngle(vectoresR[vectoresR.Count - 1], vectoresI[0])){
            return true;
        }else{
            return false;
        }
    }

    public bool primeroIzquierda(){
        if(oppositeDirections(vectoresI[0], vectoresR[0]) || isClockwiseAngle(vectoresI[0], vectoresR[0])){
            return true;
        }else{
            return false;
        }
    }

    public bool ultimoIzquierda(){
        if(oppositeDirections(vectoresI[vectoresI.Count - 1], vectoresR[0]) || !isClockwiseAngle(vectoresI[vectoresI.Count - 1], vectoresR[0])){
            return true;
        }else{
            return false;
        }
    }

    public bool par1R1I(){
        if(oppositeDirections(vectoresR[0], vectoresI[0]) || !isClockwiseAngle(vectoresR[0], vectoresI[0])){
            return true;
        }else{
            return false;
        }
    }

    public bool parURUI(){
        if(oppositeDirections(vectoresR[vectoresR.Count - 1], vectoresI[vectoresI.Count - 1]) || isClockwiseAngle(vectoresR[vectoresR.Count - 1], vectoresI[vectoresI.Count - 1])){
            return true;
        }else{
            return false;
        }
    }

    public Vector3 CalculateDirection(Nodo nodoStart, Nodo nodoEnd){
        var heading = nodoEnd.getPosicion() - nodoStart.getPosicion();
        var distance = heading.magnitude;

        var direction = heading / distance;

        return direction;
    }

    public Vector3 CalculateIntersectionPoint(Nodo N1S, Nodo N1E, Nodo N2S, Nodo N2E){
        Vector3 A1 = N1S.getPosicion();
        Vector3 A2 = N1E.getPosicion();
        Vector3 B1 = N2S.getPosicion();
        Vector3 B2 = N2E.getPosicion();

        Vector3 u = A2 - A1;
        Vector3 v = new Vector3(-u.z, 0, u.x);

        Vector3 p1 = B1 - A1;
        Vector3 p2 = B2 - B1;

        float t = - ((Vector3.Dot(p1, v)) / (Vector3.Dot(p2, v))); // A veces el denominador da 0

        Vector3 H = B1 + t * p2;

        return H;
    }

    public bool checkIntersection(Vector3 start1, Vector3 end1, Vector3 start2, Vector3 end2){
        Vector3 u = end1 - start1;
        Vector3 v = new Vector3(-u.z, 0, u.x);

        Vector3 c1 = start2 - start1;
        Vector3 c2 = end2 - start1;
        float y1 = Vector3.Dot(c1, v);
        float y2 = Vector3.Dot(c2, v);

        if((y1 * y2) < 0){
            u = end2 - start2;
            v = new Vector3(-u.z, 0, u.x);

            c1 = end1 - start2;
            c2 = start1 - start2;
            y1 = Vector3.Dot(c1, v);
            y2 = Vector3.Dot(c2, v);

            if((y1 * y2) < 0){
                return true;
            }
        }

        return false;
    }

    public bool checkPointIntoALine(Vector3 testPoint, Vector3 start, Vector3 end){
        Vector3 dirA = testPoint - start;
        Vector3 dirB = testPoint - end;
        Vector3 refDir = end - start;

        if(refDir.magnitude == (dirA.magnitude + dirB.magnitude)){
            return true;
        }else{
            return false;
        }
    }

    public bool onSegment(Vector3 p, Vector3 q, Vector3 r)
    {
        if (q.x <= Mathf.Max(p.x, r.x) && q.x >= Mathf.Min(p.x, r.x) && q.z <= Mathf.Max(p.z, r.z) && q.z >= Mathf.Min(p.z, r.z)){
            return true;
        }
    
        return false;
    }

    public int direction(Vector3 a, Vector3 b, Vector3 c)
    {
        int val = (int)((b.z - a.z) * (c.x - b.x) - (b.x - a.x) * (c.z - b.z));
    
        if (val == 0){
            // Collinear
            return 0;
        }else if (val < 0){
            // Anti-clockwise direction
            return 2;
        }else{
            // Clockwise direction
            return 1;
        }
    }

    public bool checkIntersectionCityCells(Vector3 p1, Vector3 q1, Vector3 p2, Vector3 q2){
        /*int o1 = direction(p1, q1, p2);
        int o2 = direction(p1, q1, q2);
        int o3 = direction(p2, q2, p1);
        int o4 = direction(p2, q2, q1);

        // General case
        if (o1 != o2 && o3 != o4)
            return true;

        // Special Cases
        // p1, q1 and p2 are collinear and p2 lies on segment p1q1
        if (o1 == 0 && onSegment(p1, p2, q1)) return true;

        // p1, q1 and q2 are collinear and q2 lies on segment p1q1
        if (o2 == 0 && onSegment(p1, q2, q1)) return true;

        // p2, q2 and p1 are collinear and p1 lies on segment p2q2
        if (o3 == 0 && onSegment(p2, p1, q2)) return true;

            // p2, q2 and q1 are collinear and q1 lies on segment p2q2
        if (o4 == 0 && onSegment(p2, q1, q2)) return true;

        return false; // Doesn't fall in any of the above cases*/

        if(checkIntersection(p1, q1, p2, q2)){
            return true;
        }else{
            return false;
        }
    }

    // **** Vertex Circles Creation ****

    public bool noMore180Degrees(Nodo node){
        vectoresR = new List<Vector3>();
        vectoresI = new List<Vector3>();

        Vector3 F = node.getPosicion() + Vector3.forward;
        Vector3 vectorRef = F - node.getPosicion();

        // Clasificar vectores

        foreach(Nodo c in node.getConexiones()){
            Vector3 currentVector = c.getPosicion() - node.getPosicion();

            RigthOrLeft(vectorRef, currentVector);
        }

        // Ordenamos los vectores

        orderVectores(vectorRef);

        // Comprobar ángulos

        if(vectoresI.Count == 0){ // Solo hay a la derecha
            if(oppositeDirections(vectoresR[0], vectoresR[vectoresR.Count - 1])){
                return true;
            }else{
                vectorCirc1 = vectoresR[0];
                vectorCirc2 = vectoresR[vectoresR.Count - 1];
                return false;
            }
        }else if(vectoresR.Count == 0){ // Solo hay a la izquierda
            vectorCirc1 = vectoresI[0];
            vectorCirc2 = vectoresI[vectoresI.Count - 1];
            return false;
        }else{ // Hay en ambos lados
            if(vectoresR.Count == 1 && vectoresI.Count == 1){
                if(oppositeDirections(vectoresR[0], vectoresI[0])){
                    return true;
                }else{
                    vectorCirc1 = vectoresR[0];
                    vectorCirc2 = vectoresI[0];
                    return false;
                }
            }else if(vectoresR.Count > 1 && vectoresI.Count == 1){
                if(primeroDerecha() && ultimoDerecha()){
                    return true;
                }else{
                    if(!primeroDerecha()){
                        vectorCirc1 = vectoresR[0];
                        vectorCirc2 = vectoresI[0];
                    }else{
                        vectorCirc1 = vectoresR[vectoresR.Count - 1];
                        vectorCirc2 = vectoresI[0];
                    }
                    return false;
                }
            }else if(vectoresR.Count == 1 && vectoresI.Count > 1){
                if(primeroIzquierda() && ultimoIzquierda()){
                    return true;
                }else{
                    if(!primeroIzquierda()){
                        vectorCirc1 = vectoresI[0];
                        vectorCirc2 = vectoresR[0];
                    }else{
                        vectorCirc1 = vectoresI[vectoresI.Count - 1];
                        vectorCirc2 = vectoresR[0];
                    }
                    return false;
                }
            }else{
                if(par1R1I() && parURUI()){
                    return true;
                }else{
                    if(!par1R1I()){
                        vectorCirc1 = vectoresR[0];
                        vectorCirc2 = vectoresI[0];
                    }else{
                        vectorCirc1 = vectoresR[vectoresR.Count - 1];
                        vectorCirc2 = vectoresI[vectoresI.Count - 1];
                    }
                    return false;
                }
            }
        }
    }

    public Vector3 getPCirclePoint(Vector3 P, float anchuraCarretera){
        Vector3 perpendicular1 = new Vector3(-vectorCirc1.z, 0, vectorCirc1.x);
        Vector3 perpendicular2 = -perpendicular1;

        Vector3 R = Vector3.zero;
        if(Vector3.Angle(perpendicular1, vectorCirc2) > Vector3.Angle(perpendicular2, vectorCirc2)){
            R = P + (perpendicular1 / perpendicular1.magnitude) * anchuraCarretera;
        }else{
            R = P + (perpendicular2 / perpendicular1.magnitude) * anchuraCarretera;
        }

        return R;
    }

    public Vector3 getQCirclePoint(Vector3 P, float anchuraCarretera){
        Vector3 perpendicular1 = new Vector3(-vectorCirc2.z, 0, vectorCirc2.x);
        Vector3 perpendicular2 = -perpendicular1;

        Vector3 Q = Vector3.zero;
        if(Vector3.Angle(perpendicular1, vectorCirc1) > Vector3.Angle(perpendicular2, vectorCirc1)){
            Q = P + (perpendicular1 / perpendicular1.magnitude) * anchuraCarretera;
        }else{
            Q = P + (perpendicular2 / perpendicular1.magnitude) * anchuraCarretera;
        }

        return Q;
    }

    public int calculateCurrentCircleSegmentation(Vector3 P, Vector3 Q){
        Vector3 disPQ = Q - P;
        float distance = disPQ.magnitude;
        float resultF = (distance * 12) / 0.878f;

        float resultR = Mathf.Round(resultF);

        if(resultR == 0){
            return 1;
        }else if(resultR < resultF){
            return (int)(resultR+1);
        }else{
            return (int)resultR;
        }
    }

    public Vector3 calculateProjection(Vector3 position, Vector3 roadStartPoint, Vector3 roadEndPoint){
        Vector3 P = roadStartPoint;
        Vector3 Q = roadEndPoint;
        Vector3 R = position;

        Vector3 road = Q - P;
        Vector3 auxRoad = R - P;

        Vector3 proyection = Vector3.Project(auxRoad, road);

        return (proyection + P);
    }

    public float calculateDistance(Vector3 position1, Vector3 position2){
        return Vector3.Distance(position1, position2);
    }
}
