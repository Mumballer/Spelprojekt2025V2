using UnityEngine;
using System;
using TMPro;

public class Nametag : MonoBehaviour
{
    [Header("Nametag Settings")]
    [SerializeField] private string nametagID;
    [SerializeField] private bool interactable = true;
    [SerializeField] private float pickupDistance = 2.0f;

    [Header("Visual Effects")]
    [SerializeField] private Renderer[] renderers;
    [SerializeField] private GameObject visualsContainer;
    [SerializeField] private GameObject highlightObject;
    [SerializeField] private TextMeshPro nameText;
    [SerializeField] private float fadeSpeed = 5f;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private AudioClip placeSound;

    [Header("Debug")]
    [SerializeField] private bool showDebug = false;

    // events för interaktioner
    public event Action<TableSpot, string> OnNametagPlaced;
    public event Action OnNametagPickedUp;

    // interna variabler
    private bool pickedUp = false;
    private bool placed = false;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private TableSpot currentSpot;
    private float currentAlpha = 1.0f;
    private float targetAlpha = 1.0f;
    private Transform playerTransform;
    private bool playerInRange = false;
    private AudioSource audioSource;

    void Awake()
    {
        // leta reda på och spara alla renderare om ingen är satt
        if (renderers == null || renderers.Length == 0)
        {
            renderers = GetComponentsInChildren<Renderer>();
        }

        // spara originalposition för att kunna återställa senare
        originalPosition = transform.position;
        originalRotation = transform.rotation;

        // sätt upp ljudkälla
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && (pickupSound != null || placeSound != null))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1.0f; // 3D ljud
            audioSource.volume = 0.8f;
        }
    }

    void Start()
    {
        // hitta spelaren, kan vara bra att ha
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }

        // dölj highlight vid start
        if (highlightObject != null)
        {
            highlightObject.SetActive(false);
        }

        // sätt namn om textkomponent finns
        if (nameText != null && !string.IsNullOrEmpty(nametagID))
        {
            nameText.text = nametagID;
        }
        else if (string.IsNullOrEmpty(nametagID))
        {
            // använd gameobject-namn om inget ID är satt
            nametagID = gameObject.name;
        }
    }

    void Update()
    {
        // kolla om spelaren är nära (för highlight etc)
        CheckPlayerDistance();

        // hantera alpha-fade
        UpdateVisibility();
    }

    // kollar avståndet till spelaren
    private void CheckPlayerDistance()
    {
        if (playerTransform == null || !interactable) return;

        float distance = Vector3.Distance(transform.position, playerTransform.position);
        bool inRange = distance <= pickupDistance;

        // uppdatera bara om statusen ändrats
        if (inRange != playerInRange)
        {
            playerInRange = inRange;

            // visa highlight om spelaren är i närheten
            if (highlightObject != null)
            {
                highlightObject.SetActive(playerInRange && !pickedUp && !placed);
            }

            // anropa event-metoder för att andra skript ska kunna reagera
            if (playerInRange)
            {
                OnPlayerEnterRange();
            }
            else
            {
                OnPlayerExitRange();
            }
        }
    }

    // när spelaren kommer inom räckhåll
    private void OnPlayerEnterRange()
    {
        try
        {
            if (showDebug)
            {
                Debug.Log($"Player entered range of nametag: {gameObject.name}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error in OnPlayerEnterRange: {e.Message}");
        }
    }

    // när spelaren lämnar området
    private void OnPlayerExitRange()
    {
        try
        {
            if (showDebug)
            {
                Debug.Log($"Player exited range of nametag: {gameObject.name}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error in OnPlayerExitRange: {e.Message}");
        }
    }

    // sätter genomskinlighet på namnbrickan (anropas från TableSpot)
    public void SetAlpha(float alpha)
    {
        targetAlpha = Mathf.Clamp01(alpha);

        // direkt uppdatering på alla material
        if (renderers != null && renderers.Length > 0)
        {
            foreach (Renderer renderer in renderers)
            {
                if (renderer != null)
                {
                    foreach (Material mat in renderer.materials)
                    {
                        // kolla om materialet har en färgegenskap
                        if (mat.HasProperty("_Color"))
                        {
                            Color color = mat.color;
                            color.a = targetAlpha;
                            mat.color = color;
                        }
                    }
                }
            }
        }

        currentAlpha = targetAlpha;

        if (showDebug)
        {
            Debug.Log($"Nametag '{nametagID}' alpha set to {targetAlpha}");
        }
    }

    // gradvis uppdatering av genomskinlighet
    private void UpdateVisibility()
    {
        if (Mathf.Approximately(currentAlpha, targetAlpha)) return;

        currentAlpha = Mathf.MoveTowards(currentAlpha, targetAlpha, Time.deltaTime * fadeSpeed);

        if (renderers != null)
        {
            foreach (Renderer renderer in renderers)
            {
                if (renderer != null)
                {
                    foreach (Material mat in renderer.materials)
                    {
                        if (mat.HasProperty("_Color"))
                        {
                            Color color = mat.color;
                            color.a = currentAlpha;
                            mat.color = color;
                        }
                    }
                }
            }
        }
    }

    // markerar att namnbrickan är placerad (kallas från TableSpot)
    public void PlaceNametag()
    {
        placed = true;
        pickedUp = false;

        // visa namnbrickan
        SetAlpha(1.0f);

        // spela ljud om det finns
        PlaySound(placeSound);

        if (showDebug)
        {
            Debug.Log($"Nametag '{nametagID}' placed at spot: {(currentSpot != null ? currentSpot.name : "unknown")}");
        }

        // notifiera att namnbrickan har placerats
        if (currentSpot != null)
        {
            OnNametagPlaced?.Invoke(currentSpot, nametagID);
        }
    }

    // fysiskt placerar namnbrickan på en plats (kallas från TableSpot)
    public void PlaceAtSpot(TableSpot spot)
    {
        if (spot == null)
        {
            Debug.LogError("Cannot place nametag - TableSpot is null");
            return;
        }

        // spara referensen till platsen
        currentSpot = spot;

        // flytta till platsens position/rotation
        transform.position = spot.transform.position;
        transform.rotation = spot.transform.rotation;

        // leta efter en specifik platsmarkör om sådan finns
        Transform nametagPosition = spot.transform.Find("NametagPosition");
        if (nametagPosition != null)
        {
            transform.position = nametagPosition.position;
            transform.rotation = nametagPosition.rotation;
        }

        // uppdatera status
        placed = true;
        pickedUp = false;

        // visa namnbrickan
        SetAlpha(1.0f);

        // spela ljud om det finns
        PlaySound(placeSound);

        if (showDebug)
        {
            Debug.Log($"Positioned nametag '{nametagID}' at spot: {spot.name}");
        }
    }

    // markerar att namnbrickan är upplockad
    public void PickUp()
    {
        pickedUp = true;
        placed = false;
        currentSpot = null;

        // spela ljud om det finns
        PlaySound(pickupSound);

        // notifiera att namnbrickan har plockats upp
        OnNametagPickedUp?.Invoke();

        if (showDebug)
        {
            Debug.Log($"Nametag '{nametagID}' picked up");
        }
    }

    // lägger ner namnbrickan (inte på en specifik plats)
    public void PutDown()
    {
        pickedUp = false;
        placed = false;
        currentSpot = null;

        if (showDebug)
        {
            Debug.Log($"Nametag '{nametagID}' put down");
        }
    }

    // kontrollera om namnbrickan är upplockad - krävs av TableSpot
    public bool IsPickedUp()
    {
        return pickedUp;
    }

    // återställ namnbrickan till sin originalposition
    public void ResetPosition()
    {
        transform.position = originalPosition;
        transform.rotation = originalRotation;

        pickedUp = false;
        placed = false;
        currentSpot = null;

        if (showDebug)
        {
            Debug.Log($"Nametag '{nametagID}' reset to original position");
        }
    }

    // hjälpmetod för att spela ljud
    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    // för visualisering i editorn
    private void OnDrawGizmosSelected()
    {
        // visa interaktionsradie
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupDistance);

        // visa status
        if (placed)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(transform.position + Vector3.up * 0.2f, new Vector3(0.1f, 0.1f, 0.1f));
        }
        else if (pickedUp)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(transform.position + Vector3.up * 0.2f, new Vector3(0.1f, 0.1f, 0.1f));
        }
    }
}