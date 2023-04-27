using System;
using System.Collections.Generic;
using UnityEngine;

namespace DialogueSystem
{
    [Serializable]
    public class AnswerPlayerPrefs : AnswerBase
    {
        [SerializeField] protected List<PlayerBoolPrefKeyValue> _playerPrefsKeys = new();

        public override void FinishAnswer()
        {
            base.FinishAnswer();
            WriteAnswerPlayerPrefs(_playerPrefsKeys);
        }

        protected void WriteAnswerPlayerPrefs(List<PlayerBoolPrefKeyValue> playerPrefsKeys)
        {
            if (playerPrefsKeys != null)
            {
                foreach (var kvp in playerPrefsKeys)
                {
                    SaverAdapter.SetInt(kvp.Key, SaverAdapter.GetInt(kvp.Key) + kvp.Value);
                    SaverAdapter.Save();
                }
            }
        }

        public override void ClearData()
        {
            if(_playerPrefsKeys == null)
                return;
            foreach (var kvp in _playerPrefsKeys)
                if (SaverAdapter.HasKey(kvp.Key))
                    SaverAdapter.DeleteKey(kvp.Key);
            SaverAdapter.Save();
        }

        [Serializable]
        public struct PlayerBoolPrefKeyValue
        {
            [SerializeField] internal string Key;

            [SerializeField] internal int Value;
        }
    }
}