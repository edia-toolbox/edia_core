using System.Collections.Generic;
using UnityEngine;

namespace Tobii.XR.Examples.Social
{
    public class MirroredAvatar : MonoBehaviour
    {
#pragma warning disable 649 // Field is never assigned
        [Header("Head")] [SerializeField] private Transform originalHead;
        [SerializeField] private Transform mirroredHead;

        [Header("Eyes")] [SerializeField] private Transform originalLeftEye;
        [SerializeField] private Transform mirroredLeftEye;
        [SerializeField] private Transform originalRightEye;
        [SerializeField] private Transform mirroredRightEye;

        [Header("Face")] [SerializeField] private SkinnedMeshRenderer originalFace;
        [SerializeField] private SkinnedMeshRenderer mirroredFace;
#pragma warning restore 649

        private List<int> _blendShapeIndexes = new List<int>() {17, 18, 21, 22, 23, 61, 62};

        private void LateUpdate()
        {
            // Mirror the head.
            mirroredHead.localPosition = originalHead.localPosition;
            mirroredHead.localRotation = originalHead.localRotation;

            // Mirror the eyes.
            mirroredLeftEye.localRotation = originalLeftEye.localRotation;
            mirroredRightEye.localRotation = originalRightEye.localRotation;

            // Mirror the face blend shapes.
            for (int i = 0; i < _blendShapeIndexes.Count; i++)
            {
                int blendShapeIndex = _blendShapeIndexes[i];
                float blendshapeWeight = originalFace.GetBlendShapeWeight(blendShapeIndex);
                mirroredFace.SetBlendShapeWeight(blendShapeIndex, blendshapeWeight);
            }
        }
    }
}
