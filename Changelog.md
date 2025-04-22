# Breakable Wall Randomizer 4.0

Changelog:
- Introducing *Collapsers*: These represent *60?* new items that can be included into the randomized pool. Much like Walls, Planks and Dive Floors, they can have their own group set. (18/60) 30%
- Reworked Wall Groups: As of previous patch, Wall Groups were a bit arbitrary. With the introduction of Collapsers, these need some clearer definition. Now, if this setting is enabled, all objects will be replaced by room walls. A room wall will destroy all randomized breakables in a given room, much like how Wall Groups behaved before, but with the consistency that there will be one wall item/location per room. (0%)
- Rebuilt FStats integration. Half / Total remain the same, but instead of using individual groups, walls by Map Area are selected instead. (0%)
- Removed the Softlock setting: It was rarely used, not maintained, didn't keep track of other connection interops and with the introduction of yet another wall type, whichever walls were defined under these settings are potentially more outdated. Plus, the connection menu was already big enough and we still are introducing new options. (100%)
- Restored shop descriptions: They were a thing on BWR 2+, got dropped on BWR 3.0 as an oversight and I see no reason to impede their return - with some extra new descriptions too. (100%)
- Added a new wall: Wall-QG_Glass, which is the glass window that leads to the Queen's Gardens top Grub. This is a standard wall in every aspect, except it breaks in one hit instead of four. (100%)
- Added Extra Walls: There are three groups of these - walls such as the King's Pass breakables, Ancestral Mound planks and Hive Pillars. (0%)