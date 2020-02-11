using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowText : MonoBehaviour {

    [SerializeField] float delay = default;
    [SerializeField] Text txt =default;

    int index;
    int maxIndex;
    string fullText;

    [SerializeField] bool useAnimation = default;

    public void RevealText(string textToReveal)
    {
        index = 0;
        maxIndex = textToReveal.Length;
        fullText = textToReveal;
        txt.text = "";
        if(useAnimation)
        {
            StopAllCoroutines();
            StartCoroutine(ReviewDelay());
        }
        else
        {
            txt.text = textToReveal;
        }
        
    }

    IEnumerator ReviewDelay()
    {
        if(index != 0 && txt.text[txt.text.Length - 1] == '_')
        {
            txt.text = txt.text.Substring(0, txt.text.Length - 1);
        }
        txt.text += fullText[index];
        //txt.text += "_";        
        yield return new WaitForSeconds(delay);
        index++;
        if (index < maxIndex)
        {            
            StartCoroutine(ReviewDelay());
        }
        else
        {
            if (index != 0 && txt.text[txt.text.Length - 1] == '_')
            {
                txt.text = txt.text.Substring(0, txt.text.Length - 1);
            }
        }
    }

}
