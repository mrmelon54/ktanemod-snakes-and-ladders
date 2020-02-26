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

    public Material redMat;
    public Material blueMat;
    public Material greenMat;
    public Material yellowMat;

    public GameObject[] SquareBox;
    public GameObject BoardObject;
    public GameObject SquarePrefab;
    public GameObject PlayerBox;
    public GameObject forcesolveddialog;

    public GameObject[] ladders;
    public GameObject[] snakes;

    public bool moveSquareBtn=false;
    private bool moveSquare=false;
    private int currentSquareId=0;
    private Vector3 lightOldPos=new Vector3(0f,0f,0f);
    private Vector3 lightNewPos=new Vector3(0f,0f,0f);
    private float t=0f;

    private int[] squareNumberGrid={
        100,99,98,97,96,95,94,93,92,91,
        81,82,83,84,85,86,87,88,89,90,
        80,79,78,77,76,75,74,73,72,71,
        61,62,63,64,65,66,67,68,69,70,
        60,59,58,57,56,55,54,53,52,51,
        41,42,43,44,45,46,47,48,49,50,
        40,39,38,37,36,35,34,33,32,31,
        21,22,23,24,25,26,27,28,29,30,
        20,19,18,17,16,15,14,13,12,11,
        1,2,3,4,5,6,7,8,9,10
    };
    private List<int> beenOn=new List<int>();
    private List<int> showLadders=new List<int>();
    private List<int> showSnakes=new List<int>();

    private int nextCorrectSquare=0;
    private int toAddOn=0;
    private int lastRoll=0;
    private bool preventModuleUsage=true;
    private float lightspeed = 10f;

    bool moduleSolved;
    bool cooldown;

    static int moduleIdCounter = 1;
    int moduleId;

    void Start() {
        moduleId = moduleIdCounter++;
        foreach (KMSelectable button in ModuleSelect.Children)
        {
            button.OnHighlight += delegate () { HLButton(button); };
            button.OnHighlightEnded += delegate () { HLEndButton(); };
        }
        for (int i = 0; i < ladders.Length; i++)
        {
            ladders[i].SetActive(false);
        }
        for (int i = 0; i < snakes.Length; i++)
        {
            snakes[i].SetActive(false);
        }
        for (int i = 0; i < SquareBox.Length; i++)
        {
            SquareBox[i].GetComponent<SquareScript>().text.GetComponent<TextMesh>().text = "";
        }
        BombModule.OnActivate += OnActivate;

        cooldown=true;
        moveSquare=true;
        currentSquareId=0;
        lightOldPos=PlayerBox.transform.localPosition;
        lightNewPos=PlayerBox.transform.parent.InverseTransformPoint(SquareBox[currentSquareId].transform.position);
        t=.999f;
    }

    void OnActivate()
    {
        GenerateBoard();
        cooldown = false;
    }

    void HLButton(KMSelectable hovered)
    {
        if (moduleSolved == false && cooldown == false)
        {
            for(int i = 0; i < SquareBox.Length; i++)
            {
                if(i == 99)
                {
                    SquareBox[99].GetComponent<SquareScript>().text.GetComponent<TextMesh>().text = ":)";
                }
                else
                {
                    if (SquareBox[i].GetComponent<KMSelectable>() == hovered)
                    {
                        if (SquareBox[i].GetComponent<SquareScript>().GetColour() == 0)
                        {
                            SquareBox[99].GetComponent<SquareScript>().text.GetComponent<TextMesh>().text = "R";
                        }
                        else if (SquareBox[i].GetComponent<SquareScript>().GetColour() == 1)
                        {
                            SquareBox[99].GetComponent<SquareScript>().text.GetComponent<TextMesh>().text = "B";
                        }
                        else if (SquareBox[i].GetComponent<SquareScript>().GetColour() == 2)
                        {
                            SquareBox[99].GetComponent<SquareScript>().text.GetComponent<TextMesh>().text = "G";
                        }
                        else
                        {
                            SquareBox[99].GetComponent<SquareScript>().text.GetComponent<TextMesh>().text = "Y";
                        }
                        break;
                    }
                }
            }
        }
    }

    void HLEndButton()
    {
        if(moduleSolved == false && cooldown == false)
            SquareBox[99].GetComponent<SquareScript>().text.GetComponent<TextMesh>().text = "100";
    }

    void Update() {
        if(moveSquareBtn) {
            moveSquareBtn=false;
            moveSquare=true;
            currentSquareId++;
            if(currentSquareId>99)currentSquareId=0;
            lightOldPos=PlayerBox.transform.localPosition;
            lightNewPos=PlayerBox.transform.parent.InverseTransformPoint(SquareBox[currentSquareId].transform.position);
            t=0f;
            animateLight();
        }
        if(moveSquare && !cooldown){
            animateLight();
        }
    }

    void animateLight() {
        t+=lightspeed*Time.deltaTime;
        if(t>1f){
            t=1f;
            if(toAddOn>0){
                currentSquareId++;
                toAddOn--;
                if(currentSquareId>99){
                    currentSquareId=99;
                    toAddOn=0;
                }
                lightOldPos=PlayerBox.transform.localPosition;
                lightNewPos=PlayerBox.transform.parent.InverseTransformPoint(SquareBox[currentSquareId].transform.position);
                t=0f;
            } else {
                if(beenOn.Contains(currentSquareId)) {
                    moveXSquares();
                    return;
                }
                for(int i=0;i<ladders.Length;i++) {
                    if(!showLadders.Contains(i))continue;
                    LadderScript ls=ladders[i].GetComponent<LadderScript>();
                    if(ls.start==currentSquareId+1){
                        int rando = Random.Range(0, 3);
                        if (rando == 0)
                            BombAudio.PlaySoundAtTransform("ladder1", SquareBox[currentSquareId].transform);
                        else if (rando == 1)
                            BombAudio.PlaySoundAtTransform("ladder2", SquareBox[currentSquareId].transform);
                        else if (rando == 2)
                            BombAudio.PlaySoundAtTransform("ladder3", SquareBox[currentSquareId].transform);
                        currentSquareId =ls.end-1;
                        lightOldPos=PlayerBox.transform.localPosition;
                        lightNewPos=PlayerBox.transform.parent.InverseTransformPoint(SquareBox[currentSquareId].transform.position);
                        t=0f;
                        animateLight();
                        return;
                    }
                }
                for(int i=0;i<snakes.Length;i++) {
                    if(!showSnakes.Contains(i))continue;
                    SnakeScript ss=snakes[i].GetComponent<SnakeScript>();
                    if(ss.start==currentSquareId+1){
                        int rando = Random.Range(0, 3);
                        if (rando == 0)
                            BombAudio.PlaySoundAtTransform("snake1", SquareBox[ss.end - 1].transform);
                        else if (rando == 1)
                            BombAudio.PlaySoundAtTransform("snake2", SquareBox[ss.end - 1].transform);
                        else if (rando == 2)
                            BombAudio.PlaySoundAtTransform("snake3", SquareBox[ss.end - 1].transform);
                        currentSquareId =ss.end-1;
                        lightOldPos=PlayerBox.transform.localPosition;
                        lightNewPos=PlayerBox.transform.parent.InverseTransformPoint(SquareBox[currentSquareId].transform.position);
                        t=0f;
                        animateLight();
                        return;
                    }
                }
                t=1f;
                moveSquare=false;
                preventModuleUsage=false;
                beenOn.Add(currentSquareId);
                CalculateNextAnswer();
            }
        }
        Vector3 p=lightOldPos;
        Vector3 n=lightNewPos;
        PlayerBox.transform.localPosition=Vector3.Lerp(p, n, t);
    }

    void GenerateBoard() {
        if(ladders.Length>4) {
            for(int i=0;i<4;i++) {
                int r=Random.Range(0,ladders.Length);
                while(showLadders.Contains(r)) {
                    r=Random.Range(0,ladders.Length);
                }
                showLadders.Add(r);
                doLog("ladder " + (i + 1) + ": " + ladders[showLadders[i]].GetComponent<LadderScript>().start + " to " + ladders[showLadders[i]].GetComponent<LadderScript>().end);
            }
        }

        if(snakes.Length>4) {
            for(int i=0;i<4;i++) {
                int r=Random.Range(0,snakes.Length);
                while(showSnakes.Contains(r)) {
                    r=Random.Range(0,snakes.Length);
                }
                showSnakes.Add(r);
                doLog("snake " + (i + 1) + ": " + snakes[showSnakes[i]].GetComponent<SnakeScript>().start + " to " + snakes[showSnakes[i]].GetComponent<SnakeScript>().end);
            }
        }

        for(int i=0;i<ladders.Length;i++) {
            if(showLadders.Contains(i)) {
                ladders[i].SetActive(true);
            } else {
                ladders[i].SetActive(false);
            }
        }

        for(int i=0;i<snakes.Length;i++) {
            if(showSnakes.Contains(i)) {
                snakes[i].SetActive(true);
            } else {
                snakes[i].SetActive(false);
            }
        }

        var boardStr="";
        for (int i = 0; i < SquareBox.Length; i++) {
            int j = i;

            int r=Random.Range(0,4);

            boardStr+=r.ToString();

            SquareBox[i].GetComponent<SquareScript>().SetMaterials(redMat,blueMat,greenMat,yellowMat);
            SquareBox[i].GetComponent<SquareScript>().SetNumber(i+1);
            SquareBox[i].GetComponent<SquareScript>().SetColour(r);

            if (i%10==9)boardStr+="/";

            SquareBox[i].GetComponent<KMSelectable>().OnInteract += delegate() {
                PressSquare(j);
                return false;
            };
        }
        doLog("board: "+boardStr);
    }

    void CalculateNextAnswer() {
        if(currentSquareId==99) {
            moduleSolved=true;
            doLog("100 square reached! Module Solved!");
            SquareBox[99].GetComponent<SquareScript>().text.GetComponent<TextMesh>().text = "";
            forcesolveddialog.SetActive(false);
            BombModule.HandlePass();
            return;
        }

        int n=SquareBox[currentSquareId].GetComponent<SquareScript>().GetNumber();
        doLog("The status light is currently on the " + n + " square");
        int c=SquareBox[currentSquareId].GetComponent<SquareScript>().GetColour();
        int o=0;
        string[] cards = { "between 34 and 44", "between 89 and 99", "between 12 and 22", "between 23 and 33", "between 45 and 55", "between 67 and 77", "between 78 and 88", "between 56 and 66", "N.O.T.A." };
        string[] cards2 = { "square northwest from the ", "square north from the ", "square northeast from the ", "square west from the ", "", "square east from the ", "square southwest from the ", "square south from the ", "square southeast from the " };
        string[] cards3 = { "11", "10", "5", "8", "6", "4", "3", "2", "N.O.T.A." };
        if (c==0) {
            if (n >= 34 && n <= 44) o = 0;
            else if (n >= 89 && n <= 99) o = 1;
            else if (n >= 12 && n <= 22) o = 2;
            else if (n >= 23 && n <= 33) o = 3;
            else if (n >= 45 && n <= 55) o = 4;
            else if (n >= 67 && n <= 77) o = 5;
            else if (n >= 78 && n <= 88) o = 6;
            else if (n >= 56 && n <= 66) o = 7;
            else o = 8;
            doLog("The number of this square is between " + cards[o] + ", the " + cards2[o] + "current square is correct");
        } else if(c==3) {
            int i=(int)Math.Floor(n/10f)%10;
            if (n < 10) o = 8;
            else if (i == 8) o = 0;
            else if (i == 2) o = 1;
            else if (i == 6) o = 2;
            else if (i == 3) o = 3;
            else if (i == 5) o = 4;
            else if (i == 7) o = 5;
            else if (i == 4) o = 6;
            else if (i == 9) o = 7;
            else o = 8;
            if(o == 8)
                doLog("The number of this square has N.O.T.A. in the ten's digit, the " + cards2[o] + "current square is correct");
            else
                doLog("The number of this square has a " + i + " in the ten's digit, the " + cards2[o] + "current square is correct");
        } else if(c==2) {
            int i=n%10;
            if(i==5)o=0;
            else if(i==3)o=1;
            else if(i==7)o=2;
            else if(i==9)o=3;
            else if(i==4)o=4;
            else if(i==2)o=5;
            else if(i==6)o=6;
            else if(i==8)o=7;
            else o=8;
            if (o == 8)
                doLog("The number of this square has N.O.T.A. in the one's digit, the " + cards2[o] + "current square is correct");
            else
                doLog("The number of this square has a " + i + " in the one's digit, the " + cards2[o] + "current square is correct");
        } else if(c==1) {
            if(n%11==0)o=0;
            else if(n%10==0)o=1;
            else if(n%5==0)o=2;
            else if(n%8==0)o=3;
            else if(n%6==0)o=4;
            else if(n%4==0)o=5;
            else if(n%3==0)o=6;
            else if(n%2==0)o=7;
            else o=8;
            doLog("The number of this square is divisible by " + cards3[o] + ", the " + cards2[o] + "current square is correct");
        }

        nextCorrectSquare=CalculateSquareByPosition(n,o);
        doLog("# of correct square: "+nextCorrectSquare);
    }

    int CalculateSquareByPosition(int n,int o) {
        int p=0;
        for(int i=0;i<squareNumberGrid.Length;i++) {
            if(n==squareNumberGrid[i]){
                p=i;
            }
        }
        int r=(int)Math.Floor(p/10f);
        int c=p%10;

        if(o%3==0)c--;
        if(o%3==2)c++;
        if(Math.Floor(o/3f)==0)r--;
        if(Math.Floor(o/3f)==2)r++;

        if(c<0)c=9;
        if(c>9)c=0;
        if(r<0)r=9;
        if(r>9)r=0;

        return squareNumberGrid[r*10+c];
    }

    void doLog(String m) {
        Debug.LogFormat("[Snakes and Ladders #{0}] {1}",moduleId,m);
    }

    void moveXSquares() {
        int r = (int)BombInfo.GetTime() % 10;

        preventModuleUsage=true;
        moveSquare=true;
        toAddOn=r;
        lastRoll = r;
        if (currentSquareId>99)currentSquareId=99;
        lightOldPos=PlayerBox.transform.localPosition;
        lightNewPos=PlayerBox.transform.parent.InverseTransformPoint(SquareBox[currentSquareId].transform.position);
        t=0f;
        animateLight();
    }

    void PressSquare(int buttonId) {
        BombAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, SquareBox[buttonId].transform);
        ModuleSelect.AddInteractionPunch();
        int t = (int)BombInfo.GetTime() % 10;

        if (moduleSolved || cooldown)return;
        if(preventModuleUsage)return;
        if (t == 0)return;

        if ((buttonId+1)==nextCorrectSquare) {
            doLog("The " + (buttonId + 1).ToString() + " square has been pressed, that is correct, moving status light forward by " + t + "!");
            moveXSquares();
        } else {
            doLog("The " + (buttonId + 1).ToString() + " square has been pressed, that is incorrect, strike!");
            BombModule.HandleStrike();
        }

        return;
    }

    //twitch plays
    #pragma warning disable 414
    public readonly string TwitchHelpMessage = "Use “!{0} press <n> <time>” to press the button numbered n when the last seconds digit in the bomb's timer is time! Use !{0} colorblind <n> to find out the color of the button numbered n!";
    #pragma warning restore 414
    IEnumerator ProcessTwitchCommand(string command)
    {
        command = command.ToLower();
        if (command.StartsWith("colorblind "))
        {
            string temp2 = command;
            temp2 = temp2.Replace(" ","");
            if (temp2.Equals("colorblind"))
            {
                yield break;
            }
            string[] parameters = command.Split(' ');
            int temp = -1;
            int.TryParse(parameters[1], out temp);
            yield return null;
            if (temp <= -1 || temp >= 101)
            {
                yield return "sendtochaterror Digit out of range!";
                yield break;
            }
            else
            {
                string[] colors = { "Red", "Blue", "Green", "Yellow" };
                if(temp == 100)
                {
                    yield return "sendtochat The color of the " + parameters[1] + " square is :)";
                }
                else
                {
                    yield return "sendtochat The color of the " + parameters[1] + " square is " + colors[SquareBox[temp - 1].GetComponent<SquareScript>().GetColour()];
                }
                yield break;
            }
        }
        string commandl = command.Replace("press ", "");
        int tried;
        if(commandl.StartsWith("hole")) {
            commandl=commandl.Remove(0, 4);
            commandl = commandl.Insert(0, "100");
        }
        string[] cmds = commandl.Split(' ');
        if(int.TryParse(cmds[0], out tried)){
            tried = int.Parse(cmds[0]);
            if(tried>0){
                if(tried<101){
                    yield return null;
                    int time;
                    if(!int.TryParse(cmds[1], out time))
                    {
                        yield return "sendtochaterror Time not valid.";
                        yield break;
                    }
                    if(time > 9)
                    {
                        yield return "sendtochaterror Time too big!";
                        yield break;
                    }
                    else if (time < 0)
                    {
                        yield return "sendtochaterror Time too small!";
                        yield break;
                    }
                    while ((int)BombInfo.GetTime() % 10 != time) { yield return "trycancel Halted waiting to press the square due to a request to cancel!"; yield return new WaitForSeconds(0.1f); }
                    SquareBox[tried - 1].GetComponent<KMSelectable>().OnInteract();
                    yield return new WaitForSeconds(0.1f);
                    if(currentSquareId + 1 + lastRoll > 99)
                    {
                        yield return "solve";
                    }
                }
                else{
                    yield return null;
                    yield return "sendtochaterror Square too big!";
                    yield break;
                }
            }
            else{
                yield return null;
                yield return "sendtochaterror Square too small!";
                yield break;
            }
        }
        else{
            yield return null;
            yield return "sendtochaterror Square not valid.";
            yield break;
        }
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        forcesolveddialog.SetActive(true);
        while (!moduleSolved)
        {
            while ((int)BombInfo.GetTime() % 10 == 0) { yield return true; yield return new WaitForSeconds(0.1f); }
            while (moveSquare) { yield return true; yield return new WaitForSeconds(0.1f); }
            while (currentSquareId + 1 + ((int)BombInfo.GetTime() % 10) == snakes[showSnakes[0]].GetComponent<SnakeScript>().start || currentSquareId + 1 + ((int)BombInfo.GetTime() % 10) == snakes[showSnakes[1]].GetComponent<SnakeScript>().start || currentSquareId + 1 + ((int)BombInfo.GetTime() % 10) == snakes[showSnakes[2]].GetComponent<SnakeScript>().start || currentSquareId + 1 + ((int)BombInfo.GetTime() % 10) == snakes[showSnakes[3]].GetComponent<SnakeScript>().start) { yield return true; yield return new WaitForSeconds(0.1f); }
            SquareBox[nextCorrectSquare - 1].GetComponent<KMSelectable>().OnInteract();
        }
    }
}
