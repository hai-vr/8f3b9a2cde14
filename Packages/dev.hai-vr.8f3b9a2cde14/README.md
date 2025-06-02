## Naming conventions

- P12 are MonoBehaviours and ScriptableObjects, including serializable structs and enums referenced by MonoBehaviours and ScriptableObjects.
- H12 are non-MonoBehaviours.

## Basis modifications for our app

Since this is a consumer of the Basis framework, we want to modify Basis as little as possible so that we may update the Basis
framework itself.

### Non-enacted

The following items will need to be enacted:
- We will need to add some form of support for the right-click action on desktop for our telekinesis action (grab at a distance).
- We will need to change the action of the Escape button to open our own Pause menu.
- We will need to figure out a way to open the Pause menu in desktop mode on first join, so that the cursor always works there.
- We will need to change the physics timestep of the project to 60 physics updates per second.
  - (All Rigidbodies will need to be set to Interpolate, see the README.md inside RigidbodyAdditions/)

### Separated code

- There is no built-in Basis method to load a scene from a non-bundle. I am adding a method to load a level from a scene path,
  but that scene path is not yet processed (e.g. audio mixer, and possible probe tetrahydralization?)
