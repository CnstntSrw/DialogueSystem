#region

using System;

#endregion

namespace DialogueSystem
{
    public interface ICharacterDisplayer
    {
        public abstract void ShowCharacter(DialogueNodeData data, DialogueController controller,
            bool loopFirstAnimation = true, Action onFirstAnimationEnd = null);
    }
}