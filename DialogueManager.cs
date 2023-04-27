#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

#endregion

namespace DialogueSystem
{
    public class DialogueManager : SerializedMonoBehaviour
    {
        private const string DIALOGUES_FOLDER_PATH = "Assets/ScriptableObjects/Dialogues/";
        private const string COMMON_FOLDER_NAME = "Common";

        public static event Action<Dialogue, int> OnDialogueStart;
        public static event Action OnDialogueFinish;

        public static DialogueManager Instance { get; private set; }

        [SerializeField] private DialogueController _dialogueUI;
        [SerializeField] private Button _menuButton;
        private Dictionary<string, Dialogue> _dialogues = new();
        private Dictionary<string, Dialogue> _commonDialogues = new();
        [NonSerialized] [OdinSerialize] private Dictionary<string, Dictionary<string, Dialogue>> _dialoguesSO = new();

        [NonSerialized] [OdinSerialize]
        private Dictionary<string, Dictionary<string, Dialogue>> _commonDialoguesSO = new();

        private string _currentDialogueName = "";
        private Dialogue _currentDialogue;


#if UNITY_EDITOR
        [OdinSerialize] private List<Dialogue> _debugDialogues = new();

        [ValueDropdown("_debugDialogues")] public Dialogue DialogueToPlay;

        [Button("PlayDialogue")]
        public void PlayDialogueDebug()
        {
            if (DialogueToPlay == null)
            {
                return;
            }

            StartDialogueUI();
            OnDialogueStart?.Invoke(DialogueToPlay, 0);

            _dialogueUI.OnDialogueClose += OnDebugDialogueClose;
        }

        private void OnDebugDialogueClose(bool arg1, bool arg2)
        {
            _dialogueUI.OnDialogueClose -= OnDebugDialogueClose;
            _dialogueUI_OnDialogueClose(arg1, arg2);
        }
#endif

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
            LoadCommonDialogues();
        }

        public bool IsDialogActive()
        {
            return !(_currentDialogue == null);
        }

        public IEnumerable<GameObject> GetGOForTutorial()
        {
            var answers = _dialogueUI.GetAnswerButtons();
            if (answers.Any())
            {
                return answers;
            }

            var eventGO = _dialogueUI.GetCurrentEvent();
            return eventGO ? new[] { eventGO } : null;
        }

        public GameObject GetCurrentEventGameObject()
        {
            CharacterPrefab charInstance = _dialogueUI.CharInstance;

            if (charInstance)
            {
                if (charInstance.GetEvent.activeSelf)
                {
                    return charInstance.GetEvent;
                }

                if (charInstance.UseEvent.activeSelf)
                {
                    return charInstance.UseEvent;
                }
            }

            return null;
        }


        public void StartDialogue(string name, bool isCommon = false, int startNode = 0)
        {
            if (LocationController.Current != null)
            {
                if (LocationController.Current.IsLoading)
                {
                    StartCoroutine(StartDialogueAfterAreaLoaded(name, isCommon, startNode));
                    return;
                }
            }

            _menuButton.interactable = false;
            Dialogue dialogue = GetNextDialogue(name, isCommon);

            _currentDialogueName = name;

            _currentDialogue = dialogue;

            if (dialogue != null)
            {
                StartDialogueUI();

                OnDialogueStart?.Invoke(dialogue, startNode);

                dialogue.OnActionNodePassed += SavePassedActionNode;

                _dialogueUI.OnDialogueClose += _dialogueUI_OnDialogueClose;

                if (startNode == 0)
                {
                    if (isCommon)
                    {
                        SaveDialogueUnpassedPref(name, COMMON_FOLDER_NAME, startNode);
                    }
                    else
                    {
                        SaveDialogueUnpassedPref(name, SceneManager.GetActiveScene().name, startNode);
                    }
                }
            }
        }
        public GameObject GetNotesBackScreen()
        {
            return _dialogueUI.NotesBackScreen;
        }

        private IEnumerator StartDialogueAfterAreaLoaded(string name, bool isCommon = false, int startNode = 0)
        {
            yield return new WaitUntil(() => LocationController.Current?.IsLoading == false);
            StartDialogue(name, isCommon, startNode);
        }

