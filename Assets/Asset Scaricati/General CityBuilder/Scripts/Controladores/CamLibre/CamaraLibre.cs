using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamaraLibre : MonoBehaviour
{

    public Vector2 sensibilidad;
    public Transform camara;
    public Camera freeCam;
    public Camera playerCam;
    public Controlador2 player;
    public float MaxSpeed = 3f;

    private bool activated = false;

    void Start(){
        activated = false;
        camara = transform.Find("CamaraLibre");
    }

    void Update(){
        if(activated){
            float horizontal = Input.GetAxis("Mouse X");
            float vertical = Input.GetAxis("Mouse Y");

            if (horizontal != 0)
            {
                transform.Rotate(Vector3.up * horizontal * sensibilidad.x);
            }

            if (vertical != 0)
            {
                float angulo = camara.localEulerAngles.x - vertical * sensibilidad.y;
                camara.localEulerAngles = Vector3.right * angulo;
            }

            Vector3 direccionCamara = camara.forward;
            direccionCamara = direccionCamara.normalized;

            float horizontalInput = Input.GetAxisRaw("Horizontal");
            float verticalInput = Input.GetAxisRaw("Vertical");
            Vector3 playerDirection = new Vector3(horizontalInput, 0f, verticalInput);
            playerDirection = playerDirection.normalized;

            Quaternion targetRotation = Quaternion.LookRotation(direccionCamara);
            playerDirection = targetRotation * playerDirection;
            transform.Translate(playerDirection * MaxSpeed * Time.deltaTime, Space.World);
        }

        if(Input.GetKeyDown(KeyCode.C)){
            activated = !activated;

            if(activated){
                freeCam.enabled = true;
                playerCam.enabled = false;
                player.activated = false;
            }else{
                freeCam.enabled = false;
                playerCam.enabled = true;
                player.activated = true;
            }
        }
    }
}
