using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IACamsController : MonoBehaviour
{
    public Camera birdsCam;
    public List<Camera> villagersCams;
    public bool existsBirds;

    private bool isBirdCamActive = true;
    private bool isCamSystemActive = true;
    private int currentVillager = 0;

    void Start(){
        isBirdCamActive = true;
        isCamSystemActive = true;
        currentVillager = 0;
    }

    void Update(){
        if(Input.GetKeyDown(KeyCode.X) && isCamSystemActive && (!existsBirds || !isBirdCamActive)){
            villagersCams[currentVillager].enabled = false;

            if((currentVillager+1) == villagersCams.Count){
                currentVillager = 0;
            }else{
                currentVillager++;
            }

            villagersCams[currentVillager].enabled = true;
        }else if(Input.GetKeyDown(KeyCode.Z) && isCamSystemActive && (!existsBirds || !isBirdCamActive)){
            villagersCams[currentVillager].enabled = false;

            if((currentVillager-1) == -1){
                currentVillager = villagersCams.Count-1;
            }else{
                currentVillager--;
            }

            villagersCams[currentVillager].enabled = true;
        }else if(Input.GetKeyDown(KeyCode.P) && isCamSystemActive && existsBirds){
            isBirdCamActive = !isBirdCamActive;

            if(isBirdCamActive){
                villagersCams[currentVillager].enabled = false;
                birdsCam.enabled = true;
            }else{
                villagersCams[currentVillager].enabled = true;
                birdsCam.enabled = false;
            }
        }else if(Input.GetKeyDown(KeyCode.E)){
            if(!existsBirds || !isBirdCamActive){
                villagersCams[currentVillager].enabled = !villagersCams[currentVillager].enabled;
            }else{
                birdsCam.enabled = !birdsCam.enabled;
            }

            isCamSystemActive = !isCamSystemActive;
        }
    }
}
