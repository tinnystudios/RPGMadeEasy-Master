using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class QuestManager : MonoBehaviour {

    public static QuestManager questManager;

    //private vars for quest object

    public QuestUI questUI;
    public QuestObject[] questObjects;
    public List<QuestObject> currentQuestList = new List<QuestObject>();
    public QuestObject selectedQuestObject;

    public delegate void QuestEventDelegate();
    public event QuestEventDelegate OnMonsterKilled;

    public delegate void QuestMessageCallback(string message);
    public event QuestMessageCallback OnQuestMessage;

    public void SendQuestMessage(string s) {
        //Kill
        //Amount
        //Name/ID
        print("Quest Message: " + s);

        if(OnQuestMessage != null)
        OnQuestMessage.Invoke(s);

        //questUI.textRemaining.text = selectedQuestObject.GetRemainingCount().ToString() + "/" + selectedQuestObject.m_questRequirement.requirementObject;
        questUI.textRemaining.text  = selectedQuestObject.GetDisplayRemaining();
    }

    void Awake() {
        //Cache them all and listen for the quest stuff
        questObjects = GameObject.FindObjectsOfType<QuestObject>();

        for (int i = 0; i < questObjects.Length; i++) {
            AddQuest(questObjects[i]);
        }
    }

    public void AddQuest(QuestObject questObject) {
        //Begin the listener
        questObject.OnShow += OnQuestObjectShow;
        questObject.OnStart += OnQuestObjectStart;
        questObject.OnComplete += OnQuestObjectCompleted;
        currentQuestList.Add(questObject);
    }

    //Selects the quest
    private void OnQuestObjectShow(QuestObject questObject)
    {
        QuestInfo questInfo = questObject.m_questInfo;

        //Bring up UI display
        questUI.questGroup.SetActive(true);

        //Show the quest data from quest info
        questUI.textName.text = questInfo.myName;
        questUI.textDescription.text = questInfo.descriptions;
        questUI.textObjective.text = questObject.GetFullObjective();

        selectedQuestObject = questObject;

        SetQuestTextButton(questInfo.progressState);
        questUI.textRemaining.text = selectedQuestObject.GetDisplayRemaining();
    }

    public void SetQuestTextButton(ProgressState progressState) {
        if(progressState == ProgressState.notStarted)
            questUI.textButton.text = "Start Quest";
        if (progressState == ProgressState.started)
            questUI.textButton.text = "Quest Started";
        if (progressState == ProgressState.completed)
            questUI.textButton.text = "Completed";
    }

    //When the player presses the start button.
    private void OnQuestObjectStart(QuestObject questObject)
    {
        QuestInfo questInfo = questObject.m_questInfo;
        questInfo.progressState = ProgressState.started;

        //Which event should I listen to!?
        //OnMonsterKilled += questObject.CompleteQuest;
        OnQuestMessage += questObject.OnQuestMessageReceived;
        SetQuestTextButton(questInfo.progressState);
    }

 

    private void OnQuestObjectCompleted(QuestObject questObject)
    {
        QuestInfo questInfo = questObject.m_questInfo;
        questInfo.progressState = ProgressState.completed;
        print(questObject.m_questInfo.myName + " completed");
        SetQuestTextButton(questInfo.progressState);
        OnQuestMessage -= questObject.OnQuestMessageReceived;
        //Stop listening for 
        //OnMonsterKilled -= questObject.CompleteQuest;
    }

    //Selected Quest
    public void StartQuest()
    {
        if (selectedQuestObject.m_questInfo.progressState != ProgressState.notStarted)
            return;

        selectedQuestObject.StartQuest();
        Debug.Log("Start " + selectedQuestObject.m_questInfo.myName);
    }

}

//The next step is to create requirements and a listener for when that requirement is met.