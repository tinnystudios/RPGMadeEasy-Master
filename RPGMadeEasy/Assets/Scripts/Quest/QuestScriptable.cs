using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This holds all the 'premade' quests 
[CreateAssetMenu(menuName = "RPGEasy/QuestScriptable")]
public class QuestScriptable : ScriptableObject {

    //Contains all quests
    public List<QuestInfo> mainQuests = new List<QuestInfo>();
    public List<QuestInfo> subQuests = new List<QuestInfo>();

}




