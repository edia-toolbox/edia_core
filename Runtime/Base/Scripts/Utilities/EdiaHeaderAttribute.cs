using UnityEngine;

[System.AttributeUsage(System.AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public class EdiaHeaderAttribute : System.Attribute
{
    public string Title       { get; }
    public string Subtitle    { get; }
    public string Description { get; }

    public EdiaHeaderAttribute(string title, string subtitle = "", string description = "")
    {
        Title       = title;
        Subtitle    = subtitle;
        Description = description;
    }
}