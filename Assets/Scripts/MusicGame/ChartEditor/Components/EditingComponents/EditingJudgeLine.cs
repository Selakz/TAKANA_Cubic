public class EditingJudgeLine : EditingComponent
{
    // Serializable and Public
    public JudgeLine JudgeLine => Component as JudgeLine;
    public override SelectTarget Type => SelectTarget.Others;
    public override bool IsSelected { get => false; set => throw new System.NotImplementedException(); }

    // Private

    // Static

    // Defined Functions
    public EditingJudgeLine(JudgeLine judgeLine) : base(judgeLine) { }

    public override bool Instantiate()
    {
        return Component.Instantiate();
    }
}
