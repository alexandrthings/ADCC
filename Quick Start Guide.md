# Quick Start Guide
1. Place your character, create two empty parents with one of them being at the center of mass.
2. Add a rigidbody with interpolate and a capsule collider. The capsule collider should be a little bit off the ground.
3. Add a thinner capsule collider which does touch the ground.
4. Place the Character script and a desired InputModule.
5. Add desired movement types (Walk and Airborne are required), set their parameters as needed
6. Create a copy of "defaultcont" to act as the character animation controller
7. In Layer 1, Add desired animation clips, and make a transition from "any state" to them.
8. You can also link them to each other to make animation chains (but not in a loop)
9. Set the animation layers that you want to be auto-configured in ProceduralAnimator
10. Click "Find Actions" on the character
11. Change action properties as desired
12. In your InputModule, add QueueAnimation(string name); to activate your actions.
