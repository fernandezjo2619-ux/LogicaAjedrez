# ✅ CHECKLIST: CREAR ESCENA DE LOBBY EN UNITY

**Estado:** Guía paso a paso interactiva  
**Objetivo:** Crear escena LobbyScene totalmente funcional

---

## FASE 1: PREPARACIÓN PREVIA

- [ ] **P1.1** - Verificar Unity 2021.3 LTS o superior instalado
- [ ] **P1.2** - TextMeshPro importado (viene por defecto)
- [ ] **P1.3** - Scripts C# listos:
  - ✅ NetworkLobbyManager.cs
  - ✅ LobbyUIController.cs
  - ✅ GameSyncManager.cs

**Scripts están en:** `Assets/Scripts/AjedrezLogica/multijugador/`

---

## FASE 2: ESTRUCTURA DE CARPETAS

- [ ] **P2.1** - Crear carpeta: `Assets/Scenes/`
- [ ] **P2.2** - Crear carpeta: `Assets/Scenes/Lobby/`
- [ ] **P2.3** - Crear carpeta: `Assets/Scenes/Game/`
- [ ] **P2.4** - Crear carpeta: `Assets/Prefabs/UI/`

**Estructura:**
```
Assets/
├── Scenes/
│   ├── Lobby/
│   └── Game/
├── Prefabs/
│   └── UI/
└── Scripts/
    ├── AjedrezLogica/
    │   └── multijugador/
    │       ├── NetworkLobbyManager.cs ✅
    │       ├── LobbyUIController.cs ✅
    │       └── GameSyncManager.cs ✅
    └── ...
```

---

## FASE 3: CREAR ESCENA PRINCIPAL

### Crear escena LobbyScene

- [ ] **P3.1** - Abrir Unity Editor
- [ ] **P3.2** - File > New Scene (Ctrl+N)
- [ ] **P3.3** - File > Save Scene As
- [ ] **P3.4** - Nombre: `LobbyScene`
- [ ] **P3.5** - Ubicación: `Assets/Scenes/Lobby/`
- [ ] **P3.6** - Presionar Save

---

## FASE 4: CREAR CANVAS Y FONDO

### Canvas principal

- [ ] **P4.1** - Hierarchy > UI > Panel - Image (Legacy)
- [ ] **P4.2** - Se crean automáticamente:
  - Canvas (raíz)
  - Panel (hijo del Canvas)

### Configurar Canvas

- [ ] **P4.3** - Seleccionar "Canvas"
- [ ] **P4.4** - Inspector > Canvas:
  - Render Mode: **Screen Space - Overlay**
  - Canvas Scaler > UI Scale Mode: **Scale with Screen Size**
  - Canvas Scaler > Reference Resolution: **1920 x 1080**

### Configurar fondo (Panel)

- [ ] **P4.5** - Seleccionar "Panel"
- [ ] **P4.6** - Renombrar a: `BackgroundPanel` (F2)
- [ ] **P4.7** - Inspector > Image > Color: **RGB(30, 30, 30)** (gris oscuro)
- [ ] **P4.8** - Inspector > Rect Transform:
  - Left: 0
  - Right: 0
  - Top: 0
  - Bottom: 0

---

## FASE 5: CREAR ESTRUCTURA DE UI

### Contenedor Principal

**OPCIÓN A: Si ves "Vertical Layout Group" en el menú**
- [ ] **P5.1** - Seleccionar Canvas
- [ ] **P5.2** - Click derecho > UI > Vertical Layout Group
- [ ] **P5.3** - Renombrar a: `MainContainer`

**OPCIÓN B: Si NO ves "Vertical Layout Group" (versiones recientes de Unity)**
- [ ] **P5.1** - Seleccionar Canvas
- [ ] **P5.2** - Click derecho > UI > Panel - Image (o Create Empty)
- [ ] **P5.3** - Renombrar a: `MainContainer`
- [ ] **P5.4** - Con MainContainer seleccionado, en Inspector > **Add Component**
- [ ] **P5.5** - Buscar y añadir: **Vertical Layout Group**
- [ ] **P5.6** - En Inspector, configurar Vertical Layout Group:
  - Spacing: **20**
  - Child Force Expand Height: **ON**
  - Child Force Expand Width: **ON**

