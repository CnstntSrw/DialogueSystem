using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;
using UnityEngine.Localization;

namespace DialogueSystem
{
    public class DialogueNodeData
    {
        public DialogueNodeData(LocalizedString nPCText, CharacterPrefab character, bool needInventory, bool canSkip,
            string lipsingKey, string objectToTurnOnStart, DialogueSmallNoteController notePrefab = null, AnswerBase[] answers = null,
            IDialogueEventData eventData = null, List<DialogueAnimationData> animationNamesToAnimators = null,
            string nPCName = null, bool activateTutorial = false, AudioClip voiceClip = null)
        {
            NPCText = nPCText;
            Character = character;
            NeedInventory = needInventory;
            CanSkip = canSkip;
            LipsingKey = lipsingKey;
            ObjectToTurnOnStart = objectToTurnOnStart;
            NotePrefab = notePrefab;
            VoiceClip = voiceClip;
            Answers = answers;
            EventData = eventData;
            AnimationNamesToAnimators = animationNamesToAnimators;
            NPCName = nPCName;
            ActivateTutorial = activateTutorial;
        }

        public LocalizedString NPCText { get; private set; }
        public CharacterPrefab Character { get; private set; }
        public IDialogueEventData EventData { get; set; }
        public bool NeedInventory { get; private set; }
        public bool CanSkip { get; private set; }
        public AnswerBase[] Answers { get; private set; }
        public List<DialogueAnimationData> AnimationNamesToAnimators { get; private set; }
        public bool LoopAnimation { get; internal set; }
        public string NPCName { get; internal set; }
        public bool ActivateTutorial { get; private set; }
        public string LipsingKey { get; private set; }
        public AudioClip VoiceClip { get; private set; }
        public string ObjectToTurnOnStart { get; private set; }

        public DialogueSmallNoteController NotePrefab { get; private set; }
    }
}