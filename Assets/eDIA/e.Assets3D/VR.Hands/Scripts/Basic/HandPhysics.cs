using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class HandPhysics : MonoBehaviour
{
    public float smoothingAmount = 15f;
    public Transform target = null;

    private Rigidbody rigidBody = null;
    private Vector3 targetPosition = Vector3.zero;
    private Quaternion targetRotation = Quaternion.identity;

    private void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        TeleportToTarget();
    }

    private void Update()
    {
        SetTargetPosition();
        SetTargetRotation();
    }

    private void SetTargetPosition()
    {
        float time =  smoothingAmount * Time.unscaledDeltaTime; // unscaled makes sure it keeps moving even when game is paused
        targetPosition = Vector3.Lerp(targetPosition, target.position, time);
    }

    private void SetTargetRotation()
    {
        float time =  smoothingAmount * Time.unscaledDeltaTime; // unscaled makes sure it keeps moving even when game is paused
        targetRotation = Quaternion.Slerp(targetRotation, target.rotation, time);
    }

    private void FixedUpdate()
    {
        MoveToController();
        RotateToController();
    }

    private void MoveToController()
    {
        Vector3 positionDelta = targetPosition - transform.position;
        rigidBody.velocity = Vector3.zero;
        rigidBody.MovePosition(transform.position + positionDelta);
    }

    private void RotateToController()
    {
        rigidBody.angularVelocity = Vector3.zero;
        rigidBody.MoveRotation(targetRotation);
    }

    public void TeleportToTarget()
    {
        targetPosition = target.position;
        targetRotation = target.rotation;

        transform.position = targetPosition;
        transform.rotation = targetRotation;

    }
}
