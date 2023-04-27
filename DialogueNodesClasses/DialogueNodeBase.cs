#if UNITY_EDITOR
using UnityEditor.Localization;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using CustomPropertyAttributes;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using Spine.Unity;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.SmartFormat.PersistentVariables;
using UnityEngine.Localization.Tables;

namespace DialogueSystem
{
    [Serializable]
    public class DialogueNodeBase
    {
        protected bool NeedInventory;

        [DoNotSerialize, OdinSerialize] public string ObjectToTurnOnStart = string.Empty;
        [DoNotSerialize, OdinSerialize] public string ObjectToTurnOnFinish = string.Empty;

        [DoNotSerialize, OdinSerialize, ValueDropdown("myValues")]
        public string LipsingKey = string.Empty;

        // The selectable values for the dropdown.

        private static List<string> myValues;

        [SerializeField]
#if UNITY_EDITOR
        [OnValueChanged("OnCharacterChanged")]
        // [OnInspectorDispose("OnCharacterChanged")]
        [OnInspectorInit("OnCharacterChanged")]
#endif
        public CharacterPrefab Character;

        [SerializeField] protected AudioClip _voiceClip;

        [SerializeField,
         InfoBox("Deprecated field (not used to set animation in runtime). Use Animation names to animator.")]
        public string AnimationName;

        [DoNotSerialize, OdinSerialize, ListDrawerSettings(CustomAddFunction = "AddAnimationName")]
        public List<DialogueAnimationData> AnimationNamesToAnimator = new();

        [SerializeField] protected LocalizedString NPCText;
        [SerializeField] protected string NPCName;

        [OnValueChanged("InitDependenciesList")]
        public bool UseDependency;

        [ShowIf("UseDependency")] public List<Dependency> _Dependencies = new();

        private void InitDependenciesList()
        {
            _Dependencies?.Clear();
            if (UseDependency)
            {
                _Dependencies = new List<Dependency>();
            }
        }

        public bool CheckDependencies()
        {
            foreach (var dependency in _Dependencies)
            {
                if (!dependency.CheckDependency())
                {
                    return false;
                }
            }

            return true;
        }

        public virtual NodeDisplayer GetNodeDisplayer()
        {
            return null;
        }

        public virtual bool IsDialogueEnd()
        {
            return false;
        }

        public virtual bool CanSkipNode()
        {
            return true;
        }

        public virtual int GetNextDialogueNodeID()
        {
            return 0;
        }

        public virtual void ClearData()
        {
        }

        #region Set up variables to display localized NPCname prefix

#if UNITY_EDITOR
        public void AddAnimationName()
        {
            AnimationNamesToAnimator.Add(new DialogueAnimationData(Character));
        }

        public void OnCharacterChanged()
        {
            if (Character == null || Character.LipsingOutputsList == null)
            {
                return;
            }

            FillLipsingsValuesDropdown();
        }

        private void FillLipsingsValuesDropdown()
        {
            myValues = new List<string> { "None" };
            foreach (var lipsingName in Character.LipsingOutputsList)
            {
                myValues.Add(lipsingName.Name);
            }

            if (LipsingKey == string.Empty)
            {
                LipsingKey = myValues.First();
            }
        }
#endif

        #endregion
    }
}