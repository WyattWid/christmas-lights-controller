using System;
using UnityEngine;

public class colormessage : MonoBehaviour
{
    // Define an event

    // Public variable to hold the color
    public Color selectedColor;

    // Ensure the GameObject has a Collider2D component
    void Awake()
    {
        if (GetComponent<Collider2D>() == null)
        {
            gameObject.AddComponent<BoxCollider2D>();
        }
    }

    // This method is called when the mouse button is pressed while over the Collider
    void OnMouseDown()
    {
        SendBroadcastMessage();
    }

    void SendBroadcastMessage()
    {
        string rgbMessage = $"RGB({(int)(selectedColor.r * 255)}, {(int)(selectedColor.g * 255)}, {(int)(selectedColor.b * 255)})";
        EventManager.BroadcastMessage(rgbMessage);

    }

    void OnEnable()
    {
        // Subscribe to the event
        EventManager.OnMessageBroadcast += HandleMessage;
    }

    void OnDisable()
    {
        // Unsubscribe from the event
        EventManager.OnMessageBroadcast -= HandleMessage;
    }

    void HandleMessage(string message)
    {
        // Handle the received message

        // Example of sending a response message
        // Respond based on the message received if needed
    }
}
