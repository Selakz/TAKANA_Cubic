using UnityEngine;

public class CreateTypeManager : MonoBehaviour
{
    // Serializable and Public
    [SerializeField] private CreateTypeUI createTap;
    [SerializeField] private CreateTypeUI createSlide;
    [SerializeField] private CreateTypeUI createHold;

    public static CreateTypeManager Instance { get; private set; }

    // Private

    // Static

    // Defined Functions
    public void Select(NoteType createType)
    {
        if (InScreenEditManager.Instance.CreateType == createType) return;
        InScreenEditManager.Instance.CreateType = createType;
        switch (createType)
        {
            case NoteType.Tap:
                createTap.IsSelected = true;
                createSlide.IsSelected = false;
                createHold.IsSelected = false;
                break;
            case NoteType.Slide:
                createTap.IsSelected = false;
                createSlide.IsSelected = true;
                createHold.IsSelected = false;
                break;
            case NoteType.Hold:
                createTap.IsSelected = false;
                createSlide.IsSelected = false;
                createHold.IsSelected = true;
                break;
        }
    }

    // System Functions
    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        (createTap as ICanEnableUI).Enable();
        (createSlide as ICanEnableUI).Enable();
        (createHold as ICanEnableUI).Enable();
    }

    void Update()
    {
        if (InputManager.Instance.IsHotkeyActionPressed(InputManager.Instance.InScreenEdit.SwitchCreateType))
        {
            switch (InScreenEditManager.Instance.CreateType)
            {
                case NoteType.Tap:
                    Select(NoteType.Slide);
                    break;
                case NoteType.Slide:
                    Select(NoteType.Hold);
                    break;
                case NoteType.Hold:
                    Select(NoteType.Tap);
                    break;
            }
        }
    }
}
