using UnityEngine;

namespace DialogueSystem
{
    public class DialogueNodeWithNote : DialogueNodeNoAnswer
    {
        [SerializeField] private DialogueSmallNoteController _notePrefab;

        [SerializeField] private bool _activateTutorial;
        public override NodeDisplayer GetNodeDisplayer()
        {
            return new NodeWithNoteDisplayer(new DialogueNodeData(NPCText, Character, false, CanSkipNode(), eventData: null,activateTutorial:_activateTutorial, animationNamesToAnimators: AnimationNamesToAnimator,lipsingKey: LipsingKey,objectToTurnOnStart: ObjectToTurnOnStart,notePrefab:_notePrefab, voiceClip: _voiceClip, nPCName:NPCName));
        }
        public override bool CanSkipNode()
        {
            return false;
        }
    }
}
