# Rebellion of the Path

## Descripción

**Rebellion of the Path** es un juego de acción 2D ambientado en Aethelgard, una ciudad tecno-gótica controlada por una facción conocida como la Falange. El jugador controla a **Kael**, un guerrero equipado con una daga de translocación que le permite teletransportarse a voluntad.

El objetivo es eliminar al **Ejecutor de la Falange**, un enemigo blindado con un escudo frontal de energía que bloquea todo daño directo. La única forma de derrotarlo es usar el Blink para aparecer por detrás y atacar cuando el escudo está inactivo.

---

## Mecánicas principales

- **Movimiento** — Desplazamiento horizontal, doble salto
- **Daga de Translocación** — Lanza la daga y teletranspórtate a su posición
- **Espada** — Ataque cuerpo a cuerpo con sistema de combo
- **Dash** — Movimiento rápido con invulnerabilidad temporal
- **Escudo enemigo** — El Falange bloquea daño frontal, solo es vulnerable por detrás

---

## Controles

| Acción | Tecla |
|---|---|
| Moverse | A / D |
| Saltar (doble salto) | Espacio |
| Lanzar daga / Teletransportarse | Click izquierdo |
| Atacar con espada | Click derecho |
| Cancelar daga | Click derecho (en vuelo) |
| Dash | E |

---

## Jugar en el navegador

El juego está disponible en itch.io:

👉 **[https://tonix7.itch.io/rebellion-of-the-path](https://tonix7.itch.io/rebellion-of-the-path)**

No requiere instalación — se ejecuta directamente en el navegador.

---

## Cómo clonar y abrir el proyecto

### Requisitos
- Unity 6 (versión 6000.0.71f1)
- Universal Render Pipeline (URP)
- Módulo WebGL Build Support

### Pasos

**1. Clonar el repositorio**
```bash
git clone https://github.com/nikinicole04/Rebellion-of-the-Path.git
```

**2. Abrir en Unity**
- Abre Unity Hub
- Haz clic en **Add project from disk**
- Selecciona la carpeta clonada
- Abre el proyecto con Unity 6 (6000.0.71f1)

**3. Abrir la escena**
- En el Project panel ve a `Assets/Assets/Scenes/`
- Abre `SampleScene`

**4. Ejecutar**
- Presiona el botón **Play** en el Editor

---

## Estructura del proyecto

```
Assets/
├── Assets/
│   ├── Animation/       # Clips y Animator Controllers
│   ├── Fondos/          # Tilemaps y fondos del escenario
│   ├── personje/        # Spritesheets de Kael y Falange
│   ├── Prefabs/         # Prefabs de daga y efectos
│   ├── Scenes/          # Escenas del juego
│   └── Scripts/         # Scripts de C#
│       ├── PlayerController.cs
│       ├── DaggerController.cs
│       ├── SwordController.cs
│       ├── DashController.cs
│       ├── FalangeExecutor.cs
│       ├── EnemyHealth.cs
│       ├── GameManager.cs
│       ├── CameraShake.cs
│       └── UIAutoSetup.cs
```

---

## Scripts principales

| Script | Descripción |
|---|---|
| `PlayerController.cs` | Movimiento, salto, salud de Kael |
| `DaggerController.cs` | Lanzamiento y teletransporte de la daga |
| `SwordController.cs` | Sistema de combate con espada y combo |
| `DashController.cs` | Dash con invulnerabilidad temporal |
| `FalangeExecutor.cs` | IA del enemigo con máquina de estados |
| `EnemyHealth.cs` | Sistema de salud de enemigos con callback de escudo |
| `GameManager.cs` | Objetivo, victoria, Game Over, Singleton |
| `CameraShake.cs` | Efecto de vibración de cámara al recibir daño |
| `UIAutoSetup.cs` | Generación automática de UI por código |

---

## Desarrollado por

- **Tony** 
- **Nicole**

Proyecto Final — Desarrollo de Videojuegos
