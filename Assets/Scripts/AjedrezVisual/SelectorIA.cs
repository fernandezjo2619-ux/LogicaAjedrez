
using UnityEngine;
using UnityEngine.SceneManagement;

public class SelectorIA : MonoBehaviour
{
    public GameObject panelSeleccionIA;
    public int idJugador = 0; // Master
    public GameObject interior;

    public void MostrarPanel()
    {
        panelSeleccionIA.SetActive(true);
        interior.SetActive(false);
    }

    public void VolverPanel()
    {
        panelSeleccionIA.SetActive(false);
        interior.SetActive(true);
    }

    public void SeleccionarIA(int idIA)
    {
        ConfigPartida.vsIA = true;
        ConfigPartida.idIA = idIA;
        
        // Cargar el ID del usuario si ha iniciado sesión, de lo contrario usar el default 0
        int loggedInId = PlayerPrefs.GetInt("LocalPlayerId", 0);
        ConfigPartida.idJugador1 = loggedInId > 0 ? loggedInId : idJugador;

        SceneManager.LoadScene("PartidaTablero");
    }
}
