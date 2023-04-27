namespace DialogueSystem
{
    public class NodeWithAnswerDisplayer : NodeDisplayer, IButtonsDisplayer
    {
        private DialogueNodeData Data { get;}

        public NodeWithAnswerDisplayer(DialogueNodeData data)
        {
            Data = data;
        }

        public override void Display(DialogueController dialogueController)
        {
            dialogueController.ShowDialogPanel();
            ShowCharacter(Data, dialogueController);
            ShowButtons(dialogueController);
            if (Data.ActivateTutorial)
            {
                dialogueController.ActivateTutorial(DialogueController.TutorialType.AnswersTutorial);
            }
        }

        public void ShowButtons(DialogueController controller)
        {
            controller.SetAnswerButtons(Data.Answers);
            controller.TurnOnInventoryIfNeed(false);
        }
    }
}