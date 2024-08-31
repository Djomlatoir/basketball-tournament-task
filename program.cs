using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

public class Team
{
    [JsonPropertyName("Team")]
    public string TeamName { get; set; }  // Mapa "Team" iz JSON-a na "TeamName"
    
    public string ISOCode { get; set; }
    public int FIBARanking { get; set; }
}

public class Group
{
    public List<Team> A { get; set; }
    public List<Team> B { get; set; }
    public List<Team> C { get; set; }
}

class Program
{
    static void Main(string[] args)
    {
        // Čitanje JSON fajla
        string jsonString = File.ReadAllText("groups.json");
        Group groups = JsonSerializer.Deserialize<Group>(jsonString);

        // Ispis podataka za grupu A
        Console.WriteLine("Group A Teams:");
        foreach (var team in groups.A)
        {
            Console.WriteLine($"Team: {team.TeamName}, ISO Code: {team.ISOCode}, FIBA Ranking: {team.FIBARanking}");
        }

        // Ispis podataka za grupu B
        Console.WriteLine("\nGroup B Teams:");
        foreach (var team in groups.B)
        {
            Console.WriteLine($"Team: {team.TeamName}, ISO Code: {team.ISOCode}, FIBA Ranking: {team.FIBARanking}");
        }

        // Ispis podataka za grupu C
        Console.WriteLine("\nGroup C Teams:");
        foreach (var team in groups.C)
        {
            Console.WriteLine($"Team: {team.TeamName}, ISO Code: {team.ISOCode}, FIBA Ranking: {team.FIBARanking}");
        }
    }
}
