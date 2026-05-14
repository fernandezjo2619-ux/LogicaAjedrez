using UnityEngine;

public class MenuInicio : MonoBehaviour
{
    public void JugarVsIA()
    {
        GetComponent<SelectorIA>().MostrarPanel();
        //ConfigPartida.vsIA = true;
        //UnityEngine.SceneManagement.SceneManager.LoadScene("PartidaTablero");
    }

    public void JugarVsJugador()
    {
        GetComponent<LobbyUIController>().MostrarPanel();
        //ConfigPartida.vsIA = false;
        //UnityEngine.SceneManagement.SceneManager.LoadScene("PartidaTablero");
    }
}
