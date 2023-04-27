#region

using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.UI;
using static DialogueSystem.AnswerWithAction;

#endregion

namespace DialogueSystem
{
    public class DialogueController : MonoBehaviour
    {
        public enum TutorialType
        {
            None,
            AnswersTutorial,
            GetEventTutorial,
            UseEventTutorial
        }

        public event Action<bool, bool> OnDialogueClose;

        public CharacterPrefab CharInstance
        {
            get => _charInstance;
            private set => _charInstance = value;
        }

        public GameObject NotesBackScreen;

        private readonly List<Button> ANSWER_BUTTONS = new();

        [SerializeField] private GameObject _dialogueUIRoot;
        [SerializeField] private Image _dialogueBack;
        [SerializeField] private TMP_Text _npcText;
        [SerializeField] private LocalizeStringEvent _npcTextLocalizeEvent;
        [SerializeField] private RectTransform _buttonsPanel;
        [SerializeField] private Button _buttonPrefab;
        [SerializeField] private Button _skipButton;
        [SerializeField] private GameObject _characterSlot;
        [SerializeField] private Button _clickHandlerButton;
        [SerializeField] private Transform _eventsParent;
        [SerializeField] private GameObject _getEventPrefab;

        private Dialogue Dialogue { get; set; }
        private Vector3 _defaultNPCTextPosition;
        private Vector2 _defaultNPCTextSize;
        private CharacterPrefab _charInstance;
        private List<GameObject> _answerActions = new List<GameObject>();

        private void Awake()
        {
            DialogueManager.OnDialogueStart += OnDialogueStart;
            _skipButton.onClick.AddListener(OnSkipClick);
            _defaultNPCTextPosition = _npcText.rectTransform.position;
            _defaultNPCTextSize = _npcText.rectTransform.sizeDelta;
        }

        private void OnDestroy()
        {
            DialogueManager.OnDialogueStart -= OnDialogueStart;
        }

        public IEnumerable<GameObject> GetAnswerButtons()
        {
            return ANSWER_BUTTONS.Select(a => a.gameObject);
        }

        public GameObject GetCurrentEvent()
        {
            if (_charInstance == null)
            {
                return null;
            }

            return _charInstance.GetEvent.activeSelf ? _charInstance.GetEvent :
                _charInstance.UseEvent.activeSelf ? _charInstance.UseEvent : null;
        }

        public void TurnOnObjectOnNodeStart(string ObjectName)
        {
            if (!string.IsNullOrEmpty(ObjectName))
            {
                var go = FindObjectsOfType<GameObject>(true).Where(n => n.name == ObjectName);
                if (go.Count() != 0)
                {
                    go.First().SetActive(true);
                }
            }
        }

        public void CreateNPCUseEvent(DefaultInventoryObjectData inventoryObjData, bool needTutorial,
            TutorialType tutorialType)
        {
            var onDrop = _charInstance.UseEvent.GetComponent<OnDropEvent>();
            onDrop.ActionSteps.First().InvObj = inventoryObjData;
            onDrop.InvObj = inventoryObjData;
            _charInstance.UseEvent.SetActive(true);
            if (needTutorial)
                ActivateTutorial(tutorialType, onDrop.gameObject);
        }

        public void CreateNPCGetEvent(InventoryObjectData inventoryObjectData, Sprite eventSprite, bool needTutorial,
            TutorialType tt)
        {
            var onClick = _charInstance.GetEvent.GetComponent<OnClickEvent>();
            onClick.ActionSteps.First().InvObj = inventoryObjectData;
            var image = onClick.GetComponent<Image>();
            GameObject getEvent;
            bool isAdditionalGet = false;
            if (image.raycastTarget == false && _getEventPrefab)
            {
                getEvent = Instantiate(_getEventPrefab, _charInstance.GetEvent.transform.parent);
                image = getEvent.GetComponent<Image>();
                onClick = getEvent.GetComponent<OnClickEvent>();
                onClick.OnClickEventComplete += OnContinueClick;
                onClick.ActionSteps.First().InvObj = inventoryObjectData;
                isAdditionalGet = true;
            }
            else
            {
                getEvent = _charInstance.GetEvent;
            }

            if (eventSprite != null)
            {
                image.sprite = eventSprite;
                onClick.OnClickEventComplete += () =>
                {
                    SetupSprite(onClick);
                    AudioSystem.Instance.PlayEffectSound(CommonSounds.gives_object);
                };
            }

            if (isAdditionalGet)
            {
                image.color = new Color(image.color.r, image.color.g, image.color.b, 0);
                image.raycastTarget = true;
                if (_charInstance._skeletonAnimators.FirstOrDefault()?.skeletonDataAsset.scale != 1)
                {
                    getEvent.GetComponent<RectTransform>().localScale = new Vector3(0.01f, 0.01f, 0.01f);
                }
            }

            getEvent.SetActive(true);
            if (needTutorial)
                ActivateTutorial(tt, onClick.gameObject);
        }

