using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;

public class Console : MonoBehaviour
{
    #region Attributs

    // Искомая комманда для определения доступности консоли
    public string comandLineArgument = "--console";
    public KeyCode toggleConsoleKey = KeyCode.BackQuote;
    public Boolean stopTime = true;
    public GameObject consoleObject;    
    

    private Boolean enable = false;
    private Boolean check = false;
    private Boolean show = false;

    private float oldTimeScale;

    private InputField input;
    
    private ArrayList cmdRunList = new ArrayList();
    private int cmdRunScrollCounter = 0;

    private ScrollRect cmdListView;
    private static Dictionary<string, Action<string[], Text>> commands = new Dictionary<string, Action<string[], Text>>();
    

    #endregion

    #region CustomMethods

    public static void addCommand(string name, Action<string[], Text> func)
    {
        commands.Add(name.ToLower(), func);
    }

    private Boolean isEnabled()
    {
        if (check != true)
        {
            string[] args = Environment.GetCommandLineArgs();
            int index = Array.IndexOf(args, comandLineArgument);

            if (Debug.isDebugBuild || index > -1)
            {
                enable = true;
            }
            check = true;

        }
        return enable;
    }

    protected void openConsole()
    {
        if (enable)
        {
            show = true;
            if (stopTime)
            {
                oldTimeScale = Time.timeScale;
                Time.timeScale = 0;
            }
            input = consoleObject.GetComponentInChildren<InputField>();
            cmdListView = consoleObject.GetComponentInChildren<ScrollRect>();
            input.text = "";
            consoleObject.SetActive(true);
            input.ActivateInputField();
        }
    }

    protected void closeConsole()
    {
        if (enable)
        {
            show = false;
            if (stopTime)
            {
                Time.timeScale = oldTimeScale;
            }
            consoleObject.SetActive(false);
        }
    }

    protected void toogleConsole()
    {
        if (enable && Input.GetKeyDown(KeyCode.Escape) && show == true)
        {
            closeConsole();
        }
        if (enable && Input.GetKeyDown(toggleConsoleKey))
        {
            if (show == true)
            {
                closeConsole();
            } else
            {
                openConsole();
            }
        }
    }

    public static T[] removeAt<T>(T[] source, int index)
    {
        var dest = new T[source.Length - 1];
        if (index > 0)
            Array.Copy(source, 0, dest, 0, index);

        if (index < source.Length - 1)
            Array.Copy(source, index + 1, dest, index, source.Length - index - 1);

        return dest;
    }

    protected void sendCommand()
    {
        if (enable && show && input != null && Input.GetKeyDown(KeyCode.Return))
        {
            string realCmd = input.text;
            cmdRunList.Add(realCmd.ToLower());
            cmdRunScrollCounter++;
            string[] split = realCmd.Split(new Char[] { ' ', ',', '.', ':', '\t' });
            Text cmdListText = cmdListView.GetComponentInChildren<Text>();
            string cmd = split[0];
            string time = System.DateTime.Now.ToString();
            cmdListText.text += "\n<color=\"green\">"+time+"</color> Command - "+cmd;
            split = Console.removeAt(split, 0);
            apllyCommand(cmd, split);
            input.text = "";
            input.ActivateInputField();
        }
    }

    protected string apllyCommand(string cmd, string[] param)
    {
        Text cmdListText = cmdListView.GetComponentInChildren<Text>();
        try
        {
            commands[cmd.ToLower()](param, cmdListText);
        }
        catch (KeyNotFoundException)
        {
            string time = System.DateTime.Now.ToString();
            cmdListText.text += "\n<color=\"green\">" + time + "</color> <color=\"red\">COMMAND NOT FOUND</color>";
        }
        return "";
    }

    protected void scrollCmdList()
    {
        if (enable && show)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow))
            {
                if (cmdRunScrollCounter == 0)
                {
                    input.ActivateInputField();
                    return;
                }
                if (Input.GetKeyDown(KeyCode.UpArrow))
                {
                    if (cmdRunScrollCounter == cmdRunList.Count)
                    {
                        cmdRunScrollCounter = 0;
                    }
                    cmdRunScrollCounter++;
                    input.text = cmdRunList[cmdRunList.Count - cmdRunScrollCounter] as string;
                } else if (Input.GetKeyDown(KeyCode.DownArrow))
                {
                    if (cmdRunScrollCounter <= 1)
                    {
                        cmdRunScrollCounter = cmdRunList.Count + 1;
                    }
                    cmdRunScrollCounter--;
                    input.text = cmdRunList[cmdRunList.Count - cmdRunScrollCounter] as string;

                }
                input.ActivateInputField();
            }
            
        }
        
    }

    #endregion

    #region defaultCommand

    public void HelpCommand(string[] data, Text cmdListText)
    {
        Debug.Log("HELP");
    }

    public void ClearCommand(string[] data, Text cmdListText)
    {
        cmdListText.text = "";
        Debug.ClearDeveloperConsole();
    }

    #endregion

    #region StandartMethods

    void Awake()
    {
        isEnabled();
        Console.addCommand("help", HelpCommand);
        Console.addCommand("clear", ClearCommand);
    }

    void Start()
    {
    }


    void Update()
    {
        if (enable)
        {
            toogleConsole();
            sendCommand();
            scrollCmdList();
        }
    }

    void OnGUI()
    {
    }

    #endregion
}
