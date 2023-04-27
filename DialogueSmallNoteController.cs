#region

using System;
using DialogueSystem;
using UnityEngine;
using UnityEngine.EventSystems;

#endregion

public class DialogueSmallNoteController : SmallNoteController
{
    public event Action OnCLose;

    public override void OnPointerDown(PointerEventData eventData)
    {
        GameObject bigNote;
        DialogueBigNoteControlelr bigNoteController;
        GameObject interfaceBackScreen = DialogueManager.Instance.GetNotesBackScreen();

        bigNote = Instantiate(_bigNotePrefab, interfaceBackScreen.transform);
        bigNoteController = bigNote.GetComponent<DialogueBigNoteControlelr>();
        bigNoteController.Backscreen = interfaceBackScreen;

        bigNoteController.SmallNote = this.gameObject;
        bigNoteController.OpenNote();
        bigNoteController.OnCLose += () =>
        {
            OnCLose?.Invoke();
            gameObject.SetActive(false);
        };
    }
}