using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading;
using System.Linq;

public class Loom : MonoBehaviour
{
    public static int maxThreads = 8;
    static int numThreads;

    private static Loom _current;
    public static Loom Current
    {
        get
        {
            return _current;
        }
    }

    void Awake()
    {
        _current = this;
    }

    private List<Action> _actions = new List<Action>();
    public struct DelayedQueueItem
    {
        public float time;
        public Action action;
    }
    private List<DelayedQueueItem> _delayed = new List<DelayedQueueItem>();

    List<DelayedQueueItem> _currentDelayed = new List<DelayedQueueItem>();

    public static void QueueOnMainThread(Action action)
    {
        QueueOnMainThread(action, 0f);
    }
    public static void QueueOnMainThread(Action action, float time)
    {
        if (time != 0)
        {
            lock (Current._delayed)
            {
                Current._delayed.Add(new DelayedQueueItem { time = Time.time + time, action = action });
            }
        }
        else
        {
            lock (Current._actions)
            {
                Current._actions.Add(action);
            }
        }
    }

    public static Thread RunAsync(Action a)
    {
        while (numThreads >= maxThreads)
        {
            Thread.Sleep(1);
        }
        Interlocked.Increment(ref numThreads);
        ThreadPool.QueueUserWorkItem(RunAction, a);
        return null;
    }

    private static void RunAction(object action)
    {
        try
        {
            ((Action)action)();
        }
        catch
        {
        }
        finally
        {
            Interlocked.Decrement(ref numThreads);
        }

    }

    List<Action> _currentActions = new List<Action>();

    // Update is called once per frame
    void Update()
    {
        lock (_actions)
        {
            if (_currentActions.Count > 0)
            {
                _currentActions.Clear();
            }
            
            if (_actions.Count > 0)
            {
                _currentActions.AddRange(_actions);
                _actions.Clear();
            }
        }
        foreach (var a in _currentActions)
        {
            a();
        }
        lock (_delayed)
        {
            if (_currentDelayed.Count > 0)
            {
                _currentDelayed.Clear();
            }

            if (_delayed.Count > 0)
            {
                _currentDelayed.AddRange(_delayed.Where(d => d.time <= Time.time));
                foreach (var item in _currentDelayed)
                    _delayed.Remove(item);
            }
        }
        foreach (var delayed in _currentDelayed)
        {
            delayed.action();
        }
    }
}