        public void ActivateTutorial(TutorialType tutorialType, GameObject pointTo = null)
        {
            GameObject tutorialGO = null;

            switch (tutorialType)
            {
                case TutorialType.AnswersTutorial:
                    tutorialGO =
                        DefaultTutorialCanvas.Instance.DefaultTutorials.Find(x => x.name == "DialogueOptionsTutorial");
                    break;
                case TutorialType.GetEventTutorial:
                    if (pointTo)
                    {
                        tutorialGO =
                            DefaultTutorialCanvas.Instance.DefaultTutorials.Find(x => x.name == "DialogueGetTutorial");
                    }

                    break;
                case TutorialType.UseEventTutorial:
                    if (pointTo)
                    {
                        tutorialGO =
                            DefaultTutorialCanvas.Instance.DefaultTutorials.Find(x => x.name == "DialogueUseTutorial");
                    }

                    break;
                default:
                    Debug.LogWarning("Unknown dialogue tutorial type");
                    break;
            }

            if (tutorialGO)
            {
                InterfaceController.Instance.ShowTutorial(tutorialGO);
            }
        }

        public void HideDialogPanel()
        {
            _dialogueUIRoot.SetActive(false);
        }

        public void ShowDialogPanel()
        {
            _dialogueUIRoot.SetActive(true);
        }

        public void ShowClickHandler()
        {
            _clickHandlerButton.gameObject.SetActive(true);
        }


        public void SetAnswerButtons(AnswerBase[] answers)
        {
            ClearButtons();
            foreach (var ans in answers)
            {
                if (ans.UseDependency && !ans.CheckDependencies())
                {
                    continue;
                }

                var button = Instantiate(_buttonPrefab, _buttonsPanel);
                button.GetComponentInChildren<LocalizeStringEvent>().StringReference = ans.Text;
                button.name = "Answer" + ANSWER_BUTTONS.Count;
                ANSWER_BUTTONS.Add(button);
                button.onClick.AddListener(() => OnAnswerClick(ans));
            }
        }


        public void SetNPCText(LocalizedString text, string characterName)
        {
            if (text.IsEmpty)
            {
                _npcText.text = "";
                return;
            }

            _npcTextLocalizeEvent.StringReference = text;
            _npcText.text = characterName + ": " + _npcText.text;
        }

        public void PlayVoiceOnNode(AudioClip clip)
        {
            if (clip)
                AudioSystem.Instance.PlayVoice(clip);
        }

        public void InstantiateCharacter(CharacterPrefab characterPrefab,
            List<DialogueAnimationData> animationNamesToAnimators, string lipsingKey, bool loopFirstAnimation,
            Action onFirstAnimationEnd)
        {
            if (_charInstance != null)
            {
                if (!_charInstance.name.Contains(characterPrefab.name))
                {
                    foreach (Transform oldCharacters in _characterSlot.transform)
                    {
                        Destroy(oldCharacters.gameObject);
                    }

                    _charInstance = Instantiate(characterPrefab, _characterSlot.transform);
                    SubscribeEvents();
                    _charInstance.GetEvent.SetActive(false);
                    _charInstance.UseEvent.SetActive(false);
                }
            }
            else
            {
                _charInstance = Instantiate(characterPrefab, _characterSlot.transform);
                SubscribeEvents();
                _charInstance.GetEvent.SetActive(false);
                _charInstance.UseEvent.SetActive(false);
            }

            CharInstance.SetAnimations(animationNamesToAnimators, lipsingKey, loopFirstAnimation, onFirstAnimationEnd);
        }


        public void TurnOnInventoryIfNeed(bool needInventory)
        {
            if (needInventory)
            {
                InterfaceController.Instance.SetInterfaceVisible(true);
            }
            else
            {
                InterfaceController.Instance.SetInterfaceVisible(false);
                SetDefaultTextSize();
            }
        }

        public void TurnOnCharacterView()
        {
            _characterSlot.SetActive(true);
        }

        public void ShowNote(DialogueSmallNoteController dataNotePrefab)
        {
            var instance = Instantiate(dataNotePrefab, _charInstance.NoteSlot);
            instance.OnCLose += OnContinueClick;
        }

        public void OnContinueClick()
        {
            _clickHandlerButton.gameObject.SetActive(false);
            var objectName = Dialogue.DialogueNodesNew[Dialogue.CurrentNodeIndex].ObjectToTurnOnFinish;
            if (objectName != string.Empty)
            {
                var go = FindObjectsOfType<GameObject>(true).Where(n => n.name == objectName);
                if (go.Count() != 0)
                {
                    go.First().SetActive(true);
                }
            }

            if (Dialogue.IsFinish())
            {
                CloseDialogue(Dialogue);
                return;
            }

            StartNode(Dialogue.GetNextNodeChecked());
        }

