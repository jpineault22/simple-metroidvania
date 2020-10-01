# simple-metroidvania
Repository for the Unity project of a currently unnamed 2D Metroidvania game.
Unity version: 2019.4.8f1

### Controls
|  | Controller | Keyboard |
| - | :---: | :---: |
| **Move** | Joystick/Dpad | Arrows/WASD |
| **Jump, \*Wall jump** | Button South | J |
| **Attack** | Button West | K |
| **Interact (talk, save)** | Button North | E |
| **\*Dash** | Right Trigger | L |
| **\*Throw bomb** | Button East | Space |

\*Collect powerup item to acquire ability

*Note: Controller not fully supported (especially in menus/UI)*

### Known bugs
- Not always loading the correct Player HP
- When standing on a unit-wide ground, the player is too large and both ground checks do not detect the ground, making it impossible to jump. (When I start creating real maps, the ground will likely always be at least as wide as the player, so this shouldn't matter. If I decide otherwise, I will have to fix this.)

### Next things to implement
- Keybindings + full controller support (UI)
- Sound integration with Wwise
- Various gameplay objects/elements
- Implement some more Cinemachine functionalities
- Real (non-test) maps
