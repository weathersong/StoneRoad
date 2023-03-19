# StoneRoad
This mod slows down the progression to copper age, plus adds a few interesting new ways to perform routine tasks. It currently depends, implicitly, on the mod Ancient Tools, for the adze and mortar+pestle.

Stone Road is sort of a stand-in until In Dappled Groves is fully released.

Roughly, here are the changes and the progression path to copper age:

***

* **Wood is no longer chopped in the grid to produce firewood.**
Instead, make a chopping block, using an axe and knife and log. Set it down, then "load" it by holding a log in hands and right-clicking. Now switch to either an axe, or an adze, and *right-click* to either chop or hew the log. This produces firewood, bark, and *lumber*.
You may also further chop firewood into sticks, as a convenience. Note that there is no longer a recipe to saw logs into planks, so this is the only path toward producing them.

* **Firewood and lumber must be aged.**
Create a drying rack using a few pieces of raw lumber made from the chopping block. Load it up with either firewood or raw lumber or both. It then takes some time to dry (cure), which is configurable.

* **Lumber can convert into boards.**
The drying rack will usually produce warped lumber (the conversion chance is configurable), so a straightening rack must be built, loaded with warped lumber, fueled with (either) firewood, and fired for about a day (configurable), for a chance (configurable)
to convert the warped lumber into regular, vanilla boards. This completely circumvents the need for a saw, and means that boards are possible, though time- and resource-intensive, in the stone age. This unlocks the ability to craft doors and, importantly, barrels.

* **Crucible creation process extended.**
No longer just clay-formed and fired, you must first shape the raw clay form (using *only* fire clay), then treat it in a limewater solution, in a barrel.
The treated clay form is then dried, and must then be loaded with *superior* charcoal before it can finally be fired.
(This series of steps is meant to represent the sum total of the mod's changes, hence the barrel and the superior charcoal. But also locating the uncommon elements of fire clay and limestone in the world.)
However...

* **Firewood and charcoal don't burn as hot.**
The most important consequence is that smelting won't be possible with vanilla charcoal. There are now 4 different qualities of charcoal: Poor, regular (the vanilla charcoal), Fine, and Superior.
Only Superior charcoal can reach 1100C, necessary to melt copper and its alloys.
Charcoal pits work roughly the same, and making a vanilla-style pit still works, but only produces poor charcoal. (Poor charcoal can also be produced right at a firepit using raw lumber. This is a handy substitute for firewood itself, in cooking.)
To produce higher quality charcoal, a bed of charcoal must be laid down, and *aged* firewood placed atop it.
The rule here is that one higher tier of charcoal is produced than the bed, so a block of poor charcoal under a block of aged firewood converts the firewood into regular quality charcoal.
Dig out that charcoal and the poor charcoal (unconverted) beneath it, and lay down the regular charcoal as the bed. Place aged firewood atop it, and cook again. This time, fine charcoal is the result. And then once more for superior.

***

This mod isn't generally intended for public use, but is public here to act as a useful example of several types of systems. It has only had light testing in-game, and only on version 1.17.11.

Some of the code is also adapted from Primitive Survival (or the game's own files), but has been pretty extensively modified to suit the mod's own function.