**Configurar Rect Transform (ambas opciones)**
- [ ] **P5.7** - Seleccionar MainContainer
- [ ] **P5.8** - Inspector > Rect Transform:
  - Anchor Min: **(0.5, 0.5)**
  - Anchor Max: **(0.5, 0.5)**
  - Position: **(0, 0)**
  - Size: **(1200, 900)**
  - Pivot: **(0.5, 0.5)**

---

## FASE 6: ELEMENTOS DE TITULO

### Título principal

- [ ] **P6.1** - Seleccionar MainContainer
- [ ] **P6.2** - Click derecho > UI > Text - TextMeshPro
- [ ] **P6.3** - Renombrar a: `TitleText`
- [ ] **P6.4** - Inspector > Rect Transform > Layout Element:
  - Preferred Height: **100**
- [ ] **P6.5** - Inspector > Text Mesh Pro component:
  - Text: **"=== SALA DE MULTIJUGADOR ===**
  - Font Size: **60**
  - Alignment: **Center**
  - Color: **Blanco (255, 255, 255)**

---

## FASE 7: PANEL DE CONEXIÓN

### Crear panel

- [ ] **P7.1** - Seleccionar MainContainer
- [ ] **P7.2** - Click derecho > UI > Panel - Image
- [ ] **P7.3** - Renombrar a: `ConnectionPanel`
- [ ] **P7.4** - Inspector > Image > Color: **RGB(50, 50, 50)**
- [ ] **P7.5** - Inspector > Rect Transform > Layout Element:
  - Preferred Width: **1000**
  - Preferred Height: **300**

### Añadir Vertical Layout Group

- [ ] **P7.6** - Con ConnectionPanel seleccionado en Hierarchy
- [ ] **P7.7** - Inspector > **Add Component** > Vertical Layout Group
- [ ] **P7.8** - Configurar en Inspector:
  - Spacing: **15**
  - Padding: Left 20, Right 20, Top 20, Bottom 20
  - Child Force Expand Width: **ON**
  - Child Force Expand Height: **ON**

---

## FASE 8: CAMPOS DE ENTRADA

### RoomNameInput

- [ ] **P8.1** - Seleccionar ConnectionPanel
- [ ] **P8.2** - Click derecho > UI > Input Field - TextMeshPro
- [ ] **P8.3** - Renombrar a: `RoomNameInput`
- [ ] **P8.4** - Inspector > Text Input component:
  - Placeholder Text: **"Nombre de sala..."**
  - Text: **"Mi Sala"**
  - Font Size: **32**
- [ ] **P8.5** - Inspector > Rect Transform > Layout Element:
  - Preferred Height: **50**

### IPAddressInput

- [ ] **P8.6** - Seleccionar ConnectionPanel
- [ ] **P8.7** - Click derecho > UI > Input Field - TextMeshPro
- [ ] **P8.8** - Renombrar a: `IPAddressInput`
- [ ] **P8.9** - Inspector > Text Input component:
  - Placeholder Text: **"Ej: 192.168.1.100"**
  - Font Size: **32**
- [ ] **P8.10** - Inspector > Rect Transform > Layout Element:
  - Preferred Height: **50**

### PortInput

- [ ] **P8.11** - Seleccionar ConnectionPanel
- [ ] **P8.12** - Click derecho > UI > Input Field - TextMeshPro
- [ ] **P8.13** - Renombrar a: `PortInput`
- [ ] **P8.14** - Inspector > Text Input component:
  - Placeholder Text: **"8000"**
  - Text: **"8000"**
  - Font Size: **32**
  - Content Type: **Integer Number**
- [ ] **P8.15** - Inspector > Rect Transform > Layout Element:
  - Preferred Height: **50**

---

## FASE 9: BOTONES DE ACCIÓN (HOST/JOIN)

