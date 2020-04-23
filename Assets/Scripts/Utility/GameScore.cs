using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class GameScore
{
    [SerializeField] EnemyScores[] enemyScores;
    [Header("Score Values"), SerializeField] float victoryScore = 10000;
    [SerializeField] float scorePerRifleAmmo = 5;
    [SerializeField] float scorePerHandgunAmmo = 5;
    [SerializeField] float unusedBigPotScore = 400, unusedSmallPotScore = 200, unusedMedkitScore = 500, unusedGrenadeScore = 1000;
    Dictionary<string, List<ScoreEntry>> scoreEntries = new Dictionary<string, List<ScoreEntry>>();

    Dictionary<EnemyType, string> enemyTypeMapping = new Dictionary<EnemyType, string>()
    {
        {EnemyType.Mob, "Goblin" }, {EnemyType.FastMob, "Fast Goblin"}, {EnemyType.EliteMob, "Elite Goblin"},
        {EnemyType.Turret, "Turrets" }, {EnemyType.Boss, "Troll"}
    };

    public void AddEnemyKillEntry(EnemyType type)
    {
        AddEntry("Enemy Kills", new ScoreEntry() { 
            entryName = enemyTypeMapping[type], 
            value = Array.Find(enemyScores, x => x.type == type).score 
        });
    }

    public void AddEntry(string category, ScoreEntry entry)
    {
        if (scoreEntries.ContainsKey(category))
        {
            int idx = scoreEntries[category].FindIndex(x => x.entryName == entry.entryName);
            if (idx != -1) scoreEntries[category][idx].value += entry.value;
            else scoreEntries[category].Add(entry);

        }
        else scoreEntries.Add(category, new List<ScoreEntry>() { entry });
    }

    public void AddEntry(string category, string name, float value)
    {
        AddEntry(category, new ScoreEntry() { 
            entryName = name, 
            value = value 
        });
    }

    public float FinalizeScore(bool victory, int rifleAmmo, int handgunAmmo, bool bigPot, bool smallPot, bool medkit, bool grenade)
    {
        if (victory)
        {
            if (rifleAmmo > 0) AddEntry("Remaining Ammo", "AK47", rifleAmmo * scorePerRifleAmmo);
            if (handgunAmmo > 0) AddEntry("Remaining Ammo", "Glock", handgunAmmo * scorePerHandgunAmmo);
            if (bigPot) AddEntry("Remaining Items", "Big Potion", unusedBigPotScore);
            if (smallPot) AddEntry("Remaining Items", "Small Potion", unusedSmallPotScore);
            if (medkit) AddEntry("Remaining Items", "Medikit", unusedMedkitScore);
            if (grenade) AddEntry("Remaining Items", "Grenade", unusedGrenadeScore);
            AddEntry("Victory Bonus", "Victory Bonus", victoryScore);
        }

        return GetTotalScore();
    }

    public float GetTotalScore()
    {
        float total = 0;
        foreach(var entries in scoreEntries)
        {
            for(int i = 0; i < entries.Value.Count; i++)
            {
                total += entries.Value[i].value;
            }
        }
        return total;
    }

    public List<(string category, float value)> GetPerCategoryScore()
    {
        Dictionary<string, float> scores = new Dictionary<string, float>();
        foreach (var entries in scoreEntries)
        {
            for (int i = 0; i < entries.Value.Count; i++)
            {
                if (scores.ContainsKey(entries.Key)) scores[entries.Key] += entries.Value[i].value;
                else scores.Add(entries.Key, entries.Value[i].value);
            }
        }
        return scores.Select(kvp=>(kvp.Key, kvp.Value)).ToList();
    }

    public List<(string category, float value)> GetPerCategoryScore(List<(string category, string name, float value)> allScores)
    {
        Dictionary<string, float> scores = new Dictionary<string, float>();

        for (int i = 0; i < allScores.Count; i++)
        {
            if (scores.ContainsKey(allScores[i].category)) scores[allScores[i].category] += allScores[i].value;
            else scores.Add(allScores[i].category, allScores[i].value);
        }

        return scores.Select(kvp => (kvp.Key, kvp.Value)).ToList();
    }

    public List<(string category, string name, float value)> GetAllScores()
    {
        List<(string, string, float)> scores = new List<(string category, string name, float value)>();
        foreach (var entries in scoreEntries)
        {
            for (int i = 0; i < entries.Value.Count; i++)
            {
                scores.Add((entries.Key, entries.Value[i].entryName, entries.Value[i].value));
            }
        }
        return scores;
    }

    public List<(string name, float value)> GetScoresInCategory(string category)
    {
        List<(string, float)> scores = new List<(string name, float value)>();

        for (int i = 0; i < scoreEntries[category].Count; i++)
        {
            scores.Add((scoreEntries[category][i].entryName, scoreEntries[category][i].value));
        }
        return scores;
    }
}

[Serializable]
public struct EnemyScores
{
    public EnemyType type;
    public float score;

}

[Serializable]
public class ScoreEntry
{
    public string entryName;
    public float value;


}
