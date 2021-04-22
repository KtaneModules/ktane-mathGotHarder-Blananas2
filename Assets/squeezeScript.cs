using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;

public class squeezeScript : MonoBehaviour {

    public KMBombInfo Bomb;
    public KMAudio Audio;
    public KMSelectable[] Buttons;
    public TextMesh[] Texts;
    public Color[] Colors; //white, selected, halve, double, green
    public GameObject EntireThing;
    public GameObject[] ButtonObjects;

    private string numberstring = "";
    private int numberint = -1;
    private int state = 0; //0 = nothing | 1 = 1 digit selected | 2 = multiple digit subnumber selected | 3 = single digit subnumber selected
    private int firstdigitchosen = -1;
    private int seconddigitchosen = -1;
    private string subnumberstring = "";
    private int subnumberint = -1;
    private string presub = "";
    private string postsub = "";
    private string originalstring = "";
    private int blinkingnumberstate = -1;

    //Logging
    static int moduleIdCounter = 1;
    int moduleId;
    private bool moduleSolved;

    void Awake () {
        moduleId = moduleIdCounter++;

        foreach (KMSelectable button in Buttons) {
            button.OnInteract += delegate () { buttonPress(button); return false; };
        }


    }

    // Use this for initialization
    void Start () {
        numberint = UnityEngine.Random.Range(10,10000000);
        numberstring = numberint.ToString();
        originalstring = numberstring;
        Debug.LogFormat("[Squeeze #{0}] Number starts at {1}.", moduleId, originalstring);
        ChangeButtons();
    }

    void buttonPress(KMSelectable button) {
        button.AddInteractionPunch(0.1f);
        for (int i = 0; i < 9; i++) {
            if (button == Buttons[i]) {
                switch (state) {
                    case 0:
                        firstdigitchosen = i;
                        state = 1;
                        Texts[i].color = Colors[1];
                    break;
                    case 1:
                        seconddigitchosen = i;
                        if (firstdigitchosen > seconddigitchosen) {
                            SwapTheTwoValues();
                        }
                        for (int k = firstdigitchosen; k < seconddigitchosen; k++) {
                            Texts[k].color = Colors[1];
                        }
                        Texts[firstdigitchosen].color = Colors[2];
                        Texts[seconddigitchosen].color = Colors[3];
                        state = 2;
                        subnumberstring = numberstring.Substring(firstdigitchosen, (seconddigitchosen - firstdigitchosen) + 1);
                        if (firstdigitchosen == 0) {
                            presub = "";
                        } else {
                            presub = numberstring.Substring(0, firstdigitchosen);
                        }
                        if (seconddigitchosen == numberstring.Length-1) {
                            postsub = "";
                        } else {
                            postsub = numberstring.Substring(seconddigitchosen + 1, (numberstring.Length-1)-seconddigitchosen);
                        }
                        Debug.LogFormat("[Squeeze #{0}] Selected {1}({2}){3}.", moduleId, presub, subnumberstring, postsub);
                        subnumberint = Int32.Parse(subnumberstring);
                        if (subnumberint % 2 == 1) {
                            subnumberint *= 2;
                            numberstring = presub + subnumberint.ToString() + postsub;
                            state = 0;
                            Debug.LogFormat("[Squeeze #{0}] Doubled. Results in {1}", moduleId, numberstring);
                            Audio.PlaySoundAtTransform("doub", transform);
                            ChangeButtons();
                        } else if (firstdigitchosen == seconddigitchosen) {
                            StartCoroutine(Blink(seconddigitchosen));
                            state = 3;
                        }
                    break;
                    case 2:
                        if (i == firstdigitchosen) {
                            subnumberint /= 2;
                            numberstring = presub + subnumberint.ToString() + postsub;
                            state = 0;
                            Debug.LogFormat("[Squeeze #{0}] Halved. Results in {1}", moduleId, numberstring);
                            Audio.PlaySoundAtTransform("halv", transform);
                            ChangeButtons();
                        } else if (i == seconddigitchosen) {
                            subnumberint *= 2;
                            numberstring = presub + subnumberint.ToString() + postsub;
                            state = 0;
                            Debug.LogFormat("[Squeeze #{0}] Doubled. Results in {1}", moduleId, numberstring);
                            Audio.PlaySoundAtTransform("doub", transform);
                            ChangeButtons();
                        }
                    break;
                    case 3:
                        if (blinkingnumberstate == 0) {
                            subnumberint /= 2;
                            numberstring = presub + subnumberint.ToString() + postsub;
                            state = 0;
                            Debug.LogFormat("[Squeeze #{0}] Halved. Results in {1}", moduleId, numberstring);
                            Audio.PlaySoundAtTransform("halv", transform);
                            ChangeButtons();
                        } else {
                            subnumberint *= 2;
                            numberstring = presub + subnumberint.ToString() + postsub;
                            state = 0;
                            Debug.LogFormat("[Squeeze #{0}] Doubled. Results in {1}", moduleId, numberstring);
                            Audio.PlaySoundAtTransform("doub", transform);
                            ChangeButtons();
                        }
                        StopAllCoroutines();
                    break;
                    default: break;
                }
            }
        }
    }

    void SwapTheTwoValues() {
        int temp = seconddigitchosen;
        seconddigitchosen = firstdigitchosen;
        firstdigitchosen = temp;
    }

    void ChangeButtons() {
        if (numberstring.Length > 9) {
            numberstring = originalstring;
            Debug.LogFormat("[Squeeze #{0}] Number is too long, resetting back to {1}.", moduleId, originalstring);
        }
        for (int k = 0; k < 9; k++) {
            Texts[k].text = "";
            Texts[k].color = Colors[0];
            ButtonObjects[k].SetActive(true);
        }
        for (int j = 0; j < numberstring.Length; j++) {
            Texts[j].text = numberstring[j].ToString();
        }
        EntireThing.transform.localPosition = new Vector3(0.01f * (9-numberstring.Length), 0f, 0f);
        for (int l = numberstring.Length; l < 9; l++) {
            ButtonObjects[l].SetActive(false);
        }
        if (numberstring.Length == 1) {
            GetComponent<KMBombModule>().HandlePass();
            Debug.LogFormat("[Squeeze #{0}] Number is a single digit. Module solved.", moduleId);
        }
    }

    private IEnumerator Blink(int x) {
        Redo:
        blinkingnumberstate = 0;
        Texts[x].color = Colors[2];
        yield return new WaitForSeconds(0.5f);
        blinkingnumberstate = 1;
        Texts[x].color = Colors[3];
        yield return new WaitForSeconds(0.5f);
        goto Redo;
        yield return null;
    }

}
