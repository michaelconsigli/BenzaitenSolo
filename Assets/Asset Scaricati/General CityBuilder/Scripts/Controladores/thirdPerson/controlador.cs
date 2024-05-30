using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class controlador : MonoBehaviour
{
    [SerializeField]
    MiInput miInput;

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

    [Header("Colisiones")]
    [SerializeField]
    LayerMask LayerSuelo;
    [SerializeField]
    LayerMask LayerTecho;
    [SerializeField]
    LayerMask choques;

    [Tooltip("Distancia a paredes")]
    [SerializeField]
    float separacion=2;
    [Tooltip("Cambiar para detectar escalones de otras alturas")]
    [SerializeField]
    float separacion5=1.1f;
    [Tooltip("Proporción de la altura a la que detecta el suelo")]
    [SerializeField]
    float separacion2=1;
    [Tooltip("Proporción de la altura a la que detecta el techo")]
    [SerializeField]
    float separacion3=1;



    [Header("Configuración coger")]
    [SerializeField]
    float cogerDistancia=1.5f;
    [SerializeField]
    LayerMask  LayerCoger;

    [Header("Configuración Enemigos")]

    [SerializeField]
    LayerMask  LayerEnemigos;


    [SerializeField]
    Animator animator;
     
    
    
    // Start is called before the first frame update

    public Vector3 velocidad;
    public bool chocando, suelo, techo, jumping=false, intocable=false;
    Vector3 Q, Q1,Q2, sep, size, size1, size2;

    MeshRenderer meshR1;
    float signo;
    void Start()
    {
        meshR1=GetComponent<MeshRenderer>();
        //controlJuego.player=gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        miInput.GetInput();
        detectarColisiones();

        /*
        aplicarGravedad();
        saltar();
        

        ajustar();
        emparentar();
        */

        movimientoPlano();
        
        movimientoTotal();

        /*
        coger();
        recibirHerida();
        */
    }

    void detectarColisiones(){
        signo=1;
        if(miInput.ver<0){
            signo=-1;
        }
        Q = transform.position+separacion*radius*transform.forward*signo + (1-separacion5)/2*height*Vector3.up;
        size=new Vector3(radius, separacion5*height, 0.1f);
        chocando=Physics.CheckBox(Q,size,transform.rotation, choques);
        
        Q1 = transform.position-separacion2*height*transform.up;
        size1=new Vector3(radius, 0.1f, radius);
        suelo=Physics.CheckBox(Q1,size1,transform.rotation, LayerSuelo);

        Q2 = transform.position+separacion3*height*transform.up;
        size2=new Vector3(radius, 0.1f, radius);
        techo=Physics.CheckBox(Q2,size2,transform.rotation, LayerTecho);
    }
    void movimientoPlano(){
        transform.Rotate(miInput.hor*Vector3.up*vg*Time.deltaTime,Space.World);
        
        if(!chocando){
            Vector3 velocidadPlana = miInput.ver*transform.forward*vel*Time.deltaTime;  
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
        if(!jumping){
            if(Mathf.Abs(velocidad.z)>0){
                animator.SetBool("Walk",true);
            }
            else{
                animator.SetBool("Walk",false);
            }

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


    

    
    
    void OnDrawGizmosSelected(){
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color=Color.red;
        Gizmos.DrawWireCube(Vector3.zero+separacion*radius*Vector3.forward*signo,2*size);

        Gizmos.DrawWireCube(-separacion2*height*Vector3.up,2*size1);
        Gizmos.DrawWireCube(separacion3*height*Vector3.up,2*size2);
    }

    
    
}
