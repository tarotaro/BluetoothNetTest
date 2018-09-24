using System;
using UnityEngine;
using System.Collections;

public static class JsonHelper
{
    public static T[] FromJson<T>(string json)
    {
        Wrapper<T> wrapper = UnityEngine.JsonUtility.FromJson<Wrapper<T>>(json);
        return wrapper.devices;
    }

    [Serializable]
    private class Wrapper<T>
    {
        public T[] devices;
    }
}