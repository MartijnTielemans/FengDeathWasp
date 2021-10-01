using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class LevelEndSequence : MonoBehaviour
{
    public InputAction continueButton;
    public Animator[] textObjects;
    public Animator fade;
    [SerializeField] float timer;
    [SerializeField] bool canContinue;

    private void OnEnable()
    {
        continueButton.Enable();
    }

    private void OnDisable()
    {
        continueButton.Disable();
    }

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < textObjects.Length; i++)
        {
            StartCoroutine(EndSequence(textObjects[i], timer * i +1));
        }

        StartCoroutine(SetCanContinue(textObjects.Length, timer));
    }

    private void Update()
    {
        if (canContinue && continueButton.triggered)
        {
            StartCoroutine(FadeOutAndNext());
        }
    }

    IEnumerator EndSequence(Animator text, float time)
    {
        yield return new WaitForSeconds(time);

        text.Play("Text_FadeIn");
    }

    IEnumerator SetCanContinue(float multiplier, float time)
    {
        yield return new WaitForSeconds(time * multiplier);

        canContinue = true;
    }

    IEnumerator FadeOutAndNext()
    {
        fade.Play("Fade-In");

        yield return new WaitForSeconds(2f);

        SceneManager.LoadScene("MainMenu");
    }
}
