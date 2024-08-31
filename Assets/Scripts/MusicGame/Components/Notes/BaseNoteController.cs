using UnityEngine;

public abstract class BaseNoteController : MonoBehaviour
{
    // Serializable and Public
    public abstract BaseNote Info { get; }
    public abstract float GameWidth { get; protected set; }

    [SerializeField] protected SpriteRenderer sprite;
    [SerializeField] protected GameObject hitEffect;
    [SerializeField] protected BoxCollider2D boxCollider;

    // Defined Functions
    public abstract void InfoInit(BaseNote note, InputInfo inputInfo);
    public abstract void SpriteInit();
    public abstract void UpdatePos();
    public abstract bool HandleInput(float timeInput);
    public abstract void Highlight();
    public abstract void Dehighlight();
}