### Panel de botones

- [ ] **P9.1** - Seleccionar ConnectionPanel
- [ ] **P9.2** - Click derecho > UI > Panel - Image (o Create Empty)
- [ ] **P9.3** - Renombrar a: `ButtonsPanel`
- [ ] **P9.4** - Con ButtonsPanel seleccionado, en Inspector > **Add Component** > Horizontal Layout Group
- [ ] **P9.5** - Configurar Horizontal Layout Group:
  - Spacing: **20**
  - Child Force Expand Width: **ON**
  - Child Force Expand Height: **ON**
- [ ] **P9.6** - Inspector > Rect Transform > Layout Element:
  - Preferred Height: **80**

### Botón HOST

- [ ] **P9.7** - Seleccionar ButtonsPanel
- [ ] **P9.8** - Click derecho > UI > Button - TextMeshPro
- [ ] **P9.9** - Renombrar a: `HostButton`
- [ ] **P9.10** - Seleccionar hijo "Text (TMP)" de HostButton
- [ ] **P9.11** - Inspector > Text Mesh Pro:
  - Text: **"CREAR SALA"**
  - Font Size: **36**
  - Color: **Blanco**
- [ ] **P9.12** - Seleccionar HostButton (padre)
- [ ] **P9.13** - Inspector > Image > Color: **RGB(0, 120, 0)** (verde oscuro)
- [ ] **P9.14** - Inspector > Button > Colores:
  - Normal Color: **RGB(0, 120, 0)**
  - Highlighted Color: **RGB(0, 200, 0)**
  - Pressed Color: **RGB(100, 255, 100)**
  - Disabled Color: **Gris**

### Botón JOIN

- [ ] **P9.15** - Seleccionar ButtonsPanel
- [ ] **P9.16** - Click derecho > UI > Button - TextMeshPro
- [ ] **P9.17** - Renombrar a: `JoinButton`
- [ ] **P9.18** - Seleccionar hijo "Text (TMP)" de JoinButton
- [ ] **P9.19** - Inspector > Text Mesh Pro:
  - Text: **"UNIRSE"**
  - Font Size: **36**
  - Color: **Blanco**
- [ ] **P9.20** - Seleccionar JoinButton (padre)
- [ ] **P9.21** - Inspector > Image > Color: **RGB(0, 80, 120)** (azul oscuro)
- [ ] **P9.22** - Inspector > Button > Colores:
  - Normal Color: **RGB(0, 80, 120)**
  - Highlighted Color: **RGB(0, 150, 200)**
  - Pressed Color: **RGB(100, 200, 255)**
  - Disabled Color: **Gris**

---

## FASE 10: ETIQUETA DE ESTADO

- [ ] **P10.1** - Seleccionar MainContainer
- [ ] **P10.2** - Click derecho > UI > Text - TextMeshPro
- [ ] **P10.3** - Renombrar a: `StatusLabel`
- [ ] **P10.4** - Inspector > Rect Transform > Layout Element:
  - Preferred Height: **100**
- [ ] **P10.5** - Inspector > Text Mesh Pro:
  - Text: **"Desconectado | Esperando conexión..."**
  - Font Size: **32**
  - Alignment: **Center**
  - Color: **Rojo (255, 0, 0)**

---

## FASE 11: PANEL DE JUGADORES CONECTADOS

### Panel principal

- [ ] **P11.1** - Seleccionar MainContainer
- [ ] **P11.2** - Click derecho > UI > Panel - Image
- [ ] **P11.3** - Renombrar a: `PlayersPanel`
- [ ] **P11.4** - Inspector > Image > Color: **RGB(40, 40, 40)**
- [ ] **P11.5** - Inspector > Rect Transform > Layout Element:
  - Preferred Height: **150**

### Título

- [ ] **P11.6** - Seleccionar PlayersPanel
- [ ] **P11.7** - Click derecho > UI > Text - TextMeshPro
- [ ] **P11.8** - Renombrar a: `PlayersTitle`
- [ ] **P11.9** - Inspector > Text Mesh Pro:
  - Text: **"[ JUGADORES CONECTADOS ]"**
  - Font Size: **28**
  - Alignment: **Center**
  - Color: **Verde (0, 255, 0)**

