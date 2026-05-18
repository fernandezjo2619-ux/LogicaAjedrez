using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIHabilidades : MonoBehaviour
{
    [Header("Scripts")]
    public PruebaObtenerHabilidades bd; // Referencia al script de BD
    public CambiarHabilidadSeleccionada cambiarHabilidadScript; // Referencia al script para cambiar habilidad
    // Constante para asignar un ID de usuario por defecto si no se encuentra en PlayerPrefs
    private const int ID_USUARIO_POR_DEFECTO = 1;

    private int idTipoSeleccionado = -1; // Almacena el ID del tipo de pieza seleccionado actualmente
    private int idUsuarioLocal = -1;     // Almacena el ID del usuario local

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
        // Obtener el ID del usuario local (puede ser asignado previamente en el inicio de sesión o lobby)
        idUsuarioLocal = PlayerPrefs.GetInt("LocalPlayerId", -1);
        
        // Fallback en caso de que no haya ID (para pruebas en el editor)
        if (idUsuarioLocal <= 0)
        {
            Debug.LogWarning($"[UIHabilidades] ID de usuario local no encontrado en PlayerPrefs. Usando ID {ID_USUARIO_POR_DEFECTO} por defecto.");
            idUsuarioLocal = ID_USUARIO_POR_DEFECTO;
        }

        if (bd == null)
        {
            Debug.LogError("No se asignó PruebaObtenerHabilidades en UIHabilidades");
            return;
        }
        
        if (cambiarHabilidadScript == null)
        {
            Debug.LogWarning("[UIHabilidades] No se asignó CambiarHabilidadSeleccionada en el inspector.");
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
        idTipoSeleccionado = idTipo; // Guardar el tipo de pieza seleccionado
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
            
            // Colores para feedback visual
            ColorBlock cb = btn.colors;
            Color originalColor = cb.normalColor;
            
            btn.onClick.AddListener(() =>
            {
                // Actualizar labels
                labelNombre.text = h.nombre_habilidad;
                labelDescripcion.text = h.descripcion;

                if (cambiarHabilidadScript != null && idUsuarioLocal > 0 && idTipoSeleccionado > 0)
                {
                    // Deshabilitar botón temporalmente para dar feedback visual
                    btn.interactable = false;
                    
                    // Iniciar petición
                    StartCoroutine(cambiarHabilidadScript.CambiarHabilidad(
                        idUsuarioLocal,
                        idTipoSeleccionado,
                        h.id_habilidad,
                        () => 
                        {
                            Debug.Log($"[UIHabilidades] Habilidad '{h.nombre_habilidad}' guardada exitosamente.");
                            // Restaurar botón en caso de éxito
                            if (btn != null) btn.interactable = true;
                        },
                        (error) => 
                        {
                            Debug.LogError($"[UIHabilidades] Error al guardar la habilidad: {error}");
                            // Restaurar botón incluso si hay error
                            if (btn != null) btn.interactable = true;
                        }
                    ));
                }
                else
                {
                    Debug.LogWarning("[UIHabilidades] No se puede cambiar la habilidad: faltan referencias o IDs.");
                }
            });
        }

        // Seleccionar automáticamente la primera habilidad
        labelNombre.text = listaHabilidades[0].nombre_habilidad;
        labelDescripcion.text = listaHabilidades[0].descripcion;
    }
}
