using UnityEngine;

namespace DialogueSystem
{
    public class DialogueNodeWithGetEvent : DialogueNodeNoAnswer
    {
        [SerializeField]
        private InventoryObjectData _inventoryObjectData;
        [SerializeField]
        private Sprite _getEventSprite;

        [SerializeField] private bool _activateTutorial;
        public override NodeDisplayer GetNodeDisplayer()
        {
            return new NodeWithEventDisplayer(new DialogueNodeData(NPCText, Character, false, CanSkipNode(), eventData: new DialogueGetEventData(_inventoryObjectData, _getEventSprite),activateTutorial:_activateTutorial, animationNamesToAnimators: AnimationNamesToAnimator,lipsingKey: LipsingKey,objectToTurnOnStart: ObjectToTurnOnStart, voiceClip: _voiceClip));
        }
        public override bool CanSkipNode()
        {
            return false;
        }
    }
}
