#region

using System.Collections.Generic;

#endregion

namespace DialogueSystem
{
    public class NodeWithEventDisplayer : NodeDisplayer, IEventsDisplayer
    {
        public override void Display(DialogueController dialogueController)
        {
            dialogueController.HideDialogPanel();
            ShowCharacter(Data, dialogueController, false, () => { DisplayEvent(Data.EventData, dialogueController); });
            DisplayInventory(dialogueController);
        }

        public DialogueNodeData Data { get; private set; }

        public NodeWithEventDisplayer(DialogueNodeData data)
        {
            Data = data;
        }

        public void DisplayEvent(IDialogueEventData data, DialogueController dialogueController)
        {
            data.Display(dialogueController, needTutorial: Data.ActivateTutorial);
        }

        public void DisplayInventory(DialogueController dialogueController)
        {
            InterfaceController.Instance.ShowPanelWithButtons("inventoryPanel", new List<string>() { "hint" });
        }

        public void HideDialogPanel(DialogueController dialogueController)
        {
            dialogueController.HideDialogPanel();
        }
    }
}