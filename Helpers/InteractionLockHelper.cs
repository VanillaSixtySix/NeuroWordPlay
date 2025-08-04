using System;
using System.Collections.Generic;

public static class InteractionLockHelper
{
    public static bool IsLocked { get; set; }

    private static Queue<Action> _queue = new();

    public static void AddToQueue(Action action)
    {
        if (!IsLocked)
        {
            action();
            return;
        }

        _queue.Enqueue(action);
    }

    public static void ProcessQueue()
    {
        while (_queue.Count > 0)
        {
            var action = _queue.Dequeue();
            action();
        }
    }
}
