using System.Collections;
using System.Collections.Generic;
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
        playerRigidbody.isKinematic = true;
        //When start, previous position equal to init position of both hands.
        previousLeftHandPos = leftHand.transform.position;
        previousRightHandPos = rightHand.transform.position;
    }

    void FixedUpdate()
    {
        TestModeFunction();
        // Player_Movement_Fun();
    }
    void Player_Movement_Fun()
    {
        if (Both_Hands_Fist())
        {
            Hands_Velocity_Process();

            // ApplyForce
            if (smoothedLeftHandVelocity.z < -velocityThreshold && smoothedRightHandVelocity.z < -velocityThreshold)
            {
                ApplyForwardForce();
                Debug.Log("Move");
            }
        }
    }
    private void Hands_Velocity_Process()
    {
        // Get velocity of both hands (v = dis/time)
        Vector3 leftHandVelocity = (leftHand.transform.position - previousLeftHandPos) / Time.fixedDeltaTime;
        Vector3 rightHandVelocity = (rightHand.transform.position - previousRightHandPos) / Time.fixedDeltaTime;

        // Smooth the velocity(Reduce errors)
        smoothedLeftHandVelocity = Vector3.Lerp(smoothedLeftHandVelocity, leftHandVelocity, smoothingFactor);
        smoothedRightHandVelocity = Vector3.Lerp(smoothedRightHandVelocity, rightHandVelocity, smoothingFactor);

        // Update previous pos of both hands.
        previousLeftHandPos = leftHand.transform.position;
        previousRightHandPos = rightHand.transform.position;
    }
    public void ApplyForwardForce()
    {


        //Apply the force
        //playerRigidbody.AddForce(transform.forward * forwardForce, ForceMode.Impulse);



        // Normalized direction.
        Vector3 averageDirection = (smoothedLeftHandVelocity + smoothedRightHandVelocity).normalized;
        //Force direction.(expect y dir)
        Vector3 forceDirection = -new Vector3(averageDirection.x, 0, averageDirection.z);

        if (Time.time - lastForceTime >= cooldownDuration)
        {
            //Apply the force
            playerRigidbody.AddForce(transform.forward * forwardForce, ForceMode.Impulse);
            lastForceTime = Time.time;
        }
    }
    public void Rotation_Player(GameObject target, float rotation)
    {
        StartCoroutine(SmoothRotatePlayer(target, rotation));
    }

    IEnumerator SmoothRotatePlayer(GameObject target, float rotationAngle)
    {
        yield return new WaitForSeconds(2);
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

        target.transform.rotation = targetRotation; // 確保最後設置為目標旋轉
        GameStateManager.Instance.player_Status = GameStateManager.Player_Status.PlayerMoving;
    }
    public void CanMove(bool can)
    {
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
            Debug.Log("ApplyForce");
            ApplyForwardForce();
        }
    }
}