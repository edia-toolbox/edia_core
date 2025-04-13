using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "UIButtonColors", menuName = "UI/Button Colors Palette")]
public class UIButtonColors : ScriptableObject
{
    public ColorBlock buttonColorBlock = ColorBlock.defaultColorBlock;
    
    public Sprite logoSprite = null;
    
    public Color ControllerBackgroundColor = Color.white;
    
    
#if UNITY_EDITOR
    private void OnValidate()
    {
        // notifying Unity that the asset has changed
        UnityEditor.EditorUtility.SetDirty(this);
        // tell Unity to update the scene/editor view immediately
        UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
    }
#endif

}