using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuInicio : MonoBehaviour
{
    [Tooltip("Nombre exacto de la escena del lobby multijugador (debe estar en Build Settings)")]
    [SerializeField] private string lobbySceneName = "LobbyScene";

    public void JugarVsIA()
    {
        GetComponent<SelectorIA>().MostrarPanel();
    }

    /// <summary>
    /// Abre el lobby multijugador donde el jugador puede crear o unirse a una sala.
    /// </summary>
    public void JugarVsJugador()
    {
        // Limpiar datos de partidas anteriores para que no interfieran
        PlayerPrefs.DeleteKey("LocalPlayerId");
        PlayerPrefs.DeleteKey("IdJugador1");
        PlayerPrefs.DeleteKey("IdJugador2");
        PlayerPrefs.DeleteKey("IdPartida");
        PlayerPrefs.Save();

        ConfigPartida.vsIA = false;
        SceneManager.LoadScene("PartidaTablero");
        //GetComponent<LobbyUIController>().MostrarPanel();
        //ConfigPartida.vsIA = false;
        //UnityEngine.SceneManagement.SceneManager.LoadScene("PartidaTablero");
    }
}
