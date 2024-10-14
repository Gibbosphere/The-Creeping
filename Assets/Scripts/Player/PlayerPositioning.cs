using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPositioning : MonoBehaviour
{
    public static Vector3 obstacleHitPosition;
    public static Vector3 previousTurnPosition = new Vector3(0, 1.5f, 0);
    public static bool rightTurn = false;
    public static bool leftTurn = false;

    public static Vector3 previousDecisionPosition;
    public static bool leftDecisionTurn = false;
}