### Etiqueta de jugadores

- [ ] **P11.10** - Seleccionar PlayersPanel
- [ ] **P11.11** - Click derecho > UI > Text - TextMeshPro
- [ ] **P11.12** - Renombrar a: `ConnectedPlayersLabel`
- [ ] **P11.13** - Inspector > Text Mesh Pro:
  - Text: **(Vacio)**
  - Font Size: **24**
  - Alignment: **Left**
  - Overflow: **Vertical Wrap**
  - Color: **Blanco**
- [ ] **P11.14** - Inspector > Rect Transform:
  - Margin Left: 20, Right: 20, Top: 10, Bottom: 10

---

## FASE 12: LISTA DE SALAS DESCUBIERTAS

### Etiqueta de título

- [ ] **P12.1** - Seleccionar MainContainer
- [ ] **P12.2** - Click derecho > UI > Text - TextMeshPro
- [ ] **P12.3** - Renombrar a: `RoomsLabel`
- [ ] **P12.4** - Inspector > Text Mesh Pro:
  - Text: **"=== SALAS DISPONIBLES ==="**
  - Font Size: **28**
  - Alignment: **Center**
  - Color: **Cyan (0, 255, 255)**

### Scroll View para salas

- [ ] **P12.5** - Seleccionar MainContainer
- [ ] **P12.6** - Click derecho > UI > Scroll View
- [ ] **P12.7** - Se crean automáticamente algunos elementos

**OPCIÓN A: Si se genera todo automáticamente (ScrollView, Viewport, Scrollbar, Content)**
- [ ] **P12.8a** - Renombrar ScrollView a: `RoomsScrollView`
- [ ] **P12.9a** - Renombrar Content a: `RoomListContainer`
- [ ] Ve a **P12.10**

**OPCIÓN B: Si NO se generó el Content (versiones recientes)**
- [ ] **P12.8b** - Dentro de Viewport, click derecho > UI > Panel - Image
- [ ] **P12.9b** - Renombrar a: `RoomListContainer`
- [ ] **P12.10b** - Seleccionar RoomsScrollView (padre)
- [ ] **P12.11b** - En Inspector > Scroll Rect > Content: Arrastra **RoomListContainer** desde Hierarchy

**Continuación (ambas opciones):**
- [ ] **P12.12** - Seleccionar RoomsScrollView
- [ ] **P12.13** - Inspector > Rect Transform > Layout Element:
  - Preferred Height: **250**
- [ ] **P12.14** - Inspector > Scroll Rect:
  - Content: **RoomListContainer**
  - Viewport: **Viewport**
  - Vertical: **ON**
  - Horizontal: **OFF**

### Configurar RoomListContainer

- [ ] **P12.15** - Seleccionar RoomListContainer
- [ ] **P12.16** - Inspector > **Add Component** > Vertical Layout Group:
  - Spacing: **10**
  - Child Force Expand Width: **ON**
  - Child Force Expand Height: **OFF**

---

## FASE 13: BOTONES INFERIORES

### Panel de botones

- [ ] **P13.1** - Seleccionar MainContainer
- [ ] **P13.2** - Click derecho > UI > Panel - Image (o Create Empty)
- [ ] **P13.3** - Renombrar a: `BottomButtonsPanel`
- [ ] **P13.4** - Con BottomButtonsPanel seleccionado, en Inspector > **Add Component** > Horizontal Layout Group
- [ ] **P13.5** - Configurar Horizontal Layout Group:
  - Spacing: **20**
  - Child Force Expand Width: **ON**
- [ ] **P13.6** - Inspector > Rect Transform > Layout Element:
  - Preferred Height: **80**

### Botón INICIAR JUEGO

