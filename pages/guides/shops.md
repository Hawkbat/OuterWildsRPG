# Creating Shops

## Shop Dialogue

The standard shop buy/sell/buyback interfaces should be triggered from shopkeeper dialogue by using these special node target names:

- Buy Screen: `SHOP_BUY`
- Sell Screen: `SHOP_SELL`
- Buyback Screen: `SHOP_BUYBACK`

> Note: These are only available for shopkeeper NPCs; attempting to use these in normal dialogue will fail.

For example, a typical shop NPC's dialogue might look like this:
<?prettify?>
```xml
<DialogueNode>
	<Name>DEFAULT</Name>
	<Dialogue>
		<Page>Howdy there, hatchling! Got something to trade?</Page>
	</Dialogue>
	<DialogueOptionsList>
		<DialogueOption>
			<Text>What've you got?</Text>
			<DialogueTarget>SHOP_BUY</DialogueTarget>
		</DialogueOption>
		<DialogueOption>
			<Text>I have some items to sell.</Text>
			<DialogueTarget>SHOP_SELL</DialogueTarget>
		</DialogueOption>
		<DialogueOption>
			<Text>I'd like to buy back an item.</Text>
			<DialogueTarget>SHOP_BUYBACK</DialogueTarget>
		</DialogueOption>
		<DialogueOption>
			<Text>Nothing today.</Text>
		</DialogueOption>
	</DialogueOptionsList>
</DialogueNode>
```