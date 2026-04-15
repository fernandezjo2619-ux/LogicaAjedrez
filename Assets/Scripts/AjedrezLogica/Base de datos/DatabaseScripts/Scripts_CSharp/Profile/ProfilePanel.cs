using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Script para la pantalla de PERFIL DE USUARIO
/// Muestra información del usuario y permite acceder a:
/// - Cambiar nombre de usuario
/// - Cambiar avatar
/// - Ver historial de juegos
/// - Ver notificaciones
/// </summary>
public class ProfilePanel : MonoBehaviour
{
    // Elementos principales del perfil
    [SerializeField] private Image avatarImage;
    [SerializeField] private TextMeshProUGUI usernameText;
    [SerializeField] private TextMeshProUGUI eloRatingText;
    [SerializeField] private TextMeshProUGUI emailText;
    [SerializeField] private TextMeshProUGUI bioText;
    [SerializeField] private CanvasGroup canvasGroup;
    
    // Botones principales
    [SerializeField] private Button editUsernameButton;
    [SerializeField] private Button changeAvatarButton;
    [SerializeField] private Button changePasswordButton;
    [SerializeField] private Button gameHistoryButton;
    [SerializeField] private Button notificationsButton;
    [SerializeField] private Button logoutButton;
    [SerializeField] private Button backButton;
    
    // Paneles secundarios
    private ChangeUsernamePanel changeUsernamePanel;
    private AvatarSelectorPanel avatarSelectorPanel;
    private GameHistoryPanel gameHistoryPanel;
    private NotificationsPanel notificationsPanel;
    
    private AuthenticationService authService;
    private UserData currentUser;
    
    private void Start()
    {
        authService = AuthenticationService.Instance;
        
        // Crear paneles secundarios
        CreateSubPanels();
        
        // Vincular botones
        editUsernameButton.onClick.AddListener(OnEditUsernameClicked);
        changeAvatarButton.onClick.AddListener(OnChangeAvatarClicked);
        changePasswordButton.onClick.AddListener(() => Debug.Log("Change password - coming soon"));
        gameHistoryButton.onClick.AddListener(OnGameHistoryClicked);
        notificationsButton.onClick.AddListener(OnNotificationsClicked);
        logoutButton.onClick.AddListener(OnLogoutClicked);
        backButton.onClick.AddListener(OnBackClicked);
    }
    
    private void CreateSubPanels()
    {
        // Crear ChangeUsernamePanel
        var changeUsernameObj = new GameObject("ChangeUsernamePanel");
        changeUsernameObj.transform.SetParent(transform);
        changeUsernamePanel = changeUsernameObj.AddComponent<ChangeUsernamePanel>();
        changeUsernamePanel.SetParentPanel(this);
        
        // Crear AvatarSelectorPanel
        var avatarSelectorObj = new GameObject("AvatarSelectorPanel");
        avatarSelectorObj.transform.SetParent(transform);
        avatarSelectorPanel = avatarSelectorObj.AddComponent<AvatarSelectorPanel>();
        avatarSelectorPanel.SetParentPanel(this);
        
        // Crear GameHistoryPanel
        var gameHistoryObj = new GameObject("GameHistoryPanel");
        gameHistoryObj.transform.SetParent(transform);
        gameHistoryPanel = gameHistoryObj.AddComponent<GameHistoryPanel>();
        gameHistoryPanel.SetParentPanel(this);
        
        // Crear NotificationsPanel
        var notificationsObj = new GameObject("NotificationsPanel");
        notificationsObj.transform.SetParent(transform);
        notificationsPanel = notificationsObj.AddComponent<NotificationsPanel>();
        notificationsPanel.SetParentPanel(this);
    }
    
    private void OnEditUsernameClicked()
    {
        changeUsernamePanel.Show();
    }
    
    private void OnChangeAvatarClicked()
    {
        avatarSelectorPanel.Show();
    }
    
    private void OnGameHistoryClicked()
    {
        gameHistoryPanel.Show();
    }
    
    private void OnNotificationsClicked()
    {
        notificationsPanel.Show();
    }
    
    private void OnLogoutClicked()
    {
        authService.Logout();
        Debug.Log("Logged out successfully");
    }
    
