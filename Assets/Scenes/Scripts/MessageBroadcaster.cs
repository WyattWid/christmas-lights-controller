using System;
using UnityEngine;

public class MessageBroadcaster : MonoBehaviour
{
    public string broadcast;
    public KeyCode key = KeyCode.None;

    void Awake()
    {
        if (GetComponent<Collider2D>() == null)
        {
            gameObject.AddComponent<BoxCollider2D>();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(key))
        {
            OnKeyPressed(key);
        }
    }

    void OnMouseDown()
    {
        EventManager.BroadcastMessage(broadcast);
    }

    void OnKeyPressed(KeyCode keyPressed)
    {
        if (keyPressed == key)
        {
            EventManager.BroadcastMessage(broadcast);

        }
    }
}