        private void TurnOffCharacterView()
        {
            _characterSlot.SetActive(false);
        }

        private void SetupSprite(OnClickEvent onClick)
        {
            onClick.GetComponent<Image>().color = Color.white;
        }

        private void TurnOffDialogueBack()
        {
            _dialogueBack.sprite = null;
            _dialogueBack.color = new Color(255, 255, 255, 0);
        }

        private void SubscribeEvents()
        {
            var onClick = _charInstance.GetEvent.GetComponent<OnClickEvent>();
            onClick.OnClickEventComplete += OnContinueClick;
            var onDrop = _charInstance.UseEvent.GetComponent<OnDropEvent>();
            onDrop.OnDropSuccess += OnContinueClick;
        }

        private void OnDialogueStart(Dialogue dialogue, int startNode)
        {
            foreach (Transform oldCharacters in _characterSlot.transform)
            {
                Destroy(oldCharacters.gameObject);
            }

            _charInstance = null;

            Dialogue = dialogue;
            ActivateDialogueUI();
            TurnOnDialogueBack(Dialogue.GetBackground());
            StartNode(Dialogue.StartDialogue(startNode));
        }

        private void OnSkipClick()
        {
            _clickHandlerButton.gameObject.SetActive(false);
            StartNode(Dialogue.SkipTo());
        }

        private void ActivateDialogueUI()
        {
            //TODO: refactor
            if (!string.IsNullOrEmpty(Dialogue._turnOFFOnStart))
            {
                var go = FindObjectsOfType<GameObject>(true).Where(n => n.name == Dialogue._turnOFFOnStart);
                if (go.Count() != 0)
                {
                    go.First().SetActive(false);
                }
            }

            _dialogueUIRoot.SetActive(true);
        }

        private void SetDefaultTextSize()
        {
            _npcText.enableAutoSizing = false;
            _npcText.rectTransform.position = _defaultNPCTextPosition;
            _npcText.rectTransform.sizeDelta = _defaultNPCTextSize;
        }

        private void CloseDialogue(Dialogue dialogue)
        {
            //TODO: refactor
            AudioSystem.Instance.StopAllVoices();

            InterfaceController.Instance.SetInterfaceVisible(true);

            if (!string.IsNullOrEmpty(Dialogue._turnOnOnFinish))
            {
                var go = FindObjectsOfType<GameObject>(true).Where(n => n.name == Dialogue._turnOnOnFinish);
                if (go != null && go.Count() != 0)
                {
                    go.First().SetActive(true);
                }
            }

            ClearAnswerActions();

            // DestroyCharacter();
            OnDialogueClose?.Invoke(dialogue.IsCommon, false);
            TurnOffDialogueBack();
            TurnOffCharacterView();
            _dialogueUIRoot.SetActive(false);
        }

        private void ClearAnswerActions()
        {
            foreach (var item in _answerActions)
            {
                Destroy(item);
            }

            _answerActions.Clear();
        }

        private void ClearButtons()
        {
            foreach (var item in ANSWER_BUTTONS)
            {
                Destroy(item.gameObject);
            }

            ANSWER_BUTTONS.Clear();
        }

        private void TurnOnDialogueBack(Sprite sprite)
        {
            if (sprite != null)
            {
                _dialogueBack.sprite = sprite;
                _dialogueBack.color = Color.white;
            }
        }

        private void ShowDialogueNode(DialogueNodeBase node)
        {
            var displayer = node.GetNodeDisplayer();
            displayer.Display(this);
        }

        private void StartNode(DialogueNodeBase node)
        {
            AudioSystem.Instance.StopAllVoices();

            if (node == null)
            {
                CloseDialogue(Dialogue);
                return;
            }

            _skipButton.interactable = node.CanSkipNode();
            ClearButtons();

            ShowDialogueNode(node);
        }

        private void OnAnswerClick(AnswerBase answer)
        {
            answer.FinishAnswer();
            var objectName = Dialogue.DialogueNodesNew[Dialogue.CurrentNodeIndex].ObjectToTurnOnFinish;
            if (objectName != string.Empty)
            {
                var go = FindObjectsOfType<GameObject>(true).Where(n => n.name == objectName);
                if (go.Count() != 0)
                {
                    go.First().SetActive(true);
                }
            }

            OnEnableEventData actionData = answer.GetOnEnableEventData();
            if (actionData != null)
            {
                var instance = Instantiate(actionData.OnEnableEventPrefab, _eventsParent);
                if (instance.TryGetComponent<Collider2D>(out Collider2D cd))
                {
                    cd.enabled = false;
                }

                if (instance.TryGetComponent<Renderer>(out Renderer rd))
                {
                    rd.enabled = false;
                }

                instance.ActionSteps = actionData.Steps;
                _answerActions.Add(instance.gameObject);
                instance.gameObject.SetActive(true);
            }

            StartNode(Dialogue.GetNextNodeChecked(answer));
        }
    }
}