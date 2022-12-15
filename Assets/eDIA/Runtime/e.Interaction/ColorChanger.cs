using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ColorChangert : MonoBehaviour
{
    public Material greymat = null;
    public Material pinkMat = null;

    MeshRenderer meshRenderer = null;
    XRGrabInteractable grabInteractable = null;

    void Awake() {
        meshRenderer = GetComponent<MeshRenderer>();
        grabInteractable = GetComponent<XRGrabInteractable>();

        grabInteractable.onActivate.AddListener(SetPink);
        grabInteractable.onDeactivate.AddListener(SetGrey);
        // grabInteractable.onFirstHoverEntered.AddListener(SetPink);
        // grabInteractable.onLastHoverExited.AddListener(SetGrey);
    }

    private void OnDestroy() {
        grabInteractable.onActivate.RemoveListener(SetPink);
        grabInteractable.onDeactivate.RemoveListener(SetGrey);
        // grabInteractable.onFirstHoverEntered.RemoveListener(SetPink);
        // grabInteractable.onLastHoverExited.RemoveListener(SetGrey);
    }

    public void SetGrey(XRBaseInteractor interactor)
    {
        meshRenderer.material = greymat;
    }

    public void SetPink(XRBaseInteractor interactor) 
    {
        Debug.Log("SetPink");
        meshRenderer.material = pinkMat;
    }

}