        private void _dialogueUI_OnDialogueClose(bool isCommon, bool isDebug = false)
        {
            _dialogueUI.OnDialogueClose -= _dialogueUI_OnDialogueClose;
            OnDialogueClose(isCommon, isDebug);
            OnDialogueFinish?.Invoke();
            _menuButton.interactable = true;
        }

        private void SceneManager_activeSceneChanged(Scene sceneFrom, Scene sceneTo)
        {
            if (_dialoguesSO == null || _dialoguesSO.Count == 0)
            {
                return;
            }

            LoadDialoguesAssetsForScene(sceneTo.name);
        }

        private void LoadDialoguesAssetsForScene(string sceneName)
        {
            _dialogues.Clear();
            _dialogues = GetDialoguesFromDirectory(DIALOGUES_FOLDER_PATH, sceneName, out string unpassedDialogueName,
                out int lastNodeID);

            if (!string.IsNullOrEmpty(unpassedDialogueName))
            {
                StartDialogue(unpassedDialogueName, false, lastNodeID);
            }
        }

        private int GetStartNodeID(int lastNodeID)
        {
            if (lastNodeID == 0)
            {
                return lastNodeID; //Start from the beginning
            }

            return lastNodeID + 1; //Start after "checkpoint" node
        }

        private void LoadCommonDialogues()
        {
            if (_commonDialoguesSO == null || _commonDialoguesSO.Count == 0)
            {
                return;
            }

            _commonDialogues.Clear();
            _commonDialogues = GetDialoguesFromDirectory(DIALOGUES_FOLDER_PATH, COMMON_FOLDER_NAME,
                out string unpassedDialogueName, out int lastNodeID);
            if (!string.IsNullOrEmpty(unpassedDialogueName))
            {
                StartDialogue(unpassedDialogueName, true, GetStartNodeID(lastNodeID));
            }
        }

        private Dictionary<string, Dialogue> GetDialoguesFromDirectory(string path, string dialoguesFolderName,
            out string unpassedDialogueName, out int lastNodeID)
        {
            unpassedDialogueName = "";
            lastNodeID = 0;
            var loaded = new Dictionary<string, Dialogue>();

            List<Dialogue> dialogues = new List<Dialogue>();

            if (dialoguesFolderName != COMMON_FOLDER_NAME)
            {
                if (_dialoguesSO.TryGetValue(dialoguesFolderName,
                        out Dictionary<string, Dialogue> currentSceneDialogues))
                {
                    foreach (var kvp in currentSceneDialogues)
                    {
                        dialogues.Add(kvp.Value);
                    }
                }
            }
            else
            {
                if (_commonDialoguesSO.TryGetValue(dialoguesFolderName,
                        out Dictionary<string, Dialogue> currentSceneDialogues))
                {
                    foreach (var kvp in currentSceneDialogues)
                    {
                        dialogues.Add(kvp.Value);
                    }
                }
            }

            foreach (var dialogue in dialogues)
            {
                if (IsDialoguePassed(dialogue.name, dialoguesFolderName))
                {
                    continue;
                }

                if (dialoguesFolderName == COMMON_FOLDER_NAME)
                {
                    dialogue.IsCommon = true;
                }

                loaded.Add(dialogue.name, dialogue);
                if (IsDialogueUncompleted(dialogue.name, dialoguesFolderName, out int nodeID))
                {
                    unpassedDialogueName = dialogue.name;
                    lastNodeID = nodeID;
                }
            }

            return loaded;
        }

        private void StartDialogueUI()
        {
            InterfaceController.Instance.HideBbt();
            ChangeDialogueUIState();
        }

        private void OnDialogueClose(bool isCommon = false, bool isDebug = false)
        {
            ChangeDialogueUIState();
            if (!isDebug)
            {
                if (isCommon)
                {
                    SavePassedDialogue(_currentDialogueName, COMMON_FOLDER_NAME);
                }
                else
                {
                    SavePassedDialogue(_currentDialogueName, SceneManager.GetActiveScene().name);
                }
            }

            _currentDialogueName = string.Empty;
            _currentDialogue = null;
        }

        private void ChangeDialogueUIState()
        {
            _dialogueUI.gameObject.SetActive(!_dialogueUI.gameObject.activeSelf);
        }

