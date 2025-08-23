# P12 RigidbodyAdditions

This is a small excerpt from the code of my application-agnostic home world, to add collision sounds to rigidbodies.
The original code has other effects such as damage caused by physics collisions.

This is a note for future development.

## Physics-based animation

### PID Tuning

- Total mass human-like (51.43 for Wolfram who is 1.56m high)

#### Standard

- Proportional 100
- Integral 0
- Derivative 10
- Rotation PID has a proportional of x10, x1 for the rest
- Slerp dampening 100
- Slerp spring 0
- Mass of 200 for pinning

Effect:
- Matches animation closely
- Causes lots of stretching

#### Stretch-free

- Proportional 100
- Integral 0
- Derivative 20
- Rotation PID has a proportional of x10, x1 for the rest
- Slerp dampening 100
- Slerp spring 0
- Mass of 200 for pinning

Effect:
- Causes minimal amount of stretching
- Is not responsive enough

### Keyframe drivers

TODO:
- TODO: When a rigidbody is keyframed, then its surroundings joints should probably have a slerpDrive positionDamper of 100, and the mass should be high.
- TODO: When a rigidbody is powered, then its surroundings joints should probably have a slerpDrive positionDamper of 100, and the mass should be regular.
- TODO: When a rigidbody is unpowered, then its surroundings joints should probably have a slerpDrive positionDamper of 0, and the mass should be regular.
- TODO: For keyframed or powered drivers, disable gravity on the Rigidbody

| Rigidbody type | Dampering of surrounding joints | Rigidbody Mass | Gravity  |
|----------------|---------------------------------|----------------|----------|
| Keyframed      | 100                             | High           | Disabled |
| Powered        | 100                             | Regular        | Disabled |
| Unpowered      | 0                               | Regular        | Enabled  |

### Flailing

TODO:
- Explore joint motor control, as opposed to world space velocity change. This might only be correct for "powered but flailing" limbs.
- Explore applying physics to the lower body for climbing.

### IK correction

TODO:
- Add an IK pass to correct for the physics pose, which will then be applied to the visual representation of the avatar.

### Other

TODO:
- Generated inertia tensors are incorrect. Find and set it to a uniform value for our ragdolls.
- Rig and physics character control needs to be its own class.
- Work on adding ragdoll profiles (and lerping between ragdoll profiles) for avatar, NPC, and character control.
- Include work on limb stiffness.

## Inconsistent damage due if `fixedDeltaTime` varies

During the development of the original code, I had noticed that the reported damage from collision depends on
`fixedDeltaTime`. I was not immediately able to find the reason for this time dependency, but it looks inconsistent.

There is a signification variation in damage dealt, which can go from single to triple, so it cannot be ignored.

In Basis framework projects, my understanding is that `fixedDeltaTime` is a project-wide constant, regardless of Desktop or VR,
and independent of the HMD refresh rate (which is a behaviour that differs from other apps).

This is a good thing, that constant project-wide, means at least the damage done should remain consistent.

The constant is not currently chosen intentionally. It is currently set at 50 per second (0.02s timestep).

Other apps set it to be the same as the HMD refresh rate, probably due to an old article posted by Valve on the Steam forums.

I don't know if that post is still relevant today. It does mean that at least in Basis projects, the interpolation method of the
Rigidbody component should be enforced to be not set to None, as None will result in stutter movement whenever the HMD renders
faster than the physics. Setting the physics delta time to match the HMD refresh rate effectively *hid* this issue.

I think that it's a mistake because it causes inconsistent damage for gameplay purposes across HMDs.

Maybe someone else can chime in on this.
