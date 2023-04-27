#region

using DialogueSystem;

#endregion

public class NodeWithNoteDisplayer : NodeDisplayer
{
    public NodeWithNoteDisplayer(DialogueNodeData dialogueNodeData)
    {
        Data = dialogueNodeData;
    }

    public DialogueNodeData Data { get; private set; }

    public override void Display(DialogueController dialogueController)
    {
        dialogueController.ShowDialogPanel();
        ShowCharacter(Data, dialogueController, false);
        DisplayNote(Data.NotePrefab, dialogueController);
    }

    private void DisplayNote(DialogueSmallNoteController dataNotePrefab, DialogueController dialogueController)
    {
        dialogueController.ShowNote(dataNotePrefab);
        dialogueController.TurnOnInventoryIfNeed(false);
    }
}