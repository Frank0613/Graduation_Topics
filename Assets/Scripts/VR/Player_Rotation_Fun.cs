using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Rotation_Fun : MonoBehaviour
{
    [SerializeField] private float RotationDuration;
    [SerializeField] private float RotationAngle;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Player_Movement.instance.Rotation_Player(other.gameObject, RotationAngle);
        }
    }


}

