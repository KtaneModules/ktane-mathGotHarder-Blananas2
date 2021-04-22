using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;

public class meteorScript : MonoBehaviour {

    public KMBombInfo Bomb;
    public KMAudio Audio;

    public KMSelectable[] Cards;
    public KMSelectable Call;
    public TextMesh Number;
    public TextMesh OtherNumber;

    int Limit = 10;
    private List<int> Sequence = new List<int> {  };
    bool Valid = false;
    int Gap = 1;
    string SeqText = "";
    int Current = 0;
    string Thing = "";

    //Logging
    static int moduleIdCounter = 1;
    int moduleId;
    private bool moduleSolved;

    void Awake () {
        moduleId = moduleIdCounter++;

        foreach (KMSelectable Card in Cards) {
            Card.OnInteract += delegate () { CardPress(Card); return false; };
        }


        Call.OnInteract += delegate () { CallPress(); return false; };

    }

    // Use this for initialization
    void Start () {
        GenerateSequence();
    }

    void GenerateSequence () {
        Valid = false;
        Current = 0;
        SeqText = "";
        Thing = "";
        Sequence.Clear();
        while (!Valid) {
            Sequence.Add(UnityEngine.Random.Range(0, Limit));
            SeqText += Sequence[Sequence.Count() - 1].ToString();
            Gap = 1;
            while (2*Gap + 1 <= Sequence.Count()) {
                if (((Sequence[Sequence.Count() - 2*Gap - 1]) - (Sequence[Sequence.Count() - Gap - 1])) == ((Sequence[Sequence.Count() - Gap - 1]) - (Sequence[Sequence.Count() - 1]))) {
                    Valid = true;
                    Thing = Sequence[Sequence.Count() - 2*Gap - 1].ToString() + "_" + Sequence[Sequence.Count() - Gap - 1].ToString() + "_" + Sequence[Sequence.Count() - 1].ToString() + ";" + (Gap-1).ToString();
                }
                Gap += 1;
            }
        }
        Debug.LogFormat("[Meteor #{0}] Sequence: {1}", moduleId, SeqText);
        Debug.LogFormat("[Meteor #{0}] Stop on Card #{1} ({2})", moduleId, Sequence.Count(), Thing);
        Number.text = Sequence[0].ToString();
        OtherNumber.text = "1";
    }

    void CardPress(KMSelectable Card) {
        Card.AddInteractionPunch(0.5f);
        if (!moduleSolved) {
            if (Card == Cards[0]) {
                if (Current != 0) {
                    Current -= 1;
                    Audio.PlaySoundAtTransform("exish", transform);
                }
            } else {
                Current += 1;
                Audio.PlaySoundAtTransform("exish", transform);
            }
            if (Current == Sequence.Count()) {
                StartCoroutine(YouFuckedUp());
            } else {
                Number.text = Sequence[Current].ToString();
                OtherNumber.text = (Current+1).ToString();
            }
        }
    }

    public IEnumerator YouFuckedUp () {
        Number.text = "!";
        OtherNumber.text = Thing;
        yield return new WaitForSeconds(1f);
        GetComponent<KMBombModule>().HandleStrike();
        Debug.LogFormat("[Meteor #{0}] You moved past the trap card. Strike! Generating new sequence...", moduleId);
        GenerateSequence();
        yield return null;
    }

    void CallPress() {
        Call.AddInteractionPunch();
        GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        if (Current == Sequence.Count() - 1) {
            GetComponent<KMBombModule>().HandlePass();
            moduleSolved = true;
            Debug.LogFormat("[Meteor #{0}] You submitted just before the trap card. That is correct. Module solved.", moduleId);
        } else {
            GetComponent<KMBombModule>().HandleStrike();
            Debug.LogFormat("[Meteor #{0}] You submitted in the wrong spot (Card #{1}), strike!", moduleId, Current);
        }
    }

}
