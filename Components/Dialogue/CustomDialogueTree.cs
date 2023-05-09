using OuterWildsRPG.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace OuterWildsRPG.Components.Dialogue
{
    public abstract class CustomDialogueTree<INode, IOption> : MonoBehaviour, ICustomDialogueTree
    {
        public class DialogueEvent : UnityEvent { }
        public DialogueEvent OnConversationStarted = new();
        public DialogueEvent OnConversationEnded = new();
        public class NodeEvent : UnityEvent<INode> { }
        public NodeEvent OnNodeEntered = new();
        public NodeEvent OnNodeExited = new();
        public class OptionEvent : UnityEvent<IOption, INode> { }
        public OptionEvent OnOptionSelected = new();

        ICustomDialogueTreeProvider<INode, IOption> provider;
        SingleInteractionVolume interactVolume;
        DialogueBoxVer2 dialogueBox;

        bool isRecording;
        bool timeFrozen;
        bool turnOnFlashlight;
        bool turnOffFlashlight;
        bool wasFlashlightOn;

        INode currentNode;
        int currentPage;
        List<IOption> currentOptions = new();
        readonly List<string> optionTranslationKeys = new();

        public bool InConversation() => enabled;

        public void Init(ICustomDialogueTreeProvider<INode, IOption> provider, bool enableInteractVolume)
        {
            this.provider = provider;

            if (enableInteractVolume)
            {
                interactVolume = GetComponent<SingleInteractionVolume>();
                interactVolume.OnPressInteract += OnPressInteract;
                interactVolume.ChangePrompt(provider.GetInteractPrompt());
            }

            dialogueBox = GameObject.FindWithTag("DialogueGui").GetRequiredComponent<DialogueBoxVer2>();

            OnConversationStarted.AddListener(provider.OnConversationStarted);
            OnConversationEnded.AddListener(provider.OnConversationEnded);
            OnNodeEntered.AddListener(provider.OnNodeEntered);
            OnNodeExited.AddListener(provider.OnNodeExited);
            OnOptionSelected.AddListener(provider.OnOptionSelected);
        }

        void CleanUp()
        {
            if (provider != null)
            {
                OnConversationStarted.RemoveListener(provider.OnConversationStarted);
                OnConversationEnded.RemoveListener(provider.OnConversationEnded);
                OnNodeEntered.RemoveListener(provider.OnNodeEntered);
                OnNodeExited.RemoveListener(provider.OnNodeExited);
                OnOptionSelected.RemoveListener(provider.OnOptionSelected);
            }
        }

        public void StartConversation(string context, bool silent)
        {
            enabled = true;

            if (!timeFrozen && PlayerData.GetFreezeTimeWhileReadingConversations() && !Locator.GetGlobalMusicController().IsEndTimesPlaying())
            {
                timeFrozen = true;
                OWTime.Pause(OWTime.PauseType.Reading);
            }

            Locator.GetToolModeSwapper().UnequipTool();

            GlobalMessenger.FireEvent("EnterConversation");

            isRecording = provider.GetIsRecording();

            var flashlightState = provider.GetFlashlightState();
            turnOnFlashlight = flashlightState.HasValue && flashlightState.Value;
            turnOffFlashlight = flashlightState.HasValue && !flashlightState.Value;

            if (!silent)
            {
                wasFlashlightOn = Locator.GetFlashlight().IsFlashlightOn();
                if (wasFlashlightOn && turnOffFlashlight)
                    Locator.GetFlashlight().TurnOff(false);
                if (!wasFlashlightOn && turnOnFlashlight)
                    Locator.GetFlashlight().TurnOn(false);

                Locator.GetPlayerAudioController().PlayDialogueEnter(isRecording);
            }

            DialogueConditionManager.SharedInstance.ReadPlayerData();

            OnConversationStarted.Invoke();
            ChangeNode(provider.GetEntryNode(context));
            Display();

            if (provider.GetAttentionPoint() != null && !PlayerState.InZeroG())
                Locator.GetPlayerTransform().GetRequiredComponent<PlayerLockOnTargeting>().LockOn(provider.GetAttentionPoint(), provider.GetAttentionPointOffset(), 2f);

            if (PlayerState.InZeroG() && !timeFrozen)
                Locator.GetPlayerBody().GetComponent<Autopilot>().StartMatchVelocity(this.GetAttachedOWRigidbody(false).GetReferenceFrame());
        }

        public void EndConversation(bool silent)
        {
            if (!InConversation()) return;
            enabled = false;

            if (timeFrozen)
            {
                timeFrozen = false;
                OWTime.Unpause(OWTime.PauseType.Reading);
            }

            if (interactVolume != null)
            {
                interactVolume.ResetInteraction();
            }

            GlobalMessenger.FireEvent("ExitConversation");

            if (!silent)
            {
                if (wasFlashlightOn && turnOffFlashlight)
                    Locator.GetFlashlight().TurnOn(false);
                if (!wasFlashlightOn && turnOnFlashlight)
                    Locator.GetFlashlight().TurnOff(false);

                Locator.GetPlayerAudioController().PlayDialogueExit(isRecording);
            }

            dialogueBox.OnEndDialogue();
            if (IsCurrentNodeValid()) ChangeNode(default);
            OnConversationEnded.Invoke();

            Locator.GetPlayerTransform().GetComponent<PlayerLockOnTargeting>().BreakLock();

            if (PlayerState.InZeroG())
            {
                var autopilot = Locator.GetPlayerBody().GetComponent<Autopilot>();
                if (autopilot.enabled) autopilot.Abort();
            }
        }

        public void SwitchTo(ICustomDialogueTree target, string context)
        {
            EndConversation(true);
            target.StartConversation(context, true);
        }

        bool IsCurrentNodeValid()
            => !EqualityComparer<INode>.Default.Equals(currentNode, default);

        void Advance()
        {
            var optionIndex = dialogueBox.GetSelectedOption();
            if (optionIndex >= 0)
            {
                var option = currentOptions[optionIndex];
                OnOptionSelected.Invoke(option, currentNode);
                if (!InConversation()) return;
                ChangeNode(provider.GetOptionTarget(option, currentNode));
            }
            else if (provider.GetNodeHasNextPage(currentNode, currentPage))
            {
                currentPage++;
            }
            else
            {
                ChangeNode(provider.GetNodeTarget(currentNode));
            }
            if (!InConversation()) return;
            if (IsCurrentNodeValid())
            {
                Display();
                Locator.GetPlayerAudioController().PlayDialogueAdvance();
            }
            else
            {
                EndConversation(false);
            }
        }

        void ChangeNode(INode node)
        {
            if (IsCurrentNodeValid())
            {
                OnNodeExited.Invoke(currentNode);
                foreach (var key in optionTranslationKeys)
                    TranslationUtils.UnregisterDialogue(key);
                optionTranslationKeys.Clear();
            }
            currentNode = node;
            currentPage = 0;
            if (IsCurrentNodeValid())
            {
                OnNodeEntered.Invoke(currentNode);
                currentOptions = provider.GetNodeOptions(currentNode).ToList();
            }
        }

        void Display()
        {
            var richText = provider.GetNodeText(currentNode, currentPage);
            var listOptions = new List<DialogueOption>();
            if (!provider.GetNodeHasNextPage(currentNode, currentPage))
            {
                foreach (var option in currentOptions)
                {
                    var optionText = provider.GetOptionText(option, currentNode);
                    var translationKey = Guid.NewGuid().ToString();
                    TranslationUtils.RegisterDialogue(translationKey, optionText);
                    optionTranslationKeys.Add(translationKey);
                    listOptions.Add(new DialogueOption()
                    {
                        Text = optionText,
                        _textID = translationKey,
                    });
                }
            }

            dialogueBox.SetVisible(true);
            dialogueBox.SetDialogueText(richText, listOptions);
            var characterName = provider.GetCharacterName();
            if (string.IsNullOrEmpty(characterName))
            {
                dialogueBox.SetNameFieldVisible(false);
            }
            else
            {
                dialogueBox.SetNameFieldVisible(true);
                dialogueBox.SetNameField(characterName);
            }
        }

        void OnPressInteract() => StartConversation(string.Empty, false);

        void OnPlayerDeath(DeathType deathType) => EndConversation(false);

        void Awake()
        {
            GlobalMessenger<DeathType>.AddListener("PlayerDeath", OnPlayerDeath);
            enabled = false;
        }

        void OnDestroy()
        {
            CleanUp();
            GlobalMessenger<DeathType>.RemoveListener("PlayerDeath", OnPlayerDeath);
        }

        void Update()
        {
            if (dialogueBox != null && OWInput.GetInputMode() == InputMode.Dialogue)
            {
                if (OWInput.IsNewlyPressed(InputLibrary.interact) || OWInput.IsNewlyPressed(InputLibrary.cancel) || OWInput.IsNewlyPressed(InputLibrary.enter) || OWInput.IsNewlyPressed(InputLibrary.enter2))
                {
                    if (!dialogueBox.AreTextEffectsComplete())
                    {
                        dialogueBox.FinishAllTextEffects();
                    }
                    else if (dialogueBox.TimeCompletelyRevealed() >= 0.1f)
                    {
                        Advance();
                    }
                }
                else if (OWInput.IsNewlyPressed(InputLibrary.down) || OWInput.IsNewlyPressed(InputLibrary.down2))
                {
                    dialogueBox.OnDownPressed();
                }
                else if (OWInput.IsNewlyPressed(InputLibrary.up) || OWInput.IsNewlyPressed(InputLibrary.up2))
                {
                    dialogueBox.OnUpPressed();
                }
            }
        }
    }
}
