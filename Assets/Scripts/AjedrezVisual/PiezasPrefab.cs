<<<<<<< HEAD

using LogicProject.Recursos.Class;
using LogicProject.Recursos.Enum;
using UnityEngine;

public class PiezasPrefab : MonoBehaviour
{
    public TipoPieza tipo;    // Tipo de pieza, de recursos
    public ColorPieza color;  // Color, de recursos
    public int fila;          // Posición fila en el tablero visual
    public int columna;       // Posición columna en el tablero visual

    public Pieza piezaLogica; // Referencia a la pieza lógica

    private void Start()
    {
        Renderer rend = GetComponent<Renderer>();

        // Pone Albedo en blanco para que se vea la imagen
        Color currentAlbedo = rend.material.GetColor("_Color");
        if (currentAlbedo == Color.black)
        {
            rend.material.SetColor("_Color", Color.white);  // Pone el Albedo a blanco
        }
    }

    // Inicializa la pieza con la lógica
    public void Inicializar(Pieza piezaLogica)
    {
        this.piezaLogica = piezaLogica;

        tipo = piezaLogica.Tipo;
        color = piezaLogica.Color;
        columna = piezaLogica.Posicion.X;
        fila = piezaLogica.Posicion.Y;
        transform.position = new Vector3(columna, 0.5f, fila); //intercambiarlas mueve posicion de las piezas

        //Dirección a la que mira
        float rotY = (color == ColorPieza.Blanco) ? 90f : -90f;
        transform.rotation = Quaternion.Euler(0f, rotY, 0f);

        Renderer rend = GetComponent<Renderer>();

        foreach (var mat in rend.materials)
        {
            if (mat.name.Contains("ColorPieza"))
            {
                mat.color = (color == ColorPieza.Blanco)
                    ? Color.white
                    : Color.black;
            }
        }
    }

    // Obtener el nombre de la textura basado en el tipo de pieza y color
    private string GetTextureName(TipoPieza tipo, ColorPieza color)
    {
        // Determina el sufijo de la textura con base en el tipo y color
        string colorSufijo = (color == ColorPieza.Blanco) ? "Blanco" : "Negro";
        string tipoPrefijo = tipo.ToString();

        // Retorna el nombre de la textura
        return tipoPrefijo + colorSufijo;
    }

    private void OnMouseDown()
    {
        SeleccionarPieza.Instance.SeleccionarDesdePieza(this);
    }
}
=======
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AjedrezLogica;
using AjedrezLogica.Recursos;

public class PiezasPrefab : MonoBehaviour
{
    public TipoPieza tipo;    // Tipo de pieza, de recursos
    public ColorPieza color;  // Color, de recursos
    public int fila;          // Posición fila en el tablero visual
    public int columna;       // Posición columna en el tablero visual

    public Pieza piezaLogica; // Referencia a la pieza lógica

    private void Start()
    {
        Renderer rend = GetComponent<Renderer>();

        // Pone Albedo en blanco para que se vea la imagen
        Color currentAlbedo = rend.material.GetColor("_Color");
        if (currentAlbedo == Color.black)
        {
            rend.material.SetColor("_Color", Color.white);  // Pone el Albedo a blanco
        }
    }

    // Inicializa la pieza con la lógica
    public void Inicializar(Pieza piezaLogica)
    {
        this.piezaLogica = piezaLogica;

        tipo = piezaLogica.Tipo;
        color = piezaLogica.Color;
        columna = piezaLogica.Posicion.X;
        fila = piezaLogica.Posicion.Y;
        transform.position = new Vector3(columna, 0.5f, fila); //intercambiarlas mueve posicion de las piezas

        // Cargar y asignar la textura según el tipo de pieza y color
        string textureName = GetTextureName(tipo, color);

        // Cargar la textura desde los recursos
        Texture texture = Resources.Load<Texture>("Textures/" + textureName);

        // Opcional: cambiar el material según el color
        Renderer rend = GetComponent<Renderer>();
        if (rend != null && texture != null)
        {
            rend.material.mainTexture = texture;

            // Pone Albedo en blanco para que se vea la imagen
            Color currentAlbedo = rend.material.GetColor("_Color");
            if (currentAlbedo == Color.black)
            {
                rend.material.SetColor("_Color", Color.white);  // Pone el Albedo a blanco
            }
        }
    }

    // Obtener el nombre de la textura basado en el tipo de pieza y color
    private string GetTextureName(TipoPieza tipo, ColorPieza color)
    {
        // Determina el sufijo de la textura con base en el tipo y color
        string colorSufijo = (color == ColorPieza.Blanco) ? "Blanco" : "Negro";
        string tipoPrefijo = tipo.ToString();

        // Retorna el nombre de la textura
        return tipoPrefijo + colorSufijo;
    }

    private void OnMouseDown()
    {
        SeleccionarPieza.Instance.SeleccionarDesdePieza(this);
    }
}
>>>>>>> origin/Raquel
