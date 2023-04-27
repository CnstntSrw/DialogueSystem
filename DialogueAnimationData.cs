using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using Spine.Unity;
using Unity.VisualScripting;
using UnityEngine;

namespace DialogueSystem
{
    public class DialogueAnimationData
    {
        [DoNotSerialize, OdinSerialize, ValueDropdown("availableAnimationNames")]
        public string AnimationName;
        [DoNotSerialize, OdinSerialize, ValueDropdown("availableAnimators"), OnValueChanged("FillAvailableAnimationNames"), OnValueChanged("FillAvailableAnimators"), OnInspectorInit("FillAvailableAnimationNames"), OnInspectorInit("FillAvailableAnimators")]
        public SkeletonGraphic Animator;
        [DoNotSerialize, OdinSerialize]
        public bool WaitForLipsyncEnd;
        [DoNotSerialize, OdinSerialize]
        public bool IsGetAnimation;
        private static List<SkeletonGraphic> availableAnimators;
        
        private static List<string> availableAnimationNames;
        [DoNotSerialize, OdinSerialize, HideInEditorMode]
        private CharacterPrefab _character;

        private void FillAvailableAnimationNames()
        {
            if (Animator != null)
            {
                availableAnimationNames = Animator.SkeletonData.Animations.Select(a => a.Name).ToList();
            }
        }
        private void FillAvailableAnimators()
        {
            if (_character != null)
            {
                availableAnimators = _character._skeletonAnimators;
            }
        }
        internal DialogueAnimationData(CharacterPrefab character)
        {
            _character = character;
            if (_character != null)
            {
                availableAnimators = _character._skeletonAnimators;
            }
            else
            {
                Debug.LogWarning("Character is null. Setup character prefab field to update available animations!");
            }
            availableAnimationNames = new();
            WaitForLipsyncEnd = true;
        }
    }
}