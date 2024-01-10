using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utility_Functions : MonoBehaviour
{

    IDictionary<string, IEnumerator> coroutinesDictionary = new Dictionary<string, IEnumerator>() { };
    public void ActivateTimerOnBool(float timeAmount, string boolName)
    {
        IEnumerator foundCoroutine;
        if (coroutinesDictionary.TryGetValue(boolName, out foundCoroutine))
        {
            StopCoroutine(foundCoroutine);
            coroutinesDictionary.Remove(boolName);
        }
        IEnumerator newCoroutine = TimerForABoolean(timeAmount, boolName);
        StartCoroutine(newCoroutine);
        coroutinesDictionary.Add(boolName, newCoroutine);
    }
    private IEnumerator TimerForABoolean(float waitTime, string booleanName)
    {
        var booleanProperty = this.GetType().GetProperty(booleanName);
        var booleanField = this.GetType().GetField(booleanName);

        if (booleanField != null)
        {
            booleanField.SetValue(this, true);
        }
        else
        {
            booleanProperty = this.GetType().GetProperty(booleanName);
            if (booleanProperty != null)
            {
                booleanProperty.SetValue(this, true);
            }
        }

        yield return new WaitForSeconds(waitTime);

        if (booleanField != null)
        {
            booleanField.SetValue(this, false);
        }
        else if (booleanProperty != null)
        {
            booleanProperty.SetValue(this, false);
        }
    }

    public void SetChildActive(bool ActiveStatus, string Name)
    {
        foreach (Transform child in this.transform)
        {
            if (child.gameObject.name == Name)
            {
                child.gameObject.SetActive(ActiveStatus);
            }
        }
    }

    public List<GameObject> GetChildrenByName(string Name)
    {
        List<GameObject> ObjectList = new List<GameObject>();
        foreach (Transform child in this.transform)
        {
            if (child.gameObject.name == Name)
            {
                ObjectList.Add(child.gameObject);
            }
        }
        return ObjectList;
    }

    public GameObject GetChildByName(string Name)
    {
        GameObject childObj = null;

        foreach (Transform child in this.transform)
        {
            if (child.gameObject.name == Name)
            {
                childObj = child.gameObject;
                break;
            };
        }
        return childObj;
    }

    public List<GameObject> FindAllObjectsOfLayer(string layer)
    {
        GameObject[] AllGameObjects = GameObject.FindObjectsOfType(typeof(GameObject)) as GameObject[]; //will return an array of all GameObjects in the scene
        List<GameObject> ObjectList = new List<GameObject>();
        foreach (GameObject go in AllGameObjects)
        {
            if (go.layer == LayerMask.NameToLayer(layer))
            {
                // Debug.Log(LayerMask.NameToLayer(layer));
                ObjectList.Add(go);
            }
        }
        return ObjectList;
    }

    public void CancelInvokeDifferentClass(string methodName)
    {
        CancelInvoke(methodName);
    }

    public void InvokeRepeatingDifferentClass(string methodName, float delay, float time)
    {
        InvokeRepeating(methodName, delay, time);
    }

    public static T ParseEnum<T>(string value)
    {
        return (T)Enum.Parse(typeof(T), value, true);
    }

    public virtual void SetLevel(string levelName)
    {
        Debug.Log("THIS SHOULD NOT RUN");
    }

}