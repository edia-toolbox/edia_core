using UnityEngine;
using System.Collections;

public class TargetProjectionMatrix : MonoBehaviour
{
    public Camera referenceCam;
    public Camera targetCam;

    public bool useCustomFOV = false;
    public float fov = 60f;
    [Header("limit the maximum fov")]
    public bool maxFovAsReference = false;
    public void Action_UpdateFOV(float _fov)
    {
        fov = _fov;
    }

    public bool allowUpdate = true;
    public void Action_SetAllowUpdate(bool _value)
    {
        allowUpdate = _value;
    }

    public bool ForceDisableUpdate = false;
    public void Action_SetForceDisableUpdate(bool _value)
    {
        ForceDisableUpdate = _value;
    }

    IEnumerator UpdateProjectionMatrixLoop()
    {
        while (true)
        {
            while (!allowUpdate || ForceDisableUpdate)
            {
                yield return new WaitForEndOfFrame();
            }
            Matrix4x4 rm = referenceCam.projectionMatrix;
            if (!useCustomFOV)
            {
                fov = referenceCam.fieldOfView;
            }

            if (maxFovAsReference)
            {
                if (fov > referenceCam.fieldOfView)
                {
                    fov = referenceCam.fieldOfView;
                }
            }

            float aspect = referenceCam.aspect;

            float matrixY = 1f / Mathf.Tan(fov / (2f * Mathf.Rad2Deg));
            float matrixX = matrixY / aspect; // as matrixY IS the calculated fov ratio

            rm[0, 0] = matrixX;
            rm[1, 1] = matrixY;

            targetCam.fieldOfView = fov;
            targetCam.projectionMatrix = rm;

            yield return new WaitForSeconds(0.005f);
        }
    }

    private void Start()
    {
        StartCoroutine(UpdateProjectionMatrixLoop());
    }


}