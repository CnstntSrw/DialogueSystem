namespace DialogueSystem
{
    public interface IEventsDisplayer
    {
        public void Display(IDialogueEventData eventData, DialogueController dialogueController)
        {
            DisplayEvent(eventData, dialogueController);
            DisplayInventory(dialogueController);
        }

        void DisplayInventory(DialogueController dialogueController);
        void DisplayEvent(IDialogueEventData data, DialogueController dialogueController);
        void HideDialogPanel(DialogueController dialogueController);
    }
}