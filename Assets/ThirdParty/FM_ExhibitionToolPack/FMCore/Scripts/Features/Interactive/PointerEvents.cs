using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;// Required when using Event data.
using UnityEngine.Events;

[System.Serializable] public class _UnityEventFloat : UnityEvent<float> { }
public class PointerEvents : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Image mask;

    public bool isPressed = false;


    public Color ColorOnNormal = new Color(1f,1f,1f,1f);
    public Color ColorOnPress = new Color(1f, 1f, 1f, 1f);
    [Header("On Press Event")]
    public UnityEvent OnPressEvent;

    [Header("On Press Enter Event")]
    public UnityEvent OnPressEnterEvent;
    [Header("On Press Exit Event")]
    public UnityEvent OnPressExitEvent;

    // Start is called before the first frame update
    void Start()
    {
        ColorOnNormal = mask.color;
    }

    // Update is called once per frame
    void Update()
    {
        if (isPressed)
        {
            OnPressEvent.Invoke();
            mask.color = ColorOnPress;
        }
        else
        {
            mask.color = ColorOnNormal;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        //Debug.Log(this.gameObject.name + " Was Clicked.");
        isPressed = true;
        OnPressEnterEvent.Invoke();

    }
    public void OnPointerUp(PointerEventData eventData)
    {
        //Debug.Log(this.gameObject.name + " Up");
        isPressed = false;
        OnPressExitEvent.Invoke();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        //Debug.Log(this.gameObject.name + " Enter");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        //Debug.Log(this.gameObject.name + " Exit");
    }
}
