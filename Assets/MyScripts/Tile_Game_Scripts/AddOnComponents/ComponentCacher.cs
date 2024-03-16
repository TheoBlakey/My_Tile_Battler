using System;
using System.Collections.Generic;
using UnityEngine;

public class ComponentCacher : MonoBehaviour
{
    readonly Dictionary<string, object> ComponentDictionary = new();

    public T CreateOrGetComponent<T>(string childObjectName = null) where T : Component
    {
        GameObject gameObj = gameObject;
        string componentName = typeof(T).Name;
        if (childObjectName != null)
        {
            gameObj = transform.Find("Text").gameObject;
            componentName = gameObj.name + componentName;
        }

        if (ComponentDictionary.TryGetValue(componentName, out var component))
        {
            return (T)component;

        }

        Component newOrFoundComponent;
        if (gameObj.TryGetComponent<T>(out var existingComponent))
        {
            newOrFoundComponent = existingComponent;
        }
        else
        {
            Type componentType = typeof(T);
            newOrFoundComponent = gameObj.AddComponent(componentType);
        }

        ComponentDictionary.Add(componentName, newOrFoundComponent);

        return (T)newOrFoundComponent;
    }




    //public T GetOrCacheComponent<T>(string componentName)
    //{
    //    if (ComponentDictionary.TryGetValue(componentName, out var component))
    //    {
    //        var thing = Convert.ChangeType(component, Type.GetType(componentName));
    //        return (T)component;

    //    }
    //    Type componentType = Type.GetType(componentName);
    //    gameObject.AddComponent(componentType);
    //    ComponentDictionary.Add(componentName, component);

    //    if (ComponentDictionary.TryGetValue(componentName, out var newComponent))
    //    {
    //        return (T)newComponent;
    //    }

    //    return default;
    //}


    //public UnityEngine.Object GetOrCacheComponent(string componentName)
    //{
    //    if (ComponentDictionary.TryGetValue(componentName, out var component))
    //    {
    //        var thing = Convert.ChangeType(component, Type.GetType(componentName));
    //        return (UnityEngine.Object)component;

    //    }
    //    Type componentType = Type.GetType(componentName);
    //    gameObject.AddComponent(componentType);
    //    ComponentDictionary.Add(componentName, component);

    //    if (ComponentDictionary.TryGetValue(componentName, out var newComponent))
    //    {
    //        return (UnityEngine.Object)newComponent;
    //    }

    //    return default;
    //}

    //public T ConvertObject<T>(object input)
    //{
    //    return (T)Convert.ChangeType(input, typeof(T));
    //}

}