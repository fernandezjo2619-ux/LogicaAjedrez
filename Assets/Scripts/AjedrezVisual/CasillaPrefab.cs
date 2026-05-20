using UnityEngine;

public class CasillaPrefab : MonoBehaviour
{
    // Atributos de color y nº de casilla
    public Material colorCasilla;   // Material
    public string nombreCasilla;    //Nombre A1

    public int fila;                 // 0 a 7
    public int columna;              // 0 a 7

    private Renderer rendMaterial;

    void Awake()
    {
        rendMaterial = GetComponent<Renderer>();

        // Guardar color original de la casilla
        if (colorCasilla != null)
        {
            //Copia del material original
            rendMaterial.material = colorCasilla;
        }
    }

    private void OnMouseDown()
    {
        //ControlTablero.Instance.SeleccionarCasilla(this);
        Debug.Log("Has clickeado: " + nombreCasilla +
                  " | Fila: " + fila + " Columna: " + columna);// Imprime el número de la casilla al hacer clic

        //Acceder a la lógica
        //CasillaTablero casillaLogica = CrearPiezas.Instance.juego.Tablero.Grid[columna, fila];
        SeleccionarPieza.Instance.Mover(this);
    }

    // Actualiza visual según la lógica
    public void ActualizarVisual()
    {
        var casilla = CrearPiezas.Instance.juego.Tablero.Grid[columna, fila];

        if (casilla.EstaOcupado)
        {
            // Aquí podrías mostrar modelo de pieza 3D
            Debug.Log(nombreCasilla + " tiene pieza");
        }
        else
        {
            // Casilla vacía, ocultar pieza si hay
            Debug.Log(nombreCasilla + " vacía");
        }
    }

    public void Resaltar(Material materialResaltado)
    {
        if (rendMaterial != null && materialResaltado != null)
        {
            rendMaterial.material = materialResaltado;
        }
    }

    public void ResetColor()
    {
        if (rendMaterial != null && colorCasilla != null)
        {
            rendMaterial.material = colorCasilla;
        }
    }

}
