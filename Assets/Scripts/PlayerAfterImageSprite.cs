using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAfterImageSprite : MonoBehaviour
{
    // Serialized Field allows us to Manage These in the Inspector
    [SerializeField]
    private float activeTime = 0.1f;
    private float timeActivated;
    private float alpha;
    [SerializeField]
    private float alphaSet = 0.8f;
    private float alphaMultiplier = .85f;

    private Transform player;

    private SpriteRenderer SR;
    private SpriteRenderer playerSR;

    private Color color;

    // Gets Called Anytime the Game Object is Enabled
    private void OnEnable()
    {
        SR = GetComponent<SpriteRenderer>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        playerSR = player.GetComponent<SpriteRenderer>();

        alpha = alphaSet;

        // DEBUG
        if (playerSR.sprite != null)
        {
            Debug.Log("Splayer Sprite is Assigned: " +  playerSR.sprite.name);
        }
        else
        {
            Debug.LogError("Player sprite is not assigned!");
        }

        SR.sprite = playerSR.sprite;
        // DEBUG
        Debug.Log("Afterimage Sprite Set to: " + SR.sprite.name);

        transform.position = player.position;
        transform.rotation = player.rotation;
        timeActivated = Time.time;

        // DEBUG
        Debug.Log("Afterimage Enabled at Position: " + transform.position);
    }

    private void Update()
    {
        alpha *= alphaMultiplier;
        color = new Color(1f, 1f, 1f, alpha);
        SR.color = color;

        // DEBUG
        Debug.Log($"Afterimage color set to: {color}");

        if(Time.time >= (timeActivated + activeTime)) 
        {
            PlayerAfterImagePool.Instance.AddToPool(gameObject);
            // DEBUG
            Debug.Log("Afterimage Deactivated");
        }
    }
}
