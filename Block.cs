using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class Block : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject objectBlock;
    public Vector2 target;
    public GameObject correct_click_prefab;
    public bool begin;
    void Start()
    {
        Controller.MoveBlockHandler += OnStep;
        target=objectBlock.GetComponent<RectTransform>().anchoredPosition;
        objectBlock.GetComponent<Button>().onClick.AddListener(clickBlock);
        begin = false;
    }
    // Update is called once per frame
    void Update()
    {
        if (begin)
        {
            Vector2 pos = objectBlock.GetComponent<RectTransform>().anchoredPosition;
            //Debug.Log(pos.x + "=" + target.x + ";" + pos.y + "="+target.y);
            if (Mathf.Abs(pos.x-target.x)<1e-3&&Mathf.Abs(pos.y-target.y)<1e-3)
            {
                Debug.Log("pos == target");
                objectBlock.GetComponent<RectTransform>().anchoredPosition = target;
                Controller.moveOk = true;
                begin = false;
            }
            else
            {
                Controller.moveOk = false;
                objectBlock.GetComponent<RectTransform>().anchoredPosition = Vector2.Lerp(pos, target, Controller.move_Speed);
            }
        }

    }
    private void clickBlock()
    {
        //根据是否可以Move给出动画

        if (true)
        {
            Debug.Log("OnMouseDown");
            Controller.focusBlock = objectBlock;
            GameObject correct_click = Instantiate(correct_click_prefab);
            correct_click.GetComponent<Transform>().position = objectBlock.GetComponent<Transform>().position;
            correct_click.GetComponent<Transform>().localScale = 10 * correct_click.GetComponent<Transform>().localScale;
            correct_click.GetComponent<ParticleSystem>().Play();
        }
    }
    private void OnDestroy()
    {
        Controller.MoveBlockHandler -= OnStep;
    }

    private void OnStep(char value, int num)//事件
    {
        //监听到与自己有关的事件
        int no = objectBlock.transform.GetChild(0).transform.GetComponent<Text>().text[0] - '0';
        Debug.Log("OnStep");
        if (value == no + '0')
        {
            target = new Vector2(100 * (num % 3 - 1), 100 * (1 - num / 3));
            begin = true;
        }
    }
}
    
