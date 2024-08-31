using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeselectInputFieldGet : MonoBehaviour
{
    // Serializable and Public
    TMP_InputField inputField;
    // Private

    // Static
    public static TMP_InputField DeselectedInputField { get; private set; } = null;

    // Defined Functions
    public static void Test(string useless)
    {
        Debug.Log("end edit, text: " + useless);
    }

    public IEnumerator StoreInputFieldOneFrame()
    {
        DeselectedInputField = inputField;
        Debug.Log("inputfield set");
        yield return new WaitForSeconds(0.1f);
        DeselectedInputField = null;
        Debug.Log("inputfield clear");
    }

    private void BeginStore(string useless)
    {
        StartCoroutine(StoreInputFieldOneFrame());
    }

    // System Functions
    void Start()
    {
        inputField = GetComponent<TMP_InputField>();
        inputField.onDeselect.AddListener(BeginStore);
    }
}
