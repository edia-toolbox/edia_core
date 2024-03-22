using UnityEngine;
using UnityEditor;

public class Notes : MonoBehaviour
{
    [TextArea (3, 20)]
    public string Note = "";

    // EditorGUILayout.SelectableLabel(text, EditorStyles.textArea, options);
}
