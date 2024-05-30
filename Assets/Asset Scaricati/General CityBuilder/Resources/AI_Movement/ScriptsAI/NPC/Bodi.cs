using UnityEngine;

public class Bodi : MonoBehaviour
{

    [SerializeField] protected float _maxSpeed = 8;
    [SerializeField] protected float _maxSpeedDefault = 5;
    [SerializeField] protected float _maxRotation = 360;
    [SerializeField] protected float _maxAcceleration = 15;
    [SerializeField] protected float _maxAccelerationDefault = 15;
    [SerializeField] protected float _maxAngularAcc = 60;
    [SerializeField] public float anchura = 1f;

    //Los personajes se mueven en 2 dimensiones pero pueden tener una cierta altura en la escena
    [SerializeField] public float alturaEnEscena = 0f;

    protected Vector3 _acceleration; // aceleración lineal
    protected float _angularAcc;  // aceleración angular
    protected Vector3 _velocity; // velocidad lineal
    protected float _rotation;  // velocidad angular
    public float _speed;  // velocidad escalar
    [SerializeField]
    protected float _orientation;  // 'posición' angular -> En rango (0, 360)

    public float MaxSpeed
    {
        get { return _maxSpeed; }
        set { _maxSpeed = Mathf.Max(0, value); }
    }

    public float MaxSpeedDefault
    {
        get { return _maxSpeedDefault; }
        set { _maxSpeedDefault = Mathf.Max(0, value); }
    }

    public Vector3 Velocity
    {
        get { return _velocity;  }
        set 
        {
            if(value.magnitude > _maxSpeed){ 
                _velocity = (value / value.magnitude) * _maxSpeed;
            }else if(value.magnitude < 0.001){
                _velocity = Vector3.zero;
            }else{
                _velocity = value;
            }

            Speed = _velocity.magnitude;
        }
    }
    
    public float MaxRotation
    {
        get { return _maxRotation; }
        set { _maxRotation = Mathf.Max(0, value); }
    }

    public float Rotation
    {
        get { return _rotation; }
        set { _rotation = Mathf.Min(Mathf.Max(value, -_maxRotation), _maxRotation); }
    }

    public float MaxAcceleration
    {
        get { return _maxAcceleration; }
        set { _maxAcceleration = Mathf.Max(0, value); }
    }

    public float MaxAccelerationDefault
    {
        get { return _maxAccelerationDefault; }
        set { _maxAccelerationDefault = Mathf.Max(0, value); }
    }

    public float MaxAngularAcc
    {
        get { return _maxAngularAcc; }
        set { _maxAngularAcc = Mathf.Max(0, value); }
    }

    public Vector3 Acceleration
    {
        get { return _acceleration; }
        set 
        {
            float maxAcc = _maxAcceleration;
            float magn = value.magnitude;
            float acc = Mathf.Min(Mathf.Max(magn, -maxAcc), maxAcc);
            if (magn < 0.001f && magn > -0.001f)
            {
                _acceleration = Vector3.zero;
            }
            else if(acc == magn)
            {
                _acceleration = value;
            }
            else
            {
                _acceleration = (value / magn) * acc;
            }
        }
    }

    public float AngularAcc
    {
        get { return _angularAcc; }
        set 
        {
            if (value < 0.001 && value > -0.001) _angularAcc = 0;
            else _angularAcc = Mathf.Min(Mathf.Max(value, -_maxAngularAcc), _maxAngularAcc); 
        }
    }

    //Position -> Esta es la única propiedad que trabaja sobre transform.
    public Vector3 Position
    {
        get { return transform.position; }
        set { 
            transform.position = new Vector3(value.x, alturaEnEscena, value.z); 
        }
    }

    public float Orientation
    {
        get { return _orientation; }
        set {
            _orientation = value % 360; 

            if(_orientation < 0){
                _orientation = _orientation + 360;
            }
        }
    }

    // Es una propiedad calculada a partir del Velocity
    public float Speed
    {
        get { return _velocity.magnitude; }
        set { _speed = value; }
    }

    public float Heading() //Retorna el ángulo heading en (-180, 180) -> En grados. Suponemos que estaba en el rango (0, 360)
    {
        if(_orientation > 180){
            return (_orientation - 360);
        }else{
            return _orientation;
        }
    }

    public static float Heading(float rotacion) //Retorna el ángulo pasado como parámetro en (-180, 180)
    {
        if(rotacion > 180){
            return (rotacion - 360);
        }else{
            return rotacion;
        }
    }
    
    public static float MapToRange(float rotation) //Retorna un ángulo de (-180, 180) a (0, 360) expresado en grado or radianes
    {
        if(rotation < 0){
            return (rotation + 360);
        }else{
            return rotation;
        }
    }
    
    public float MapToRange() //Retorna la orientación de este bodi, un ángulo de (-180, 180), a (0, 360) expresado en grado or radianes
    {
        if(_orientation < 0){
            return (_orientation + 360);
        }else{
            return _orientation;
        }
    }
    
    public Vector3 OrientationToVector() //Retorna un vector a partir de nuestra orientación usando Z como primer eje
    {
        return Quaternion.Euler(0, Orientation, 0) * Vector3.forward;
    }

    public static Vector3 OrientationToVector(float Orient) // Retorna un vector a partir de una orientación usando Z como primer eje
    {
        return Quaternion.Euler(0, Orient, 0) * Vector3.forward;
    }

    public void ResetOrientation() { // Resetea la orientación del bodi
        _orientation = 0;
    }
}
