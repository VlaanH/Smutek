using UnityEngine;
using UnityEngine.UI;

public class InteractionUIController : MonoBehaviour
{
    public static InteractionUIController Instance;

    [Header("UI Elements")]
    public GameObject uiPanel;
    public Button firstButton;
    public Button secondButton;
    public Text firstButtonText;
    public Text secondButtonText;

    void Awake()
    {
        Instance = this;
        uiPanel.SetActive(false);
    }

    public void ShowUI(InteractableObject obj)
    {Instance.
        
        uiPanel.SetActive(true);

        switch (obj.objectType)
        {
            case InteractableObject.ObjectType.Drink:
                firstButtonText.text = "Пить";
                secondButtonText.text = "Не пить";
                break;
            case InteractableObject.ObjectType.Food:
                firstButtonText.text = "Съесть";
                secondButtonText.text = "Не съесть";
                break;
        }

        firstButton.onClick.RemoveAllListeners();
        secondButton.onClick.RemoveAllListeners();

        firstButton.onClick.AddListener(() => obj.OnUseObject(true));
        secondButton.onClick.AddListener(() => obj.OnUseObject(false));
    }

    public void HideUI()
    {
        uiPanel.SetActive(false);
    }
}