using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Player_Movement : MonoBehaviour
{
    public static Player_Movement instance;
    public bool isFist_L { get; private set; } = false;
    public bool isFist_R { get; private set; } = false;

    [Header("Hands")]
    [SerializeField] private GameObject leftHand;
    [SerializeField] private GameObject rightHand;

    [Header("Properties")]
    [SerializeField] bool TestMode;
    [SerializeField] private float forwardForce = 10f;
    [SerializeField] private float cooldownDuration = 1f;
    [SerializeField] private float velocityThreshold = 1f;
    [SerializeField] private float smoothingFactor = 0.1f;

    private Rigidbody playerRigidbody;
    private float lastForceTime = -Mathf.Infinity;
    private Vector3 previousLeftHandPos;
    private Vector3 previousRightHandPos;
    private Vector3 smoothedLeftHandVelocity;
    private Vector3 smoothedRightHandVelocity;

    private void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
        playerRigidbody = GetComponent<Rigidbody>();
        //When start, previous position equal to init position of both hands.
        previousLeftHandPos = leftHand.transform.position;
        previousRightHandPos = rightHand.transform.position;
    }

    void FixedUpdate()
    {
        TestModeFunction();
    }
    public void ApplyForwardForce()
    {
        if (Time.time - lastForceTime >= cooldownDuration)
        {
            playerRigidbody.AddForce(transform.forward * forwardForce, ForceMode.Impulse);
            lastForceTime = Time.time;
        }
    }
    public void Rotation_Player(GameObject target, float rotation, float triggerDelay, bool keepMove)
    {
        StartCoroutine(SmoothRotatePlayer(target, rotation, triggerDelay, keepMove));
    }

    IEnumerator SmoothRotatePlayer(GameObject target, float rotationAngle, float triggerDelay, bool keepMove)
    {
        yield return new WaitForSeconds(triggerDelay);
        GameStateManager.Instance.player_Status = GameStateManager.Player_Status.PlayerRotating;
        Quaternion startRotation = target.transform.rotation;
        Quaternion targetRotation = startRotation * Quaternion.Euler(0, rotationAngle, 0); // 基於當前旋轉，增加相對旋轉
        float elapsedTime = 0f;
        while (elapsedTime < 8)
        {

            target.transform.rotation = Quaternion.Slerp(startRotation, targetRotation, elapsedTime / 8);
            elapsedTime += Time.deltaTime;
            yield return null; // Continue the rotation over multiple frames
        }
        IsKeepMoving(keepMove);
        target.transform.rotation = targetRotation; // 確保最後設置為目標旋轉
    }
    public void Stopping_Player(GameObject target, float delay, bool keepMove)
    {
        StartCoroutine(SmoothStoppingPlayer(target, delay, keepMove));
    }
    IEnumerator SmoothStoppingPlayer(GameObject target, float delay, bool keepMove)
    {
        yield return new WaitForSeconds(delay);
        target.GetComponent<Player_Movement>().CanMove(false);
        IsKeepMoving(keepMove);
    }
    private void IsKeepMoving(bool keepMove)
    {
        if (keepMove)
        {
            GameStateManager.Instance.player_Status = GameStateManager.Player_Status.PlayerMoving;
        }
        else
        {
            GameStateManager.Instance.player_Status = GameStateManager.Player_Status.PlayerInGame;
        }
    }
    public void CanMove(bool can)
    {
        //Debug.Log("CanMove called with: " + can);
        playerRigidbody.isKinematic = !can;
    }


    public bool Both_Hands_Fist()
    {
        return isFist_L && isFist_R;
    }

    public void WhenFist_L()
    {
        isFist_L = true;
    }

    public void WhenNotFist_L()
    {
        isFist_L = false;
    }

    public void WhenFist_R()
    {
        isFist_R = true;
    }

    public void WhenNotFist_R()
    {
        isFist_R = false;
    }
    void TestModeFunction()
    {
        if (Input.GetKeyDown(KeyCode.Space) && TestMode)
        {
            ApplyForwardForce();
        }
    }
}