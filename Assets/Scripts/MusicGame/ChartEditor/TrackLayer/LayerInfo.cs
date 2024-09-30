using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

public class LayerInfo
{
    // Serializable and Public
    public List<string> LayerNames { get; set; } = new();
    public List<int> TrackBelongings { get; set; } = new();

    // Private

    // Static

    // Defined Functions
    public LayerInfo() { }
    public LayerInfo(List<string> layerNames, List<int> trackBelongings)
    {
        LayerNames = new(layerNames);
        TrackBelongings = new(trackBelongings);
    }

    public static LayerInfo Load(string path)
    {
        return JsonConvert.DeserializeObject<LayerInfo>(File.ReadAllText(path));
    }

    public void Save(string path)
    {
        File.WriteAllText(path, JsonConvert.SerializeObject(this, Formatting.Indented));
    }

    /// <summary>
    /// 返回完整的带换行的双行图层语句
    /// </summary>
    public override string ToString()
    {
        StringBuilder sb = new();
        if (LayerNames.Count > 0)
        {
            sb.Append("layername(");
            for (int i = 0; i < LayerNames.Count - 1; i++)
            {
                sb.Append($"{i + 3}, {LayerNames[i]}, ");
            }
            sb.AppendLine($"{LayerNames.Count + 2}, {LayerNames[^1]});");
        }
        if (TrackBelongings.Count > 0)
        {
            sb.Append("layerorder(");
            for (int i = 0; i < TrackBelongings.Count - 1; i++)
            {
                sb.Append($"{TrackBelongings[i]}, ");
            }
            sb.AppendLine($"{TrackBelongings[^1]});");
        }
        return sb.ToString();
    }
}
