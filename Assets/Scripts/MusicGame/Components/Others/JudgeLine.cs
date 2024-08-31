using Takana3.Settings;
using UnityEngine;
using UnityEngine.Assertions;

/// <summary>
/// 作为BaseNote和Track的基准位置的判定线
/// </summary>
public class JudgeLine : IComponent
{
    // Implement IComponent
    public int Id { get; }

    public bool IsInitialized { get; private set; } = false;

    public GameObject ThisObject { get; private set; } = null;

    public float TimeInstantiate { get; internal set; } = float.NegativeInfinity;

    // Self Properties
    public float Rotation { get; private set; } = 0f;

    public Vector3 Position { get; private set; } = new(0, 0, -0.5f);

    [SerializeField] private GameObject linePrefab;
    private JudgeLineController judgeLineController;

    // Defined Functions
    public JudgeLine(int id) // 默认判定线
    {
        Id = id;
    }

    public JudgeLine(int id, float timeInstantiate, float rotation, Vector3 position)
    {
        Id = id;
        TimeInstantiate = timeInstantiate;
        Rotation = rotation;
        Position = position;
    }

    public void Initialize(MusicSetting setting)
    {
        // TODO: 根据两个deviation调整位置
        linePrefab = Get();
        PositionInit();
        IsInitialized = true;
    }

    public bool Instantiate()
    {
        Assert.IsTrue(IsInitialized);
        if (ThisObject != null) return true;
        ThisObject = Object.Instantiate(linePrefab);
        judgeLineController = ThisObject.GetComponent<JudgeLineController>();
        judgeLineController.InfoInit(this);
        return true;
    }

    public void PositionInit()
    {
        linePrefab.transform.SetPositionAndRotation(Position, new(0, 0, Rotation, 0));
    }

    public GameObject Get()
    {
        string path = "Prefabs/JudgeLine";
        return MyResources.Load<GameObject>(path);
    }
}
