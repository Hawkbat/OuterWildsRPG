# Advanced Dialogue

This mod extends Outer Wilds' dialogue system with several special conditions and node names,
which let you interact with the mod's systems through normal dialogue XML files.

## Mod-Provided Conditions

Anywhere you can use dialogue condtions in character dialogue XML, you can use special values injected by this mod.
For example, to show a dialogue node only if the player has started a quest:
<?prettify?>
```xml
<DialogueNode>
	<Name>MyQuestIntro</name>
	<EntryCondition>Hawkbar.OuterWildsRPG@MY_AWESOME_QUEST</EntryCondition>
	<Dialogue><!-- etc. --></Dialogue>
</DialogueNode>
```
Or, to do the same for a dialogue option:
<?prettify?>
```xml
<DialogueNode>
	<Name>DEFAULT</name>
	<EntryCondition>ENTRY</EntryCondition>
	<Dialogue><!-- etc. --></Dialogue>
	<DialogueOptionsList>
		<DialogueOption>
			<RequiredCondition>.OuterWildsRPG@MY_AWESOME_QUEST</RequiredCondition>
			<Text>How's the quest going?</Text>
		</DialogueOption>
	</DialogueOptionsList>
</DialogueNode>
```

### Quest Conditions

- Quest Started: `{MyUserName.MyModUniqueName}@{QUEST_ID}`, e.g. `Hawkbar.OuterWildsRPG@MY_AWESOME_QUEST`
- Quest Completed: `{MyUserName.MyModUniqueName}@{QUEST_ID}:COMPLETED`, e.g. `Hawkbar.OuterWildsRPG@MY_AWESOME_QUEST:COMPLETED`
- Quest Step Started: `{MyUserName.MyModUniqueName}@{QUEST_ID}:{QUEST_STEP_ID}`, e.g. `Hawkbar.OuterWildsRPG@MY_AWESOME_QUEST:TALK_TO_SLATE`
- Quest Step Completed: `{MyUserName.MyModUniqueName}@{QUEST_ID}:{QUEST_STEP_ID}:COMPLETED`, e.g. `Hawkbar.OuterWildsRPG@MY_AWESOME_QUEST:TALK_TO_SLATE:COMPLETED`

### Drop Conditions

- Player has Drop (in hotbar, inventory, or equipped): `{MyUserName.MyModUniqueName}@{DROP_ID}`, e.g. `Hawkbar.OuterWildsRPG@BASIC_FLASHLIGHT`
- Player has Drop Equipped: `{MyUserName.MyModUniqueName}@{DROP_ID}:EQUIPPED`, e.g. `Hawkbar.OuterWildsRPG@BASIC_FLASHLIGHT:EQUIPPED`
- Player has Drop in Inventory: `{MyUserName.MyModUniqueName}@{DROP_ID}:INVENTORY`, e.g. `Hawkbar.OuterWildsRPG@BASIC_FLASHLIGHT:INVENTORY`
- Player has Drop in Toolbelt/Hotbar: `{MyUserName.MyModUniqueName}@{DROP_ID}:HOTBAR`, e.g. `Hawkbar.OuterWildsRPG@BASIC_FLASHLIGHT:HOTBAR`

### Perk Conditions

- Player has Unlocked Perk: `{MyUserName.MyModUniqueName}@{PERK_ID}`, e.g. `Hawkbar.OuterWildsRPG@JUMP_SPEED_1`

## Shop Dialogue

Shopkeepers have special dialogue target names used to trigger shop interfaces. [Learn more here.](/guides/shops.html#shop-dialogue)