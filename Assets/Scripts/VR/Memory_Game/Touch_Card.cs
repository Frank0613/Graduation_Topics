using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Touch_Card : MonoBehaviour
{
    public string card_Tag;
    public bool isRotating { get; private set; }

    public void Rotate(float angle, float duration)
    {
        if (!isRotating)
        {
            StartCoroutine(RotateRoutine(angle, duration));
        }
    }

    private IEnumerator RotateRoutine(float angle, float duration)
    {
        isRotating = true;
        Quaternion startRotation = transform.rotation;
        Quaternion endRotation = startRotation * Quaternion.Euler(0, angle, 0);
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            transform.rotation = Quaternion.Lerp(startRotation, endRotation, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.rotation = endRotation;
        isRotating = false;
    }
}
