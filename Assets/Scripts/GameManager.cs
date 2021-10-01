using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public GameObject uICanvas;
    public GameObject healthPoint;
    [SerializeField] List<GameObject> healthPoints = new List<GameObject>();
    public Vector2 healthInitialPosition;
    public float healthSpacing;

    [SerializeField] List<GameObject> enemyList;

    private void Awake()
    {
        gameObject.tag = "GameManager";

        // Grab every enemy in the scene
        enemyList.AddRange(GameObject.FindGameObjectsWithTag("Enemy"));
    }

    // Spawn in the health points according to the player's health and store them
    public void SpawnHealthPoints(int health)
    {
        for (int i = 0; i < health; i++)
        {
            // Set the initial position
            Vector2 position = healthInitialPosition;
            position.x += (i * healthSpacing); // Change position on the x
            GameObject go = Instantiate(healthPoint, position, Quaternion.identity, uICanvas.transform); // Instantiate the prefab
            go.GetComponent<RectTransform>().anchoredPosition = position; // Set the rectTransform anchoredposition to position;
            healthPoints.Add(go);
        }
    }

    // Sets health point gameobjects active or inactive
    public void ChangeHealthPoints(int currentHealth)
    {
        // Deactivate all points
        for (int i = 0; i < healthPoints.Count; i++)
        {
            healthPoints[i].SetActive(false);
        }

        // Then activate only the ones needed
        for (int i = 0; i < currentHealth; i++)
        {
            healthPoints[i].SetActive(true);
        }
    }

    // Resets enemy positions, health and canMove
    public void ResetEnemies()
    {
        for (int i = 0; i < enemyList.Count; i++)
        {
            enemyList[i].SetActive(true);
            enemyList[i].GetComponent<Hazard>().Reset();
        }
    }

    public void StartLoadScene(string scene, float time)
    {
        StartCoroutine(LoadScene(scene, time));
    }

    IEnumerator LoadScene(string scene, float time)
    {
        yield return new WaitForSeconds(time);
        SceneManager.LoadScene(scene);
    }
}
