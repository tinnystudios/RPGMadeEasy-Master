using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestObject : MonoBehaviour {

    //This is the data that defines what sorta quest this is
    public QuestInfo m_questInfo; //must be an instance of the actual thingo
    private QuestInfo.QuestRequirement m_questRequirement { get; set; }
    private int m_questObjectiveCount;
    private QuestObject questObject;

    //Has a set of delegates
    public delegate void QuestDelgate(QuestObject questObject);

    public event QuestDelgate OnShow; //Display the quest
    public event QuestDelgate OnStart; //Start the quest
    public event QuestDelgate OnComplete; //End the quest

    void Awake() {
        questObject = GetComponent<QuestObject>();
        m_questRequirement = m_questInfo.questRequirement;

        //Create objective string
        m_questRequirement.questObjective = m_questRequirement.requirementType.ToString() + " "+ m_questRequirement.requirementObject;

    }

    public void ShowQuest() {
        OnShow.Invoke(questObject);
    }

    public void StartQuest() {
        OnStart.Invoke(questObject);
    }

    public void OnQuestMessageReceived(string message) {
        //print(m_questInfo.myName + "Received Message: " + message);

        //Validate objective
        if (m_questRequirement.questObjective == message) {

            //Increase objective count
            m_questObjectiveCount++;

            int remaining = m_questRequirement.questObjectiveCount - m_questObjectiveCount;
            print("Objective Matches: " + remaining + " left");

            //Check the objective count
            if (remaining <= 0) {
                CompleteQuest();
            }
            
        }
 
    }

    public int GetRemainingCount() {
        return m_questRequirement.questObjectiveCount - m_questObjectiveCount;
    }

    public string GetDisplayRemaining() {
        return m_questObjectiveCount + "/" + m_questRequirement.questObjectiveCount + " " + m_questRequirement.requirementObject;
    }

    public string GetFullObjective() {
        string s = "";

        s = m_questRequirement.requirementType.ToString() + " " + m_questRequirement.questObjectiveCount + " "  + m_questRequirement.requirementObject;

        return s;
    }

    public void EndQuest() {
        OnComplete.Invoke(questObject);
    }

    public void CompleteQuest() {
        OnComplete.Invoke(questObject);
    }

}
