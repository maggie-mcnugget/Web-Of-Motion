
using UnityEngine;

public class SpiderLegCoordinator : MonoBehaviour
{
    public bool leg01IsStepping;
    public bool leg02IsStepping;

    public bool CanLegStep(int legNumber)
    {
        if (legNumber == 1)
            return !leg02IsStepping;

        if (legNumber == 2)
            return !leg01IsStepping;

        return false;
    }

    public void SetLegStepping(int legNumber, bool stepping)
    {
        if (legNumber == 1)
            leg01IsStepping = stepping;

        if (legNumber == 2)
            leg02IsStepping = stepping;
    }
}
