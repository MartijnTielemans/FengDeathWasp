using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MenuButton : MonoBehaviour
{
    MenuManager manager;

    RectTransform currentPosition;
    Vector2 originalPosition;
    Vector2 selectedPosition;

    bool doOnce;

    private void Start()
    {
        manager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<MenuManager>();

        // Set positions
        currentPosition = GetComponent<RectTransform>();
        originalPosition = currentPosition.anchoredPosition;
        selectedPosition = originalPosition + Vector2.right * 5;
    }

    private void Update()
    {
        if (EventSystem.current.currentSelectedGameObject == this.gameObject)
        {
            currentPosition.anchoredPosition = new Vector2 (Mathf.Lerp(currentPosition.anchoredPosition.x, selectedPosition.x, 5 * Time.deltaTime), currentPosition.anchoredPosition.y);
            if (!doOnce)
                doOnce = true;
        }
        else if (EventSystem.current.currentSelectedGameObject != this.gameObject && originalPosition != GetComponent<RectTransform>().anchoredPosition)
        {
            currentPosition.anchoredPosition = new Vector2 (Mathf.Lerp(currentPosition.anchoredPosition.x, originalPosition.x, 5 * Time.deltaTime), currentPosition.anchoredPosition.y);
            if (doOnce)
            {
                manager.PlaySound(0);
                doOnce = false;
            }
        }
    }

    public void ChangeScene(string scene)
    {
        if (manager.canMove)
        {
            manager.ChangeScene(scene, 2f);
            manager.PlaySound(1);
        }
    }

    public void ShowControls()
    {
        if (manager.canMove)
        {
            manager.ActivateControlsScreen();
            manager.PlaySound(1);
        }
    }

    public void ExitApplication()
    {
        if (manager.canMove)
        {
            manager.QuitApplication(1f);
            manager.PlaySound(1);
        }
    }
}
