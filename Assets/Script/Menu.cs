using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    // Este método se llama cuando se presiona el botón de inicio del juego
    public void StartGame()
    {
        // Asegúrate de que el nombre de la escena sea correcto
        SceneManager.LoadScene(1);
    }

    // Este método se llama cuando se presiona el botón de salir
    public void ExitGame()
    {
        // Funciona en compilaciones de escritorio, pero no en el editor
        Application.Quit();
    }
}
