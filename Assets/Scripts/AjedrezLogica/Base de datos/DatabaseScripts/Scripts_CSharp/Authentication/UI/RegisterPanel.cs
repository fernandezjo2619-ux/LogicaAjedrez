using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Script para la pantalla de REGISTRO
/// Permite a nuevos usuarios registrarse con nombre, email y contraseña
/// </summary>
public class RegisterPanel : MonoBehaviour
{
    [SerializeField] private TMP_InputField usernameInput;
    [SerializeField] private TMP_InputField emailInput;
    [SerializeField] private TMP_InputField passwordInput;
    [SerializeField] private TMP_InputField confirmPasswordInput;
    [SerializeField] private Button registerButton;
    [SerializeField] private Button backButton;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private Toggle termsToggle;
    [SerializeField] private CanvasGroup canvasGroup;
    
    // Referencia a otros paneles
    [SerializeField] private VerificationPanel verificationPanel;
    [SerializeField] private LoginPanel loginPanel;
    
    private AuthenticationService authService;
    private bool isRegistering = false;
    
    private void Start()
    {
        authService = AuthenticationService.Instance;
        
        // Suscribirse a eventos
        authService.OnRegisterSuccess += OnRegisterSuccess;
        authService.OnRegisterFailed += OnRegisterFailed;
        
        // Vincular botones
        registerButton.onClick.AddListener(OnRegisterClicked);
        backButton.onClick.AddListener(OnBackClicked);
        
        // Validación en tiempo real
        usernameInput.onValueChanged.AddListener(_ => ValidateForm());
        emailInput.onValueChanged.AddListener(_ => ValidateForm());
        passwordInput.onValueChanged.AddListener(_ => ValidateForm());
        confirmPasswordInput.onValueChanged.AddListener(_ => ValidateForm());
        
        statusText.text = "";
        UpdateRegisterButtonState();
    }
    
    private void OnRegisterClicked()
    {
        // Validar que las contraseñas coincidan
        if (passwordInput.text != confirmPasswordInput.text)
        {
            ShowError("Las contraseñas no coinciden");
            return;
        }
        
        if (!termsToggle.isOn)
        {
            ShowError("Debes aceptar los términos y condiciones");
            return;
        }
        
        // Llamar al servicio de autenticación
        isRegistering = true;
        registerButton.interactable = false;
        statusText.text = "Registrando...";
        
        authService.Register(
            usernameInput.text.Trim(),
            emailInput.text.Trim(),
            passwordInput.text
        );
    }
    
    private void OnRegisterSuccess(int userId)
    {
        isRegistering = false;
        registerButton.interactable = true;
        statusText.text = "¡Registro exitoso! Se envió un email de verificación.";
        
        // Esperar 2 segundos y mostrar panel de verificación
        Invoke(nameof(ShowVerificationPanel), 2f);
    }
    
    private void OnRegisterFailed(string error)
    {
        isRegistering = false;
        registerButton.interactable = true;
        ShowError(error);
    }
    
    private void ShowError(string message)
    {
        statusText.text = $"<color=red>{message}</color>";
        statusText.color = Color.red;
    }
    
    private void OnBackClicked()
    {
        // Volver al panel de login
        canvasGroup.interactable = false;
        canvasGroup.alpha = 0;
        loginPanel.Show();
    }
    
    private void ValidateForm()
    {
        UpdateRegisterButtonState();
    }
    
    private void UpdateRegisterButtonState()
    {
        bool isValid = !string.IsNullOrEmpty(usernameInput.text)
                    && !string.IsNullOrEmpty(emailInput.text)
                    && !string.IsNullOrEmpty(passwordInput.text)
                    && !string.IsNullOrEmpty(confirmPasswordInput.text)
                    && passwordInput.text == confirmPasswordInput.text
                    && passwordInput.text.Length >= 8
                    && IsValidEmail(emailInput.text)
                    && usernameInput.text.Length >= 3;
        
        registerButton.interactable = isValid && !isRegistering;
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
    
    private void ShowVerificationPanel()
    {
        Hide();
        verificationPanel.Show(PlayerPrefs.GetInt("RegisteredUserId", 0), emailInput.text);
    }
    
    public void Show()
    {
        canvasGroup.interactable = true;
        canvasGroup.alpha = 1;
        usernameInput.text = "";
        emailInput.text = "";
        passwordInput.text = "";
        confirmPasswordInput.text = "";
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
            authService.OnRegisterSuccess -= OnRegisterSuccess;
            authService.OnRegisterFailed -= OnRegisterFailed;
        }
    }
}
