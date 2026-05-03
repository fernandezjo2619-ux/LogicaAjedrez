
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
        ConfigPartida.idJugador1 = idJugador;

        SceneManager.LoadScene("PartidaTablero");
    }
}
