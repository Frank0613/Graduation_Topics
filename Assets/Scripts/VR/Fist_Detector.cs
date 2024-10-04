using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Fist_Detector : MonoBehaviour
{
    [SerializeField] private float cooldownTime = 1f;
    public Player_Movement Target_Player;


    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Rock" && Target_Player.Both_Hands_Fist())
        {
            Debug.Log("Move");
            Target_Player.ApplyForwardForce();
        }
    }
}
