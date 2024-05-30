using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class AgentNPC : Agent
{
    // Este será el steering final que se aplique al personaje.
    [SerializeField] protected Steering steer;
    [SerializeField] protected float factorDesaceleracion = 0.98f;
    [SerializeField] float factorDesaceleracionAngular = 0.7f;
    
    public ArbitroPonderado arbitro;

    // Nodo en el que se encuentra y al que se dirige
    public NodoObj nodoActual;
    public NodoObj nodoObjetivo;

    protected virtual void Awake()
    {
        this.steer = new Steering();

        arbitro = gameObject.AddComponent<ArbitroPonderado>() as ArbitroPonderado;
    }

    public virtual void Start()
    {
        // Inicializamos los desplazamientos y giros a 0
        this.Velocity = Vector3.zero;
        this.Acceleration = Vector3.zero;
        this.Rotation = 0;
        this.AngularAcc = 0;
        factorDesaceleracion = 0.92f;

        if(this.Orientation < 0 || this.Orientation > 360){
            this.Orientation = 0;
        }
    }  

    // Update is called once per frame
    public override void Update()
    {
        ApplySteering(Time.deltaTime);
    }

    protected void ApplySteering(float deltaTime)
    {
        // Actualizar las propiedades para Time.deltaTime según NewtonEuler
        // La actualización de las propiedades se puede hacer en LateUpdate()
        Acceleration = this.steer.linear; // cambia este valor si steer son aceleraciones
        if (Acceleration.magnitude == 0) Velocity *= factorDesaceleracion;

        Velocity = Velocity+Acceleration*deltaTime; // steer se interpreta como velocidades.
        Position += Velocity * deltaTime; // Si steer fueran aceleraciones

        AngularAcc = (float) this.steer.angular; // Aplicamos Newton-Euler para a=0
        if(AngularAcc == 0) Rotation *= factorDesaceleracionAngular;

        Rotation += AngularAcc * deltaTime;
        Orientation += Rotation * deltaTime; // deberás cambiar las expresiones.
        // Pasar los valores Position y Orientation a Unity.
        // Posición no es necesario. Ver observación final.
        transform.rotation = new Quaternion(); //Quaternion.identity;
        transform.Rotate(Vector3.up, Heading());
        // Ni se te ocurra usar cuaterniones para la rotación.
        // Aquí tienes la solución sin cuaterniones.
    }

    public virtual void LateUpdate()
    {
        this.steer = arbitro.GetSteering(this);
    }
}
