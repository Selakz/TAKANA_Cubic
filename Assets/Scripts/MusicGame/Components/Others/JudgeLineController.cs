using UnityEngine;

public class JudgeLineController : MonoBehaviour
{
    // Serializable and Public
    public JudgeLine Info => _judgeLine;

    [SerializeField] Transform sprite;
    [SerializeField] Transform leftEdge;
    [SerializeField] Transform rightEdge;

    // Private
    private JudgeLine _judgeLine;

    // Static

    // Defined Function
    public void InfoInit(JudgeLine judgeLine)
    {
        _judgeLine = judgeLine;
    }

    // System Function
    void Start()
    {
        sprite.SetPositionAndRotation(_judgeLine.Position, new(0, 0, _judgeLine.Rotation, 0));
        sprite.localScale = new(Camera.main.G2WPosX(9.0f), Camera.main.G2WPosY(0.3f));
        leftEdge.localPosition = new(Camera.main.G2WPosX(-4.5f), 0);
        leftEdge.localScale = new(Camera.main.G2WPosY(0.2f), Camera.main.G2WPosY(18f));
        rightEdge.localPosition = new(Camera.main.G2WPosX(4.5f), 0);
        rightEdge.localScale = new(Camera.main.G2WPosY(0.2f), Camera.main.G2WPosY(18f));
    }
}