    private void OnBackClicked()
    {
        Hide();
    }
    
    public void UpdateUserProfile(UserData user)
    {
        currentUser = user;
        usernameText.text = user.nombre_usuario;
        eloRatingText.text = $"ELO Rating: {user.rating_elo}";
        emailText.text = user.email;
        bioText.text = user.bio ?? "Sin biografía";
        
        // Cargar avatar si no es null
        if (!string.IsNullOrEmpty(user.avatar_url))
        {
            // Aquí se podría cargar la imagen desde URL
        }
    }
    
    public void Show()
    {
        canvasGroup.interactable = true;
        canvasGroup.alpha = 1;
    }
    
    public void Hide()
    {
        canvasGroup.interactable = false;
        canvasGroup.alpha = 0;
    }
}

/// <summary>
/// Panel para cambiar el nombre de usuario
/// </summary>
public class ChangeUsernamePanel : MonoBehaviour
{
    private TMP_InputField newUsernameInput;
    private Button confirmButton;
    private Button cancelButton;
    private TextMeshProUGUI statusText;
    private CanvasGroup canvasGroup;
    private ProfilePanel parentPanel;
    
    private AuthenticationService authService;
    
    private void Awake()
    {
        CreateUI();
    }
    
    private void CreateUI()
    {
        authService = AuthenticationService.Instance;
        
        // Crear Canvas
        var canvasObj = new GameObject("Canvas");
        canvasObj.transform.SetParent(transform);
        canvasGroup = canvasObj.AddComponent<CanvasGroup>();
        
        // Input para nuevo nombre de usuario
        var inputObj = new GameObject("UsernameInput");
        inputObj.transform.SetParent(canvasObj.transform);
        newUsernameInput = inputObj.AddComponent<TMP_InputField>();
        newUsernameInput.contentType = TMP_InputField.ContentType.Standard;
        
        // Texto de estado
        var statusObj = new GameObject("StatusText");
        statusObj.transform.SetParent(canvasObj.transform);
        statusText = statusObj.AddComponent<TextMeshProUGUI>();
        
        // Botones
        var confirmObj = new GameObject("ConfirmButton");
        confirmObj.transform.SetParent(canvasObj.transform);
        confirmButton = confirmObj.AddComponent<Button>();
        
        var cancelObj = new GameObject("CancelButton");
        cancelObj.transform.SetParent(canvasObj.transform);
        cancelButton = cancelObj.AddComponent<Button>();
        
        // Vincular eventos
        confirmButton.onClick.AddListener(OnConfirmClicked);
        cancelButton.onClick.AddListener(OnCancelClicked);
        newUsernameInput.onValueChanged.AddListener(_ => ValidateUsername());
    }
    
    private void ValidateUsername()
    {
        bool isValid = !string.IsNullOrEmpty(newUsernameInput.text)
                    && newUsernameInput.text.Length >= 3
                    && newUsernameInput.text.Length <= 60;
        confirmButton.interactable = isValid;
    }
    
    private void OnConfirmClicked()
    {
        if (!ValidateUsernameFormat(newUsernameInput.text))
        {
            statusText.text = "<color=red>Nombre de usuario inválido (3-60 caracteres)</color>";
            return;
        }
        
        authService.ChangeUsername(newUsernameInput.text);
        statusText.text = "<color=yellow>Cambiando nombre...</color>";
        confirmButton.interactable = false;
        
        Invoke(nameof(OnConfirmSuccess), 2f);
    }
    
    private void OnConfirmSuccess()
    {
        statusText.text = "<color=green>¡Nombre de usuario actualizado!</color>";
        Invoke(nameof(OnCancelClicked), 2f);
    }
    
    private void OnCancelClicked()
    {
        Hide();
    }
    
    private bool ValidateUsernameFormat(string username)
    {
        return !string.IsNullOrEmpty(username) && username.Length >= 3 && username.Length <= 60;
    }
    
    public void SetParentPanel(ProfilePanel parent)
    {
        parentPanel = parent;
    }
    
    public void Show()
    {
        canvasGroup.interactable = true;
        canvasGroup.alpha = 1;
        newUsernameInput.text = "";
        statusText.text = "";
    }
    
