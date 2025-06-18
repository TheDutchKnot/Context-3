using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;


public class DialogueSystem : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI textComponent;
    [SerializeField] string[] textLines;
    [SerializeField] float textSpeed;
    [SerializeField] float TimeUntillnextLine;
    [SerializeField] float TImeBeforeFirstLine = 0;

    private int index;

    Image backdrop;

    bool first;

    void Start()
    {
        textComponent.text = string.Empty;
        startDialogue();
        backdrop = GetComponent<Image>();
    }

    void startDialogue()
    {
        index = 0;
        StartCoroutine(TypeLine());
    }

    IEnumerator TypeLine()
    {
        if (!first)
        {
            yield return new WaitForSeconds(TImeBeforeFirstLine);
            backdrop.enabled = true;
        }
        first = true;

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
        }
    }
}
