using UnityEngine;

/// <summary>
/// Manager general de autenticación
/// Inicializa y coordina todos los servicios y paneles de autenticación
/// Se ejecuta en la escena de autenticación (login/registro)
/// </summary>
public class AuthenticationManager : MonoBehaviour
{
    [SerializeField] private Canvas authCanvas;
    [SerializeField] private LoginPanel loginPanel;
    [SerializeField] private RegisterPanel registerPanel;
    [SerializeField] private ForgotPasswordPanel forgotPasswordPanel;
    [SerializeField] private VerificationPanel verificationPanel;
    
    // Configuración de Supabase
    [SerializeField] private string supabaseUrl = "https://xxxxx.supabase.co";
    [SerializeField] private string supabaseKey = "eyJhbGciOiJIUzI1NiIsInR5cCI...";
    
    private AuthenticationService authService;
    
    private void Start()
    {
        // Asegurar que AuthenticationService esté inicializado
        authService = AuthenticationService.Instance;
        
        // Si ya hay sesión activa, ir a juego
        if (authService.IsLoggedIn())
        {
            Debug.Log("Sesión activa detectada. Cargando juego...");
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainGame");
            return;
        }
        
        // Mostrar panel de login por defecto
        ShowLoginPanel();
    }
    
    public void ShowLoginPanel()
    {
        HideAllPanels();
        if (loginPanel != null)
            loginPanel.Show();
    }
    
    public void ShowRegisterPanel()
    {
        HideAllPanels();
        if (registerPanel != null)
            registerPanel.Show();
    }
    
    public void ShowForgotPasswordPanel()
    {
        HideAllPanels();
        if (forgotPasswordPanel != null)
            forgotPasswordPanel.Show();
    }
    
    public void ShowVerificationPanel(int userId, string email)
    {
        HideAllPanels();
        if (verificationPanel != null)
            verificationPanel.Show(userId, email);
    }
    
    private void HideAllPanels()
    {
        if (loginPanel != null)
            loginPanel.Hide();
        if (registerPanel != null)
            registerPanel.Hide();
        if (forgotPasswordPanel != null)
            forgotPasswordPanel.Hide();
        if (verificationPanel != null)
            verificationPanel.Hide();
    }
}
