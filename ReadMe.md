# BreakableWallRandomiser

A Hollow Knight Randomizer add-on for breakable walls.

## Overview

This is a fork of the original mod by **Bentechy66**, which randomizes several objects in the game, adding

Randomizes some walls in the game hollow knight. Currently randomizes, with a few exceptions:
 - Any wall with FSM breakable_wall_v2 or "FSM". These are labeled as "Wall", and are mostly placed in the map hiding scenes or items.
 - Any wall with FSM break_floor. These are labaled as "Plank", and are mostly placed in the map as shortcut openers. They always have a wooden plank in the map.
 - Any floor broken by Dive / Descending Dark in the game. These are labeled as "Dive Floor".

Hitting a wall won't result in it breaking; you'll get an item instead. In order to break the wall, you'll need to find its corresponding item. If you find a wall's item but haven't collected its check yet, it will become translucent and you will be able to walk through it, but still hit it.

There are, however, a few exceptions to the rule:

### King's Pass
King's Pass displays a particular behaviour. Since having only the plank randomized affects logic for other objects that may not be obtainable, since for some reason logic for the right transition is always defined as available, and it currently just not being interesting enough, the King's Pass check will include both the plank and the collapsing floor, resulting in making items below the collapsing floor inaccessible without some movement or the wall item itself, while not affecting the access to Dirtmouth and thus avoiding logic problems.

### Godhome Walls
Currently unavailable, but may add the breakable wall before Zote's ordeal in the future.

### Hunter's Mark planks
These are left out by design. Might add as an optional setting at some point.

## Settings

- Enabled --> Boolean to define if the connection should be active or not.
- Wooden Planks --> Boolean to define if one-way walls should be randomized or not.
- Rock Walls --> Boolean to define if standard walls should be randomized or not.
- Dive Floors --> Boolean to define if Dive breakable floors should be randomized or not.
- King's Pass --> Boolean to define if King's Pass should be included or not.

### Myla Shop
- Enabled --> Boolean to define if the Godhome Shop should appear or not.
- Minimum/Maximum Cost --> A range from 0 to 1, where 0 is 0% and 1 is 100% (176) of all available wall objects. Default is 25% to 75%.
- Include In Junk Shop --> Allows the defined costs to appear as currency in More Locations' Junk Shop, regardless of if Myla's Shop is enabled or not.

## Dependencies:
- ItemChanger
- MenuChanger
- Randomizer 4
- RandomizerCore

## Integrations:
- FStats: When enabled, a new page for diverse Wall achievements will be added, and it'll display the achievements regardless of randomization settings for them.
- More Locations: Having the mod enabled will allow for Statue Marks to be included as currency for the Junk Shop.
- RandoSettingsManager

## Acknowledgements

This is but the tweaking of an already amazing existing mod. So all acknowledgements go to the original author **Bentechy66**, but I'd like to make a special mention to **glowstonetrees** for their Logic Fix injector, which resolved several logic complications the mod originally had and made its reimplementation a lot easier.