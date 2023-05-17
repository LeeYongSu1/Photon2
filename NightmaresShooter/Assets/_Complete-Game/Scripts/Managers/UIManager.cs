using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
  
    Text text;                      // Reference to the Text component.
    Animator anim;
    void Awake()
    {
        // Set up the reference.
        text = transform.GetChild(3).GetComponent<Text>();
        anim = GetComponent<Animator>();
    }


    public void UpdateScoreText(int score)
    {
        text.text = "Score : " + score;
    }

    //public void SetActiveGameoverUI()
    //{
    //    anim.SetTrigger("GameOver");
    //}
}
