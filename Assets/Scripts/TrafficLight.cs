using UnityEngine;
using System.Collections;

public class TrafficLight : MonoBehaviour
{
    [Header("Light Settings")]
    [SerializeField] private bool startRed = true;
    [SerializeField] private float redDuration = 8f;
    [SerializeField] private float greenDuration = 5f;
    [SerializeField] private float initialDelay = 0f;

    [Header("Visual Feedback")]
    [SerializeField] private MeshRenderer lightRenderer;
    [SerializeField] private Material redMaterial;
    [SerializeField] private Material greenMaterial;
    [SerializeField] private Material yellowMaterial;
    [SerializeField] private float yellowBlinkDuration = 0.5f;
    [SerializeField] private int yellowBlinks = 3;

    [Header("VFX")]
    [SerializeField] private GameObject greenLightVFXPrefab; // Assign your VFX prefab in inspector
    [SerializeField] private Transform vfxSpawnPoint; // Where to spawn the VFX
    [SerializeField] private float vfxDuration = 2f; // How long to keep VFX active

    private bool isRed;
    private Coroutine lightCycleCoroutine;
    private GameObject currentVFXInstance;

    public bool IsRed => isRed;

    private void Start()
    {
        isRed = startRed;
        UpdateLightVisual();
        lightCycleCoroutine = StartCoroutine(LightCycle());
    }

    private void OnDisable()
    {
        if (lightCycleCoroutine != null)
        {
            StopCoroutine(lightCycleCoroutine);
        }
        CleanUpVFX();
    }

    private IEnumerator LightCycle()
    {
        yield return new WaitForSeconds(initialDelay);

        while (true)
        {
            // Green -> Red transition (with yellow warning)
            if (!isRed)
            {
                yield return new WaitForSeconds(greenDuration);
                yield return StartCoroutine(BlinkYellow());
                SetLightState(true);
                SpawnGreenLightVFX(); // Spawn VFX when turning red
            }
            // Red -> Green transition
            else
            {
                yield return new WaitForSeconds(redDuration);
                SetLightState(false);
            }
        }
    }

    private void SpawnGreenLightVFX()
    {
        CleanUpVFX(); // Clean up any existing VFX first

        if (greenLightVFXPrefab != null && vfxSpawnPoint != null)
        {
            currentVFXInstance = Instantiate(greenLightVFXPrefab, vfxSpawnPoint.position, vfxSpawnPoint.rotation);
            Destroy(currentVFXInstance, vfxDuration); // Auto-destroy after duration
        }
    }

    private void CleanUpVFX()
    {
        if (currentVFXInstance != null)
        {
            Destroy(currentVFXInstance);
            currentVFXInstance = null;
        }
    }

    private IEnumerator BlinkYellow()
    {
        if (yellowMaterial == null) yield break;

        for (int i = 0; i < yellowBlinks; i++)
        {
            lightRenderer.material = yellowMaterial;
            yield return new WaitForSeconds(yellowBlinkDuration);
            lightRenderer.material = greenMaterial;
            yield return new WaitForSeconds(yellowBlinkDuration);
        }
    }

    private void SetLightState(bool red)
    {
        isRed = red;
        UpdateLightVisual();
        Debug.Log($"Traffic light changed to: {(isRed ? "RED" : "GREEN")}");
    }

    private void UpdateLightVisual()
    {
        if (lightRenderer == null) return;
        lightRenderer.material = isRed ? redMaterial : greenMaterial;
    }
}