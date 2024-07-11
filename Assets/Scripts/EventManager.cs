using System;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public static EventManager Instance { get; private set; }
    private event Action onBashingAction;

    public void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

    }

    public void OnBashing()
    {
        onBashingAction?.Invoke();
    }

    public void SubscribeToOnBasing(Action action)
    {
        onBashingAction += action;
    }

    public void UnsubscribeFromOnBashing(Action action)
    {
        onBashingAction -= action;
    }
}
