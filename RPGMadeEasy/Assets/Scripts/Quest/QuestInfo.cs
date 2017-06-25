using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Quest Base!
[System.Serializable]
public class QuestInfo{ 

    public ProgressState progressState;
    public string myName = "default quest name";
    public string descriptions = "default quest descriptions";
    public string hint = "This is a hint";
    public string congratulation = "Congratulation on completing the quest";

    public QuestRequirement questRequirement;
    public QuestReward questReward;

    [System.Serializable]
    public class QuestReward
    {
        public int expReward;
        public int goldReward;
        public string itemReward;
    }

    [System.Serializable]
    public class QuestRequirement {
        public RequirementType requirementType; //The type of requirement
        public int questObjectiveCount; //Current number of quest objectives
        public string requirementObject; //The unique id of the object you require

        public string questObjective; //name of the quest objective (also for removing items) (this is a combination of requirement type + requirementObject)
        public int nextQuest; //UNID?
    }
    
}



//not available
//available
//accepted (started) 
public enum ProgressState
{
    notStarted,
    started,
    completed
}

public enum RequirementType {

    kill,
    collect

}