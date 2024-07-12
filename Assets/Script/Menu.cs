using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    // Este m�todo se llama cuando se presiona el bot�n de inicio del juego
    public void StartGame()
    {
        // Aseg�rate de que el nombre de la escena sea correcto
        SceneManager.LoadScene(1);
    }

    // Este m�todo se llama cuando se presiona el bot�n de salir
    public void ExitGame()
    {
        // Funciona en compilaciones de escritorio, pero no en el editor
        Application.Quit();
    }
}
