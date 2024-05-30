using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controlador2 : MonoBehaviour
{
    [SerializeField]
    MiInput2 miInput;
    [SerializeField]
    Camera camara;
    [Header("Velocidades")]
    [SerializeField]
    float vel=12;
    [SerializeField]
    float vg=90;
    
    [Header("Tamaño")]
    [SerializeField]
    float radius=0.5f;
    [SerializeField]
    float height=1;

    [Header("Fisica")]
    [SerializeField]
    float gravedad=1;
    [SerializeField]
    float velocidadCaida=-1;
    [SerializeField]
    float velocidadSalto=1;

    [Header("Colisiones")]
    [SerializeField]
    LayerMask LayerSuelo;
    [SerializeField]
    LayerMask LayerTecho;
    [SerializeField]
    LayerMask choques;
    [SerializeField]
    LayerMask NPCs;

    [Tooltip("Distancia a paredes")]
    [SerializeField]
    float separacion=2;
    [Tooltip("Cambiar para detectar escalones de otras alturas")]
    [SerializeField]
    float separacion5=0.8f;
    [Tooltip("Proporción de la altura a la que detecta el suelo")]
    [SerializeField]
    float separacion2=1;
    [Tooltip("Proporción de la altura a la que detecta el techo")]
    [SerializeField]
    float separacion3=1;
    [SerializeField]
    float separacion4=1.5f;



    [Header("Configuración coger")]
    [SerializeField]
    float cogerDistancia=1.5f;
    [SerializeField]
    LayerMask  LayerCoger;

    [Header("Configuración Enemigos")]

    [SerializeField]
    LayerMask  LayerEnemigos;
    [SerializeField]
     float vida=100;
     [SerializeField]
     float hurt=5;
     [SerializeField]
     float tiempo=3;
     [SerializeField]
     float velocidadParpadeo=0.2f;

    [HideInInspector]
    public bool activated = true;
     
    
    
    // Start is called before the first frame update

    Vector3 velocidad;
    bool chocando, suelo, techo, jumping=false, intocable=false;
    Vector3 Q, Q1,Q2, sep, size, size1, size2;

    MeshRenderer meshR1, meshR2, meshR3;
    float signo;
    void Start()
    {
        meshR1=GetComponent<MeshRenderer>();
        meshR2=transform.GetChild(0).gameObject.GetComponent<MeshRenderer>();
        meshR3=transform.GetChild(1).gameObject.GetComponent<MeshRenderer>();
        activated = true;
    }

    // Update is called once per frame
    void Update()
    {
        if(activated){
            miInput.GetInput();
            detectarColisiones();

            /*aplicarGravedad();
            saltar();*/

            /*ajustar();
            emparentar();*/

            movimientoPlano();
            
            movimientoTotal();

            giraCabeza();

            /*coger();
            recibirHerida();*/
        }
    }

    void detectarColisiones(){
        signo=1;
        if(miInput.avanzar<0){
            signo=-1;
        }
        Q = transform.position+separacion*radius*transform.forward*signo + (1-separacion5)/2*height*Vector3.up;
        size=new Vector3(radius, separacion5*height, 0.1f);
        chocando=Physics.CheckBox(Q,size,transform.rotation, choques);

        if(!chocando){
            chocando=Physics.CheckBox(Q,size,transform.rotation, NPCs);
        }
        
        Q1 = transform.position-separacion2*height*transform.up;
        size1=new Vector3(radius, 0.1f, radius);
        suelo=Physics.CheckBox(Q1,size1,transform.rotation, LayerSuelo);

        Q2 = transform.position+separacion3*height*transform.up;
        size2=new Vector3(radius, 0.1f, radius);
        techo=Physics.CheckBox(Q2,size2,transform.rotation, LayerTecho);
    }

    void movimientoPlano(){
        transform.Rotate(miInput.hor*Vector3.up*vg*Time.deltaTime,Space.World);
        if(! chocando){
            Vector3 velocidadPlana = miInput.avanzar*transform.forward*vel*Time.deltaTime;  
            velocidad.x=velocidadPlana.x;  
            velocidad.z=velocidadPlana.z;  
        }
        else{
            Vector3 velocidadPlana= Vector3.zero; 
            velocidad.x=velocidadPlana.x;  
            velocidad.z=velocidadPlana.z;     
        }
    }

    void movimientoTotal(){
        transform.Translate(velocidad, Space.World);
    }

    void aplicarGravedad(){
        if(!suelo){
            velocidad.y-=gravedad*Time.deltaTime;
            if(velocidad.y<velocidadCaida){
                velocidad.y=velocidadCaida;
            }
        }
        else{
            velocidad.y=0;
            if(jumping){
                jumping=false;
            }
        }
    }

    void saltar(){
        if(suelo && miInput.saltar){
            velocidad.y=velocidadSalto;
            jumping=true;
        }
        if(techo){
            if(velocidad.y>0){velocidad.y=0;}
        }
    }

    void ajustar(){
        Vector3 Q3 = transform.position;
        Vector3 direccion=-Vector3.up;
        float d=1.3f*height;
        RaycastHit hit;
        if(Physics.Raycast(Q3, direccion, out hit, d, LayerSuelo)){
            Vector3 P=hit.point;
            if(!jumping){transform.position=P+height*Vector3.up;}
        }
    }

    void emparentar(){
        Vector3 Q3 = transform.position;
        Vector3 direccion=-Vector3.up;
        float d=separacion4*height;
        RaycastHit hit;
        if(Physics.Raycast(Q3, direccion, out hit, d, LayerSuelo)){
            if(hit.transform.gameObject.tag=="movil"){
                transform.parent=hit.transform;
            }
            else{
                transform.parent=null;
            }
        }
        else{
                transform.parent=null;
            }
    }

    void coger(){
        Vector3 centro=transform.position;
        Vector3 tam =new Vector3(radius,height, 0.1f);
        Vector3 dirCaja=transform.forward;
        RaycastHit hit;

        if(Physics.BoxCast(centro, tam, dirCaja, out hit, transform.rotation, cogerDistancia, LayerCoger)){
            // puntos, modificar inventario
            Destroy(hit.transform.gameObject);
        }
    }


    void recibirHerida(){
        if(! intocable){
            Vector3 centro=transform.position;

            if(Physics.CheckCapsule(centro-height*Vector3.up, centro+height*Vector3.up, radius, LayerEnemigos)){
                vida-=hurt;
                intocable=true;
                parpadeo();
                Invoke("volver",tiempo);
                if(vida<0){
                    Destroy(gameObject);
                }
            }
            
        }
    }

    void volver(){
        intocable=false;
        meshR1.enabled=true;
        meshR2.enabled=true;
        meshR3.enabled=true;
        CancelInvoke();
    }

    void parpadeo(){
        bool activos=meshR1.enabled;
        meshR1.enabled=! activos;
        meshR2.enabled=! activos;
        meshR3.enabled=! activos;
        Invoke("parpadeo",velocidadParpadeo);
    }
    void OnDrawGizmosSelected(){
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color=Color.red;
        Gizmos.DrawWireCube(Vector3.zero+separacion*radius*Vector3.forward*signo,2*size);

        Gizmos.DrawWireCube(-separacion2*height*Vector3.up,2*size1);
        Gizmos.DrawWireCube(separacion3*height*Vector3.up,2*size2);
    }

    void giraCabeza(){ // Parece que hay que tocar esta función para arreglar la cámara
        bool limite=false;
        if(camara.transform.forward.y<-0.5f & miInput.ver<0){
            limite=true;
        }
        if(camara.transform.forward.y>0.8f & miInput.ver>0){
            limite=true;
        }
        if(!limite){
            camara.transform.Rotate(-vg*Time.deltaTime*transform.right*miInput.ver,Space.World);
            camara.gameObject.GetComponent<CamaraRot>().giraCamara();
        }
    }
}
