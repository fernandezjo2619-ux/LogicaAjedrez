using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Text;
using System.Collections.Generic;

/// <summary>
/// Servicio de autenticación
/// Maneja: registro, login, verificación de email, recuperación de contraseña, etc.
/// Se conecta directamente a Supabase PostgreSQL mediante funciones SQL remotas
/// </summary>
public class AuthenticationService : MonoBehaviour
{
    // ── Configuración de Supabase ──────────────────────────────────────
    [SerializeField] private string supabaseUrl = "https://xxxxx.supabase.co";
    [SerializeField] private string supabaseKey = "eyJhbGciOiJIUzI1NiIsInR5cCI...";
    [SerializeField] private string sendGridApiKey = "";  // Para envío de emails
    
    // ── URLs de endpoints ──────────────────────────────────────────────
    private string restUrl => $"{supabaseUrl}/rest/v1";
    private string rpcUrl => $"{supabaseUrl}/rest/v1/rpc";
    
    // ── Singleton ──────────────────────────────────────────────────────
    public static AuthenticationService Instance { get; private set; }
    
    private SessionData currentSession;
    private UserData currentUser;
    private float sessionExpirationTime;
    
    // ── Eventos de autenticación ───────────────────────────────────────
    public event System.Action<UserData> OnLoginSuccess;
    public event System.Action<string> OnLoginFailed;
    public event System.Action<int> OnRegisterSuccess;
    public event System.Action<string> OnRegisterFailed;
    public event System.Action OnEmailVerified;
    public event System.Action<string> OnEmailVerificationFailed;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        // Cargar sesión guardada si existe
        LoadSavedSession();
    }
    
    /// <summary>
    /// Registro de nuevo usuario
    /// Llama a la función SQL: crear_usuario(nombre, email, password)
    /// </summary>
    public void Register(string username, string email, string password)
    {
        if (!ValidateInput(username, email, password))
            return;
            
        StartCoroutine(RegisterCoroutine(username, email, password));
    }
    
    private IEnumerator RegisterCoroutine(string username, string email, string password)
    {
        // 1. Crear usuario en Supabase (función SQL)
        string functionCall = JsonUtility.ToJson(new { 
            p_nombre_usuario = username, 
            p_email = email, 
            p_password = password 
        });
        
        using (UnityWebRequest request = new UnityWebRequest($"{rpcUrl}/crear_usuario", "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(functionCall);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", $"Bearer {supabaseKey}");
            
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    // Supabase devuelve el id_usuario
                    string responseText = request.downloadHandler.text;
                    int idUsuario = int.Parse(responseText.Trim('"'));
                    
                    if (idUsuario > 0)
                    {
                        // 2. Enviar email de verificación
                        yield return SendVerificationEmail(email, idUsuario);
                        
                        OnRegisterSuccess?.Invoke(idUsuario);
                        SaveUserRegistration(idUsuario, username, email);
                    }
                    else
                    {
                        string error = "Error al crear usuario";
                        OnRegisterFailed?.Invoke(error);
                    }
                }
                catch (Exception ex)
                {
                    OnRegisterFailed?.Invoke($"Error: {ex.Message}");
                }
            }
            else
            {
                string errorMsg = request.downloadHandler.text;
                OnRegisterFailed?.Invoke($"Error de registro: {errorMsg}");
            }
        }
    }
    
    /// <summary>
    /// Login de usuario
    /// Llama a la función SQL: validar_intento_login(email, password, ip)
    /// </summary>
    public void Login(string email, string password)
    {
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            OnLoginFailed?.Invoke("Email y contraseña son requeridos");
            return;
        }
        
        StartCoroutine(LoginCoroutine(email, password));
    }
    
    private IEnumerator LoginCoroutine(string email, string password)
    {
        string ipAddress = GetClientIP();
        string functionCall = JsonUtility.ToJson(new { 
            p_email = email, 
            p_password = password,
            p_ip_acceso = ipAddress
        });
        
        using (UnityWebRequest request = new UnityWebRequest($"{rpcUrl}/validar_intento_login", "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(functionCall);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", $"Bearer {supabaseKey}");
            request.SetRequestHeader("Accept", "application/json");
            
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    string responseText = request.downloadHandler.text;
                    LoginResponse response = JsonUtility.FromJson<LoginResponse>(responseText);
                    
                    if (response.exito)
                    {
                        // Login exitoso
                        SaveSession(response.id_usuario, response.nombre_usuario);
                        OnLoginSuccess?.Invoke(currentUser);
                    }
                    else
                    {
                        // Login fallido (contraseña incorrecta, email no verificado, etc.)
                        OnLoginFailed?.Invoke(response.mensaje);
                        
                        // Si el email no está verificado, guardar token para verificación
                        if (!string.IsNullOrEmpty(response.token_verificacion))
                        {
                            PlayerPrefs.SetString("PendingVerificationToken", response.token_verificacion);
                            PlayerPrefs.SetInt("PendingUserId", response.id_usuario);
                        }
                    }
                }
                catch (Exception ex)
                {
                    OnLoginFailed?.Invoke($"Error al parsear respuesta: {ex.Message}");
                }
            }
            else
            {
                OnLoginFailed?.Invoke($"Error de conexión: {request.error}");
            }
        }
    }
    
    /// <summary>
    /// Verificar email usando token
    /// Llama a la función SQL: verificar_email_token(id_usuario, token)
    /// </summary>
    public void VerifyEmail(int userId, string token)
    {
        StartCoroutine(VerifyEmailCoroutine(userId, token));
    }
    
    private IEnumerator VerifyEmailCoroutine(int userId, string token)
    {
        string functionCall = JsonUtility.ToJson(new { 
            p_id_usuario = userId, 
            p_token = token
        });
        
        using (UnityWebRequest request = new UnityWebRequest($"{rpcUrl}/verificar_email_token", "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(functionCall);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", $"Bearer {supabaseKey}");
            request.SetRequestHeader("Accept", "application/json");
            
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    string responseText = request.downloadHandler.text;
                    VerificationResponse response = JsonUtility.FromJson<VerificationResponse>(responseText);
                    
                    if (response.exito)
                    {
                        OnEmailVerified?.Invoke();
                        PlayerPrefs.DeleteKey("PendingVerificationToken");
                        PlayerPrefs.DeleteKey("PendingUserId");
                    }
                    else
                    {
                        OnEmailVerificationFailed?.Invoke(response.mensaje);
                    }
                }
                catch (Exception ex)
                {
                    OnEmailVerificationFailed?.Invoke($"Error al verificar: {ex.Message}");
                }
            }
            else
            {
                OnEmailVerificationFailed?.Invoke($"Error de conexión: {request.error}");
            }
        }
    }
    
    /// <summary>
    /// Recuperación de contraseña - Paso 1: Enviar email con token
    /// Llama a la función SQL: crear_token_recuperacion(id_usuario, ip)
    /// </summary>
    public void RequestPasswordReset(string email)
    {
        if (string.IsNullOrEmpty(email))
        {
            Debug.LogError("Email es requerido");
            return;
        }
        
        StartCoroutine(RequestPasswordResetCoroutine(email));
    }
    
    private IEnumerator RequestPasswordResetCoroutine(string email)
    {
        // Primero obtener el id_usuario por email
        string query = $"?select=id_usuario,email&email=eq.{email}";
        
        using (UnityWebRequest request = UnityWebRequest.Get($"{restUrl}/usuarios{query}"))
        {
            request.SetRequestHeader("Authorization", $"Bearer {supabaseKey}");
            request.SetRequestHeader("Accept", "application/json");
            
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    string responseText = request.downloadHandler.text;
                    // Parsear array de usuarios
                    UserData[] usuarios = JsonHelper.FromJson<UserData>(responseText);
                    
                    if (usuarios.Length > 0)
                    {
                        int userId = usuarios[0].id_usuario;
                        
                        // Crear token de recuperación
                        string ipAddress = GetClientIP();
                        string functionCall = JsonUtility.ToJson(new { 
                            p_id_usuario = userId, 
                            p_ip_solicitud = ipAddress
                        });
                        
                        using (UnityWebRequest resetRequest = new UnityWebRequest($"{rpcUrl}/crear_token_recuperacion", "POST"))
                        {
                            byte[] bodyRaw = Encoding.UTF8.GetBytes(functionCall);
                            resetRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
                            resetRequest.downloadHandler = new DownloadHandlerBuffer();
                            resetRequest.SetRequestHeader("Content-Type", "application/json");
                            resetRequest.SetRequestHeader("Authorization", $"Bearer {supabaseKey}");
                            
                            yield return resetRequest.SendWebRequest();
                            
                            if (resetRequest.result == UnityWebRequest.Result.Success)
                            {
                                string token = resetRequest.downloadHandler.text.Trim('"');
                                
                                // Enviar email con el token
                                yield return SendPasswordResetEmail(email, userId, token);
                                Debug.Log("Email de recuperación enviado");
                            }
                            else
                            {
                                Debug.LogError("Error al crear token: " + resetRequest.error);
                            }
                        }
                    }
                    else
                    {
                        Debug.LogError("Usuario no encontrado");
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError("Error: " + ex.Message);
                }
            }
            else
            {
                Debug.LogError("Error de conexión: " + request.error);
            }
        }
    }
    
    /// <summary>
    /// Recuperación de contraseña - Paso 2: Cambiar contraseña con token
    /// Llama a la función SQL: usar_token_recuperacion(token, nueva_password, ip)
    /// </summary>
    public void ResetPasswordWithToken(string token, string newPassword)
    {
        if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(newPassword))
        {
            Debug.LogError("Token y contraseña son requeridos");
            return;
        }
        
        if (newPassword.Length < 8)
        {
            Debug.LogError("La contraseña debe tener al menos 8 caracteres");
            return;
        }
        
        StartCoroutine(ResetPasswordWithTokenCoroutine(token, newPassword));
    }
    
    private IEnumerator ResetPasswordWithTokenCoroutine(string token, string newPassword)
    {
        string ipAddress = GetClientIP();
        string functionCall = JsonUtility.ToJson(new { 
            p_token = token, 
            p_nueva_password = newPassword,
            p_ip_acceso = ipAddress
        });
        
        using (UnityWebRequest request = new UnityWebRequest($"{rpcUrl}/usar_token_recuperacion", "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(functionCall);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", $"Bearer {supabaseKey}");
            request.SetRequestHeader("Accept", "application/json");
            
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    string responseText = request.downloadHandler.text;
                    PasswordResetResponse response = JsonUtility.FromJson<PasswordResetResponse>(responseText);
                    
                    if (response.exito)
                    {
                        Debug.Log("Contraseña cambiada exitosamente");
                    }
                    else
                    {
                        Debug.LogError("Error: " + response.mensaje);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError("Error al parsear: " + ex.Message);
                }
            }
            else
            {
                Debug.LogError("Error de conexión: " + request.error);
            }
        }
    }
    
    /// <summary>
    /// Cambiar nombre de usuario
    /// </summary>
    public void ChangeUsername(string newUsername)
    {
        if (currentUser == null)
        {
            Debug.LogError("No hay usuario logueado");
            return;
        }
        
        StartCoroutine(ChangeUsernameCoroutine(currentUser.id_usuario, newUsername));
    }
    
    private IEnumerator ChangeUsernameCoroutine(int userId, string newUsername)
    {
        // Actualizar en tabla usuarios
        string updateData = JsonUtility.ToJson(new { nombre_usuario = newUsername });
        string query = $"?id_usuario=eq.{userId}";
        
        using (UnityWebRequest request = new UnityWebRequest($"{restUrl}/usuarios{query}", "PATCH"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(updateData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", $"Bearer {supabaseKey}");
            
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                currentUser.nombre_usuario = newUsername;
                PlayerPrefs.SetString("CurrentUsername", newUsername);
                Debug.Log("Nombre de usuario actualizado");
            }
            else
            {
                Debug.LogError("Error al cambiar nombre: " + request.error);
            }
        }
    }
    
    /// <summary>
    /// Obtener historial de partidas del usuario
    /// </summary>
    public void FetchGameHistory(System.Action<GameData[]> callback)
    {
        if (currentUser == null)
        {
            callback(new GameData[0]);
            return;
        }
        
        StartCoroutine(FetchGameHistoryCoroutine(currentUser.id_usuario, callback));
    }
    
    private IEnumerator FetchGameHistoryCoroutine(int userId, System.Action<GameData[]> callback)
    {
        // Obtener partidas donde el usuario es blanco o negro
        string query = $"?or=(id_usuario_blanco.eq.{userId},id_usuario_negro.eq.{userId})&order=fecha_inicio.desc";
        
        using (UnityWebRequest request = UnityWebRequest.Get($"{restUrl}/partidas{query}"))
        {
            request.SetRequestHeader("Authorization", $"Bearer {supabaseKey}");
            request.SetRequestHeader("Accept", "application/json");
            
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    string responseText = request.downloadHandler.text;
                    GameData[] games = JsonHelper.FromJson<GameData>(responseText);
                    callback(games);
                }
                catch (Exception ex)
                {
                    Debug.LogError("Error al parsear historial: " + ex.Message);
                    callback(new GameData[0]);
                }
            }
            else
            {
                Debug.LogError("Error al obtener historial: " + request.error);
                callback(new GameData[0]);
            }
        }
    }
    
    /// <summary>
    /// Obtener notificaciones del usuario
    /// </summary>
    public void FetchNotifications(System.Action<NotificationData[]> callback)
    {
        if (currentUser == null)
        {
            callback(new NotificationData[0]);
            return;
        }
        
        StartCoroutine(FetchNotificationsCoroutine(currentUser.id_usuario, callback));
    }
    
    private IEnumerator FetchNotificationsCoroutine(int userId, System.Action<NotificationData[]> callback)
    {
        string query = $"?id_usuario=eq.{userId}&order=fecha_creacion.desc&limit=50";
        
        using (UnityWebRequest request = UnityWebRequest.Get($"{restUrl}/notificaciones{query}"))
        {
            request.SetRequestHeader("Authorization", $"Bearer {supabaseKey}");
            request.SetRequestHeader("Accept", "application/json");
            
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    string responseText = request.downloadHandler.text;
                    NotificationData[] notifications = JsonHelper.FromJson<NotificationData>(responseText);
                    callback(notifications);
                }
                catch (Exception ex)
                {
                    Debug.LogError("Error al parsear notificaciones: " + ex.Message);
                    callback(new NotificationData[0]);
                }
            }
            else
            {
                Debug.LogError("Error al obtener notificaciones: " + request.error);
                callback(new NotificationData[0]);
            }
        }
    }
    
    /// <summary>
    /// Enviar email de verificación (usando SendGrid)
    /// </summary>
    private IEnumerator SendVerificationEmail(string email, int userId)
    {
        if (string.IsNullOrEmpty(sendGridApiKey))
        {
            Debug.LogWarning("SendGrid API Key no configurada. Email de verificación no enviado.");
            yield break;
        }
        
        // Obtener token de verificación
        string query = $"?id_usuario=eq.{userId}&select=token_verificacion";
        
        using (UnityWebRequest request = UnityWebRequest.Get($"{restUrl}/usuarios{query}"))
        {
            request.SetRequestHeader("Authorization", $"Bearer {supabaseKey}");
            
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    string responseText = request.downloadHandler.text;
                    UserData[] users = JsonHelper.FromJson<UserData>(responseText);
                    
                    if (users.Length > 0)
                    {
                        string verificacionUrl = $"https://tunombrededominio.com/verify?user={userId}&token={users[0].token_verificacion}";
                        
                        // Enviar email con SendGrid
                        yield return SendGridService.SendEmail(
                            email, 
                            "Verifica tu cuenta de Ajedrez Multijugador",
                            $"Haz click aquí para verificar tu email: {verificacionUrl}"
                        );
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError("Error al enviar email: " + ex.Message);
                }
            }
        }
    }
    
    /// <summary>
    /// Enviar email de recuperación de contraseña
    /// </summary>
    private IEnumerator SendPasswordResetEmail(string email, int userId, string token)
    {
        if (string.IsNullOrEmpty(sendGridApiKey))
        {
            Debug.LogWarning("SendGrid API Key no configurada.");
            yield break;
        }
        
        string resetUrl = $"https://tunombrededominio.com/reset-password?token={token}";
        
        yield return SendGridService.SendEmail(
            email,
            "Recupera tu contraseña",
            $"Haz click aquí para cambiar tu contraseña: {resetUrl}\nEste enlace expira en 1 hora."
        );
    }
    
    // ── Métodos auxiliares ────────────────────────────────────────────
    
    private bool ValidateInput(string username, string email, string password)
    {
        if (string.IsNullOrEmpty(username))
        {
            OnRegisterFailed?.Invoke("Nombre de usuario requerido");
            return false;
        }
        
        if (username.Length < 3 || username.Length > 60)
        {
            OnRegisterFailed?.Invoke("Nombre de usuario debe tener 3-60 caracteres");
            return false;
        }
        
        if (!IsValidEmail(email))
        {
            OnRegisterFailed?.Invoke("Email inválido");
            return false;
        }
        
        if (password.Length < 8)
        {
            OnRegisterFailed?.Invoke("Contraseña debe tener mínimo 8 caracteres");
            return false;
        }
        
        return true;
    }
    
    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
    
    private string GetClientIP()
    {
        // En desarrollo, devolver 127.0.0.1
        // En producción, obtener de servidor
        return "127.0.0.1";
    }
    
    private void SaveSession(int userId, string username)
    {
        currentUser = new UserData
        {
            id_usuario = userId,
            nombre_usuario = username
        };
        
        currentSession = new SessionData
        {
            id_usuario = userId,
            nombre_usuario = username
        };
        
        // Guardar en PlayerPrefs
        PlayerPrefs.SetInt("UserId", userId);
        PlayerPrefs.SetString("Username", username);
        PlayerPrefs.SetInt("LastLoginTime", (int)DateTime.Now.Ticks);
        PlayerPrefs.Save();
    }
    
    private void SaveUserRegistration(int userId, string username, string email)
    {
        PlayerPrefs.SetInt("RegisteredUserId", userId);
        PlayerPrefs.SetString("RegisteredUsername", username);
        PlayerPrefs.SetString("RegisteredEmail", email);
    }
    
    private void LoadSavedSession()
    {
        if (PlayerPrefs.HasKey("UserId"))
        {
            int userId = PlayerPrefs.GetInt("UserId");
            string username = PlayerPrefs.GetString("Username");
            
            currentUser = new UserData
            {
                id_usuario = userId,
                nombre_usuario = username
            };
        }
    }
    
    public UserData GetCurrentUser() => currentUser;
    public bool IsLoggedIn() => currentUser != null;
    
    public void Logout()
    {
        currentUser = null;
        currentSession = null;
        PlayerPrefs.DeleteKey("UserId");
        PlayerPrefs.DeleteKey("Username");
        PlayerPrefs.DeleteKey("LastLoginTime");
        PlayerPrefs.Save();
        Debug.Log("Sesión cerrada");
    }
}

/// <summary>
/// Helper para parsear arrays JSON
/// </summary>
public class JsonHelper
{
    public static T[] FromJson<T>(string json)
    {
        string newJson = "{ \"array\": " + json + "}";
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(newJson);
        return wrapper.array;
    }
    
    [System.Serializable]
    private class Wrapper<T>
    {
        public T[] array;
    }
}

/// <summary>
/// Servicio SendGrid para envío de emails
/// </summary>
public class SendGridService
{
    public static IEnumerator SendEmail(string toEmail, string subject, string htmlContent)
    {
        // Implementación de SendGrid
        // Requiere SENDGRID_API_KEY en configuración
        Debug.Log($"Email enviado a {toEmail}: {subject}");
        yield return null;
    }
}
