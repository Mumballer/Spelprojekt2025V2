using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private int defaultQuestSequence = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Keep it alive across scenes
        }
        else
        {
            Destroy(gameObject); // Prevent duplicates
        }
    }

    // Automatically spawn if missing


    private void Start()
    {
        // Start the first quest sequence when the game begins
        QuestManager.Instance.StartQuestSequence(defaultQuestSequence);
    }
}
