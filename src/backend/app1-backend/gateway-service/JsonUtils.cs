using System.Linq;
using Newtonsoft.Json.Linq;

namespace GatewayService;

public static class JsonUtils
{
    /// <summary>
    /// Sorts a JSON object by the $id property if it exists (otherwise, sort by int.MaxValue)
    /// </summary>
    /// <param name="target">The JObject to sort</param>
    /// <returns>The sorted JObject by the $id property</returns>
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

    /// <summary>
    /// Merges two JSON objects recursively by combining the properties of the source object into the target object
    /// </summary>
    /// <param name="target">The target JObject to merge into</param>
    /// <param name="source">The source JObject to merge from</param>
    /// <returns>The merged JObject</returns>
    public static JObject MergeJsonObjects(JObject target, JObject source)
    {
        if (target == null || !target.HasValues)
            return source?.DeepClone() as JObject ?? [];

        if (source == null || !source.HasValues)
            return target;

        foreach (var property in source.Properties())
        {
            // Merge the property value into the target object
            target[property.Name] = property.Value switch
            {
                JObject sourceObj when target[property.Name] is JObject targetObj => MergeJsonObjects(targetObj, sourceObj),
                JArray sourceArray when target[property.Name] is JArray targetArray => MergeElements(targetArray, sourceArray),
                _ => property.Value.DeepClone(),
            };
        }

        return target;
    }

    /// <summary>
    /// Merges two JSON arrays by combining the elements of the source array into the target array
    /// </summary>
    /// <param name="target">The target JArray to merge into</param>
    /// <param name="source">The source JArray to merge from</param>
    /// <returns>The merged JArray</returns>
    private static JArray MergeElements(JArray target, JArray source)
    {
        foreach (var sourceElement in source.OfType<JObject>())
        {
            var type = sourceElement["type"]?.ToString();
            // Find the matching element in the target array by type and label/scope
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