- [ ] **P13.7** - Seleccionar BottomButtonsPanel
- [ ] **P13.8** - Click derecho > UI > Button - TextMeshPro
- [ ] **P13.9** - Renombrar a: `StartGameButton`
- [ ] **P13.10** - Seleccionar hijo "Text"
- [ ] **P13.11** - Inspector > Text Mesh Pro:
  - Text: **"INICIAR JUEGO"**
  - Font Size: **36**
- [ ] **P13.12** - Seleccionar StartGameButton (padre)
- [ ] **P13.13** - Inspector > Image > Color: **RGB(200, 150, 0)** (dorado)
- [ ] **P13.14** - Inspector > Button > Colores en tonos dorado/amarillo

### Botón VOLVER

- [ ] **P13.15** - Seleccionar BottomButtonsPanel
- [ ] **P13.16** - Click derecho > UI > Button - TextMeshPro
- [ ] **P13.17** - Renombrar a: `BackButton`
- [ ] **P13.18** - Seleccionar hijo "Text"
- [ ] **P13.19** - Inspector > Text Mesh Pro:
  - Text: **"VOLVER"**
  - Font Size: **36**
- [ ] **P13.20** - Seleccionar BackButton (padre)
- [ ] **P13.21** - Inspector > Image > Color: **RGB(120, 0, 0)** (rojo oscuro)
- [ ] **P13.22** - Inspector > Button > Colores en tonos rojo

---

## FASE 14: CREAR PREFAB DE BOTÓN DE SALA

### Crear prefab temporal

- [ ] **P14.1** - En Hierarchy: Click derecho > UI > Button - TextMeshPro
- [ ] **P14.2** - Renombrar a: `RoomListButton`
- [ ] **P14.3** - Inspector > Rect Transform > Layout Element:
  - Preferred Height: **60**
- [ ] **P14.4** - Seleccionar hijo "Text"
- [ ] **P14.5** - Inspector > Text Mesh Pro:
  - Text: **"Sala Ejemplo"**
  - Font Size: **24**
  - Alignment: **Left**
  - Overflow: **Vertical Wrap**
- [ ] **P14.6** - Seleccionar RoomListButton (padre)
- [ ] **P14.7** - Inspector > Image > Color: **RGB(60, 80, 100)** (azul grisáceo)
- [ ] **P14.8** - Inspector > Button > Colores:
  - Normal Color: **RGB(60, 80, 100)**
  - Highlighted Color: **RGB(100, 150, 200)**
  - Pressed Color: **RGB(150, 200, 255)**

### Guardar como prefab

- [ ] **P14.9** - Seleccionar RoomListButton
- [ ] **P14.10** - Arrastrar desde Hierarchy a `Assets/Prefabs/UI/`
- [ ] **P14.11** - Renombrar archivo a: `RoomListButtonPrefab`
- [ ] **P14.12** - Eliminar del Canvas (click derecho > Delete)

---

## FASE 15: CREAR MANAGERS EN ESCENA

### NetworkManager

- [ ] **P15.1** - Hierarchy > Click derecho > Create Empty
- [ ] **P15.2** - Renombrar a: `NetworkManager`
- [ ] **P15.3** - Inspector > Add Component > NetworkLobbyManager

### GameSyncManager

- [ ] **P15.4** - Hierarchy > Click derecho > Create Empty
- [ ] **P15.5** - Renombrar a: `GameSyncManager`
- [ ] **P15.6** - Inspector > Add Component > GameSyncManager

### LobbyUIManager

- [ ] **P15.7** - Hierarchy > Click derecho > Create Empty
- [ ] **P15.8** - Renombrar a: `LobbyUIManager`
- [ ] **P15.9** - Inspector > Add Component > LobbyUIController

---

## FASE 16: ASIGNAR REFERENCIAS EN LOBBYUICONTROLLER

**Seleccionar LobbyUIManager en Hierarchy**

### Asignar Botones

- [ ] **P16.1** - Host Button: Arrastra **HostButton** desde Hierarchy
- [ ] **P16.2** - Join Button: Arrastra **JoinButton** desde Hierarchy
- [ ] **P16.3** - Start Game Button: Arrastra **StartGameButton** desde Hierarchy
- [ ] **P16.4** - Back Button: Arrastra **BackButton** desde Hierarchy

