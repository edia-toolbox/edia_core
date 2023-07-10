using UnityEngine;

public interface IExampleInterface
{
    // Method to move an object in a specified direction
    void Move(Vector3 direction);

    // Method to rotate an object by a specified angle
    void Rotate(float angle);

    // Method to scale an object by a specified factor
    void Scale(Vector3 factor);

    // Method to change the color of an object
    void SetColor(Color color);

    // Method to play a sound
    void PlaySound(AudioClip clip);

    // Method to pause a sound
    void PauseSound();

    // Method to resume a paused sound
    void ResumeSound();

    // Method to stop a sound
    void StopSound();

    // Method to instantiate a prefab
    GameObject InstantiatePrefab(GameObject prefab, Vector3 position, Quaternion rotation);

    // Method to destroy an object
    void Destroy(GameObject gameObject);
}
