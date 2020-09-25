# simple-metroidvania
Repository for a currently unnamed 2D metroidvania game.

#### Controls
*                           **Controller**      **Keyboard**
**Move**                     Joystick/DPad      Arrows/WASD
**Jump/\*Wall jump**         Button South            J
**Attack**                   Button West             K
**Interact (talk, save)**    Button North            E
**\*Dash**                   Right trigger           L     
**\*Throw bomb**             Button East           Space

\*Collect powerup to acquire

*Note: Controller not fully supported (especially in menus/UI)

#### Known bugs
- The camera doesn't snap to the player when the first map is loaded after the menu (more apparent when loading from save file and spawning on saving spot).
- When dashing through a map exit, the player gets stuck in a loop quickly transitioning back and forth between the two maps.

#### Next things to implement
- Enemies and bosses (+cutscenes)
- Player health system
- Sound integration with Wwise
- Keybindings + full controller support (UI)