### Asignar Input Fields

- [ ] **P16.5** - Room Name Input: Arrastra **RoomNameInput**
- [ ] **P16.6** - IP Address Input: Arrastra **IPAddressInput**
- [ ] **P16.7** - Port Input: Arrastra **PortInput**

### Asignar Labels

- [ ] **P16.8** - Status Label: Arrastra **StatusLabel**
- [ ] **P16.9** - Connected Players Label: Arrastra **ConnectedPlayersLabel**

### Asignar Containers

- [ ] **P16.10** - Room List Container: Arrastra **RoomListContainer**
- [ ] **P16.11** - Room List Button Prefab: Arrastra **RoomListButtonPrefab** desde `Assets/Prefabs/UI/`

### Asignar Scene Name

- [ ] **P16.12** - Game Scene Name: Escribe **"GameScene"**

---

## FASE 17: VERIFICACIÓN DE JERARQUÍA

**Verificar que la estructura sea correcta:**

```
Canvas
├── BackgroundPanel (fondo gris)
└── MainContainer (layout vertical)
    ├── TitleText
    ├── ConnectionPanel
    │   ├── RoomNameInput
    │   ├── IPAddressInput
    │   ├── PortInput
    │   └── ButtonsPanel
    │       ├── HostButton
    │       └── JoinButton
    ├── StatusLabel
    ├── PlayersPanel
    │   ├── PlayersTitle
    │   └── ConnectedPlayersLabel
    ├── RoomsLabel
    ├── RoomsScrollView
    │   ├── Viewport
    │   │   └── RoomListContainer
    │   └── Scrollbar Vertical
    └── BottomButtonsPanel
        ├── StartGameButton
        └── BackButton

+ NetworkManager (con NetworkLobbyManager)
+ GameSyncManager (con GameSyncManager)
+ LobbyUIManager (con LobbyUIController)
```

- [ ] **P17.1** - Verificar estructura completa
- [ ] **P17.2** - Contar elementos (debe haber ~25 UI elements)

---

## FASE 18: CREAR ESCENA DE JUEGO

### Crear escena GameScene

- [ ] **P18.1** - File > New Scene
- [ ] **P18.2** - File > Save Scene As
- [ ] **P18.3** - Nombre: **GameScene**
- [ ] **P18.4** - Ubicación: `Assets/Scenes/Game/`

### Contenido mínimo

- [ ] **P18.5** - Crear UI > Text - TextMeshPro
- [ ] **P18.6** - Texto: **"Escena de Juego - En desarrollo"**
- [ ] **P18.7** - Guardar escena (Ctrl+S)

---

## FASE 19: CONFIGURAR BUILD SETTINGS

- [ ] **P19.1** - File > Build Settings (Ctrl+Shift+B)
- [ ] **P19.2** - Arrastrar `Assets/Scenes/Lobby/LobbyScene.unity` al index 0
- [ ] **P19.3** - Arrastrar `Assets/Scenes/Game/GameScene.unity` al index 1
- [ ] **P19.4** - Verificar que aparecen 2 escenas en la lista
- [ ] **P19.5** - Cerrar Build Settings

---

## FASE 20: GUARDAR Y PRUEBAS INICIALES

### Guardar proyecto

- [ ] **P20.1** - Ctrl+S para guardar escena actual
- [ ] **P20.2** - File > Save Project

### Pruebas básicas

- [ ] **P20.3** - Ir a Scenes/Lobby/LobbyScene.unity
- [ ] **P20.4** - Presionar Play (botón arriba centro)
- [ ] **P20.5** - Verificar que UI aparece correctamente
- [ ] **P20.6** - No debe haber errores rojos en Console
- [ ] **P20.7** - Intentar escribir en campos input
- [ ] **P20.8** - Presionar botones (deben cambiar color)
- [ ] **P20.9** - Presionar Play nuevamente para salir

### Verificar Console

