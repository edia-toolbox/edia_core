using UnityEngine;
using UnityEngine.UI;

public class EButton : MonoBehaviour {
    public ColorBlock colorBlock;

    [ContextMenu("Set Colors")]
    public void SetColors() {
        this.GetComponent<Button>().colors = colorBlock;
    }
}