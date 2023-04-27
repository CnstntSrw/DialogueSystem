namespace DialogueSystem
{
    public class DialogueUseEventData : IDialogueEventData
    {
        public DefaultInventoryObjectData UseEventInventoryObjectData { get; private set; }

        public DialogueController.TutorialType TutorialType { get; } = DialogueController.TutorialType.UseEventTutorial;
        public DialogueUseEventData(DefaultInventoryObjectData useEventInventoryObjectData)
        {
            UseEventInventoryObjectData = useEventInventoryObjectData;
        }
        public void Display(DialogueController controller, bool needTutorial = false)
        {
            controller.CreateNPCUseEvent(UseEventInventoryObjectData,needTutorial, TutorialType);
        }

    }
}
