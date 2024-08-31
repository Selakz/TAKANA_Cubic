public class EditingJudgeLine : EditingComponent
{
    // Serializable and Public
    public JudgeLine JudgeLine => Component as JudgeLine;
    public override SelectTarget Type => SelectTarget.Others;

    // Private

    // Static

    // Defined Functions
    public EditingJudgeLine(JudgeLine judgeLine) : base(judgeLine) { }

    public override bool Instantiate()
    {
        return Component.Instantiate();
    }

    public override void Select()
    {
        throw new System.NotImplementedException();
    }

    public override void Unselect()
    {
        throw new System.NotImplementedException();
    }
}