    public void Hide()
    {
        canvasGroup.interactable = false;
        canvasGroup.alpha = 0;
    }
}

/// <summary>
/// Panel para seleccionar avatar
/// </summary>
public class AvatarSelectorPanel : MonoBehaviour
{
    private Image selectedAvatarImage;
    private List<Button> avatarButtons;
    private Button confirmButton;
    private Button cancelButton;
    private CanvasGroup canvasGroup;
    private ProfilePanel parentPanel;
    
    private int selectedAvatarId = -1;
    
    private void Awake()
    {
        CreateUI();
    }
    
    private void CreateUI()
    {
        avatarButtons = new List<Button>();
        
        // Crear Canvas
        var canvasObj = new GameObject("Canvas");
        canvasObj.transform.SetParent(transform);
        canvasGroup = canvasObj.AddComponent<CanvasGroup>();
        
        // Imagen del avatar seleccionado
        var avatarObj = new GameObject("SelectedAvatarImage");
        avatarObj.transform.SetParent(canvasObj.transform);
        selectedAvatarImage = avatarObj.AddComponent<Image>();
        
        // Crear botones de avatares (ejemplo: 4 avatares)
        for (int i = 0; i < 4; i++)
        {
            var buttonObj = new GameObject($"AvatarButton{i}");
            buttonObj.transform.SetParent(canvasObj.transform);
            var button = buttonObj.AddComponent<Button>();
            int avatarId = i;
            button.onClick.AddListener(() => OnAvatarSelected(avatarId));
            avatarButtons.Add(button);
        }
        
        // Botón confirmar
        var confirmObj = new GameObject("ConfirmButton");
        confirmObj.transform.SetParent(canvasObj.transform);
        confirmButton = confirmObj.AddComponent<Button>();
        confirmButton.onClick.AddListener(OnConfirmClicked);
        
        // Botón cancelar
        var cancelObj = new GameObject("CancelButton");
        cancelObj.transform.SetParent(canvasObj.transform);
        cancelButton = cancelObj.AddComponent<Button>();
        cancelButton.onClick.AddListener(OnCancelClicked);
    }
    
    private void OnAvatarSelected(int avatarId)
    {
        selectedAvatarId = avatarId;
        // Actualizar imagen seleccionada
        Debug.Log($"Avatar {avatarId} seleccionado");
    }
    
    private void OnConfirmClicked()
    {
        if (selectedAvatarId >= 0)
        {
            // Guardar cambio de avatar
            Debug.Log($"Cambiar avatar a {selectedAvatarId}");
            Hide();
        }
    }
    
    private void OnCancelClicked()
    {
        Hide();
    }
    
    public void SetParentPanel(ProfilePanel parent)
    {
        parentPanel = parent;
    }
    
    public void Show()
    {
        canvasGroup.interactable = true;
        canvasGroup.alpha = 1;
    }
    
    public void Hide()
    {
        canvasGroup.interactable = false;
        canvasGroup.alpha = 0;
    }
}

/// <summary>
/// Panel para mostrar el historial de juegos
/// </summary>
public class GameHistoryPanel : MonoBehaviour
{
    private Transform gameListContainer;
    private Button backButton;
    private CanvasGroup canvasGroup;
    private ProfilePanel parentPanel;
    
    private AuthenticationService authService;
    
    private void Awake()
    {
        CreateUI();
    }
    
    private void CreateUI()
    {
        authService = AuthenticationService.Instance;
        
        // Crear Canvas
        var canvasObj = new GameObject("Canvas");
        canvasObj.transform.SetParent(transform);
        canvasGroup = canvasObj.AddComponent<CanvasGroup>();
        
        // Contenedor para lista de juegos
        var containerObj = new GameObject("GameListContainer");
        containerObj.transform.SetParent(canvasObj.transform);
        gameListContainer = containerObj.transform;
        
        // Botón atrás
        var backObj = new GameObject("BackButton");
        backObj.transform.SetParent(canvasObj.transform);
        backButton = backObj.AddComponent<Button>();
        backButton.onClick.AddListener(OnBackClicked);
    }
    
