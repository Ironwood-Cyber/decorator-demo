using System.Linq;
using Newtonsoft.Json.Linq;

namespace GatewayService;

public static class JsonUtils
{
    public static JObject SortJsonById(this JObject target)
    {
        foreach (var property in target.Properties())
        {
            if (property.Value is JArray array)
            {
                // Sort the array by the $id property if it exists (otherwise, sort by int.MaxValue) and recursively sort the array elements
                var sortedArray = new JArray(array.Children<JObject>().OrderBy(o => o["$id"]?.Value<int>() ?? int.MaxValue).Select(o => o.SortJsonById()));
                property.Value = sortedArray;
            }
            else if (property.Value is JObject childObject)
            {
                childObject.SortJsonById();
            }
        }

        return target;
    }

    public static JObject MergeJsonObjects(JObject target, JObject source)
    {
        if (target == null || !target.HasValues)
            return source?.DeepClone() as JObject ?? [];

        if (source == null || !source.HasValues)
            return target;

        foreach (var property in source.Properties())
        {
            target[property.Name] = property.Value switch
            {
                JObject sourceObj when target[property.Name] is JObject targetObj => MergeJsonObjects(targetObj, sourceObj),
                JArray sourceArray when target[property.Name] is JArray targetArray => MergeElements(targetArray, sourceArray),
                _ => property.Value.DeepClone(),
            };
        }

        return target;
    }

    private static JArray MergeElements(JArray target, JArray source)
    {
        foreach (var sourceElement in source.OfType<JObject>())
        {
            var type = sourceElement["type"]?.ToString();
            var match = type switch
            {
                "Group" => target.FirstOrDefault(e => e["type"]?.ToString() == "Group" && e["label"]?.ToString() == sourceElement["label"]?.ToString()) as JObject,
                "Control" => target.FirstOrDefault(e => e["type"]?.ToString() == "Control" && e["scope"]?.ToString() == sourceElement["scope"]?.ToString()) as JObject,
                _ => null,
            };

            if (match != null)
                MergeJsonObjects(match, sourceElement);
            else
                target.Add(sourceElement.DeepClone());
        }

        return target;
    }
}
