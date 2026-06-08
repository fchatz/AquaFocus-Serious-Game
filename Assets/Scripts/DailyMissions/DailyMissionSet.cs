using UnityEngine;
using System;
using System.Collections.Generic;


[System.Serializable]
public class DailyMissionSet
{
    public List<DailyMission> missions = new List<DailyMission>();
    public string lastGeneratedDate = "";
}

