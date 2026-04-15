using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// Script para la pantalla de LOGIN
/// Permite a usuarios existentes loguearse con email y contraseña
/// </summary>
public class LoginPanel : MonoBehaviour
{
    [SerializeField] private TMP_InputField emailInput;
    [SerializeField] private TMP_InputField passwordInput;
    [SerializeField] private Button loginButton;
    [SerializeField] private Button registerButton;
    [SerializeField] private Button forgotPasswordButton;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private Toggle rememberMeToggle;
    [SerializeField] private CanvasGroup canvasGroup;
    
    // Referencia a otros paneles
    [SerializeField] private RegisterPanel registerPanel;
    [SerializeField] private ForgotPasswordPanel forgotPasswordPanel;
    
    private AuthenticationService authService;
    private bool isLogging = false;
    
    private void Start()
    {
        authService = AuthenticationService.Instance;
        
        // Suscribirse a eventos
        authService.OnLoginSuccess += OnLoginSuccess;
        authService.OnLoginFailed += OnLoginFailed;
        
        // Vincular botones
        loginButton.onClick.AddListener(OnLoginClicked);
        registerButton.onClick.AddListener(OnRegisterClicked);
        forgotPasswordButton.onClick.AddListener(OnForgotPasswordClicked);
        
        // Validación en tiempo real
        emailInput.onValueChanged.AddListener(_ => ValidateForm());
        passwordInput.onValueChanged.AddListener(_ => ValidateForm());
        
        statusText.text = "";
        
        // Cargar datos guardados si "Recuérdame" estaba activo
        LoadSavedCredentials();
    }
    
    private void OnLoginClicked()
    {
        if (string.IsNullOrEmpty(emailInput.text) || string.IsNullOrEmpty(passwordInput.text))
        {
            ShowError("Email y contraseña son requeridos");
            return;
        }
        
        // Guardar credenciales si "Recuérdame" está activo
        if (rememberMeToggle.isOn)
        {
            PlayerPrefs.SetString("SavedEmail", emailInput.text);
            PlayerPrefs.SetInt("RememberMe", 1);
        }
        else
        {
            PlayerPrefs.DeleteKey("SavedEmail");
            PlayerPrefs.SetInt("RememberMe", 0);
        }
        PlayerPrefs.Save();
        
        isLogging = true;
        loginButton.interactable = false;
        statusText.text = "Ingresando...";
        statusText.color = Color.yellow;
        
        authService.Login(emailInput.text.Trim(), passwordInput.text);
    }
    
    private void OnLoginSuccess(UserData user)
    {
        isLogging = false;
        loginButton.interactable = true;
        statusText.text = $"¡Bienvenido, {user.nombre_usuario}!";
        statusText.color = Color.green;
        
        // Cargar escena principal del juego después de 2 segundos
        Invoke(nameof(LoadMainScene), 2f);
    }
    
    private void OnLoginFailed(string error)
    {
        isLogging = false;
        loginButton.interactable = true;
        ShowError(error);
    }
    
    private void ShowError(string message)
    {
        statusText.text = $"<color=red>{message}</color>";
        statusText.color = Color.red;
    }
    
    private void OnRegisterClicked()
    {
        // Mostrar panel de registro
        Hide();
        registerPanel.Show();
    }
    
    private void OnForgotPasswordClicked()
    {
        // Mostrar panel de recuperación de contraseña
        Hide();
        forgotPasswordPanel.Show();
    }
    
    private void ValidateForm()
    {
        bool isValid = !string.IsNullOrEmpty(emailInput.text)
                    && !string.IsNullOrEmpty(passwordInput.text);
        loginButton.interactable = isValid && !isLogging;
    }
    
    private void LoadSavedCredentials()
    {
        if (PlayerPrefs.HasKey("RememberMe") && PlayerPrefs.GetInt("RememberMe") == 1)
        {
            emailInput.text = PlayerPrefs.GetString("SavedEmail", "");
            rememberMeToggle.isOn = true;
        }
    }
    
    private void LoadMainScene()
    {
        // Cambiar a la escena principal del juego
        SceneManager.LoadScene("MainGame");
    }
    
    public void Show()
    {
        canvasGroup.interactable = true;
        canvasGroup.alpha = 1;
        statusText.text = "";
    }
    
    public void Hide()
    {
        canvasGroup.interactable = false;
        canvasGroup.alpha = 0;
    }
    
    private void OnDestroy()
    {
        if (authService != null)
        {
            authService.OnLoginSuccess -= OnLoginSuccess;
            authService.OnLoginFailed -= OnLoginFailed;
        }
    }
}
