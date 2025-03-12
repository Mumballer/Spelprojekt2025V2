using UnityEngine;

public class QuestStarter : MonoBehaviour
{
    public Quest collectItemsQuest;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            QuestManager.Instance.StartQuest(collectItemsQuest);
        }
    }
}