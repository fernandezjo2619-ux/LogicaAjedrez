# ⚡ Quick Start - Sistema de Autenticación (15 minutos)

## 🟦 Paso 1: Setup en Supabase (5 min)

### 1.1 Crear Proyecto
```bash
1. Ve a https://supabase.com
2. Haz login / Crea cuenta
3. New Project
4. Espera 2-3 minutos...
```

### 1.2 Copiar Credenciales
```
Settings → API → Copia:
- Project URL (p.ej. https://xxxxx.supabase.co)
- anon/public key (p.ej. eyJhbGciOi...)
```

### 1.3 Ejecutar SQL (3 min)
```
1. SQL Editor → New Query
2. Pega TODO el SQL de GUIA_COMPLETA_AUTENTICACION.md
3. Haz clic en ▶ (Run)
4. Espera a que termine ✅
```

---

## 🎮 Paso 2: Setup en Unity (10 min)

### 2.1 Crear Escenas
```
Assets → Scenes → New Scene
Guarda como: AuthenticationScene

Assets → Scenes → New Scene  
Guarda como: MainGame
```

### 2.2 Crear Canvas (en AuthenticationScene)
```
Right-click Canvas
  → UI → Panel → LoginPanel
  → UI → InputField (Email)
  → UI → InputField (Password)
  → UI → Button (Login)
  → UI → Text (Status)

Repite para: RegisterPanel, ForgotPasswordPanel, VerificationPanel
```

### 2.3 Copiar Scripts
```
Copia todos los archivos .cs a Assets/Scripts/Authentication/
```

### 2.4 Configurar AuthenticationService
```
Canvas → Add Component → AuthenticationManager

En Inspector:
- Supabase URL: https://xxxxx.supabase.co
- Supabase Key: eyJhbGciOi...

Arrastra cada Panel al campo correspondiente
```

### 2.5 Build Settings
```
File → Build Settings
- Agregar AuthenticationScene (Index 0)
- Agregar MainGame (Index 1)
```

---

## 🧪 Paso 3: Prueba Rápida (1 min)

### En Supabase SQL Editor:
```sql
-- Crear usuario de prueba
SELECT crear_usuario('test_user', 'test@example.com', 'password123');

-- Verificar email
SELECT verificar_email_token(1, 
  (SELECT token_verificacion FROM usuarios WHERE id_usuario = 1)
);

-- Login test
SELECT validar_intento_login('test@example.com', 'password123', '127.0.0.1');
```

### En Unity:
```
1. Play
2. Intenta login con test@example.com / password123
3. ¡Debe entrar! ✅
```

---

## 📋 Checklist Rápido

```
Supabase:
  ☐ Proyecto creado
  ☐ URL y Key copiadas
  ☐ SQL ejecutado
  ☐ Funciones disponibles (en SQL Editor → Functions)
  
Unity:
  ☐ Scripts descargados
  ☐ Escenas creadas
  ☐ Canvas con UI
  ☐ Scripts asignados
  ☐ Credenciales configuradas
  ☐ Build Settings actualizado
  
Test:
  ☐ Usuario de prueba creado
  ☐ Email verificado
  ☐ Login exitoso
  ☐ PlayerPrefs guardado
```

---

## 🆘 Si Algo Falla

### "Cannot POST /rpc/crear_usuario"
```
→ Verificar que el URL es exacto
→ Sin trailing slash: https://xxxxx.supabase.co (no ...co/)
```

### "Authorization header required"
```
→ Verificar que la Key está configurada en inspector
→ No mezcles URL y Key de diferentes proyectos
```

### "Email already exists"
```
→ Usar otro email de prueba
→ O borrar o registro anterior en SQL:
  DELETE FROM usuarios WHERE email = 'test@example.com';
```

### "Not Found" (404)
```
→ Asegúrate que ejecutaste TODO el SQL correctamente
→ Revisa en SQL Editor → Functions que exista crear_usuario
```

---

## 🎯 Funcionalidad Básica: LISTA

```
✅ Registro                (RegisterPanel.cs)
✅ Login                   (LoginPanel.cs)
✅ Verificación Email      (VerificationPanel.cs)
✅ Recuperación Password   (ForgotPasswordPanel.cs)
✅ Cambiar Perfil          (ProfilePanel.cs)
✅ Sesiones Persistentes   (PlayerPrefs)
✅ Seguridad Bcrypt        (SQL función)
✅ Rate Limiting           (SQL función)
```

---

## 📈 Siguientes Pasos (Opcional)

```
1. Implementar UI mejorada (Canvas más bonito)
2. Agregar 2FA (Two-Factor Authentication)
3. Integrar SendGrid para emails reales
4. Sistema de amigos/mensajes
5. Leaderboard con ELO
6. Social login (Google, GitHub)
7. Analytics
8. Push notifications
```

---

## 📚 Documentación Completa

- `GUIA_COMPLETA_AUTENTICACION.md` - Setup detallado
- `EJEMPLOS_USO.md` - Ejemplos de código
- `README_SISTEMA_AUTENTICACION.md` - Resumen visual

---

## 💪 ¡Listo para comenzar!

```
tiempo estimado: 15 minutos
dificultad: Media ⭐⭐
satisfacción: MÁXIMA 🚀

¡Que disfrutes! 🎮
```

