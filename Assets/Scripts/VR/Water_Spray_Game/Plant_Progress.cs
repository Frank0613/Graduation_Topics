using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using System;

public class Plant_Progress : MonoBehaviour
{
    [SerializeField] private Slider progressBar;
    public GameObject Tutorial;
    
    [SerializeField] private Image[] Count_Success_Dots;
    [SerializeField] private RectTransform Fill_Area;
    [SerializeField] private float Cursor_Speed = 1f;
    
    private float Total_PosX;
    private float Start_PosX_To_Normal;
    private float End_PosX_To_Normal;
    private int Level_index = 0;
    private int Current_Dot_index = 0;
    public bool In_Range {get; private set;} = false;

    
    public event EventHandler OnGameOver;
    
    void Start()
    {
        
        Total_PosX = progressBar.GetComponent<RectTransform>().rect.width; // Initialize the Total_PosX.
        Change_Level(); // Initialize level.

        for(int i = 0; i < Count_Success_Dots.Length; i++){
            Change_To_Success_Dot(i,0);  
        }      
    }
    void Update()
    {
        Current_Dot_index = Level_index-1;

        if (progressBar != null)
        {
            progressBar.value = Mathf.PingPong(Time.time * Cursor_Speed, 1f); // Cursor movement
            Detect_Wether_In_Range();
            Detect_Wether_Spray_IsFinished(); 
        }
        else{
            Debug.LogError("No progressBar being assigned.");
        }
    }
    private void Detect_Wether_In_Range(){
        if( progressBar.value >= Start_PosX_To_Normal && progressBar.value <= End_PosX_To_Normal){
            In_Range = true;
        }
        else
        {
            In_Range = false;
        }
    }
    private void Level(float fill_size){

        // Change width scale
        Vector2 Size_Delta = Fill_Area.sizeDelta;
        Size_Delta.x = fill_size;
        Fill_Area.sizeDelta = Size_Delta;

        // Limited position where able to spawn
        Vector2 AnchoredPosition = Fill_Area.anchoredPosition;
        AnchoredPosition.x = UnityEngine.Random.Range(0, Max_Spawn_PosX(fill_size));
        float Start_PosX = AnchoredPosition.x;
        float End_PosX = Start_PosX + fill_size;
        Fill_Area.anchoredPosition = AnchoredPosition;
        Start_PosX_To_Normal = NormalizeValue(Start_PosX, 0f, 160f);
        End_PosX_To_Normal = NormalizeValue(End_PosX, 0f, 160f);
    }
    public void Change_Level(){
        Level_index += 1;
        Change_To_Success_Dot(Current_Dot_index,1);

        switch(Level_index){
            case 1:
                Level(80);        
                break;
            case 2:
                Level(60);
                break;
            case 3:
                Level(40);
                break;
            case 4:
                Level(30);
                break;
        }
    }
    private float Max_Spawn_PosX(float size){
        return Total_PosX - size;
    }
    public float NormalizeValue(float value, float min, float max)
    {
        return (value - min) / (max - min);
    }
    private void Detect_Wether_Spray_IsFinished(){

        if(progressBar.value>=1 || Level_index>=5 || Current_Dot_index>=4){
            Level_index =0;
            Current_Dot_index =0 ;
            this.enabled = false;

            // Spray Finish...
            OnGameOver?.Invoke(this, EventArgs.Empty);
            
            
        }
    }

    public void Change_To_Success_Dot(int dot_index,float alpha){
        Color color = Count_Success_Dots[dot_index].color;
        color.a = alpha; 
        Count_Success_Dots[dot_index].color = color;
    }

}
