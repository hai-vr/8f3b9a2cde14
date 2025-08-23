Supplemental Kinematics
---

This is a set of inverse kinematics solvers designed specifically to be run **after** a traditional IK solver.

## Pose Merger

The purpose of the Pose Merger is to combine the results of a traditional IK solver with the results
of a physics-based animation.

Physics-based animation may not preserve the proper lengths between limbs as the simulation fails to stabilize.

The pose merger will choose a strategy to rearrange the bones of the physics-based animation, and may also
run additional IK solves to move some critical parts of the avatar so that it is either not or less influenced by
the delay of the physics simulation, for instance the hands.

This may lead to a result where the head, neck, upper chest, and hands are not influenced by the physics simulation,
but the arm bend, lower body, and feet may be to some extent.