- [ ] **P20.10** - No debe haber errores (rojo)
- [ ] **P20.11** - Puede haber warnings (amarillo) - normal
- [ ] **P20.12** - Si hay errores rojos, revisar Scripts folder

---

## FASE 21: EVENTOS DE BOTONES (OPCIONAL PERO RECOMENDADO)

### ¿Qué es "On Click()"?

Es un **evento del botón** que se ejecuta cuando presionas el botón en Play mode. Vincula el botón a una función del código.

### Paso a paso para CUALQUIER BOTÓN:

**Ejemplo: HostButton**

**Paso 1: Seleccionar el botón**
- [ ] **P21.1** - En Hierarchy (lado izquierdo), selecciona **HostButton**
- [ ] **P21.2** - Mira el Inspector (lado derecho), deberías ver "Button" component

**Paso 2: Encontrar "On Click()"**
- [ ] **P21.3** - En Inspector, busca el componente llamado **Button**
- [ ] **P21.4** - Desplázate hacia abajo en ese componente
- [ ] **P21.5** - Verás un apartado llamado **"On Click ()"** con una lista vacía

**Paso 3: Añadir evento**
- [ ] **P21.6** - En "On Click ()", verás un botón **"+"** (pequeño, en la esquina derecha)
- [ ] **P21.7** - Presiona ese botón **"+"**
- [ ] **P21.8** - Se añadirá una nueva línea en la lista

**Paso 4: Asignar el objeto (LobbyUIManager)**
- [ ] **P21.9** - En la nueva línea, verás un campo vacío (dice "None (Object)")
- [ ] **P21.10** - Arrastra **LobbyUIManager** desde Hierarchy hacia ese campo vacío
- [ ] **P21.11** - Soltalo ahí

**Paso 5: Seleccionar la función**
- [ ] **P21.12** - A la derecha del campo que acabas de llenar, verás otro dropdown (dice "No Function")
- [ ] **P21.13** - Haz clic en ese dropdown
- [ ] **P21.14** - Busca: **LobbyUIController > OnHostButtonPressed()**
- [ ] **P21.15** - Haz clic en él

**Resultado:** Cuando presiones HostButton en Play mode, ejecutará OnHostButtonPressed()

---

### Repetir para cada botón:

### Join Button - Los MISMOS pasos:
- [ ] **P21.16** - Seleccionar **JoinButton** en Hierarchy
- [ ] **P21.17** - Inspector > Button > On Click() > **"+"**
- [ ] **P21.18** - Arrastra **LobbyUIManager** al campo vacío
- [ ] **P21.19** - Dropdown derecho > **LobbyUIController > OnJoinButtonPressed()**

### Start Game Button - Los MISMOS pasos:
- [ ] **P21.20** - Seleccionar **StartGameButton** en Hierarchy
- [ ] **P21.21** - Inspector > Button > On Click() > **"+"**
- [ ] **P21.22** - Arrastra **LobbyUIManager** al campo vacío
- [ ] **P21.23** - Dropdown derecho > **LobbyUIController > OnStartGameButtonPressed()**

### Back Button - Los MISMOS pasos:
- [ ] **P21.24** - Seleccionar **BackButton** en Hierarchy
- [ ] **P21.25** - Inspector > Button > On Click() > **"+"**
- [ ] **P21.26** - Arrastra **LobbyUIManager** al campo vacío
- [ ] **P21.27** - Dropdown derecho > **LobbyUIController > OnBackButtonPressed()**

---

### Visual del proceso:

```
Inspector (lado derecho)
├── Transform
├── Rect Transform
├── ...
└── ► Button                      ← Componente Button
    ├── Interactable: ☑
    ├── Transition: Color Tint
    ├── Navigation: Automatic
    └── ► On Click ()             ← Sección que buscas
        ├── Size: 1
        └── Element 0
            ├── [LobbyUIManager]                    ← Arrastra aquí
            └── LobbyUIController.OnHostButtonPressed() ← Selecciona aquí
```

### Funciones disponibles en LobbyUIController:

