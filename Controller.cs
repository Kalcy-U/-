using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Controller : MonoBehaviour
{
    // Start is called before the first frame update
    private string savePath = System.Environment.CurrentDirectory + "\\Assets\\executor";
    public GameObject Tfeedback;
    public GameObject BlockPrefab;
    public GameObject inputSrc;
    public GameObject inputDst;
    public GameObject selectFun;
    public GameObject submit;
    public GameObject panel_left;
    public GameObject next;
    public GameObject auto;
    public GameObject completed;
    public GameObject speedSlider;
    public GameObject playM;
    public GameObject complete_effect;
    static public GameObject focusBlock;

    public delegate void MoveBlock(char value, int num);
    public static event MoveBlock MoveBlockHandler;

    public delegate void Completed();
    public static event Completed CompleteHandler;

    public delegate void sliderValue();
    public static event sliderValue sliderValueHandler;

    public static bool moveOk = true;
    public static float move_Speed;//lerp



    private Button btn_subm;
    private Button btn_next;
    private Button btn_auto;
    private Button btn_play;
    private string src;
    private string dst;
    private bool gamingFlag;//����ɳ�ʼ������Ϸ������
    private string[] steps;
    private int stepnum;
    
    private bool autoMove;
    private bool playMode;
    private int OnStepNo;
    private const int OFFSET=7;
    private int pos0_last;
    private int pos0_curr;
    private char[] currentState;//��ǰ���� 9����
    private string targetState;
    //public GameObject submit;
    void Start()
    {
        //RunExeByProcess("235106487 123405678 default");
        btn_subm = submit.transform.GetComponent<Button>();
        btn_subm.onClick.AddListener(OnSubmit);
        btn_next = next.transform.GetComponent<Button>();
        btn_next.onClick.AddListener(NextStep);
        btn_auto = auto.transform.GetComponent<Button>();
        btn_auto.onClick.AddListener(changeMode);
        btn_play= playM.transform.GetComponent<Button>();
        btn_play.onClick.AddListener(setPlayMode);
        playMode = false;
        sliderValueHandler += sliderValueChange;
        CompleteHandler += GameComplete;
        completed.SetActive(false);
        
        autoMove = false;
        OnStepNo = 0;
        move_Speed = 0.3f;
        gamingFlag = false;
        //btn_subm.interactable = 1;
    }
    private void setPlayMode()
    {
        if (playMode == false)
        {
            playMode = true;
            btn_next.interactable = false;
            btn_auto.interactable = false;
            currentState = steps[OFFSET + OnStepNo-1].ToCharArray();
            btn_play.transform.GetChild(0).GetComponent<Text>().text = "�Զ�����";
        }
        else
        {
            playMode = false;
            btn_next.interactable = true;
            btn_auto.interactable = true;
            btn_play.transform.GetChild(0).GetComponent<Text>().text = "�ֶ�����";
            //����һ����Ϸ
            gamingFlag = false;
            src = new string(currentState);
            RunExeByProcess(0);

        }
    }
    private void sliderValueChange()
    {
        move_Speed = speedSlider.transform.GetComponent<Slider>().value * 0.8f + 0.06f;
    }
    
    private void GameComplete()
    {
        //Debug.Log(completed.active);
        completed.SetActive(true);
        gamingFlag = false;
        moveOk = true;
        GameObject complete= Instantiate(complete_effect);
        complete.transform.position = GameObject.Find("Canvas").transform.position;
        complete.transform.localScale = 25 * complete.transform.localScale;
        complete.GetComponent<ParticleSystem>().Play();
    }

    private void changeMode()
    {
        autoMove =!autoMove;
        if (autoMove == true)
        {
            btn_next.interactable = false;
            auto.transform.GetChild(0).GetComponent<Text>().text = "����ִ��";
        }
        else
        {
            btn_next.interactable = true;
            auto.transform.GetChild(0).GetComponent<Text>().text = "�Զ�ִ��";
            
        }
    }

    private void NextStep()
    {
        //Debug.Log("step");
        if (!playMode&&gamingFlag)
        {
            if (OnStepNo == 0)
                pos0_last = steps[OFFSET-1].IndexOf('0');
            pos0_curr = steps[OFFSET + OnStepNo].IndexOf('0');
            MoveBlockHandler.Invoke(steps[OFFSET + OnStepNo-1][pos0_curr],pos0_last);
            pos0_last = pos0_curr;
            OnStepNo++;
            if (OnStepNo == stepnum)
            {
                gamingFlag = false;
                CompleteHandler.Invoke();
                OnStepNo = 0;
            }
        }
        else if (playMode&&focusBlock!=null && gamingFlag)
        {
            //�ֶ���Ϸģʽ
            //�����ֽ׶�״̬�ж��Ƿ�����ƶ�
            
            string str = new string(currentState);
            Debug.Log(str);
            pos0_curr = str.IndexOf('0');
            char focus = focusBlock.transform.GetChild(0).transform.GetComponent<Text>().text[0];
            int pos_focus = str.IndexOf(focus);
            int dist = Math.Abs(pos_focus - pos0_curr);
            if (dist==1||dist==3)
            {
                MoveBlockHandler.Invoke(focus, pos0_curr);//�����ƶ�
                                                          //����״̬
                currentState[pos0_curr] = focus;
                currentState[pos_focus] = '0';

                if (new string(currentState) == targetState)
                {
                    gamingFlag = false;
                    CompleteHandler.Invoke();
                }
            }
            
        }

    }
  
    // Update is called once per frame
    void Update()
    {
        if (playMode == false)//���ֶ�
        {
            if (gamingFlag && Input.GetKeyDown(KeyCode.Space) || (autoMove && moveOk))
            {
                NextStep();
            }
        }
        else
        {
            if (gamingFlag && Input.GetKeyDown(KeyCode.Space) && moveOk)
            {
                NextStep();
            }
        }
    }
    private void OnSubmit()
    {
        src = inputSrc.transform.GetComponent<InputField>().text;
        dst = inputDst.transform.GetComponent<InputField>().text; 
        int DdValue = selectFun.transform.GetComponent<Dropdown>().value;

        RunExeByProcess(DdValue);
    }
    [Obsolete]
    public void RunExeByProcess(int DdValue)
    {
        //ǰ����������Ϸ�����
        int childn = panel_left.transform.GetChildCount();
        for (int i = childn - 1; i >= 0; i--)
        {
            Destroy(panel_left.transform.GetChild(i).gameObject);
        }
        gamingFlag = false;
        //�������߳�

        //Debug.Log(inputSrc.transform.GetComponent<InputField>().text);
        Debug.Log(inputDst.transform.GetComponent<InputField>().text);
        
        //int DdValue = selectFun.transform.GetComponent<Dropdown>().value;
        move_Speed = speedSlider.transform.GetComponent<Slider>().value * 0.8f + 0.06f;
        string h = "";
        switch (DdValue) {
            case 0:
                h = "ManhattanDistance";
                break;
            case 1:
                h = "NotAtPos";
                break;
            case 2:
                h = "ManhattanDistance_with0";
                break;
            case 3:
                h = "NotAtPos_with0";
                break;
            case 4:
                h = "LinearDistance";
                break;
            default:
                h = "d";
                break;
    }
        System.Diagnostics.Process process = new System.Diagnostics.Process();
        //���õ�exe����
        process.StartInfo.FileName = "8nums";
        process.StartInfo.WorkingDirectory = savePath;
        //���ݲ���
        process.StartInfo.Arguments = src+" "+ dst + " " + h;
        Debug.Log(savePath);
        process.Start();
        
        process.WaitForExit();//ͣ�٣����ⲿ�����˳�����ܼ���ִ��

        if (process.ExitCode == 0)//�����˳�
        {
            Debug.Log("exeִ�гɹ�");
            //Tfeedback.GetComponent<Text>().text = "�������";
            gamebegin();
            return;
        }
        else if (process.ExitCode == 7)
        {
            Debug.Log("����Ƿ�");
            Tfeedback.GetComponent<Text>().text = "�����ʽ����ȷ";
        }
        else if (process.ExitCode == 1001)
        {
            Debug.Log("���ɽ�");
            Tfeedback.GetComponent<Text>().text = "�����޽�";
        }
        else if (process.ExitCode == 2000)
        {
            Debug.Log("������������ȷ");
            Tfeedback.GetComponent<Text>().text = "ȱ���������";
        }
    }
    void gamebegin()
    {
        //ǰ����������Ϸδ��ʼ
        if (gamingFlag)
            return;
        //ʵ����8��block����һ��ʼ��
        gamingFlag = true;
        completed.SetActive(false);
        OnStepNo = 0;
        Debug.Log("game begin");
        GameObject[] BlockInstans=new GameObject[10];
        for (int i = 0; i < 9; i++) {
            if (src[i] != '0')
            {
                Debug.Log("ʵ����"+i);
                BlockInstans[i]= Instantiate(BlockPrefab);
                BlockInstans[i].transform.GetChild(0).transform.GetComponent<Text>().text = ""+src[i];
                BlockInstans[i].transform.SetParent(panel_left.transform,false);
                BlockInstans[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(100 * (i % 3 - 1), 100 * (1 - i / 3));
                //BlockInstans[i].ResetValue(src[i] - '0', i);
            }
        }
        //���ļ�
        steps = File.ReadAllLines(savePath+"\\step.dat", System.Text.Encoding.ASCII);
        stepnum = Int32.Parse(steps[1].Split(' ')[2]);
        Tfeedback.GetComponent<Text>().text = steps[0] + "\n" + steps[1] + "\n"+ steps[2] + "\n" + steps[3]+ "\n" + steps[4]+ "\n" + steps[5];
        currentState = src.ToCharArray();//�洢��״̬
        targetState = dst;//�洢Ŀ��״̬
        //��ʾ��Ϣ
    }
}
