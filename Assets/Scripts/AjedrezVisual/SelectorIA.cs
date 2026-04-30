
using UnityEngine;
using UnityEngine.SceneManagement;

public class SelectorIA : MonoBehaviour
{
    public GameObject panelSeleccionIA;
    public int idJugador = 1;
    public GameObject interior;

    public void MostrarPanel()
    {
        panelSeleccionIA.SetActive(true);
        interior.SetActive(false);
    }

    public void SeleccionarIA(int idIA)
    {
        ConfigPartida.vsIA = true;
        ConfigPartida.idIA = idIA;
        ConfigPartida.idJugador = idJugador;

        SceneManager.LoadScene("PartidaTablero");
    }
}
