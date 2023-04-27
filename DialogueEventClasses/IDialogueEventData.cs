namespace DialogueSystem
{
    public interface IDialogueEventData
    {
        void Display(DialogueController controller, bool needTutorial = false);
        DialogueController.TutorialType TutorialType { get;}
    }
}