using UnityEngine;


[System.Serializable]
public class DailyMission
{
    public string missionId;
    public int target;
    public int progress;
    public bool completed;
    public bool claimed;     // NEW: prevents claiming twice
    public int reward = 50;  // NEW: default reward

    public DailyMission(string id, int target, int reward = 50)
    {
        missionId = id;
        this.target = target;
        this.reward = reward;
        progress = 0;
        completed = false;
        claimed = false;
    }
}