        private Dialogue GetNextDialogue(string dialogueName, bool isCommon = false)
        {
            if (!isCommon)
            {
                if (_dialogues.TryGetValue(dialogueName, out Dialogue dialogue))
                {
                    return dialogue;
                }
            }
            else
            {
                if (_commonDialogues.TryGetValue(dialogueName, out Dialogue dialogue))
                {
                    return dialogue;
                }
            }

            return null;
        }

        #region Dialogue player prefs

        public enum DialogStatus
        {
            _started,
            _passed
        }

        private bool IsDialogueUncompleted(string dialogueName, string sceneName, out int lastNodeID)
        {
            lastNodeID = 0;
            if (SaverAdapter.HasKey(PlayerInfo.Instance.PlayerName + "_" + dialogueName + "_" + sceneName +
                                    DialogStatus._started))
            {
                int nodeID = SaverAdapter.GetInt(PlayerInfo.Instance.PlayerName + "_" + dialogueName + "_" + sceneName +
                                                 DialogStatus._started);
                lastNodeID = nodeID;
                return true;
            }

            return false;
        }

        private bool IsDialoguePassed(string dialogueName, string sceneName)
        {
            if (SaverAdapter.GetInt(PlayerInfo.Instance.PlayerName + "_" + dialogueName + "_" + sceneName +
                                    DialogStatus._passed) != 1)
            {
                return false;
            }

            return true;
        }

        private void SavePassedActionNode(int nodeID, bool isCommon)
        {
            if (isCommon)
            {
                SaveDialogueUnpassedPref(_currentDialogueName, COMMON_FOLDER_NAME, nodeID);
            }
            else
            {
                SaveDialogueUnpassedPref(_currentDialogueName, SceneManager.GetActiveScene().name, nodeID);
            }
        }

        private void SaveDialogueUnpassedPref(string dialogueName, string sceneName, int dialogueCheckPoint)
        {
            string key = PlayerInfo.Instance.PlayerName + "_" + dialogueName + "_" + sceneName + DialogStatus._started;
            SaverAdapter.SetInt(key, dialogueCheckPoint);
            SaverAdapter.Save();
        }

        private void SavePassedDialogue(string dialogueName, string sceneName)
        {
            string key = PlayerInfo.Instance.PlayerName + "_" + dialogueName + "_" + sceneName;

            if (SaverAdapter.HasKey(key + DialogStatus._started))
            {
                SaverAdapter.DeleteKey(key + DialogStatus._started);
            }

            key += DialogStatus._passed;
            SaverAdapter.SetInt(key, 1);
            SaverAdapter.Save();
        }

        public void RemoveDialoguesPrefsForCurrentScene(string sceneName)
        {
            foreach (var kvp in _dialoguesSO)
            {
                foreach (var dlgKVP in kvp.Value)
                {
                    if (SaverAdapter.HasKey(PlayerInfo.Instance.PlayerName + "_" + dlgKVP.Key + "_" + sceneName))
                    {
                        SaverAdapter.DeleteKey(PlayerInfo.Instance.PlayerName + "_" + dlgKVP.Key + "_" + sceneName);
                    }

                    RemoveAnswerPlayerPrefs(dlgKVP.Value);
                }
            }

            // string[] dialogueFiles =
            //     Directory.GetFiles(DIALOGUES_FOLDER_PATH + sceneName, "*.asset", SearchOption.AllDirectories);
            //
            // foreach (var file in dialogueFiles)
            // {
            //     // string assetPath = file.Replace(Application.dataPath, "").Replace('\\', '/');
            //     // Dialogue dialogue = (Dialogue)AssetDatabase.LoadAssetAtPath(assetPath, typeof(Dialogue));
            //     //
            //     // if (SaverAdapter.HasKey(PlayerInfo.Instance.PlayerName + "_" + dialogue.name + "_" + sceneName))
            //     // {
            //     //     SaverAdapter.DeleteKey(PlayerInfo.Instance.PlayerName + "_" + dialogue.name + "_" + sceneName);
            //     // }
            //     //
            //     // RemoveAnswerPlayerPrefs(dialogue);
            // }
        }


        private static void RemoveAnswerPlayerPrefs(Dialogue dialogue)
        {
            dialogue.ClearData();
        }

        #endregion
    }
}