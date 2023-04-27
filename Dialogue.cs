#region

using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

#endregion

namespace DialogueSystem
{
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/DialogueScriptableObject", order = 1)]
    // [Serializable]
    public class Dialogue : SerializedScriptableObject
    {
        [SerializeReference] [HideInInspector] public List<DialogueNodeBase> _dialogueNodes;
        [SerializeField] public Sprite _background;
        [SerializeField] private bool _createdTable;
        [SerializeField] public string _turnOnOnFinish;
        [SerializeField] public string _turnOFFOnStart;

        [NonSerialized] [OdinSerialize] [ListDrawerSettings(ShowIndexLabels = true)]
        public List<DialogueNodeBase> DialogueNodesNew;

        public bool IsCommon { get; set; }

        public int CurrentNodeIndex { get; private set; }

        public event Action<int, bool> OnActionNodePassed;
        //Comment/uncomment for migrate old data.


#if UNITY_EDITOR
        protected override void OnAfterDeserialize()
        {
            if (_dialogueNodes != null && DialogueNodesNew == null) DialogueNodesNew = _dialogueNodes;
        }
#endif


        public DialogueNodeBase StartDialogue(int startNode)
        {
            CurrentNodeIndex = startNode;
            if (DialogueNodesNew.Count > 0)
            {
                if (DialogueNodesNew[CurrentNodeIndex].UseDependency &&
                    !DialogueNodesNew[CurrentNodeIndex].CheckDependencies())
                {
                    return GetNextNodeChecked();
                }

                return DialogueNodesNew[CurrentNodeIndex];
            }

            return null;
        }

        public bool IsFinish()
        {
            return DialogueNodesNew[CurrentNodeIndex].IsDialogueEnd();
        }

        public void FinishNode()
        {
            if (!DialogueNodesNew[CurrentNodeIndex].CanSkipNode())
                OnActionNodePassed?.Invoke(CurrentNodeIndex, IsCommon);
        }

        public DialogueNodeBase GetNextNodeChecked(AnswerBase answer = null)
        {
            if (answer is { DialogueEnd: true }) return null;
            if (IsFinish())
            {
                return null;
            }

            var nextNode = GetNextNode(answer);
            if (nextNode.UseDependency && !nextNode.CheckDependencies())
            {
                nextNode = GetNextNodeChecked();
            }

            return nextNode;
        }

        public DialogueNodeBase GetNextNode(AnswerBase answer = null)
        {
            DialogueNodeBase nextNode;
            if (answer != null)
            {
                if (answer.DialogueEnd)
                    return null;
                CurrentNodeIndex = answer.NextDialogueNodeOdin;
                OnActionNodePassed?.Invoke(CurrentNodeIndex, IsCommon);
                nextNode = DialogueNodesNew[CurrentNodeIndex];
            }
            else
            {
                var nextNodeID = DialogueNodesNew[CurrentNodeIndex].GetNextDialogueNodeID();
                CurrentNodeIndex = nextNodeID;
                FinishNode();
                nextNode = DialogueNodesNew[CurrentNodeIndex];
            }

            return nextNode;
        }

        public DialogueNodeBase SkipTo()
        {
            for (var i = CurrentNodeIndex; i < DialogueNodesNew.Count; i++)
                if (!DialogueNodesNew[i].CanSkipNode())
                {
                    CurrentNodeIndex = i;
                    return DialogueNodesNew[i];
                }

            return null;
        }

        internal Sprite GetBackground()
        {
            return _background;
        }

        internal void ClearData()
        {
            foreach (var node in DialogueNodesNew) node.ClearData();
        }
    }
}