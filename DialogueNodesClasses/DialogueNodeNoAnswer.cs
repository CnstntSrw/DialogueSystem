using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace DialogueSystem
{
    public class DialogueNodeNoAnswer : DialogueNodeBase
    {
        [OdinSerialize]
        private bool _closeDialogue;
        [OdinSerialize]
        [HideIf("_closeDialogue")]
        private int _nextDialogueNode;

        public override NodeDisplayer GetNodeDisplayer()
        {
            return new NodeNoAnswersDisplayer(new DialogueNodeData(NPCText, Character, false, CanSkipNode(),objectToTurnOnStart: ObjectToTurnOnStart, animationNamesToAnimators: AnimationNamesToAnimator,lipsingKey: LipsingKey, nPCName: NPCName, voiceClip: _voiceClip));
        }
        public override bool IsDialogueEnd()
        {
            return _closeDialogue;
        }
        public override int GetNextDialogueNodeID()
        {
            return _nextDialogueNode;
        }
    }
}
