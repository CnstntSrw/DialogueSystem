#region

using System;

#endregion

namespace DialogueSystem
{
    public abstract class NodeDisplayer : ICharacterDisplayer
    {
        public abstract void Display(DialogueController dialogueController);

        public virtual void ShowCharacter(DialogueNodeData data, DialogueController controller,
            bool loopFirstAnimation = true, Action onFirstAnimationEnd = null)
        {
            controller.TurnOnCharacterView();
            controller.TurnOnObjectOnNodeStart(data.ObjectToTurnOnStart);

            if (data.Character != null)
            {
                controller.InstantiateCharacter(data.Character, data.AnimationNamesToAnimators, data.LipsingKey,
                    loopFirstAnimation, onFirstAnimationEnd);
            }

            controller.SetNPCText(data.NPCText, data.NPCName);
            controller.PlayVoiceOnNode(data.VoiceClip);
        }
    }
}