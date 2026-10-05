using System;

[Serializable]
public class PlayerAccount
{
    public string username;
    public string password;

    public PlayerStats stats;

    public PlayerAccount(string username, string password)
    {
        this.username = username;
        this.password = password;

        stats = new PlayerStats();
        stats.lastUsedName = username;
    }

}