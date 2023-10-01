# Concepts

## Quests

Quests are just like quests in any other game.
You have a series of steps that must be completed in order,
and then when all steps are complete, the quest is completed, and the player is rewarded.
What separates Outer Wilds RPG's quests from other games is the extremely non-linear nature of Outer Wilds,
which means players may end up experiencing the different parts of a quest in unexpected or unintended orders.


Quests give a certain amount of XP points when completed.
After the player reaches the amount of XP needed to level up, they gain a perk point,
which can then be used to unlock perks.

### Quest Steps

Quests are divided up into individual steps.
Each step represents a single 'task' in the quest, and will show as a checklist item in the game interface.
Steps have a set of 'start' conditions, which is when they will start being shown to the player,
and 'completion' conditions, which is when they will be marked as completed.
When all non-optional steps are completed, the quest as a whole will be marked as complete.

### Quest Conditions

Both 'start' and 'complete' criteria are considered to be quest conditions.
Quest conditions are what describe the actual actions that the player needs to perform in-game
for the quest step to progress. This includes things like discovering a location, learning a ship log fact,
speaking to a character, completing a different quest step, and many more.


Conditions determine where quest markers will be placed while the quest step is in progress.
The location of the quest markers will be inferred based on the type of the condition.
For example, a condition to speak with a character will place the marker on the character's focal point,
or a condition to discover a location will use the same place as marking that entry in Rumor Mode.
These auto-placed quest markers can be disabled, or a user-specified point can be provided.


A step can be configured to require either any condition or all conditions to be met before progressing.
If all conditions must be met, the number of conditions remaining will be displayed in the interface.

## Drops

Drops are items that can be picked up and stored in the player's ship inventory, and also function as
equipment or consumable items. They are called "drops" by this mod to distinguish them
from the objects that you can pick up in the vanilla game, such as the Nomai scrolls or projection stones.


Consumable drops have an instantaneous effect on the player when used from the ship inventory menu, and can
also provide passive buffs for a certain duration of time afterwards.


All of the tools in the vanilla game have been converted into equipment drops, meaning the player must have
a drop of the appropriate type equipped in your inventory in order to use the tool. You can create
additional drops that occupy the same equipment slot but provide different benefits, such as a spacesuit
that provides additional protection against certain types of hazards, or a Nomai translator that can
translate text faster than the base one. Equipment drops apply certain buffs while they are equipped.


Vanilla items that can be picked up and dropped, like Nomai scrolls, lanterns from the DLC, etc. are also
converted into the drops system, although they work slightly differently. By default, picked up items are
placed in a "toolbelt" or hotbar, and can be easily accessed by the player by cycling through hotbar slots.
These psuedo-drops can then be transferred to the ship inventory, sold to shopkeepers, or otherwise
interacted with like other drops.

## Shops

Shops are NPCs (either existing NPCs or clones) where the player can exchange currency for new drops,
or sell drops they have in their inventory or hotbar in exchange for currency. Each shop is tied to
exactly one shopkeeper NPC, and can only be interacted with through their dialogue.


Each shop has a set list of items that it sells. Shop items can either restock at the start of each loop,
or carry the remaining stock over from loop to loop.


Drops sold to a shopkeeper can be repurchased through a "buyback" menu, although the prices are higher than
the amount the drops initially sold for, to discourage using shopkeepers as extra inventory space.
The buyback system allows the player to gain currency without permanently losing access to items that
may be necessary for game or quest progression.

## Perks

Perks are unlocked by the player using perk points gained from level ups. An unlocked perk permanently
provide certain buffs to the player, unless the perk is refunded, which returns all spent perk points.
Perks can require a previous perk to be unlocked before they become available, forming a standard RPG
perk tree.

## Buffs

Buffs are positive or detrimental effects applied to the player while they have an equippable drop equipped,
have consumed a consumable drop, or have a perk unlocked. These effects can cover a wide range of functionality,
such as modifying the amount of damage taken from different sources, healing or damaging the player,
giving the player additional drops, and many more features.