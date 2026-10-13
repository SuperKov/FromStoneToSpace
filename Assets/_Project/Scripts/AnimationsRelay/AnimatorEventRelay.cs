using UnityEngine;
using System;

public class AnimatorEventRelay : MonoBehaviour
{
    public event Action<string> EventExecuted;

    // Этот метод мы выбираем в окне Animation на нужном кадре удара
    public void ExecuteEvent(string eventId)
    {
        EventExecuted?.Invoke(eventId);
    }
}