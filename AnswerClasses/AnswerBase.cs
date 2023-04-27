#region

using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.Localization;
using static DialogueSystem.AnswerWithAction;

#endregion

namespace DialogueSystem
{
    [Serializable]
    public class AnswerBase : ISerializationCallbackReceiver
    {
        public event Action OnAnswerFinished;

        [SerializeField] private LocalizedString _text;

        public LocalizedString Text
        {
            get => _text;
            private set => _text = value;
        }

        [OdinSerialize] public bool DialogueEnd { get; private set; }

        [OdinSerialize, HideInInspector] public int NextDialogueNode { get; private set; }

        [OdinSerialize]
        [HideIf("DialogueEnd")]
        public int NextDialogueNodeOdin { get; private set; }

        [OnValueChanged("InitDependenciesList")]
        public bool UseDependency;

        [ShowIf("UseDependency")] public List<Dependency> _Dependencies = new();


        public virtual void FinishAnswer()
        {
            OnAnswerFinished?.Invoke();
        }

        public virtual void ClearData()
        {
        }

        public virtual OnEnableEventData GetOnEnableEventData()
        {
            return null;
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

        private void InitDependenciesList()
        {
            _Dependencies?.Clear();
            if (UseDependency)
            {
                _Dependencies = new List<Dependency>();
            }
        }

        //Transfer data from Unity serialization to Odin and vice versa
        public void OnBeforeSerialize()
        {
            if (!DialogueEnd && NextDialogueNodeOdin == 0 && NextDialogueNode != 0)
            {
                NextDialogueNodeOdin = NextDialogueNode;
            }
        }

        public void OnAfterDeserialize()
        {
            if (!DialogueEnd && NextDialogueNodeOdin == 0 && NextDialogueNode != 0)
            {
                NextDialogueNodeOdin = NextDialogueNode;
            }
        }
    }


    public abstract class Dependency
    {
        public abstract bool CheckDependency();
    }

    public class TraitsDependency : Dependency
    {
        public CHARACTERNAMES CharacterName;
        public CHARACTERTRAITS TraitName;
        public int NeedTraitValue;

        public override bool CheckDependency()
        {
            return CharactersInfoController.Instance.GetValue(CharacterName, TraitName) >= NeedTraitValue;
        }
    }

    public class RelationshipDependency : Dependency
    {
        public CHARACTERNAMES CharacterName;
        public CHARACTERRELATIONSHIPS RelationshipName;
        public int NeedRelationshipValue;

        public override bool CheckDependency()
        {
            return CharactersInfoController.Instance.GetValue(CharacterName, RelationshipName) >= NeedRelationshipValue;
        }
    }

    public class IsEventDoneDependency : Dependency
    {
        public string EventName;
        public string LocationName;

        public override bool CheckDependency()
        {
            if (!string.IsNullOrEmpty(EventName))
            {
                if (PlayerInfo.Instance.IsEventDone(LocationName, EventName))
                {
                    return true;
                }
            }

            return false;
        }
    }

    public class PlayerPrefDependency : Dependency
    {
        public string PlayerPrefKey;
        public int MinValue;

        public override bool CheckDependency()
        {
            if (SaverAdapter.HasKey(PlayerPrefKey))
            {
                return SaverAdapter.GetInt(PlayerPrefKey) >= MinValue;
            }

            return false;
        }
    }
}