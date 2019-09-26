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
    private bool preventModuleUsage=true;

    bool moduleSolved;

    static int moduleIdCounter = 1;
    int moduleId;

    void Start() {
        moduleId = moduleIdCounter++;
        GenerateBoard();

        moveSquare=true;
        currentSquareId=0;
        lightOldPos=PlayerBox.transform.localPosition;
        lightNewPos=PlayerBox.transform.parent.InverseTransformPoint(SquareBox[currentSquareId].transform.position);
        t=.999f;
        animateLight();
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
        if(moveSquare){
            animateLight();
        }
    }

    void animateLight() {
        t+=2f*Time.deltaTime;
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
                        currentSquareId=ls.end-1;
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
                        currentSquareId=ss.end-1;
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
            }
        }

        if(snakes.Length>4) {
            for(int i=0;i<4;i++) {
                int r=Random.Range(0,snakes.Length);
                while(showSnakes.Contains(r)) {
                    r=Random.Range(0,snakes.Length);
                }
                showSnakes.Add(r);
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

            if(i%10==9)boardStr+="/";

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
            BombModule.HandlePass();
        }

        int n=SquareBox[currentSquareId].GetComponent<SquareScript>().GetNumber();
        int c=SquareBox[currentSquareId].GetComponent<SquareScript>().GetColour();
        int o=0;
        if(c==0) {
            if(n>=34&&n<=44)o=0;
            else if(n>=89&&n<=99)o=1;
            else if(n>=12&&n<=22)o=2;
            else if(n>=23&&n<=33)o=3;
            else if(n>=45&&n<=55)o=4;
            else if(n>=67&&n<=77)o=5;
            else if(n>=78&&n<=88)o=6;
            else if(n>=56&&n<=66)o=7;
            else o=8;
        } else if(c==3) {
            int i=(int)Math.Floor(n/10f)%10;
            if(n<10)o=8;
            else if(i==8)o=0;
            else if(i==2)o=1;
            else if(i==6)o=2;
            else if(i==3)o=3;
            else if(i==5)o=4;
            else if(i==7)o=5;
            else if(i==4)o=6;
            else if(i==9)o=7;
            else o=8;
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
        }

        nextCorrectSquare=CalculateSquareByPosition(n,o);
        doLog("answer: "+nextCorrectSquare);
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
        int r=Random.Range(1,7);

        preventModuleUsage=true;
        moveSquare=true;
        toAddOn=r;
        if(currentSquareId>99)currentSquareId=99;
        lightOldPos=PlayerBox.transform.localPosition;
        lightNewPos=PlayerBox.transform.parent.InverseTransformPoint(SquareBox[currentSquareId].transform.position);
        t=0f;
        animateLight();
    }

    void PressSquare(int buttonId) {
        BombAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        ModuleSelect.AddInteractionPunch();

        if(moduleSolved)return;
        if(preventModuleUsage)return;

        doLog("pressed: "+(buttonId+1).ToString());

        if((buttonId+1)==nextCorrectSquare) {
            moveXSquares();
        } else {
            BombModule.HandleStrike();
        }

        return;
    }

#pragma warning disable 414
    public readonly string TwitchHelpMessage = "Use “!{0} press <n>” to press the button numbered n!";
#pragma warning restore 414
    IEnumerator ProcessTwitchCommand(string command)
    {
        string commandl = command.Replace("press ", "");
        int tried;
        if(commandl=="hole") {
            commandl="100";
        }
        if(int.TryParse(commandl, out tried)){
            tried = int.Parse(commandl);
            if(tried>0){
                if(tried<101){
                    yield return null;
                    yield return SquareBox[tried-1].GetComponent<KMSelectable>();
                    yield return SquareBox[tried-1].GetComponent<KMSelectable>();
                }
                else{
                    yield return null;
                    yield return "sendtochaterror Digit too big!";
                    yield break;
                }
            }
            else{
                yield return null;
                yield return "sendtochaterror Digit too small!";
                yield break;
            }
        }
        else{
            yield return null;
            yield return "sendtochaterror Digit not valid.";
            yield break;
        }
    }
}
