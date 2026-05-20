using UnityEngine;
using TMPro;

public class VictoriaUI : MonoBehaviour
{
    [Header("UI Elements")]
    [Tooltip("Asigna aquí el objeto TextMeshPro de la UI que mostrará el ganador")]
    public TMP_Text textoGanador;

    void Start()
    {
        // Recuperar el ID del ganador guardado al terminar la partida
        int idGanador = PlayerPrefs.GetInt("IdGanador", 0);

        if (textoGanador != null)
        {
            if (idGanador > 0)
            {
                textoGanador.text = $"¡El Ganador es el Jugador {idGanador}!";
                Debug.Log($"[VictoriaUI] Mostrando ganador: Jugador {idGanador}");
            }
            else
            {
                textoGanador.text = "¡La partida ha terminado!";
                Debug.LogWarning("[VictoriaUI] No se encontró el ID del ganador en PlayerPrefs.");
            }
        }
        else
        {
            Debug.LogError("[VictoriaUI] La etiqueta 'textoGanador' no está asignada en el inspector.");
        }
    }

    /// <summary>
    /// Función opcional por si quieres poner un botón para volver al menú.
    /// Solo tienes que asignarla al evento OnClick de tu botón.
    /// </summary>
    public void VolverAlMenu()
    {
        // Asegúrate de que el nombre de la escena del menú coincide
        UnityEngine.SceneManagement.SceneManager.LoadScene("MenuInicio"); 
    }
}
