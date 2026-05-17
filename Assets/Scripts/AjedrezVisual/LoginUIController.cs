using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class LoginUIController : MonoBehaviour
{
    [Header("Componentes de la UI")]
    [SerializeField] private TMP_InputField inputEmail;
    [SerializeField] private TMP_InputField inputPassword;
    [SerializeField] private TMP_Text txtMensajeError;
    [SerializeField] private TMP_Text txtMensajeErrorEmail;
    [SerializeField] private TMP_Text txtMensajeErrorPassword;

    [Header("Navegación")]
    [SerializeField] private string nombreSiguienteEscena = "Menu_Inicio";

    private IniciarSesion scriptIniciarSesion;

    private void Start()
    {
        txtMensajeError.text = "";
        txtMensajeErrorEmail.text = "Escribe tu email.";
        txtMensajeErrorPassword.text = "Escribe tu contraseña.";
    }

    /// <summary>
    /// Este método se vincula al evento OnClick de tu botón de Login
    /// </summary>
    public void PresionarBotonLogin()
    {
        string email = inputEmail.text.Trim();
        string password = inputPassword.text.Trim();

        if (string.IsNullOrEmpty(email))
        {
            MostrarMensajeEmail("Por favor, introduce tu correo electrónico.", Color.red);
            return;
        }

        if (string.IsNullOrEmpty(password))
        {
            MostrarMensajePassword("Por favor, introduce tu contraseña.", Color.red);
            return;
        }

        MostrarMensaje("Conectando con el servidor...", Color.yellow);

        StartCoroutine(scriptIniciarSesion.IniciarSesion_DevolverAmbos(
            email,
            password,
            onSuccess: (respuesta) => ManejarLoginExitoso(respuesta),
            onError: (mensajeError) => ManejarLoginFallido(mensajeError)
        ));
    }

    // Se ejecuta automáticamente si Supabase encuentra al usuario
    private void ManejarLoginExitoso(SesionResponse datosUsuario)
    {
        Debug.Log($"¡Bienvenido {datosUsuario.nombre_usuario}! ID: {datosUsuario.id_usuario}");

        PlayerPrefs.SetInt("LocalPlayerId", datosUsuario.id_usuario);
        PlayerPrefs.SetString("UsuarioNombre", datosUsuario.nombre_usuario);
        PlayerPrefs.Save();

        SceneManager.LoadScene(nombreSiguienteEscena);
    }

    // Se ejecuta automáticamente si las credenciales fallan o no hay internet
    private void ManejarLoginFallido(string error)
    {
        Debug.LogWarning("Fallo en el inicio de sesión: " + error);
        MostrarMensaje(error, Color.red);
    }

    private void MostrarMensaje(string texto, Color color)
    {
        txtMensajeError.text = texto;
        txtMensajeError.color = color;
    }

    private void MostrarMensajeEmail(string texto, Color color)
    {
        txtMensajeErrorEmail.text = texto;
        txtMensajeErrorEmail.color = color;
    }

    private void MostrarMensajePassword(string texto, Color color)
    {
        txtMensajeErrorPassword.text = texto;
        txtMensajeErrorPassword.color = color;
    }
}