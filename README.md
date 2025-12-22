# 🎮 Proyecto shooter 3D

![Unity 6.3](https://img.shields.io/badge/Engine-Unity%206.3-black?logo=unity) 
![Status](https://img.shields.io/badge/Status-In%20Progress-yellow) 
![Platform PC](https://img.shields.io/badge/Platform-PC-blue?logo=windows) 
![Itch.io](https://img.shields.io/badge/Platform-Itch.io-purple)
![License](https://img.shields.io/badge/License-MIT-green)
![CLA Required](https://img.shields.io/badge/CLA-Required-blue.svg)

> ⚠️ **Proyecto en construcción:** Actualmente se está desarrollando la base del juego, incluyendo personajes, enemigos, mecánicas de movimiento, combate y cambio de cámara/armas.

---

## 📌 Descripción general

Este proyecto es un **shooter 3D** en Unity 6.3. El jugador podrá cambiar entre **primera y tercera persona con zoom**, moverse libremente y atacar enemigos usando **2 o 3 armas distintas**.  
El juego contará con **mínimo 3 niveles**, enemigos animados con Mixamo, sistemas de stamina, power-ups y efectos de audio/partículas avanzados.  

Se está desarrollando con un enfoque en **arquitectura limpia** utilizando patrones de diseño (State, Strategy, Factory, Observer, Command, Singleton, Object Pool) para facilitar escalabilidad y mantenimiento.

---

## 🎮 Mecánicas de juego

1. **Movimiento del jugador**
   - Primera y tercera persona
   - Caminar, correr y sprintar
   - Stamina y regeneración
   - Interacción con pickups (power-ups)

2. **Sistema de armas**
   - Cambio entre 2–3 armas
   - Las armas pueden ser proyectiles tipo bolas, botellas o piedras
   - Cada arma con comportamiento propio usando Strategy Pattern

3. **Enemigos**
   - Animaciones y modelos de Mixamo
   - IA basada en State Machine (Patrullar, Perseguir, Atacar)
   - Respawn y Object Pooling para optimización

4. **Power-Ups**
   - Pickup animados con DOTween (bouncing & rotating)
   - Mejora temporal de velocidad, daño o defensa

5. **Sistema de cámara**
   - Cambio dinámico de cámara principal
   - Zoom in / out
   - Transiciones suaves entre cámaras usando Singleton Controller

6. **Audio 3D**
   - Sonido espacial para armas, enemigos y ambiente
   - Footsteps diferenciados según superficie
   - Zonas de reverb y fade in/out de audio por sala

7. **Partículas**
   - Disparo, impactos y pickups
   - Sistema escalable para futuras armas y enemigos

8. **Input**
   - Compatible con teclado/ratón y GamePad
   - Feedback de vibración mínima en gamepad

---

## 🗂 Arquitectura / Patrones de diseño

| Sistema | Patrón Aplicado | Detalles |
|--------|----------------|---------|
| Movimiento jugador | State | Idle, Walk, Run, Jump |
| IA enemigos | State | Patrullar → Perseguir → Atacar |
| Gestión armas | Strategy | Cada arma implementa interface `IWeapon` |
| Spawn enemigos | Factory / Object Pool | Reutilización de enemigos para optimización |
| Pickup / PowerUps | Command | Activación de efecto al recoger |
| Audio y Partículas | Observer | Event-driven para disparos, pasos y pickups |
| Cámara | Singleton | Control centralizado de cámaras y transición |
| GameManager | Singleton | Control de estado global del juego |

---

## 🛠 Estructura del proyecto

```plaintext
Assets/
+---Art
|   +---Animations
|   +---Images
|   \---Materials
+---Plugins
|   +---AssetUsageDetector
|   |   \---Editor
|   +---Demigiant
|   |   \---DOTween
|   |       +---Editor
|   |       |   \---Imgs
|   |       \---Modules
|   +---Resources
|   \---TextMesh Pro
|       +---Examples & Extras
|       |   +---Fonts
|       |   +---Materials
|       |   +---Prefabs
|       |   +---Resources
|       |   |   +---Color Gradient Presets
|       |   |   +---Fonts & Materials
|       |   |   \---Sprite Assets
|       |   +---Scenes
|       |   |   \---28 - HDRP Shader Example
|       |   +---Scripts
|       |   +---Sprites
|       |   \---Textures
|       +---Fonts
|       +---Resources
|       |   +---Fonts & Materials
|       |   +---Sprite Assets
|       |   \---Style Sheets
|       +---Shaders
|       \---Sprites
+---Prefabs
|   +---Items
|   +---Map
|   +---UI
|   \---Weapons
+---Scripts
|   +---Core
|   |   +---Player
|   |   |   +---Camera
|   |   |   \---States
|   |   +---PowerUps
|   |   \---Weapons
|   |       +---Components
|   |       +---Data
|   |       \---Systems
|   +---Data
|   |   +---PowerUps
|   |   \---Weapons
|   +---Interfaces
|   \---UI
\---Settings
    \---HDRPDefaultResources
```
---

## ⚙ Requisitos / Herramientas

- Unity **6.3** (HDRP o URP)
- DOTween para animaciones de pickups
- Input System Both (provisional)
- GamePad compatible
- Modelos y animaciones Mixamo
- Partículas y efectos visuales 3D

---

## 📝 Features en construcción

- Cambio de cámara FPS / TPS con zoom
- Cambio de armas dinámico (2–3 armas)
- IA de enemigos por estados
- Pickups animados y rotativos
- Audio 3D con footsteps y reverb
- Partículas de disparo y pickups
- Stamina del jugador
- Compatible teclado/ratón y GamePad con vibración

---

## 🔮 Roadmap

- [x] Implementar **jugador con movimiento y stamina**
- [x] Implementar **cámaras y zoom**
- [x] Crear **2–3 armas funcionales** con cambio dinámico
- [x] Crear **inventario de armas**
- [ ] Añadir **enemigos con IA básica**
- [x] Añadir **pickups y power-ups**
- [ ] Añadir **modelos y animaciones finales**
- [ ] Añadir **audio y partículas 3D**
- [ ] Primer **nivel jugable**
- [ ] Extender a **mínimo 3 niveles**
- [ ] Pulir optimización y efectos visuales
