using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>Player-side rewind input, gated by sand; auto-links enemies tagged autoLinkTag</summary>
public class RewindController : MonoBehaviour
{
    [SerializeField] private Rewindable playerRewindable;
    [SerializeField] private SandMeter sandMeter;

    [Tooltip("true = click toggles rewind on/off. false = hold the button to rewind.")]
    [SerializeField] private bool toggleMode = true;
    [SerializeField] private KeyCode rewindKey = KeyCode.Mouse0; // click on the dagger

    [Header("Linked Rewindables")]
    [Tooltip("Manually assigned extra objects (e.g. a specific crate) that should rewind together with the player.")]
    [SerializeField] private Rewindable[] linkedRewindables;
    [Tooltip("Every object with this tag that has a Rewindable component is automatically linked too. Leave empty to disable.")]
    [SerializeField] private string autoLinkTag = "Enemy";

    public bool IsRewinding { get; private set; }

    private readonly List<Rewindable> allLinked = new List<Rewindable>();

    private void Start()
    {
        CollectLinkedRewindables();
    }

    /// <summary>Rebuilds the linked list; call again if you spawn tagged enemies at runtime</summary>
    public void CollectLinkedRewindables()
    {
        allLinked.Clear();

        if (linkedRewindables != null)
        {
            allLinked.AddRange(linkedRewindables);
        }

        if (!string.IsNullOrEmpty(autoLinkTag))
        {
            foreach (GameObject taggedObject in GameObject.FindGameObjectsWithTag(autoLinkTag))
            {
                Rewindable r = taggedObject.GetComponent<Rewindable>();
                if (r != null && !allLinked.Contains(r))
                {
                    allLinked.Add(r);
                }
            }
        }
    }

    private void Update()
    {
        HandleInput();

        if (IsRewinding)
        {
            bool stillHasSand = sandMeter.TryDrain(Time.deltaTime);
            bool stillHasHistory = playerRewindable.HasHistory;

            if (!stillHasSand || !stillHasHistory)
            {
                SetRewinding(false);
            }
        }
    }

    private void HandleInput()
    {
        if (toggleMode)
        {
            if (RewindPressedThisFrame() && sandMeter.HasSand)
            {
                SetRewinding(!IsRewinding);
            }
        }
        else
        {
            if (RewindPressedThisFrame() && sandMeter.HasSand)
            {
                SetRewinding(true);
            }
            else if (RewindReleasedThisFrame())
            {
                SetRewinding(false);
            }
        }
    }

    // Combines mouse/keyboard (rewindKey) with the gamepad's West button (X on Xbox-style layout)
    private bool RewindPressedThisFrame()
    {
        if (Input.GetKeyDown(rewindKey)) return true;
        return Gamepad.current != null && Gamepad.current.buttonWest.wasPressedThisFrame;
    }

    private bool RewindReleasedThisFrame()
    {
        if (Input.GetKeyUp(rewindKey)) return true;
        return Gamepad.current != null && Gamepad.current.buttonWest.wasReleasedThisFrame;
    }

    private void SetRewinding(bool value)
    {
        if (IsRewinding == value) return;
        IsRewinding = value;

        if (IsRewinding)
        {
            playerRewindable.StartRewinding();
        }
        else
        {
            playerRewindable.StopRewinding();
        }

        foreach (Rewindable r in allLinked)
        {
            if (r == null) continue;
            if (IsRewinding) r.StartRewinding();
            else r.StopRewinding();
        }
    }
}