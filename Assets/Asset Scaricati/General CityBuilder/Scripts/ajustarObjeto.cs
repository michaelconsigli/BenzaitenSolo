using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ajustarObjeto : MonoBehaviour
{
    [SerializeField]
    GameObject ob;
    [SerializeField]
    Transform P0,P1,P2,P3;
    [SerializeField]
    float h=100;
    Vector3 A,B,C,D;

    Mesh mesh, mesh2;
    public Vector3[] pos,pos0;

    float signo=1;
    // Start is called before the first frame update
    void Start()
    {
        
        mesh=ob.GetComponent<MeshFilter>().mesh;
        mesh2=new Mesh();
        pos = new Vector3[mesh.vertices.Length];
        pos0=mesh.vertices;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.P)){
            ajusta();
        }
    }

    void ajusta(){
        A=Vector3.zero;
        B=P1.position-P0.position;
        C=P2.position-P0.position;
        D=P3.position-P0.position;

        Vector3 v=new Vector3(-B.z,0,B.x);
        if(Vector3.Dot(C,v)<0){signo=-1;}
        for(int i=0;i<pos.Length;i++){
            Vector3 H1=A+(C-A)*pos0[i].y;
            Vector3 H2=B+(D-B)*pos0[i].y;
            Vector3 H=H1+(H2-H1)*pos0[i].x;
            pos[i]=new Vector3(H.x,-pos0[i].z*h,H.z);
        }
        mesh2.vertices=pos;
        
        
        if(signo==-1){
            int[] triangulos2=new int[mesh.triangles.Length];
            for(int i=0;i<triangulos2.Length;i+=3){
                triangulos2[i]=mesh.triangles[i];
                triangulos2[i+1]=mesh.triangles[i+2];
                triangulos2[i+2]=mesh.triangles[i+1];
            }
            mesh2.triangles=triangulos2;
        }
        else{
            mesh2.triangles=mesh.triangles;
        }
        mesh2.normals=mesh.normals;
        mesh2.uv=mesh.uv;
        mesh2.RecalculateNormals();
        
        ob.GetComponent<MeshFilter>().mesh=mesh2;
        ob.transform.position=P0.position;
        ob.AddComponent<MeshCollider>();
        //ob.GetComponent<MeshCollider>().convex=true;
    }
}
