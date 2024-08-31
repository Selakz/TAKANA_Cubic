using System;
using TMPro;

public class InputParser
{
    private TMP_InputField inputField;
    private Func<string, bool> parser;
    private string onSelectedText;

    private void RecordOnSelect(string useless)
    {
        onSelectedText = inputField.text;
    }

    private void TryReverse(string useless)
    {
        if (!parser(inputField.text))
        {
            inputField.text = onSelectedText;
        }
    }

    private InputParser(TMP_InputField inputField, Func<string, bool> parser)
    {
        this.inputField = inputField;
        this.parser = parser;
        inputField.onSelect.AddListener(RecordOnSelect);
        inputField.onEndEdit.AddListener(TryReverse);
    }

    /// <summary>
    /// ��Ҫ��ϣ��Parse�ɹ�ʱ���õķ�����Ƕ��parser��
    /// </summary>
    public static void StartMonitoring(TMP_InputField inputField, Func<string, bool> parser)
    {
        new InputParser(inputField, parser);
    }
}