using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CrearCasillas : MonoBehaviour
{
    //Prefab casilla
    public GameObject casillasPrefab;
    //Guardar casilla creada
    public static CasillaPrefab[,] casillas;

    //Tamaño del tablero
    public int ancho = 8;
    public int alto = 8;

    //Colores del tablero
    public Material color1;
    public Material color2;

    //Creación del tablero, Awake
    private void Awake()
    {
        casillas = new CasillaPrefab[ancho, alto]; //Posicion de la casilla
        CrearTablero();
    }

    public void CrearTablero()
    {
        for (int i = 0; i < ancho; i++)
        {
            for (int j = 0; j < alto; j++)
            {
                GameObject casilla = Instantiate(casillasPrefab, new Vector3(i, 0, j), Quaternion.identity);

                //Agregar materiales por posición
                Material matAsignado = (i + j) % 2 == 0 ? color1 : color2;
                Renderer rend = casilla.GetComponent<Renderer>();
                rend.material = matAsignado;

                //Funcion nombre de las casillas A1, B2, etc.
                string nombre = NombreCasilla(i, j);
                casilla.name = nombre;

                //Asignar valores al script Casilla
                CasillaPrefab scriptCasilla = casilla.GetComponent<CasillaPrefab>();
                scriptCasilla.nombreCasilla = nombre;
                scriptCasilla.colorCasilla = matAsignado;
                scriptCasilla.fila = j; // + 1 
                scriptCasilla.columna = i; // + 1

                //Guardamos casilla
                casillas[i, j] = scriptCasilla;

            }
        }
    }

    string NombreCasilla(int x, int z)
    {
        char letra = (char)('A' + x); // Pasa de num a letra 0 -> A
        int num = z + 1;
        return letra.ToString() + num.ToString();
    }
}
