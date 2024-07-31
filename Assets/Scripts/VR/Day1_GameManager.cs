using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System;

public class Day1_GameManager : MonoBehaviour
{
    private NarrationManager narrationManager;
    [SerializeField] private Fade_Screen fadeScreen;
    [SerializeField] private GameObject SprayBottle;
    [SerializeField] private GameObject SprayBottle_UI;
    [SerializeField] private Plant_Progress plant_Progress;

    [SerializeField] List<Transform> waypoints;
    [SerializeField] private NavMeshAgent player_agent;
    private int currentWaypointIndex = 0;
    private bool StartMoving = false;
    [SerializeField] float RotationDuration;
    
    
    void Start()
    {
        Hide_SprayBottle_UI();
        fadeScreen.FadeIn();
        narrationManager = GetComponent<NarrationManager>();
        player_agent.updateRotation = false;
        if(plant_Progress != null)
            plant_Progress.OnGameOver += HandleOnGameOver;
        
        
        
    }
    void Update()
    {
        

        //=======================

        if (StartMoving && !player_agent.pathPending && player_agent.remainingDistance < 0.5f)
        {
            MoveToDestination();
        }
        //=======================
    }
    private void HandleOnGameOver(object sender, EventArgs e){

        StartCoroutine(When_The_Game1_End());
        plant_Progress.OnGameOver -= HandleOnGameOver;
    }
    
    IEnumerator When_The_Game1_End(){
        yield return new WaitForSeconds(1f);
        Hide_SprayBottle_UI();
        
        // Start smooth rotation
        Quaternion startRotation = player_agent.gameObject.transform.rotation;
        Quaternion endRotation = startRotation * Quaternion.Euler(0, 180, 0);
        
        float elapsedTime = 0f;

        while (elapsedTime < RotationDuration)
        {
            player_agent.gameObject.transform.rotation = Quaternion.Slerp(startRotation, endRotation, elapsedTime / RotationDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // 确保最终角度
        player_agent.gameObject.transform.rotation = endRotation;

        StartMoving = true;
        narrationManager.Narration_After_Game1_End();
    }

    public void Hide_SprayBottle_UI(){
        SprayBottle.SetActive(false);
        SprayBottle_UI.SetActive(false);
    }
     public void Show_SprayBottle_UI(){
        
        SprayBottle.SetActive(true);
        SprayBottle_UI.SetActive(true);
    }









    void MoveToDestination()
    {
        if (currentWaypointIndex < waypoints.Count)
        {
            player_agent.SetDestination(waypoints[currentWaypointIndex].position);
            currentWaypointIndex++;
        }
        else
        {
            StartMoving = false; 
            
            
        }
    }
}
