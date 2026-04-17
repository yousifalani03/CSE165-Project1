using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class TwoHandScale : MonoBehaviour
{
    private XRGrabInteractable grabInteractable;
    private Vector3 initialScale;
    private float initialDistance;
    private bool isScaling = false;
    
    private Transform firstHand;
    private Transform secondHand;

    void Start()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnGrab);
            grabInteractable.selectExited.AddListener(OnRelease);
        }
    }

    void Update()
    {
        if (isScaling && firstHand != null && secondHand != null)
        {
            float currentDistance = Vector3.Distance(firstHand.position, secondHand.position);
            float scaleFactor = currentDistance / initialDistance;
            transform.localScale = initialScale * scaleFactor;
        }
    }

    void OnGrab(SelectEnterEventArgs args)
    {
        if (firstHand == null)
        {
            firstHand = args.interactorObject.transform;
        }
        else if (secondHand == null)
        {
            secondHand = args.interactorObject.transform;
            initialDistance = Vector3.Distance(firstHand.position, secondHand.position);
            initialScale = transform.localScale;
            isScaling = true;
        }
    }

    void OnRelease(SelectExitEventArgs args)
    {
        if (args.interactorObject.transform == secondHand)
        {
            secondHand = null;
            isScaling = false;
        }
        else if (args.interactorObject.transform == firstHand)
        {
            firstHand = secondHand;
            secondHand = null;
            isScaling = false;
        }
    }
}