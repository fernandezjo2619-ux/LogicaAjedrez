using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CasillaPrefab : MonoBehaviour
{
    // Atributos de color y nº de casilla
    public Material colorCasilla;   // Material
    public string nombreCasilla;    //Nombre A1

    public int fila;                 // 0 a 7
    public int columna;              // 0 a 7

    private Renderer rendMaterial;
    private Material materialActual;

    void Awake()
    {
        rendMaterial = GetComponent<Renderer>();

        // Guardar color original de la casilla
        if (colorCasilla != null)
        {
            //Copia del material original
            materialActual = new Material(colorCasilla);
            rendMaterial.material = materialActual;
        }
        else
        {
            materialActual = new Material(rendMaterial.material);
            rendMaterial.material = materialActual;
        }
    }

    private void OnMouseDown()
    {
        //ControlTablero.Instance.SeleccionarCasilla(this);

        Debug.Log("Has clickeado: " + nombreCasilla +
                  " | Fila: " + fila + " Columna: " + columna);// Imprime el número de la casilla al hacer clic

    }
    public void Resaltar(Material azul)
    {
        if (rendMaterial != null && azul != null)
        {
            rendMaterial.material = new Material(azul);
        }
    }

    public void ResetColor()
    {
        if (rendMaterial != null && materialActual != null)
        {
            rendMaterial.material = materialActual;
        }
    }

}
