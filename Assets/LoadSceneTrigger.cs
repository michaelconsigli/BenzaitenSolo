using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSceneTrigger : MonoBehaviour
{
    public string nomeScena;

    private void OnTriggerEnter(Collider other)
    {
        SceneManager.LoadScene(nomeScena);
    }
}