    private void PopulateGameHistory()
    {
        // Limpiar lista anterior
        foreach (Transform child in gameListContainer)
        {
            Destroy(child.gameObject);
        }
        
        // Obtener historial de juegos
        authService.FetchGameHistory(games =>
        {
            foreach (var game in games)
            {
                CreateGameEntry(game);
            }
        });
    }
    
    private void CreateGameEntry(GameData game)
    {
        var entryObj = new GameObject($"Game_{game.id_partida}");
        entryObj.transform.SetParent(gameListContainer);
        
        var text = entryObj.AddComponent<TextMeshProUGUI>();
        text.text = $"Juego {game.id_partida}: {game.resultado}";
    }
    
    private void OnBackClicked()
    {
        Hide();
    }
    
    public void SetParentPanel(ProfilePanel parent)
    {
        parentPanel = parent;
    }
    
    public void Show()
    {
        canvasGroup.interactable = true;
        canvasGroup.alpha = 1;
        PopulateGameHistory();
    }
    
    public void Hide()
    {
        canvasGroup.interactable = false;
        canvasGroup.alpha = 0;
    }
}

/// <summary>
/// Panel para mostrar notificaciones
/// </summary>
public class NotificationsPanel : MonoBehaviour
{
    private Transform notificationListContainer;
    private Button clearAllButton;
    private Button backButton;
    private CanvasGroup canvasGroup;
    private ProfilePanel parentPanel;
    
    private AuthenticationService authService;
    
    private void Awake()
    {
        CreateUI();
    }
    
    private void CreateUI()
    {
        authService = AuthenticationService.Instance;
        
        // Crear Canvas
        var canvasObj = new GameObject("Canvas");
        canvasObj.transform.SetParent(transform);
        canvasGroup = canvasObj.AddComponent<CanvasGroup>();
        
        // Contenedor para notificaciones
        var containerObj = new GameObject("NotificationListContainer");
        containerObj.transform.SetParent(canvasObj.transform);
        notificationListContainer = containerObj.transform;
        
        // Botón limpiar todo
        var clearObj = new GameObject("ClearAllButton");
        clearObj.transform.SetParent(canvasObj.transform);
        clearAllButton = clearObj.AddComponent<Button>();
        clearAllButton.onClick.AddListener(OnClearAllClicked);
        
        // Botón atrás
        var backObj = new GameObject("BackButton");
        backObj.transform.SetParent(canvasObj.transform);
        backButton = backObj.AddComponent<Button>();
        backButton.onClick.AddListener(OnBackClicked);
    }
    
    private void PopulateNotifications()
    {
        // Limpiar lista anterior
        foreach (Transform child in notificationListContainer)
        {
            Destroy(child.gameObject);
        }
        
        // Obtener notificaciones
        authService.FetchNotifications(notifications =>
        {
            foreach (var notification in notifications)
            {
                CreateNotificationEntry(notification);
            }
        });
    }
    
    private void CreateNotificationEntry(NotificationData notification)
    {
        var entryObj = new GameObject($"Notification_{notification.id_notificacion}");
        entryObj.transform.SetParent(notificationListContainer);
        
        var text = entryObj.AddComponent<TextMeshProUGUI>();
        string readStatus = notification.leida ? "[Leída]" : "[Nueva]";
        text.text = $"{readStatus} {notification.mensaje}";
        
        var button = entryObj.AddComponent<Button>();
        button.onClick.AddListener(() => OnNotificationClicked(notification.id_notificacion));
    }
    
    private void OnNotificationClicked(int notificationId)
    {
        // Marcar como leída
        Debug.Log($"Notificación {notificationId} marcada como leída");
    }
    
    private void OnClearAllClicked()
    {
        // Mostrar confirmación
        if (EditorUtility.DisplayDialog("Confirmación", "¿Limpiar todas las notificaciones?", "Sí", "No"))
        {
            PopulateNotifications();
        }
    }
    
    private void OnBackClicked()
    {
        Hide();
    }
    
    public void SetParentPanel(ProfilePanel parent)
    {
        parentPanel = parent;
    }
    
    public void Show()
    {
        canvasGroup.interactable = true;
        canvasGroup.alpha = 1;
        PopulateNotifications();
    }
    
    public void Hide()
    {
        canvasGroup.interactable = false;
        canvasGroup.alpha = 0;
    }
}
