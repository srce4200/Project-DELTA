using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadBob : MonoBehaviour
{
     private Transform headTransform;
    [SerializeField] private float bobFrequency = 2f;
    [SerializeField] private float bobAmplitude = 0.1f;

    private float timer = 0f;
    private Vector3 originalLocalPosition;

    private void Start()
    {
        if (headTransform == null)
            headTransform = transform;

        originalLocalPosition = headTransform.localPosition;
    }

    private void Update()
    {
        // Calculate the vertical head bobbing motion
        float bobX = Mathf.Sin(timer * bobFrequency) * bobAmplitude;
        float bobY = Mathf.Cos(timer * bobFrequency * 2f) * bobAmplitude * 0.5f;
        Vector3 bobOffset = new Vector3(bobX, bobY, 0f);

        // Apply the bobbing motion to the head position
        headTransform.localPosition = originalLocalPosition + bobOffset;

        // Update the timer for the bobbing motion
        timer += Time.deltaTime;
    }
}

