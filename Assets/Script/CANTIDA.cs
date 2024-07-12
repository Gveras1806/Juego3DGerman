using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CANTIDA : MonoBehaviour
{
    private int enemyCount;
    public Text enmicon;
    void Start()
    {
        // Contar todos los enemigos en la escena al inicio
        enemyCount = GameObject.FindGameObjectsWithTag("Enemigo").Length;

        enmicon.text = "enemigo restante " + enemyCount.ToString();
    }

    public void OnEnemyKilled()
    {
        enemyCount--;

        // Si no quedan enemigos, cargar el siguiente nivel
        if (enemyCount <= 0)
        {
            LoadNextLevel();
        }
    }

    void LoadNextLevel()
    {
        // Asume que los niveles están numerados consecutivamente en el Build Settings
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;

        // Si hay un siguiente nivel, cargarlo
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            Debug.Log("No hay más niveles por cargar. Juego completado.");
        }
    }
}
