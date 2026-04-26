using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Script para la pantalla de VERIFICACIÓN DE EMAIL
/// Permite al usuario verificar su email usando el código recibido
/// </summary>
public class VerificationPanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI instructionText;
    [SerializeField] private TMP_InputField verificationCodeInput;
    [SerializeField] private Button verifyButton;
    [SerializeField] private Button resendButton;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private CanvasGroup canvasGroup;
    
    // Referencia a otros paneles
    [SerializeField] private LoginPanel loginPanel;
    
    private AuthenticationService authService;
    private int currentUserId;
    private string currentEmail;
    private float resendCooldown = 0f;
    private bool isVerifying = false;
    
    private void Start()
    {
        authService = AuthenticationService.Instance;
        
        // Vincular botones
        verifyButton.onClick.AddListener(OnVerifyClicked);
        resendButton.onClick.AddListener(OnResendClicked);
        
        // Validación en tiempo real
        verificationCodeInput.onValueChanged.AddListener(_ => ValidateForm());
        
        statusText.text = "";
    }
    
    private void Update()
    {
        // Controlar cooldown de resend
        if (resendCooldown > 0)
        {
            resendCooldown -= Time.deltaTime;
            resendButton.interactable = false;
            resendButton.GetComponentInChildren<TextMeshProUGUI>().text = $"Reintentar ({Mathf.Ceil(resendCooldown)}s)";
        }
        else if (resendCooldown <= 0 && resendCooldown > -1) // Solo una vez
        {
            resendCooldown = -1;
            resendButton.interactable = true;
            resendButton.GetComponentInChildren<TextMeshProUGUI>().text = "Resend Email";
        }
    }
    
    private void OnVerifyClicked()
    {
        if (string.IsNullOrEmpty(verificationCodeInput.text))
        {
            ShowError("Ingresa el código de verificación");
            return;
        }
        
        isVerifying = true;
        verifyButton.interactable = false;
        statusText.text = "Verificando...";
        statusText.color = Color.yellow;
        
        // Llamar al servicio
        authService.VerifyEmail(currentUserId, verificationCodeInput.text.Trim());
        
        // Esperar respuesta
        Invoke(nameof(HandleVerificationResult), 2f);
    }
    
    private void OnResendClicked()
    {
        // Resend email verification
        statusText.text = "Reenviando email...";
        statusText.color = Color.yellow;
        resendButton.interactable = false;
        
        // Simular reenví (en realidad ya se envió en el registro)
        statusText.text = "<color=green>Email reenviado. Revisa tu bandeja de entrada.</color>";
        resendCooldown = 60f; // 60 segundos de cooldown
    }
    
    private void HandleVerificationResult()
    {
        isVerifying = false;
        verifyButton.interactable = true;
        
        // If verification was successful, the event will be triggered
        // Show success message and return to login after 2 seconds
        statusText.text = "<color=green>¡Email verificado exitosamente!</color>";
        Invoke(nameof(BackToLogin), 2f);
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
    
    private void ValidateForm()
    {
        bool isValid = !string.IsNullOrEmpty(verificationCodeInput.text) && verificationCodeInput.text.Length >= 6;
        verifyButton.interactable = isValid && !isVerifying;
    }
    
    public void Show(int userId, string email)
    {
        currentUserId = userId;
        currentEmail = email;
        
        canvasGroup.interactable = true;
        canvasGroup.alpha = 1;
        
        titleText.text = "Verifica tu Email";
        instructionText.text = $"Hemos enviado un código de verificación a:\n<b>{email}</b>\n\nIngresa el código en el campo de abajo.";
        verificationCodeInput.text = "";
        statusText.text = "";
        resendCooldown = 0; // Permitir resend inmediatamente
    }
    
    public void Hide()
    {
        canvasGroup.interactable = false;
        canvasGroup.alpha = 0;
    }
}
