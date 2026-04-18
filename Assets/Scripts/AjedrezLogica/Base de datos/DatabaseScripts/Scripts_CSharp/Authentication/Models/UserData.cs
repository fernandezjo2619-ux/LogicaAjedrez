using System;
using UnityEngine;

/// <summary>
/// Modelo de datos para usuario
/// Almacena toda la información del usuario en la base de datos
/// </summary>
[System.Serializable]
public class UserData
{
    public int id_usuario;
    public string nombre_usuario;
    public string email;
    public bool email_verificado;
    public string avatar_url;
    public string biografia;
    public int rating_elo;
    public DateTime fecha_registro;
    public DateTime ultimo_login;
    public int id_partida_activa;  // NULL si no está jugando
    public string estado;  // 'activo', 'inactivo', 'suspendido'
}

/// <summary>
/// Respuesta de login desde Supabase
/// </summary>
[System.Serializable]
public class LoginResponse
{
    public bool exito;
    public int id_usuario;
    public string nombre_usuario;
    public string avatar_url;
    public string mensaje;
    public string token_verificacion;  // Si el email no está verificado
}

/// <summary>
/// Respuesta de registro desde Supabase
/// </summary>
[System.Serializable]
public class RegisterResponse
{
    public bool exito;
    public int id_usuario;
    public string mensaje;
    public string token_verificacion;
}

/// <summary>
/// Respuesta de verificación de email
/// </summary>
[System.Serializable]
public class VerificationResponse
{
    public bool exito;
    public string mensaje;
}

/// <summary>
/// Respuesta de recuperación de contraseña
/// </summary>
[System.Serializable]
public class PasswordRecoveryResponse
{
    public bool exito;
    public string mensaje;
}

/// <summary>
/// Respuesta de cambio de contraseña
/// </summary>
[System.Serializable]
public class PasswordResetResponse
{
    public bool exito;
    public string mensaje;
}

/// <summary>
/// Sesión del usuario (JWT y tokens)
/// </summary>
[System.Serializable]
public class SessionData
{
    public int id_usuario;
    public string nombre_usuario;
    public string token_jwt;  // Token de acceso
    public string token_refresh;  // Token para renovar JWT
    public DateTime fecha_expiracion;
    public string dispositivo;
    public string ip_acceso;
}

/// <summary>
/// Partida (historial de juegos)
/// </summary>
[System.Serializable]
public class GameData
{
    public int id_partida;
    public int id_usuario_blanco;
    public int id_usuario_negro;
    public DateTime fecha_inicio;
    public DateTime fecha_fin;
    public string resultado;  // 'en_progreso', 'blanco_gana', 'negro_gana', 'tablas', 'abandono'
    public string movimientos;
    public int tiempo_limite_segundos;
}

/// <summary>
/// Notificación del juego
/// </summary>
[System.Serializable]
public class NotificationData
{
    public int id_notificacion;
    public string tipo;  // 'partida_disponible', 'retado', 'partida_iniciada', etc.
    public string titulo;
    public string mensaje;
    public bool leida;
    public DateTime fecha_creacion;
    public string datos_adicionales;  // JSON adicional
}

/// <summary>
/// Avatar disponible para el usuario
/// </summary>
[System.Serializable]
public class AvatarData
{
    public int id_avatar;
    public string nombre_avatar;
    public string url_imagen;
    public string descripcion;
}

/// <summary>
/// Lista genérica de respuestas
/// </summary>
[System.Serializable]
public class ListResponse<T>
{
    public T[] data;
    public bool exito;
    public string mensaje;
}
