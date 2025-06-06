## Naming conventions

- P12 are MonoBehaviours and ScriptableObjects, including serializable structs and enums referenced by MonoBehaviours and ScriptableObjects.
- H12 are non-MonoBehaviours.
- I12 are interfaces.
- Custom attributes belong in the Supporting assembly, and shouldn't have prefixes.

## Basis modifications for our app

Since this is a consumer of the Basis framework, we want to modify Basis as little as possible so that we may update the Basis
framework itself.

### Non-enacted

Unsolved issues that may require a modification, separation, or hijack.

The following items will need to be enacted:
- We will need to change the action of the Escape button to open our own Pause menu.
  - This is currently prevented because the default Escape behaviour cannot be unhooked from BasisLocalInputActions.
  - It is currently unclear how to change basic behaviour of Basis with minimal modification of the framework; if that is even a framework goal to achieve.
  - After discussion, the default Escape button action may be made public, so that it can be unhooked.
- We will need to figure out a way to open the Pause menu in desktop mode on first join, so that the cursor always works there.

### Separated code

Code that was copied from Basis in order to derive functionality. May have to be pushed to upstream if relevant.

- There is no built-in Basis method to load a scene from a non-bundle. I am adding a method to load a level from a scene path,
  but that scene path is not yet processed (e.g. audio mixer, and possible probe tetrahedralization?)

### Hooked code

Code that listens to standard Basis events, probably for their intended purpose.
- We are currently adding an event listener to the BasisLocalInputActions to detect right-click on desktop for our telekinesis action (grab at a distance).
- We are listening to a few avatar-loading and camera rendering hooks to render the head shadow in first person.

### Hijacked code

Code that modifies the behaviour of stock Basis components beyond their intended purpose, which may be considered a hack.
May have to be reimplemented as native upstream (if relevant).

- We are currently hijacking the BasisPointRaycaster to ignore the world geometry if we're interacting with a non-world menu that intersects with world geometry.
    - In-world UIs may still affect the operation of the main menu, this may need some consultation on layer management.
    - TODO: Also do this for the hand controllers.

## Adaptations

The following things don't modify Basis, but may still affect the behaviour of the app.

Things to keep in mind while building scenes:

- All Rigidbodies need to have interpolation set to Interpolate. This is because the physics update rate is lower than the HMD update rate,
  which makes it different from what some of us may be used to in apps where the physics update rate matched the HMD update rate.
  - Having a known physics update rate makes it better for consistent rigidbody physics damage, so we value this consistency over that.
- Unlike the Basis default UI, all of our UI uses a custom shader that derives from UI/Default, and a custom TextMeshPro-derived shader, which
  crushes Z so that our UI shows on top of *most* of the world geometry. This is to allow opening the UI in cramped spaces.
  - TODO: Fix the VR controller laser shader to also crush Z.

## Bugs in Basis

Issues related to domain reload OFF:
- BootManager will occasionally get added to the main scene while in Edit Mode. Repro steps not yet known.
  - There is a good chance `BootSequence.cs`'s async is bleeding out of Play Mode.
- If the avatar is non-default, entering Play Mode for the second time since last domain reload will fail.
  - Current workaround is to start the default Basis app, and reset the avatar to the default avatar.
- `BasisLocalInputActions.Instance` may instantiate too late for some dependents to hook into it, and there does not appear to be
  an initialization callback.
- In `BasisOpenVRManagement`, StopSDK is not compatible with domain reload OFF between Play Mode sessions. A fix has been applied in P12HotReloadFixes/.
