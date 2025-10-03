# StoneRoad
This mod slows down the progression to copper age, plus adds a few interesting new ways to perform routine tasks.

Roughly, here are the changes and the progression path to copper age:

***

* **Wood is no longer chopped in the grid to produce firewood.**
Instead, make a chopping block, using an axe and knife and log. Set it down, then load it by holding a log in hands and right-clicking. With an axe, *right-click* to chop the log, or hold sneak and right-click to debark the log.
Depending on which is used, this produces firewood, bark, and *lumber*. You may also further chop firewood into sticks, as a convenience.
There is no longer a recipe to saw logs into planks, so this is the only path toward producing them.

* **Firewood and lumber must be aged.**
Create a drying rack using a few pieces of raw lumber made from the chopping block. Load it up with either firewood or raw lumber or both. It then takes some time to dry (cure), which is configurable. Note that if you find aged firewood in ruins,
this does work the same.

* **Lumber can convert into boards.**
The drying rack will usually produce warped lumber (the conversion chance is configurable), so a straightening rack must be built, loaded with warped lumber, fueled with (either) firewood, and fired for about a day (configurable), for a chance (configurable)
to convert the warped lumber into regular, vanilla boards. This completely circumvents the need for a saw, and means that boards are possible, though time- and resource-intensive, in the stone age. This unlocks the ability to craft doors and, importantly, barrels.

* **Crucible creation process extended.**
No longer just clay-formed and fired, you must first shape the raw clay form (using *only* fire clay), then treat it in a limewater solution, in a barrel.
The treated clay form is then dried, and must then be loaded with *superior* charcoal before it can finally be fired.
(This series of steps is meant to represent the sum total of the mod's changes, hence the barrel and the superior charcoal. But also locating the uncommon elements of fire clay and limestone in the world.)
And speaking of charcoal...

* **Firewood and charcoal don't burn as hot.**
The most important change in the mod is that smelting isn't possible with vanilla charcoal. There are now 4 different qualities of charcoal: Poor, regular (the vanilla charcoal), fine, and superior.
Only superior charcoal can reach 1100C, necessary to melt copper and its alloys.
Charcoal pits work roughly the same, and making a vanilla-style pit still works, but only produces poor charcoal. (Poor charcoal can also be produced right at a firepit using raw lumber. This is a handy substitute for firewood itself, in cooking.)
To produce higher quality charcoal, a bed of charcoal must be laid down, and *aged* firewood placed atop it.
The rule here is that one higher tier of charcoal is produced than the bed, so a block of poor charcoal under a block of aged firewood converts the firewood into regular quality charcoal.
Dig out that charcoal pile and the poor charcoal (unconverted) beneath it, and lay down the regular charcoal as the bed. Place aged firewood atop it, and cook again. This time, fine charcoal is the result. And then once more for superior.
The cook time for charcoal now defaults to 60 hours, though this can be configured. Keep this duration (*times the number of upgrade steps, plus aging more firewood each time*) in mind when planning your initial settlement!

***

## Additional Features
These are some general quality-of-life features to make living without copper somewhat more bearable.

* **Fruit can be juiced by hand.** This feature is experimental. It's a patch to all fruit (and, even more experimentally, all Wildcraft fruit) allowing them to work like honeycombs:
Place a container (a bowl, or a bucket if you've made it that far) on the ground, hold sneak, and hold left mouse. Not much juice is produced so that the fruit press remains desirable later. No mash is produced.
This doesn't work with all fruits mainly because the logic is very simple and can't account for special cases like (pine)apples. But it should enable stone age syrup and even wine!

* **Wood bowls.** First carve a pan (the one used for panning) then carve it again, to yield a bowl. This is an alternative to the typical clay bowl and is functionally identical. It serves a few important purposes:
First, it's immediately and always available, so if you have no nearby clay source - a real problem on icy starts - you'll have at least some limited means of gathering liquids.
This is crucial with Ancient Tools where pine bark can be made into dough and bread, as an emergency ration in areas where there is no food whatsoever, only pine trees and freshwater.
This also works in conjunction with fruit juicing, which is again useful on icy starts since certain (Wildcraft) native berries are poisonous (unless juiced).
And finally, it's something you don't necessarily need to carry with you, and could be crafted / disposed on the go during long overland (or overwater) expeditions; you only need to pack a pot.

* **Tannin from oak bark.** Interesting to note, 1.18 added oak bark, but it's currently unobtainable. So I've added it as a product of debarking oak, and a recipe to use that bark (instead of the whole log) to make tannin.

***

## Final Notes
This mod isn't generally designed for public use, but is public here to act as a useful example of several types of systems. It has had a fair amount of testing in-game, but may still be buggy.
Stone Road is currently (only) compatible and tested with game version 1.19. (Previous versions here on Git depended on Ancient Tools.)

Some of the code is also adapted from Primitive Survival (or the game's own files), but has been pretty extensively modified to suit the mod's own function.