```
LobbyUIController > 
  ├── OnHostButtonPressed()
  ├── OnJoinButtonPressed()
  ├── OnStartGameButtonPressed()
  └── OnBackButtonPressed()
```

### Si no ves "On Click()":

- [ ] **P21.24** - Scroll down en el Inspector (sube con la rueda)
- [ ] **P21.25** - Asegúrate que el componente **Button** está expandido (triángulo ▼)
- [ ] **P21.26** - Verifica que seleccionaste el botón correcto (HostButton, JoinButton, etc.)

---

## VERIFICACIÓN FINAL

**Antes de considerar completo, verificar:**

- [ ] ✅ Escena LobbyScene.unity existe en Assets/Scenes/Lobby/
- [ ] ✅ Canvas con todos los UI elements creados
- [ ] ✅ NetworkManager con componente NetworkLobbyManager
- [ ] ✅ GameSyncManager con componente GameSyncManager
- [ ] ✅ LobbyUIManager con componente LobbyUIController y referencias asignadas
- [ ] ✅ RoomListButtonPrefab guardado en Assets/Prefabs/UI/
- [ ] ✅ GameScene.unity existe en Assets/Scenes/Game/
- [ ] ✅ Build Settings con ambas escenas en orden correcto
- [ ] ✅ Play mode funciona sin errores
- [ ] ✅ UI visible y botones responden
- [ ] ✅ Console limpia (sin errores rojos)

---

## PROXIMOS PASOS DESPUÉS DE COMPLETAR

1. ✅ Ejecutar proyecto en Play mode
2. ✅ Hacer clic en "CREAR SALA"
3. ✅ Verificar logs en Console
4. ✅ Hacer clic en "VOLVER" para volver al menú
5. ✅ Documentar cualquier error que aparezca

---

## NOTAS IMPORTANTES

- **TextMeshPro:** Si pide descargar essentials, aceptar (primera vez)
- **EventSystem:** Si botones no funcionan, crear: Hierarchy > Create > Event System
- **Prefabs:** Importante guardar prefab antes de usar en RoomListContainer
- **Referencias:** Si algún campo queda vacío, la escena no funcionará correctamente
- **Play Mode:** Cambios en Play Mode se pierden al salir

---

## AYUDA RÁPIDA

### ¿Dónde está "Add Component"?

1. **Selecciona el objeto en Hierarchy**
2. **Mira el Inspector (derecha)**
3. **Al final del Inspector, verás un botón "Add Component"**
4. **Haz clic en él**
5. **En el buscador, escribe el nombre del componente:**
   - `Vertical Layout Group`
   - `Horizontal Layout Group`
6. **Presiona Enter o haz clic en el resultado**

**Ejemplo visual:**
```
Hierarchy              Inspector (derecha)
├── Canvas             ┌─ Transform
├── Panel              ├─ Canvas Scaler
└── MyPanel    ────→   └─ [ADD COMPONENT] ← Botón azul
```

### Problemas comunes con Layout Groups

| Problema | Solución |
|----------|----------|
| No se generó Content en Scroll View | Crear manualmente: Dentro de Viewport > Click derecho > UI > Panel - Image, renombrar a RoomListContainer |
| Scroll View no scrollea | Asignar RoomListContainer en Scroll Rect > Content |
| No veo la opción en menú | Usa "Add Component" en Inspector |
| Referencias vacías | Arrastra de Hierarchy, no de Prefabs |
| Botones no funcionan | Crea Event System (Hierarchy > Create > Event System) |
| UI se ve pixelada | Aumenta Font Size en Text Mesh Pro |
| Layout desalineado | Activa Preferred Width/Height en Layout Element |
| No se ve el texto | Cambia color a blanco o ajusta opacidad |
| Canvas no se ve | Verifica Render Mode: Screen Space - Overlay |

---

**Estado:** 🔴 NO COMPLETADO  
**Progreso:** 0/100%  
**Última actualización:** 2026-05-05  

**Notas del usuario:**
[Aquí se pueden añadir observaciones]

---

