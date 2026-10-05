using System;

[Serializable]
public class PlayerStats
{
    public int thrownObjects;
    public int wins;
    public int playedGames;

    public float lastPlayedGameResult;
    public float playtime;

    // Laatst gebruikte multiplayernaam
    public string lastUsedName;
}