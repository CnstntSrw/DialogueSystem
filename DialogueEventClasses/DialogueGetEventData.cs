using UnityEngine;

namespace DialogueSystem
{
    public class DialogueGetEventData : IDialogueEventData
    {
        public InventoryObjectData GetEventInventoryObjectData { get; private set; }
        public Sprite GetEventSprite { get; private set; }

        public DialogueController.TutorialType TutorialType { get; } = DialogueController.TutorialType.GetEventTutorial;

        public DialogueGetEventData(InventoryObjectData getEventInventoryObjectData, Sprite getEventSprite)
        {
            GetEventInventoryObjectData = getEventInventoryObjectData;
            GetEventSprite = getEventSprite;
        }

        public void Display(DialogueController controller, bool needTutorial) //mb need tutorial
        {
            controller.CreateNPCGetEvent(GetEventInventoryObjectData, GetEventSprite,needTutorial, TutorialType); //if need tutorial - create it too
        }

    }
}
