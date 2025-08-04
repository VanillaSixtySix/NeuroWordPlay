using UnityEngine;

public static class GameObjectHelper
{
    public static GameObject GetOrCreate(string name)
    {
        return GameObject.Find(name) ?? new GameObject(name);
    }
}
