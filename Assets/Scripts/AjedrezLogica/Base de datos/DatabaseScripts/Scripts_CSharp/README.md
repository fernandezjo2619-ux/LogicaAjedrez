# DatabaseScripts - Sistema de Autenticación Unity

## 📁 Estructura de Carpetas

```
DatabaseScripts/
├── Scripts_CSharp/
│   ├── Authentication/
│   │   ├── Models/
│   │   │   └── UserData.cs              (11 clases serializables)
│   │   ├── Services/
│   │   │   └── AuthenticationService.cs (650+ líneas, servicio principal)
│   │   ├── UI/
│   │   │   ├── LoginPanel.cs            (Pantalla de login)
│   │   │   ├── RegisterPanel.cs         (Pantalla de registro)
│   │   │   ├── ForgotPasswordPanel.cs   (Recuperación de contraseña)
│   │   │   └── VerificationPanel.cs     (Verificación de email)
│   │   └── AuthenticationManager.cs     (Orchestrador de paneles)
│   └── Profile/
│       └── ProfilePanel.cs              (Perfil de usuario + 4 sub-paneles)
└── [Documentación SQL y guías]
```

## 🎯 Descripción de Archivos

### **Models/UserData.cs** (138 líneas)
Defines data structures para JSON deserialization de Supabase:
- `UserData` - Datos del usuario completo
- `LoginResponse`, `RegisterResponse`, `VerificationResponse` - Respuestas API
- `SessionData` - JWT tokens y sesión
- `GameData`, `NotificationData`, `AvatarData` - Datos auxiliares

### **Services/AuthenticationService.cs** (650+ líneas)
**Core del sistema de autenticación.** Singleton con métodos:
- `Register()` - Crear usuario (bcrypt + email)
- `Login()` - Autenticación con rate limiting
- `VerifyEmail()` - Verificar código de email
- `RequestPasswordReset()` / `ResetPasswordWithToken()` - Recuperación de contraseña
- `ChangeUsername()` - Actualizar nombre
- `FetchGameHistory()` - Obtener partidas del usuario
- `FetchNotifications()` - Obtener notificaciones
- `Logout()` - Cerrar sesión

**Características:**
- Manejo de sesiones con PlayerPrefs
- Rate limiting automático
- Eventos delegados (`OnLoginSuccess`, `OnLoginFailed`, etc.)

### **AuthenticationManager.cs** (70 líneas)
Orchestrador de UI que controla la visibilidad de paneles:
- `ShowLoginPanel()`, `ShowRegisterPanel()`, etc.
- Auto-login si hay sesión activa
- Carga de escena principal

### **Authentication/UI/LoginPanel.cs** (145 líneas)
**Pantalla de Login:**
- Email + contraseña inputs
- Botón "Recuérdame" para persistencia
- Validación en tiempo real
- Navigation a Registro / Recuperación contraseña

### **Authentication/UI/RegisterPanel.cs** (155 líneas)
**Pantalla de Registro:**
- Username, email, password, confirm password
- Términos y condiciones checkbox
- Validación: contraseña mínimo 8 caracteres, email válido, username 3-60 caracteres
- Muestra panel de verificación tras registro exitoso

### **Authentication/UI/ForgotPasswordPanel.cs** (175 líneas)
**Recuperación de Contraseña (2 pasos):**
- **Paso 1:** Ingresa email → recibe código
- **Paso 2:** Ingresa código + nueva contraseña
- Interfaz visual que alterna entre pasos

### **Authentication/UI/VerificationPanel.cs** (135 líneas)
**Verificación de Email:**
- Input para código de verificación
- Botón "Resend" con cooldown (60 segundos)
- Muestra email del usuario

### **Profile/ProfilePanel.cs** (450+ líneas)
**Perfil de Usuario + 4 Sub-Paneles:**

1. **ChangeUsernamePanel** - Cambiar nombre de usuario (validación 3-60 chars)
2. **AvatarSelectorPanel** - Seleccionar avatar de galería
3. **GameHistoryPanel** - Mostrar historial de partidas
4. **NotificationsPanel** - Mostrar notificaciones, marcar leídas, limpiar todo

---

## 🔌 Cómo Usar en Unity

### **1. Setup Inicial**

Copiar todos los archivos desde `DatabaseScripts/Scripts_CSharp/` a tu proyecto:
```
Assets/Scripts/
├── Authentication/
│   ├── Models/UserData.cs
│   ├── Services/AuthenticationService.cs
│   ├── UI/LoginPanel.cs
│   ├── UI/RegisterPanel.cs
│   ├── UI/ForgotPasswordPanel.cs
│   ├── UI/VerificationPanel.cs
│   └── AuthenticationManager.cs
└── Profile/
    └── ProfilePanel.cs
```

### **2. En tu Escena de Login (Unity Editor)**

1. **Crear Canvas UI:**
   - Create → UI → Text Mesh Pro → Button

2. **Instanciar AuthenticationManager:**
   ```csharp
   GameObject authObj = new GameObject("AuthenticationManager");
   AuthenticationManager manager = authObj.AddComponent<AuthenticationManager>();
   ```

3. **Vincular Paneles en Inspector:**
   - Asignar LoginPanel, RegisterPanel, ForgotPasswordPanel, VerificationPanel

4. **Configurar referencias en cada Panel:**
   - LoginPanel → vincular InputFields, Buttons, TextMeshPro elements
   - RegisterPanel → vincular InputFields, Toggle de términos
   - etc.

### **3. Uso en Código**

