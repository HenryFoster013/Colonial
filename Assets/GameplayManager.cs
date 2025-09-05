using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameplayManager : MonoBehaviour
{
    SessionManager _SessionManager;
    
    public int current_turn { get; private set; }
    public int current_stars { get; private set; }
    int stars_per_turn;

    public void DefaultValues(){
        current_stars = 3;
        current_turn = 1;
    }

    public void UpTurn(){current_turn++;}
    public void UpStars(){current_stars += stars_per_turn;}
    public void SpendStars(int cost){current_stars -= cost;}
    public void SetSession(SessionManager sm){_SessionManager = sm;}
}
