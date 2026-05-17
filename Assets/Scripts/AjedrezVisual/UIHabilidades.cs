using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIHabilidades : MonoBehaviour
{
    [Header("Scripts")]
    public PruebaObtenerHabilidades bd; // Referencia al script de BD

    [Header("Botones de tipos")]
    public Button[] botonesTipos; // Los botones superiores, 6 en tu caso

    [Header("Scroll")]
    public Transform scrollContent; // Content del Scroll
    public GameObject habilidadPrefab; // Prefab de un botón para cada habilidad

    [Header("Detalles de habilidad")]
    public TMP_Text labelNombre;
    public TMP_Text labelDescripcion;

    void Start()
    {
        if (bd == null)
        {
            Debug.LogError("No se asignó PruebaObtenerHabilidades en UIHabilidades");
            return;
        }

        // Primero obtener los tipos de piezas de la BD
        StartCoroutine(AsignarBotonesATipos());
    }

    IEnumerator AsignarBotonesATipos()
    {
        // Esperar a que bd obtenga los tipos de pieza
        yield return StartCoroutine(bd.ObtenerTiposPiezas());

        List<TipoPiezaData> tiposObtenidos = bd.ObtenerListaTipos();

        if (tiposObtenidos.Count == 0)
        {
            Debug.LogWarning("No se encontraron tipos de pieza en la BD");
            yield break;
        }

        // Asignar cada tipo de pieza a un botón
        for (int i = 0; i < botonesTipos.Length && i < tiposObtenidos.Count; i++)
        {
            int idTipo = tiposObtenidos[i].id_tipo; // ⚡ ID real
            string nombre = tiposObtenidos[i].nombre_tipo;

            // Cambiar el texto del botón
            TMP_Text txt = botonesTipos[i].GetComponentInChildren<TMP_Text>();
            if (txt != null) txt.text = nombre;

            // Limpiar listeners previos
            botonesTipos[i].onClick.RemoveAllListeners();

            // Asignar evento al botón usando el ID real
            botonesTipos[i].onClick.AddListener(() => OnClickBotonTipo(idTipo));
        }
    }

    /// Evento al pulsar un botón de tipo de pieza
    void OnClickBotonTipo(int idTipo)
    {
        Debug.Log("Botón tipo " + idTipo + " pulsado");
        StartCoroutine(ObtenerYMostrarHabilidades(idTipo));
    }

    /// Obtiene las habilidades de la BD y las muestra en el scroll
    IEnumerator ObtenerYMostrarHabilidades(int idTipo)
    {
        // Obtener habilidades desde Supabase
        yield return StartCoroutine(bd.ObtenerYGuardarHabilidades(idTipo));

        // Limpiar scroll
        foreach (Transform child in scrollContent)
        {
            Destroy(child.gameObject);
        }

        // Obtener lista de habilidades
        List<DatosHabilidad> listaHabilidades = bd.ObtenerListaHabilidades();

        if (listaHabilidades.Count == 0)
        {
            Debug.LogWarning("No se encontraron habilidades para id_tipo " + idTipo);
            labelNombre.text = "";
            labelDescripcion.text = "";
            yield break;
        }

        // Llenar scroll con botones de habilidades
        foreach (DatosHabilidad h in listaHabilidades)
        {
            GameObject go = Instantiate(habilidadPrefab, scrollContent);

            // Cambiar el texto del botón a TMP_Text
            TMP_Text txt = go.GetComponentInChildren<TMP_Text>();
            if (txt != null) txt.text = h.nombre_habilidad;

            // Asignar evento al botón
            Button btn = go.GetComponent<Button>();
            btn.onClick.AddListener(() =>
            {
                labelNombre.text = h.nombre_habilidad;
                labelDescripcion.text = h.descripcion;
            });
        }

        // Seleccionar automáticamente la primera habilidad
        labelNombre.text = listaHabilidades[0].nombre_habilidad;
        labelDescripcion.text = listaHabilidades[0].descripcion;
    }
}
