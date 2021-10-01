using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MenuManager))]
public class MainMenuIntroSequence : MonoBehaviour
{
    MenuManager manager;

    public Animator fade;
    public GameObject introVideo;
    public GameObject splashScreen;

    public float timer = 2;

    // Start is called before the first frame update
    void Start()
    {
        manager = GetComponent<MenuManager>();
        StartCoroutine(Sequencer(timer));
    }

    IEnumerator Sequencer(float time)
    {
        introVideo.SetActive(true);
        yield return new WaitForSeconds(time);
        fade.Play("Fade-In");
        yield return new WaitForSeconds(2f);
        introVideo.SetActive(false);
        splashScreen.SetActive(true);
        fade.Play("Fade-Out");
        yield return new WaitForSeconds(time);
        fade.Play("Fade-In");
        yield return new WaitForSeconds(2f);
        splashScreen.SetActive(false);
        fade.Play("Fade-Out");
        yield return new WaitForSeconds(2.5f);
        manager.canMove = true;
    }
}
