using UnityEngine;
using System.Collections.Generic;

public class TutorialZoneManager : MonoBehaviour
{
    [System.Serializable]
    public class TutorialZone
    {
        public Collider triggerZone;
        public TutorialPrompt tutorialPrompt;
        public bool showOnce = true;
        [HideInInspector] public bool hasBeenShown = false;
    }

    [SerializeField] private TutorialZone[] tutorialZones;
    [SerializeField] private bool showInSequence = false;

    private int currentZoneIndex = 0;
    private Dictionary<Collider, TutorialZone> zoneDictionary = new Dictionary<Collider, TutorialZone>();

    private void Awake()
    {
        foreach (var zone in tutorialZones)
        {
            if (zone.triggerZone != null)
            {
                zoneDictionary[zone.triggerZone] = zone;

                if (!zone.triggerZone.TryGetComponent<ZoneTrigger>(out _))
                {
                    var trigger = zone.triggerZone.gameObject.AddComponent<ZoneTrigger>();
                    trigger.Initialize(this, zone.triggerZone);
                }

                if (showInSequence)
                {
                    zone.triggerZone.gameObject.SetActive(currentZoneIndex == 0);
                }
            }
        }
    }

    public void OnZoneEntered(Collider zone)
    {
        if (zoneDictionary.TryGetValue(zone, out var tutorialZone))
        {
            if (tutorialZone.showOnce && tutorialZone.hasBeenShown) return;

            tutorialZone.tutorialPrompt.StartTutorial();
            tutorialZone.hasBeenShown = true;

            if (showInSequence)
            {
                zone.gameObject.SetActive(false);
                currentZoneIndex++;

                if (currentZoneIndex < tutorialZones.Length)
                {
                    tutorialZones[currentZoneIndex].triggerZone.gameObject.SetActive(true);
                }
            }
        }
    }
}

public class ZoneTrigger : MonoBehaviour
{
    private TutorialZoneManager manager;
    private Collider zoneCollider;

    public void Initialize(TutorialZoneManager manager, Collider collider)
    {
        this.manager = manager;
        this.zoneCollider = collider;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            manager.OnZoneEntered(zoneCollider);
        }
    }
}