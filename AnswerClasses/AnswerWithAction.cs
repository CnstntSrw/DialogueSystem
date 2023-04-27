using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace DialogueSystem
{
    [Serializable]
    public class AnswerWithAction : AnswerPlayerPrefs
    {
        [SerializeField] private OnEnableEvent OnEnableEventPrefab;

        [NonSerialized] [OdinSerialize] [ListDrawerSettings(AlwaysAddDefaultValue = true)]
        private List<ActionStep> Steps = new() { new ActionStep() };

        [NonSerialized] [OdinSerialize] [OnValueChanged("InitPlayerPrefsList")]
        private bool WritePrefs;

        [NonSerialized] [OdinSerialize] [ShowIf("WritePrefs")]
        private new List<AnswerPlayerPrefs.PlayerBoolPrefKeyValue> _playerPrefsKeys = new();

        public override OnEnableEventData GetOnEnableEventData()
        {
            return OnEnableEventPrefab != null ? new OnEnableEventData(OnEnableEventPrefab, Steps) : null;
        }

        public override void FinishAnswer()
        {
            base.FinishAnswer();

            WriteAnswerPlayerPrefs(_playerPrefsKeys);
        }

        private void InitPlayerPrefsList()
        {
            _playerPrefsKeys?.Clear();
            if (WritePrefs)
            {
                _playerPrefsKeys = new List<AnswerPlayerPrefs.PlayerBoolPrefKeyValue> { new() };
            }
        }

        public class OnEnableEventData
        {
            public OnEnableEvent OnEnableEventPrefab;

            public List<ActionStep> Steps;

            public OnEnableEventData(OnEnableEvent onEnableEventPrefab, List<ActionStep> steps)
            {
                OnEnableEventPrefab = onEnableEventPrefab;
                Steps = steps;
            }
        }
    }
}