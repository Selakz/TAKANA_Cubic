using UnityEngine;

/// <summary>
/// A packaged animator used in other components that directly instantiates an animation object.<para/>
/// All called animation object(prefab) should be in folder "Resources".<para/>
/// Most suitable for simple animation which has only linear transition. <para/> 
/// </summary>
public class PackagedAnimator
{
    // Serializable and Public
    public bool IsOver => !animObject;

    // Private
    private GameObject animObject;
    private Animator animator = null;
    private readonly Transform parent = null;
    private readonly bool worldPositionStays = false;

    // Static

    // Defined Function
    public PackagedAnimator(string animationObject, string inPath = "Prefabs/AnimPrefabs/")
    {
        animObject = (GameObject)Resources.Load(inPath + animationObject);
    }
    public PackagedAnimator(string animationObject, Transform parent, bool worldPositionStays = false, string inPath = "Prefabs/AnimPrefabs/")
    {
        animObject = (GameObject)Resources.Load(inPath + animationObject);
        this.parent = parent;
        this.worldPositionStays = worldPositionStays;
    }

    public void Play()
    {
        animObject = Object.Instantiate(animObject);
        if (parent) animObject.transform.SetParent(parent, worldPositionStays);
        animator = animObject.GetComponent<Animator>();
    }

    public void SetParam(string paramName)
    {
        if (!animator) Debug.LogError("Set the animator's parameter before starts the animation.");
        animator.SetTrigger(paramName);
    }
    public void SetParam(string paramName, bool value)
    {
        if (!animator) Debug.LogError("Set the animator's parameter before starts the animation.");
        animator.SetBool(paramName, value);
    }
    public void SetParam(string paramName, int value)
    {
        if (!animator) Debug.LogError("Set the animator's parameter before starts the animation.");
        animator.SetInteger(paramName, value);
    }
    public void SetParam(string paramName, float value)
    {
        if (!animator) Debug.LogError("Set the animator's parameter before starts the animation.");
        animator.SetFloat(paramName, value);
    }
}
