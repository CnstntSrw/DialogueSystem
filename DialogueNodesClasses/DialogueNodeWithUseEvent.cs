#region

using UnityEngine;

#endregion

namespace DialogueSystem
{
    public class DialogueNodeWithUseEvent : DialogueNodeNoAnswer
    {
        [SerializeField] private DefaultInventoryObjectData _inventoryObjectData;
        [SerializeField] private bool _activateTutorial;

        public override NodeDisplayer GetNodeDisplayer()
        {
            return new NodeWithEventDisplayer(new DialogueNodeData(NPCText, Character, false, CanSkipNode(),
                eventData: new DialogueUseEventData(_inventoryObjectData), activateTutorial: _activateTutorial,
                animationNamesToAnimators: AnimationNamesToAnimator, lipsingKey: LipsingKey,
                objectToTurnOnStart: ObjectToTurnOnStart, voiceClip: _voiceClip));
        }

        public override bool CanSkipNode()
        {
            return false;
        }
    }
}