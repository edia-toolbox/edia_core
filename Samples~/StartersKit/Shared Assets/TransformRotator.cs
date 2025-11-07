using UnityEngine;

public class TransformRotator : MonoBehaviour
{
    [SerializeField]
    private float rotationSpeed = 50f;

    [SerializeField]
    private Vector3 rotationDirection = Vector3.up;

    private void Update()
    {
        transform.Rotate(rotationDirection * rotationSpeed * Time.deltaTime);
    }
}
