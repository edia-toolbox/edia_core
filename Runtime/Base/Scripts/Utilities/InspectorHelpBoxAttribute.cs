using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Attribute used to display a help box in the Unity inspector for the associated property.
/// This is a decorator attribute that does not affect the value or functionality of the property.
/// </summary>
/// <remarks>
/// The help box by default display a message with the Edia icon,
/// but this is overruled when using a specified message type, such as Info, Warning, or Error.
/// </remarks>
public class InspectorHelpBoxAttribute : PropertyAttribute
{
    public string message;
    
#if UNITY_EDITOR
    public MessageType type;

    public InspectorHelpBoxAttribute(string message, MessageType type = MessageType.None)
    {
        this.message = message;
        this.type    = type;
    }
#else
    public InspectorHelpBoxAttribute(string message)
    {
        this.message = message;
    }
#endif
}