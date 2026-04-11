using System;
using System.Collections.Generic;

/// <summary>
/// Estructura para parsear las habilidades devueltas por Supabase
/// </summary>
[System.Serializable]
public class DatosHabilidad
{
    public int id_habilidad;
    public string nombre_habilidad;
    public string descripcion;
    public int poder_base;
}

/// <summary>
/// Wrapper para parsear array de habilidades del JSON de Supabase
/// </summary>
[System.Serializable]
public class ListaHabilidades
{
    public DatosHabilidad[] habilidades;
}
