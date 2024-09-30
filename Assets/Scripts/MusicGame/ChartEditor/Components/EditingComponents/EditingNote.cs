using UnityEngine;

public abstract class EditingNote : EditingComponent
{
    // Serializable and Public
    public BaseNote Note => Component as BaseNote;

    public override SelectTarget Type => SelectTarget.Note;

    public override bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected == value) return;
            if (Note.Controller != null)
            {
                Note.Controller.IsHighlight = value;
            }
            _isSelected = value;
        }
    }

    // Private
    private bool _isSelected = false;

    // Static

    // Defined Functions
    public EditingNote(BaseNote baseNote) : base(baseNote) { }

    public override abstract bool Instantiate();
}
