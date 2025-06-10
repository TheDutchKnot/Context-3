using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FinalDialogue : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI textComponent;
    [SerializeField] string[] textLines;
    [SerializeField] float textSpeed;
    [SerializeField] float TimeUntillnextLine;

    private int index;

    [SerializeField] GameObject fadeOut;


    void Start()
    {
        textComponent.text = string.Empty;
        startDialogue();
    }

    void startDialogue()
    {
        index = 0;
        StartCoroutine(TypeLine());
    }

    IEnumerator TypeLine()
    {
        foreach (char c in textLines[index].ToCharArray())
        {
            textComponent.text += c;
            yield return new WaitForSeconds(textSpeed);
        }

        yield return new WaitForSeconds(TimeUntillnextLine);
        NextLine();
    }

    void NextLine()
    {
        if (index < textLines.Length - 1)
        {
            index++;
            textComponent.text = string.Empty;
            StartCoroutine(TypeLine());
        }
        else
        {
            gameObject.SetActive(false);
            fadeOut.SetActive(true);
        }
    }
}

