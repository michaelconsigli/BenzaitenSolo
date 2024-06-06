using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public Transform player; // Assegna il giocatore (o la sua fotocamera) a questa variabile nell'Inspector
    public float loadDistance = 50f; // Distanza alla quale caricare le scene
    private Vector2 currentChunk = new Vector2(-1, -1); // Chunk corrente del giocatore

    // Dizionario per memorizzare i nomi delle scene e le loro coordinate chunk
    private Dictionary<Vector2, string> scenes = new Dictionary<Vector2, string>
    {
        { new Vector2(0, 0), "City_Do_Est" },
        { new Vector2(1, 0), "City_Do_Nord" },
        { new Vector2(0, 1), "City_Do_Sud" },
        //{ new Vector2(1, 0), "City_Do_Mare" },
        //{ new Vector2(1, 0), "City_Do_Lago" }
        // Aggiungi altri chunk e scene se necessario
    };

    void Update()
    {
        // Calcola la posizione del giocatore in termini di chunk
        Vector2 newChunk = new Vector2(
            Mathf.FloorToInt(player.position.x / loadDistance),
            Mathf.FloorToInt(player.position.z / loadDistance)
        );

        // Se il giocatore è entrato in un nuovo chunk, aggiorna i chunk caricati
        if (newChunk != currentChunk)
        {
            LoadSurroundingChunks(newChunk);
            currentChunk = newChunk;
        }
    }

    // Carica i chunk circostanti al chunk centrale
    void LoadSurroundingChunks(Vector2 centerChunk)
    {
        List<Vector2> chunksToLoad = new List<Vector2>();
        List<Vector2> chunksToUnload = new List<Vector2>(scenes.Keys);

        // Determina i chunk da caricare e scaricare
        for (int x = -1; x <= 1; x++)
        {
            for (int z = -1; z <= 1; z++)
            {
                Vector2 chunk = new Vector2(centerChunk.x + x, centerChunk.y + z);
                if (scenes.ContainsKey(chunk))
                {
                    chunksToLoad.Add(chunk);
                    chunksToUnload.Remove(chunk);
                }
            }
        }

        // Carica i nuovi chunk
        foreach (var chunk in chunksToLoad)
        {
            LoadScene(chunk);
        }

        // Scarica i chunk non più necessari
        foreach (var chunk in chunksToUnload)
        {
            UnloadScene(chunk);
        }
    }

    // Carica una scena in modo additivo
    void LoadScene(Vector2 chunk)
    {
        string sceneName = scenes[chunk];
        if (!SceneManager.GetSceneByName(sceneName).isLoaded)
        {
            StartCoroutine(LoadSceneAsync(sceneName));
        }
    }

    // Scarica una scena
    void UnloadScene(Vector2 chunk)
    {
        string sceneName = scenes[chunk];
        if (SceneManager.GetSceneByName(sceneName).isLoaded)
        {
            StartCoroutine(UnloadSceneAsync(sceneName));
        }
    }

    // Coroutine per caricare una scena in modo additivo
    IEnumerator LoadSceneAsync(string sceneName)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }

    // Coroutine per scaricare una scena
    IEnumerator UnloadSceneAsync(string sceneName)
    {
        AsyncOperation asyncUnload = SceneManager.UnloadSceneAsync(sceneName);
        while (!asyncUnload.isDone)
        {
            yield return null;
        }
    }
}