```csharp
// Acceder al servicio
AuthenticationService authService = AuthenticationService.Instance;

// Registrarse
authService.Register("username", "email@example.com", "password");

// Loguearse
authService.Login("email@example.com", "password");

// Verificar email
authService.VerifyEmail(userId, "verification_code");

// Cambiar contraseña
authService.RequestPasswordReset("email@example.com");
authService.ResetPasswordWithToken("reset_token", "new_password");

// Cambiar nombre de usuario
authService.ChangeUsername("new_username");

// Ver historial
authService.FetchGameHistory(games => {
    foreach(var game in games) {
        Debug.Log(game.resultado);
    }
});

// Ver notificaciones
authService.FetchNotifications(notifications => {
    foreach(var notif in notifications) {
        Debug.Log(notif.mensaje);
    }
});

// Logout
authService.Logout();
```

### **4. Suscribirse a Eventos**

```csharp
authService.OnLoginSuccess += HandleLoginSuccess;
authService.OnLoginFailed += HandleLoginFailed;
authService.OnRegisterSuccess += HandleRegisterSuccess;
authService.OnRegisterFailed += HandleRegisterFailed;
authService.OnEmailVerified += HandleEmailVerified;
authService.OnEmailVerificationFailed += HandleVerificationFailed;

private void HandleLoginSuccess(UserData user) {
    Debug.Log($"Bienvenido, {user.nombre_usuario}!");
}

private void HandleLoginFailed(string error) {
    Debug.LogError($"Error: {error}");
}
```

---

## 🔐 Seguridad Implementada

✅ **Bcrypt Hashing** - Contraseñas hasheadas con coste 10 en PostgreSQL  
✅ **Rate Limiting** - 5 intentos fallidos = 15 min bloqueado  
✅ **Email Verification** - Token de 32 bytes aleatorio  
✅ **Session Tokens** - JWT con refresh tokens (30 días)  
✅ **Row Level Security (RLS)** - Acceso solo a datos propios del usuario  
✅ **Input Validation** - Validación en cliente y servidor  
✅ **PlayerPrefs Encryptado** - Sesión persiste entre sesiones de juego  

---

## 🗄️ Base de Datos (Supabase)

Los scripts se conectan a 8 tablas:
1. `usuarios` - Datos de usuarios
2. `sesiones` - Sesiones activas
3. `intentos_login` - Rate limiting
4. `recuperacion_contraseña` - Tokens de recuperación
5. `avatares` - Avatares de usuario
6. `partidas` - Historial de juegos
7. `notificaciones` - Notificaciones
8. `cambios_nombre` - Auditoría de cambios

**6 Funciones SQL:**
- `crear_usuario()` - Crear usuario con email verificación
- `verificar_password()` - Verificar contraseña vs bcrypt
- `validar_intento_login()` - Rate limiting
- `crear_token_recuperacion()` - Token para reset
- `usar_token_recuperacion()` - Consumir token
- `verificar_email_token()` - Verificar email

---

## 📋 Tabla de Métodos

| Método | Parámetros | Retorna | Uso |
|--------|-----------|---------|-----|
| `Register()` | username, email, password | userId | Registrar nuevo usuario |
| `Login()` | email, password | SessionData | Autenticarse |
| `VerifyEmail()` | userId, token | bool | Verificar email |
| `RequestPasswordReset()` | email | - | Enviar token reset |
| `ResetPasswordWithToken()` | token, password | bool | Cambiar contraseña |
| `ChangeUsername()` | newUsername | bool | Cambiar nombre |
| `FetchGameHistory()` | callback | List<GameData> | Ver partidas |
| `FetchNotifications()` | callback | List<NotificationData> | Ver notificaciones |
| `Logout()` | - | - | Cerrar sesión |

---

## 🎓 Documentación Completa

Para más detalles, ver:
- `GUIA_COMPLETA_AUTENTICACION.md` - Documentación exhaustiva
- `QUICK_START.md` - Inicio rápido 15 minutos
- `EJEMPLOS_USO.md` - Ejemplos de código
- `README_SISTEMA_AUTENTICACION.md` - Arquitectura visual

---

## ⚡ Arquitectura

```
Unity Scene
   ↓
AuthenticationManager (orchestrador)
   ↓
[LoginPanel] [RegisterPanel] [ForgotPasswordPanel] [VerificationPanel] [ProfilePanel]
   ↓
AuthenticationService (singleton)
   ↓
UnityWebRequest API calls
   ↓
Supabase PostgreSQL
```

---

## 📝 Checklist de Integración

- [ ] Copiar Scripts_CSharp/ a Assets/Scripts/
- [ ] Copiar tablas SQL a Supabase
- [ ] Copiar funciones SQL a Supabase
- [ ] Configurar .env con credenciales
- [ ] Crear Canvas UI en escena
- [ ] Vincular paneles en AuthenticationManager
- [ ] Vincular InputFields/Buttons a cada panel
- [ ] Testar login/registro/email verification
- [ ] Testar recuperación de contraseña
- [ ] Testar cambio de nombre de usuario
- [ ] Implementar UI visual (estilos, colores, fonts)

---

## 🆘 Troubleshooting

**"Connection failed"** → Verificar credenciales Supabase en .env  
**"Email not verified"** → Revisar token enviado a email y ValidEmail en DB  
**"Rate limited"** → Esperar 15 minutos o revisar tabla `intentos_login`  
**"Session expired"** → Refresh token, o hacer login de nuevo  
**Prefab not found** → Asegurar que casos y SceneNames coinciden  

---

**Última actualización:** 2024  
**Versión del Sistema:** 1.0 (Completo)
