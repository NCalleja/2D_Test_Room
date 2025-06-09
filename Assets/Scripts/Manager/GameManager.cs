using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{

    [SerializeField]
    private Transform respawnPoint;

    [SerializeField]
    private GameObject player;

    [SerializeField]
    private float respawnTime;

    private float respawnTimeStart;

    private bool respawn;

    private CinemachineVirtualCamera CVC;

    // Tripod Test Build
    private int activeTripods = 0;
    private bool isRestarting = false;

    private void Start()
    {
        CVC = GameObject.Find("Player Camera").GetComponent<CinemachineVirtualCamera>();
    }

    private void Update()
    {
        CheckRespawn();
    }

    public void Respawn()
    {
        respawnTimeStart = Time.time;

        respawn = true;
    }

    private void CheckRespawn()
    {
        if(Time.time >= respawnTimeStart + respawnTime && respawn)
        {
            var playerTemp = Instantiate(player, respawnPoint);

            CVC.m_Follow = playerTemp.transform;

            SceneManager.LoadScene(SceneManager.GetActiveScene().name);

            respawn = false;
        }
    }

    // Registering Tripods for the Tripod Test Build
    public void RegisterTripod()
    {

        activeTripods++;
        Debug.Log("Tripod Registered. Total: " + activeTripods);

    }

    // Unregistering Tripods for the Tripod Test Build
    public void UnregisteredTripods()
    {
        activeTripods--;
        Debug.Log("Tripod Unregistered. Remaining: " + activeTripods);

        if(activeTripods <= 0 && !isRestarting)
        {
            isRestarting = true;
            Debug.Log("All Tripods Defeated! Restarting Level...");
            Invoke("RestartScene", 2f);
        }
    }

    public void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        
    }



}
