using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Linea
{
    private Vector3 start;
    private Vector3 end;

    public Linea(Vector3 s, Vector3 e){
        start = s;
        end = e;
    }

    public Vector3 getStart(){
        return start;
    }

    public Vector3 getEnd(){
        return end;
    }

    public void setStart(Vector3 s){
        start = s;
    }

    public void setEnd(Vector3 e){
        end = e;
    }

    public Vector3 getDirection(){
        var heading = end - start;
        var distance = heading.magnitude;
        var direction = heading / distance; // This is now the normalized direction.

        return direction;
    }

    public void DrawLine(Material lineMaterial, GameObject ciudad){
        GameObject line = new GameObject("Linea");
        line.transform.parent = ciudad.transform;
        line.transform.position = start;
        var lineRenderer = line.AddComponent<LineRenderer>();
        lineRenderer.material = lineMaterial;
        lineRenderer.startWidth = 0.1f;
        lineRenderer.SetPosition(0, start);
        lineRenderer.endWidth = 0.1f;
        lineRenderer.SetPosition(1, end);
    }

    public void DrawLineSinCity(Material lineMaterial, float width=0.1f){
        GameObject line = new GameObject("Linea");
        line.transform.position = start;
        var lineRenderer = line.AddComponent<LineRenderer>();
        lineRenderer.material = lineMaterial;
        lineRenderer.startWidth = width;
        lineRenderer.SetPosition(0, start);
        lineRenderer.endWidth = width;
        lineRenderer.SetPosition(1, end);
    }
}
