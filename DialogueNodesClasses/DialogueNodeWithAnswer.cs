using System;
using UnityEngine;

namespace DialogueSystem
{
    [Serializable]
    public class DialogueNodeWithAnswer : DialogueNodeBase
    {
        [SerializeReference]
        private AnswerBase[] _answers;
        [SerializeField]
        private bool _activateTutorial;

        public override NodeDisplayer GetNodeDisplayer()
        {
            return new NodeWithAnswerDisplayer(new DialogueNodeData(NPCText, Character, false, CanSkipNode(), lipsingKey: LipsingKey,objectToTurnOnStart: ObjectToTurnOnStart,null, _answers, animationNamesToAnimators: AnimationNamesToAnimator, nPCName: NPCName, activateTutorial: _activateTutorial, voiceClip: _voiceClip));
        }
        public override bool CanSkipNode()
        {
            return false;
        }
        public override void ClearData()
        {
            foreach (var answer in _answers)
            {
                answer.ClearData();
            }
        }
    }
}
