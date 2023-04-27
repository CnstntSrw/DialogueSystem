namespace DialogueSystem
{
    public class NodeNoAnswersDisplayer : NodeDisplayer, IButtonsDisplayer
    {
        private DialogueNodeData Data { get;}

        public NodeNoAnswersDisplayer(DialogueNodeData data)
        {
            Data = data;
        }

        public override void Display(DialogueController dialogueController)
        {
            dialogueController.ShowDialogPanel();
            ShowButtons(dialogueController);
            ShowCharacter(Data, dialogueController);
        }

        public void ShowButtons(DialogueController controller)
        {
            controller.ShowClickHandler();
            //controller.SetContinueButton();
            controller.TurnOnInventoryIfNeed(false);
        }
    }
}