using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using UnityEngine;


class DataPrinter
{
    static int fileNo = -1;
    List<PrintData> dataList = new List<PrintData>();

    internal void NextFile()
    {
        string path = Path.Combine(Application.dataPath, ".dataNo.txt");
        if (fileNo == -1)
        {
            if (File.Exists(path))
            {
                using (TextReader reader = File.OpenText(path))
                {
                    string num = reader.ReadLine();
                    if (num != null && int.TryParse(num, out int no)) fileNo = no;
                    else
                    {
                        reader.Close();
                        Debug.LogError("dataNo.txt is not in the correct format. Discarding file");
                        fileNo = 1;
                    }
                }
                File.WriteAllText(path, (++fileNo).ToString());
            }
            else
            {
                fileNo = 1;
                File.WriteAllText(path, "1");
            }
        }
        else
        {
            fileNo++;
            File.WriteAllText(path, fileNo.ToString());
        }
    }
    internal void Print(PrintType type, PrintData data)
    {
        if (fileNo == -1) NextFile();

        dataList.Add(data);
        Print(type);
    }

    void Print(PrintType type)
    {
        if (type == PrintType.CSV)
        {
            StringBuilder sb = new StringBuilder();
            List<string> lineDump = new List<string>();
            foreach(var data in dataList)
            {
                List<string> lineContents = new List<string>();
                lineContents.Add(data.enemyCount.ToString());
                lineContents.Add(data.distance.avg.ToString("n1"));
                lineContents.Add(data.distance.min.ToString("n1"));
                lineContents.Add(data.distance.max.ToString("n1"));
                lineContents.Add(data.currentState.ToString());
                lineContents.Add(data.maxEnemies.ToString());
                lineContents.Add(data.spawnRate.ToString("n3"));
                lineContents.Add(data.peakTimer.ToString("n1"));
                lineContents.Add(data.hordeMode.ToString());
                lineContents.Add(data.hordeTimer.ToString("n1"));
                lineContents.Add(data.stressLevel.ToString("n2"));
                lineContents.Add(data.stressRate.ToString("n2"));
                lineContents.Add(data.varHP.ToString("n2"));
                lineContents.Add(data.varAmmo.ToString("n2"));
                lineContents.Add(data.bigPotionAvailable.ToString());
                lineContents.Add(data.smallPotionAvailable.ToString());
                lineContents.Add(data.grenadeAvailable.ToString());
                lineContents.Add(data.enemiesKilled.ToString());
                lineContents.Add(data.score.ToString("n0"));
                lineContents.Add(data.FEREnabled.ToString());
                lineContents.Add(data.faceDetected.ToString());
                lineContents.Add(data.joyVal.ToString("n2"));
                lineContents.Add(data.angerVal.ToString("n2"));
                lineContents.Add(data.fearVal.ToString("n2"));
                lineContents.Add(data.disgustVal.ToString("n2"));
                lineContents.Add(data.sadnessVal.ToString("n2"));
                lineContents.Add(data.surpriseVal.ToString("n2"));
                lineContents.Add(data.valenceVal.ToString("n2"));
                lineContents.Add(data.contemptVal.ToString("n2"));
                lineContents.Add(data.engagementVal.ToString("n2"));
                lineContents.Add(data.primaryAmmo.ToString());
                lineContents.Add(data.primaryClip.ToString());
                lineContents.Add(data.secondaryAmmo.ToString());
                lineContents.Add(data.secondaryClip.ToString());
                lineContents.Add(data.health.ToString("n0"));
                lineContents.Add(data.stamina.ToString("n0"));
                lineDump.Add(string.Join(",", lineContents));

            }

            File.WriteAllLines(GetFilePath("csv"), lineDump);

        }
        else if(type == PrintType.JSON)
            File.WriteAllText(GetFilePath("json"), JsonUtility.ToJson(new JSONData() { version = 1, data = dataList }));
        else if (type == PrintType.JSONBeautified)
            File.WriteAllText(GetFilePath("json"), JsonUtility.ToJson(new JSONData() { version = 1, data = dataList }, true));
    }

    string GetFilePath(string extension, string fileName = "data", string basePath = "DataFile")
    {
        string path = Application.platform == RuntimePlatform.WindowsEditor ? Path.Combine(Application.dataPath, basePath + "~") : Path.Combine(Application.dataPath, basePath);
        string file = Path.Combine(path, $"{fileName}{fileNo}.{extension}");
        if (!Directory.Exists(path)) Directory.CreateDirectory(path);
        return file;
    }
}

struct JSONData
{
    public int version;
    public List<PrintData> data;
}

[Serializable]
public struct PrintData
{
    public string subjectName;

    public int enemyCount;
    public Range distance;
    public GameState currentState;
    public int maxEnemies;
    public float spawnRate;
    public float peakTimer;

    public bool hordeMode;
    public float hordeTimer;
    public float stressLevel, varHP, varAmmo, varFER, stressRate;

    public bool bigPotionAvailable, smallPotionAvailable, grenadeAvailable;
    public int enemiesKilled;
    public float score;

    public bool FEREnabled, faceDetected;
    public float joyVal, angerVal, fearVal, disgustVal, sadnessVal, surpriseVal, valenceVal, contemptVal, engagementVal;

    public int primaryAmmo, primaryClip, secondaryAmmo, secondaryClip;
    public float health, stamina;
    //35 variables to print!
}

[Serializable]
public struct Range
{
    public float avg, max, min;

    public Range(float avg, float max, float min)
    {
        this.avg = avg;
        this.max = max;
        this.min = min;
    }
}

enum PrintType
{
    CSV, JSON, JSONBeautified
}