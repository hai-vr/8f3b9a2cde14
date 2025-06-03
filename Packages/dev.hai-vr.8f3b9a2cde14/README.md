## Naming conventions

- P12 are MonoBehaviours and ScriptableObjects, including serializable structs and enums referenced by MonoBehaviours and ScriptableObjects.
- H12 are non-MonoBehaviours.
- Custom attributes belong in the Supporting assembly, and shouldn't have prefixes.

## Basis modifications for our app

Since this is a consumer of the Basis framework, we want to modify Basis as little as possible so that we may update the Basis
framework itself.

### Non-enacted

Unsolved issues that may require a modification, separation, or hijack.

The following items will need to be enacted:
- We will need to add some form of support for the right-click action on desktop for our telekinesis action (grab at a distance).
- We will need to change the action of the Escape button to open our own Pause menu.
  - It is currently unclear how to change basic behaviour of Basis with minimal modification of the framework; if that is even a framework goal to achieve.
- We will need to figure out a way to open the Pause menu in desktop mode on first join, so that the cursor always works there.

### Separated code

Code that was copied from Basis in order to derive functionality. May have to be pushed to upstream if relevant.

- There is no built-in Basis method to load a scene from a non-bundle. I am adding a method to load a level from a scene path,
  but that scene path is not yet processed (e.g. audio mixer, and possible probe tetrahedralization?)

### Hijacked code

Code that modifies the behaviour of stock Basis components beyond their intended purpose, which may be considered a hack.
May have to be reimplemented as native upstream (if relevant).

- We are currently hijacking the BasisPointRaycaster to ignore the world geometry if we're interacting with a non-world menu that intersects with world geometry.
    - In-world UIs may still affect the operation of the main menu, this may need some consultation on layer management.

## Adaptations

The following things don't modify Basis, but may still affect the behaviour of the app.

Things to keep in mind while building scenes:

- All Rigidbodies need to have interpolation set to Interpolate. This is because the physics update rate is lower than the HMD update rate,
  which makes it different from what some of us may be used to in apps where the physics update rate matched the HMD update rate.
  - Having a known physics update rate makes it better for consistent rigidbody physics damage, so we value this consistency over that.
- Unlike the Basis default UI, all of our UI uses a custom shader that derives from UI/Default, and a custom TextMeshPro-derived shader, which
  crushes Z so that our UI shows on top of *most* of the world geometry. This is to allow opening the UI in cramped spaces.
