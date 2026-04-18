using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Script para la pantalla de RECUPERACIÓN DE CONTRASEÑA
/// Permite a usuarios recuperar su contraseña en dos pasos:
/// 1. Introducir email para recibir código de recuperación
/// 2. Introducir código y nueva contraseña
/// </summary>
public class ForgotPasswordPanel : MonoBehaviour
{
    [SerializeField] private TMP_InputField emailInput;
    [SerializeField] private TMP_InputField tokenInput;
    [SerializeField] private TMP_InputField newPasswordInput;
    [SerializeField] private TMP_InputField confirmPasswordInput;
    [SerializeField] private Button sendTokenButton;
    [SerializeField] private Button resetPasswordButton;
    [SerializeField] private Button backButton;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private CanvasGroup step1Group;  // Grupo para paso 1
    [SerializeField] private CanvasGroup step2Group;  // Grupo para paso 2
    
    // Referencia a otros paneles
    [SerializeField] private LoginPanel loginPanel;
    
    private AuthenticationService authService;
    private bool isSendingToken = false;
    private bool isResettingPassword = false;
    
    private void Start()
    {
        authService = AuthenticationService.Instance;
        
        // Vincular botones
        sendTokenButton.onClick.AddListener(OnSendTokenClicked);
        resetPasswordButton.onClick.AddListener(OnResetPasswordClicked);
        backButton.onClick.AddListener(OnBackClicked);
        
        // Validación en tiempo real
        emailInput.onValueChanged.AddListener(_ => ValidateStep1());
        tokenInput.onValueChanged.AddListener(_ => ValidateStep2());
        newPasswordInput.onValueChanged.AddListener(_ => ValidateStep2());
        confirmPasswordInput.onValueChanged.AddListener(_ => ValidateStep2());
        
        statusText.text = "";
        
        // Inicialmente mostrar paso 1
        ShowStep1();
    }
    
    private void OnSendTokenClicked()
    {
        if (string.IsNullOrEmpty(emailInput.text))
        {
            ShowError("Email es requerido");
            return;
        }
        
        if (!IsValidEmail(emailInput.text))
        {
            ShowError("Email inválido");
            return;
        }
        
        isSendingToken = true;
        sendTokenButton.interactable = false;
        statusText.text = "Enviando correo de recuperación...";
        statusText.color = Color.yellow;
        
        // Llamar al servicio
        authService.RequestPasswordReset(emailInput.text.Trim());
        
        // Mostrar paso 2 después de 2 segundos (simulando que se envió el email)
        Invoke(nameof(ShowStep2), 2f);
    }
    
    private void OnResetPasswordClicked()
    {
        if (string.IsNullOrEmpty(tokenInput.text))
        {
            ShowError("Código de recuperación es requerido");
            return;
        }
        
        if (newPasswordInput.text != confirmPasswordInput.text)
        {
            ShowError("Las contraseñas no coinciden");
            return;
        }
        
        if (newPasswordInput.text.Length < 8)
        {
            ShowError("La contraseña debe tener mínimo 8 caracteres");
            return;
        }
        
        isResettingPassword = true;
        resetPasswordButton.interactable = false;
        statusText.text = "Cambiando contraseña...";
        statusText.color = Color.yellow;
        
        // Llamar al servicio
        authService.ResetPasswordWithToken(tokenInput.text, newPasswordInput.text);
        
        // Mostrar mensaje de éxito y volver
        statusText.text = "<color=green>Contraseña cambiada exitosamente. Regresando a login...</color>";
        Invoke(nameof(BackToLogin), 2f);
    }
    
    private void OnBackClicked()
    {
        BackToLogin();
    }
    
    private void BackToLogin()
    {
        Hide();
        loginPanel.Show();
    }
    
    private void ShowError(string message)
    {
        statusText.text = $"<color=red>{message}</color>";
        statusText.color = Color.red;
    }
    
    private void ShowStep1()
    {
        step1Group.interactable = true;
        step1Group.alpha = 1;
        step2Group.interactable = false;
        step2Group.alpha = 0;
    }
    
    private void ShowStep2()
    {
        isSendingToken = false;
        sendTokenButton.interactable = true;
        
        statusText.text = "Se envió un código a tu email. Ingresa el código y tu nueva contraseña.";
        statusText.color = Color.green;
        
        step1Group.interactable = false;
        step1Group.alpha = 0.5f;
        step2Group.interactable = true;
        step2Group.alpha = 1;
    }
    
    private void ValidateStep1()
    {
        bool isValid = !string.IsNullOrEmpty(emailInput.text) && IsValidEmail(emailInput.text);
        sendTokenButton.interactable = isValid && !isSendingToken;
    }
    
    private void ValidateStep2()
    {
        bool isValid = !string.IsNullOrEmpty(tokenInput.text)
                    && !string.IsNullOrEmpty(newPasswordInput.text)
                    && !string.IsNullOrEmpty(confirmPasswordInput.text)
                    && newPasswordInput.text == confirmPasswordInput.text
                    && newPasswordInput.text.Length >= 8;
        
        resetPasswordButton.interactable = isValid && !isResettingPassword;
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
    
    public void Show()
    {
        canvasGroup.interactable = true;
        canvasGroup.alpha = 1;
        
        // Limpiar campos
        emailInput.text = "";
        tokenInput.text = "";
        newPasswordInput.text = "";
        confirmPasswordInput.text = "";
        statusText.text = "";
        
        ShowStep1();
    }
    
    public void Hide()
    {
        canvasGroup.interactable = false;
        canvasGroup.alpha = 0;
    }
}
