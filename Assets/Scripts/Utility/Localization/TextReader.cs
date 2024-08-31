using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class TextReader
{
    // TODO: Assets方法大概要废弃
    public enum ReadTextFrom { Resources_Texts, Assets_Texts };
    public delegate Dictionary<string, MultiLanguageString> TextParser(string content);
    private static readonly Dictionary<string, Dictionary<string, MultiLanguageString>> textValues = new();

    /// <summary>
    /// Get the MultiLanguageString through fileName and label.<br/>
    /// Notice that the text file should be written as Dictionary of (string, MultiLanguageString) in json format.<br/>
    /// It's adviced that putting files to Resources/Texts.
    /// </summary>
    public static MultiLanguageString Get(string fileName, string label, ReadTextFrom readFrom = ReadTextFrom.Resources_Texts)
    {
        // The File has been read, so directly use it.
        if (textValues.TryGetValue(fileName, out var dict))
        {
            if (dict.TryGetValue(label, out var ret))
            {
                return ret;
            }
            else
            {
                Debug.LogError("Trying to get text from a label not existing.");
                return new();
            }
        }
        // The file has not been read, then read it.
        else
        {
            Dictionary<string, MultiLanguageString> toRead;
            string texts = readFrom switch
            {
                ReadTextFrom.Assets_Texts => File.ReadAllText($"{Application.dataPath}/Texts/{fileName}.txt"),
                _ => MyResources.Load<TextAsset>($"Texts/{fileName}").text,
            };
            toRead = JsonConvert.DeserializeObject<Dictionary<string, MultiLanguageString>>(texts);
            textValues.Add(fileName, toRead);
            if (toRead.TryGetValue(label, out var ret))
            {
                return ret;
            }
            else
            {
                Debug.LogError("Trying to get text from a label not existing.");
                return new();
            }
        }
    }
    /// <summary>
    /// Use custom text parser to get the MultiLanguageString through fileName and label.<br/>
    /// It's adviced that putting files to Resources/Texts.
    /// </summary>
    public static MultiLanguageString Get(string fileName, string label, TextParser textParser, ReadTextFrom readFrom = ReadTextFrom.Resources_Texts)
    {
        // The File has been read, so directly use it.
        if (textValues.TryGetValue(fileName, out var dict))
        {
            if (dict.TryGetValue(label, out var ret))
            {
                return ret;
            }
            else
            {
                Debug.LogError("Trying to get text from a label not existing.");
                return new();
            }
        }
        // The file has not been read, then read it.
        else
        {
            Dictionary<string, MultiLanguageString> toRead;
            string texts = readFrom switch
            {
                ReadTextFrom.Assets_Texts => File.ReadAllText($"{Application.dataPath}/Texts/{fileName}.txt"),
                _ => MyResources.Load<TextAsset>($"Texts/{fileName}").text,
            };
            toRead = textParser(texts);
            textValues.Add(fileName, toRead);
            if (toRead.TryGetValue(label, out var ret))
            {
                return ret;
            }
            else
            {
                Debug.LogError("Trying to get text from a label not existing.");
                return new();
            }
        }
    }

}