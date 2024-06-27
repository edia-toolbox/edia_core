using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Edia
{
    public class HandHider : MonoBehaviour
    {
        public GameObject handObject = null;

        private HandPhysics handPhysics = null;
        private XRDirectInteractor interactor = null;

        private void Awake()
        {
            handPhysics = handObject.GetComponent<HandPhysics>();
            interactor = GetComponent<XRDirectInteractor>();
        }

        private void OnEnable()
        {
            interactor.selectEntered.AddListener(Hide);
            interactor.selectExited.AddListener(Show);
        }

        private void OnDisable()
        {
            interactor.selectEntered.RemoveListener(Hide);
            interactor.selectExited.RemoveListener(Show);
        }

        private void Show(SelectExitEventArgs args)
        {
            handPhysics.TeleportToTarget();
            handObject.SetActive(true);
        }

        private void Hide(SelectEnterEventArgs args)
        {
            handObject.SetActive(false);
        }

    }
}