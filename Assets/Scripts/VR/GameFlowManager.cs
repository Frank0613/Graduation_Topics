using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System;

public class GameFlowManager : MonoBehaviour
{




    public static GameFlowManager Instance;
    [Header("Fade Screen Control")]
    [SerializeField] private Fade_Screen fadeScreen;

    [Header("Player Route Control")]
    [SerializeField] private NavMeshAgent player_agent;
    [SerializeField] List<Transform> PhotoRoute_Waypoints;
    [SerializeField] List<Transform> DoorRoute_Waypoints;
    [SerializeField] private float RotationDuration;

    [Header("GameObject Control")]
    [SerializeField] GameObject Neighbor_OBJ;
    [SerializeField] GameObject Daughter_OBJ;
    [SerializeField] GameObject Door_OBJ;

    public GameObject Player;

    private Coroutine moveCoroutine;
    private Coroutine RotationCoroutine;

    // Event
    public EventHandler OnStart_PhotoRoute;
    public EventHandler OnStart_DoorRoute;

    //GetComponent

    private NarrationManager narro;


    private void Awake()
    {
        //Singleton
        if (Instance == null)
        {
            Instance = this;
        }
        //Component
        narro = GetComponent<NarrationManager>();

        //Whether auto facing the destination
        player_agent.updateRotation = false;

        //Fade in when starting
        fadeScreen.FadeIn();

        //Events Initializations
        OnStart_PhotoRoute += Handle_OnStartPhotoRoute;
        OnStart_DoorRoute += Handle_OnStartDoorRoute;
        //Characters Initializations
        DisplayNeighbor(false);
        DisplayDaughter(false);

    }
    #region Character Display & Move
    public void DisplayNeighbor(bool visible)
    {
        Neighbor_OBJ.SetActive(visible);
    }
    public void NeighborMoving()
    {
        if (Neighbor_OBJ.activeInHierarchy)
        {
            Neighbor_OBJ.GetComponentInChildren<Animator>().SetTrigger("Neighbor_Move");
        }
    }
    public void DisplayDaughter(bool visible)
    {
        Daughter_OBJ.SetActive(visible);
    }
    public void DaughterMoving()
    {
        if (Daughter_OBJ.activeInHierarchy)
        {
            Daughter_OBJ.GetComponentInChildren<Animator>().SetTrigger("Daughter_Move");
        }
    }
    public void DoorMoving()
    {
        if (Door_OBJ.activeInHierarchy)
        {
            Door_OBJ.GetComponentInChildren<Animator>().SetTrigger("Door_Open");
        }
    }
    #endregion

    #region AfterSprayGames
    public void After_SprayGame()
    {
        //StartCoroutine(Route_After_SprayGame());

    }

    IEnumerator Route_After_SprayGame()
    {
        // Hide_SprayBottle_UI
        yield return new WaitForSeconds(1f);
        //GetComponent<SprayGameManager>().Display_SprayBottle_UI(false);
        yield return new WaitForSeconds(1f);
        Player.GetComponent<Rigidbody>().isKinematic = false;
        if (RotationCoroutine == null)
        {
            RotationCoroutine = StartCoroutine(SmoothRotatePlayer(182f));
            //GetComponent<SprayGameManager>().Display_SprayBottle_UI(true);
        }

        // Start moving & narration
        OnStart_PhotoRoute?.Invoke(this, EventArgs.Empty);
    }

    private void Handle_OnStartPhotoRoute(object sender, EventArgs e)
    {
        narro.Narration_After_SprayGame();
        StartMoving(PhotoRoute_Waypoints);
    }
    #endregion

    #region AfterMatchGames
    public void After_MatchGame()
    {
        StartCoroutine(SmoothRotatePlayer(90));
    }
    IEnumerator Route_After_MatchGame(float RotationY)
    {
        yield return new WaitForSeconds(1f);
        OnStart_DoorRoute?.Invoke(this, EventArgs.Empty);
    }
    private void Handle_OnStartDoorRoute(object sender, EventArgs e)
    {
        narro.Narration_After_MatchGame();
        StartMoving(DoorRoute_Waypoints, narro.Narration_FrontDoor);
    }

    #endregion

    #region Moving & Rotation

    void StartMoving(List<Transform> waypoints, Action onComplete = null)
    {
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
        }
        moveCoroutine = StartCoroutine(MoveThroughWaypoints(waypoints, onComplete));
    }
    IEnumerator MoveThroughWaypoints(List<Transform> waypoints, Action onComplete = null)
    {
        int index = 0;
        while (index < waypoints.Count)
        {
            // Calculate the direction to the next waypoint
            Vector3 directionToNextWaypoint = waypoints[index].position - player_agent.transform.position;

            // Calculate the target rotation based on the direction * offset
            Quaternion targetRotation = Quaternion.LookRotation(directionToNextWaypoint) * Quaternion.Euler(0, 180, 0);

            // Smoothly rotate the player to face the next waypoint
            //yield return StartCoroutine(SmoothRotatePlayer(targetRotation));

            // Set the destination to the next waypoint
            player_agent.SetDestination(waypoints[index].position);

            // Wait until the agent reaches the destination
            while (!player_agent.pathPending && player_agent.remainingDistance > 0.5f)
            {
                yield return null;
            }
            while (player_agent.pathPending || player_agent.remainingDistance > 0.1f)
            {
                yield return null;
            }
            index++;
        }
        onComplete?.Invoke();
    }

    IEnumerator SmoothRotatePlayer(float targetRotationY)
    {
        Quaternion startRotation = player_agent.transform.rotation;
        float elapsedTime = 0f;

        while (elapsedTime < RotationDuration)
        {
            player_agent.transform.rotation = Quaternion.Slerp(startRotation, Quaternion.Euler(0, targetRotationY, 0), elapsedTime / RotationDuration);
            elapsedTime += Time.deltaTime;
            yield return null; // Continue the rotation over multiple frames
        }
        player_agent.transform.rotation = Quaternion.Euler(0, targetRotationY, 0);
        RotationCoroutine = null;
    }
    #endregion

    void OnDestroy()
    {
        OnStart_PhotoRoute -= Handle_OnStartPhotoRoute;
        OnStart_DoorRoute -= Handle_OnStartDoorRoute;
    }
}
