using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuInicio : MonoBehaviour
{
    public void JugarVsIA()
    {
        ConfigPartida.vsIA = true;
        UnityEngine.SceneManagement.SceneManager.LoadScene("PartidaTablero");
    }

    public void JugarVsJugador()
    {
        ConfigPartida.vsIA = false;
        UnityEngine.SceneManagement.SceneManager.LoadScene("PartidaTablero");
    }
}
