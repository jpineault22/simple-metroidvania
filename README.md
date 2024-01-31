# simple-metroidvania
Repository for the Unity project of a 2D Metroidvania game template.
Unity version: 2022.2.8

*Note: Menu/UI layouts currently built only for full screen*

### Controls
|  | Controller | Keyboard |
| - | :---: | :---: |
| **Move** | Left Stick/D-Pad | Arrows/WASD |
| **Jump, \*Wall jump** | Button South | J |
| **Attack** | Button West | K |
| **Interact (talk, save)** | Button North | E |
| **\*Dash** | Right Trigger | L |
| **\*Throw bomb** | Button East | Space |

\*Collect powerup item to acquire ability

### Interested in the code behind?
[This is where the code begins when you launch the game.](/SimpleMetroidvania/Assets/Scripts/GameManagement/GameManager.cs)

[And this has most of the logic behind the player character.](/SimpleMetroidvania/Assets/Scripts/Player/PlayerController.cs)

### Known bugs
- When standing on a unit-wide ground, the player is too large and both ground checks do not detect the ground, making it impossible to jump. (When I start creating real maps, the ground will likely always be at least as wide as the player, so this shouldn't matter. If I decide otherwise, I will have to fix this.)

### Next things to implement
- Various gameplay objects/elements
- Implement some more Cinemachine functionalities
- Real (non-test) maps
