# P12 RigidbodyAdditions

This is a small excerpt from the code of my application-agnostic home world, to add collision sounds to rigidbodies.
The original code has other effects such as damage caused by physics collisions.

This is a note for future development.

## Inconsistent damage due if `fixedDeltaTime` varies

During the development of the original code, I had noticed that the reported damage from collision depends on
`fixedDeltaTime`. I was not immediately able to find the reason for this time dependency, but it looks inconsistent.

There is a signification variation in damage dealt, which can go from single to triple, so it cannot be ignored.

In Basis framework projects, my understanding is that `fixedDeltaTime` is a project-wide constant, regardless of Desktop or VR,
and independent of the HMD refresh rate (which is a behaviour that differs from other apps).

This is a good thing, that constant project-wide, means at least the damage done should remain consistent.

The constant is not currently chosen intentionally. I think it should be set to 60, but it's half that at the moment.

Other apps set it to be the same as the HMD refresh rate, probably due to an old article posted by Valve on the Steam forums.

I don't know if that post is still relevant today. It does mean that at least in Basis projects, the interpolation method of the
Rigidbody component should be enforced to be not set to None, as None will result in stutter movement whenever the HMD renders
faster than the physics. Setting the physics delta time to match the HMD refresh rate effectively *hid* this issue.

I think that it's a mistake because it causes inconsistent damage for gameplay purposes across HMDs.

Maybe someone else can chime in on this.
