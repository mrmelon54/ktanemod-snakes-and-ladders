using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;

using Random = UnityEngine.Random;

public class snakesAndLaddersModScript : MonoBehaviour {
    public KMAudio BombAudio;
    public KMBombInfo BombInfo;
    public KMBombModule BombModule;
    public KMSelectable ModuleSelect;

    private List<GameObject> SquareBox=new List<GameObject>();
    public GameObject BoardObject;
    public GameObject SquarePrefab;

    bool moduleSolved;

    static int moduleIdCounter = 1;
    int moduleId;

    void Start() {
        moduleId = moduleIdCounter++;
        GenerateBoard();
    }

    void GenerateBoard() {
        var boardStr="";
        for (int i = 0; i < 100; i++) {
            int j = i;

            int r=Random.Range(0,3);

            boardStr+=r.ToString();
            if(i%10==0)boardStr+="/";

            GameObject temp=Instantiate(SquarePrefab,new Vector3(0f,0f,0f),Quaternion.identity);

            SquareBox.Add(temp);

            ModuleSelect.Children.Add(SquareBox[i].GetComponent<KMSelectable>());
            SquareBox[i].GetComponent<KMSelectable>().Parent=ModuleSelect;

            SquareBox[i].transform.SetParent(BoardObject.transform);
            SquareBox[i].GetComponent<SquareScript>().SetNumber(i+1);
            SquareBox[i].GetComponent<SquareScript>().SetColour(r);

            SquareBox[i].GetComponent<KMSelectable>().OnInteract += delegate() {
                PressSquare(j);
                return false;
            };
        }
        doLog(boardStr);
    }

    void doLog(String m) {
        Debug.LogFormat("[Snakes and Ladders #{0}] {1}",moduleId,m);
    }

    void PressSquare(int buttonId) {
        BombAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        ModuleSelect.AddInteractionPunch();

        if (moduleSolved) {
            return;
        }

        doLog("Button pressed "+buttonId.ToString());

        return;

        /*String buttonText=SquareBox[buttonId].gameObject.name.Replace("Button","");

        String myTextSpliting="";
        for(int i=0;i<myText.Length;i++) {
            myTextSpliting+=myText[i]+".";
        }
        String[] myTextSplit=myTextSpliting.Split('.');

        if(ArrayCountAnArray(myTextSplit,"0.1.2.3.4.5.6.7.8.9".Split('.'))==2) {
            int TimerAsInteger=int.Parse(BombInfo.GetTime().ToString().Split('.')[0]);
            if(((TimerAsInteger%60)%10).ToString()==buttonText) {
                myText+=buttonText;
            } else {
                doLog("Press this digit when the last digit of the seconds is equal to it");
                BombModule.HandleStrike();
            }
        } else {
            myText+=buttonText;
        }

        if(myText.Length>7) {
            myText=myText.Remove(myText.Length-1,1);
        }

        PrepareRenderReadyText();
        RenderScreen();*/
    }
/*
#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"Submit your answer with “!{0} press 1|R|Y|9|P|0s0|3 (add sX [where 'X' is a digit] to press the button when the last seconds digit of the bomb is 'X')”. Delete screen with “!{0} delete 5 (number of times to press the button)”. Submit answer with “!{0} go 40 (submit when the seconds of the bomb is the number)”.";
#pragma warning restore 414

    IEnumerator ProcessTwitchCommand(string command) {
        command = command.ToLowerInvariant().Trim();

        if (Regex.IsMatch(command, @"^press +[0-9roygbps|]+$")) {
            command = command.Substring(6).Trim();
            var presses = command.Split('|');

            for (var i = 0; i < presses.Length; i++) {
                KMSelectable pressButton;

                if (Regex.IsMatch(presses[i], @"^[0-9]$") || Regex.IsMatch(presses[i], @"^[0-9][s][0-9]$")) {
                    pressButton = SquareBox[int.Parse(presses[i].First().ToString())];

                    if (Regex.IsMatch(presses[i], @"^[0-9][s][0-9]$")) {
                        string formattedTime;

                        do {
                            formattedTime = BombInfo.GetFormattedTime();

                            if (BombInfo.GetTime() < 60f)
                                formattedTime = BombInfo.GetFormattedTime().Substring(0, 2);

                            yield return new WaitForSeconds(0.1f);
                        } while (int.Parse(formattedTime.Last().ToString()) != int.Parse(presses[i].Last().ToString()));
                    }
                } else if (Regex.IsMatch(presses[i], @"^[r|o|y|g|b|p]$")) {
                    var colorLetters = new[] { "r", "o", "y", "g", "b", "p" };
                    pressButton = ColouredButtons[Array.IndexOf(colorLetters, presses[i])];
                } else {
                    continue;
                }

                yield return pressButton;
                yield return new WaitForSeconds(0.1f);
                yield return pressButton;
            }
        }

        if (Regex.IsMatch(command, @"^delete [1-7]$")) {
            yield return null;
            for (var i = 0; i < int.Parse(command.Substring(7).Trim()); i++) {
                yield return deleteButton;
                yield return new WaitForSeconds(0.1f);
                yield return deleteButton;
            }
        }

        if (Regex.IsMatch(command, @"^go \d\d$")) {
            command = command.Substring(3);

            if (int.Parse(command) < 60) {
                string formattedTime;

                do {
                    formattedTime = BombInfo.GetFormattedTime();

                    if (BombInfo.GetTime() < 60f) {
                        formattedTime = BombInfo.GetFormattedTime().Substring(0, 2);
                    } else {
                        formattedTime = formattedTime.Substring(formattedTime.Length - 2, 2);
                    }

                    yield return new WaitForSeconds(0.1f);
                } while (int.Parse(formattedTime) != int.Parse(command.ToString()));

                yield return submitButton;
                yield return new WaitForSeconds(0.1f);
                yield return submitButton;
            }
        }

        yield break;
    }*/
}
