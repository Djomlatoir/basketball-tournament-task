using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

public class Timovi
{
    [JsonPropertyName("Team")]
    public string Team { get; set; }
    public string ISOCode { get; set; }
    public int FIBARanking { get; set; }
    public int Points { get; set; } = 0;
    public int ScoredPoints { get; set; } = 0;
    public int ConcededPoints { get; set; } = 0;
    public int Wins { get; set; } = 0;
    public int Losses { get; set; } = 0;
    public string GroupName { get; set; }
}

public class Group
{
    public List<Timovi> A { get; set; }
    public List<Timovi> B { get; set; }
    public List<Timovi> C { get; set; }
}

class Program
{
    static void Main(string[] args)
    {
        // Čitanje JSON fajla
        string jsonString = File.ReadAllText("groups.json");
        Group groups = JsonSerializer.Deserialize<Group>(jsonString);

        // Simulacija podataka za grupe A, B i C
        SimulateGroupStage(groups.A, "A");
        SimulateGroupStage(groups.B, "B");
        SimulateGroupStage(groups.C, "C");
        // Ispis podataka za grupe A, B i C
        DisplayGroupStandings(groups.A, "A");
        DisplayGroupStandings(groups.B, "B");
        DisplayGroupStandings(groups.C, "C");

        var rankedTeams = RankTeamsAcrossGroups(groups);

        var eliminationMatches = DrawEliminationPhase(rankedTeams);

        SimulateEliminationPhase(eliminationMatches);
    }

    // Simulacija podataka za grupe A, B i C
    static void SimulateGroupStage(List<Timovi> group, string groupName)
    {
        Console.WriteLine($"Grupna faza - I kolo:");
        Console.WriteLine($"    Grupa {groupName}:");

        for (int i = 0; i < group.Count; i++)
        {
            for (int j = i + 1; j < group.Count; j++)
            {
                var team1 = group[i];
                var team2 = group[j];

                int team1Points = GenerateMatchPoints(team1.FIBARanking);
                int team2Points = GenerateMatchPoints(team2.FIBARanking);

                team1.ScoredPoints += team1Points;
                team1.ConcededPoints += team2Points;
                team2.ScoredPoints += team2Points;
                team2.ConcededPoints += team1Points;

                if (team1Points > team2Points)
                {
                    team1.Points += 2;
                    team1.Wins++;
                    team2.Points += 1;
                    team2.Losses++;
                    Console.WriteLine($"        {team1.Team} - {team2.Team} ({team1Points}:{team2Points})");
                }
                else
                {
                    team2.Points += 2;
                    team2.Wins++;
                    team1.Points += 1;
                    team1.Losses++;
                    Console.WriteLine($"        {team2.Team} - {team1.Team} ({team2Points}:{team1Points})");
                }
            }
        }
    }

    static int GenerateMatchPoints(int fibarank)
    {
        Random rnd = new Random();
        return rnd.Next(70, 100) - fibarank / 10;
    }

    // Ispis podataka za grupe A, B i C
    static void DisplayGroupStandings(List<Timovi> group, string groupName)
    {
        Console.WriteLine($"\nKonačan plasman u grupama:");
        Console.WriteLine($"    Grupa {groupName} (Ime - pobede/porazi/bodovi/postignuti koševi/primljeni koševi/koš razlika):");

        var standings = group.OrderByDescending(t => t.Points)
                             .ThenByDescending(t => t.ScoredPoints - t.ConcededPoints)
                             .ThenByDescending(t => t.ScoredPoints)
                             .ToList();

        for (int i = 0; i < standings.Count; i++)
        {
            var team = standings[i];
            Console.WriteLine($"        {i + 1}. {team.Team,-10} {team.Wins} / {team.Losses} / {team.Points} / {team.ScoredPoints} / {team.ConcededPoints} / {team.ScoredPoints - team.ConcededPoints}");
        }
    }

    static List<Timovi> RankTeamsAcrossGroups(Group groups)
    {
        List<Timovi> allTeams = new List<Timovi>();
        allTeams.AddRange(groups.A);
        allTeams.AddRange(groups.B);
        allTeams.AddRange(groups.C);
        var rankedTeams = allTeams.OrderBy(t => t.Points)
                                  .ThenByDescending(t => t.ScoredPoints - t.ConcededPoints)
                                  .ThenByDescending(t => t.ScoredPoints)
                                  .ToList();
        return rankedTeams.Take(8).ToList();
    }

    static Dictionary<string, List<Timovi>> DrawEliminationPhase(List<Timovi> rankedTeams)
    {
        Dictionary<string, List<Timovi>> pots = new Dictionary<string, List<Timovi>>()
    {
        {"D", new List<Timovi>{rankedTeams[0], rankedTeams[1]}},
        {"E", new List<Timovi>{rankedTeams[2], rankedTeams[3]}},
        {"F", new List<Timovi>{rankedTeams[4], rankedTeams[5]}},
        {"G", new List<Timovi>{rankedTeams[6], rankedTeams[7]}}
    };

        Console.WriteLine("\nŠeširi:");
        foreach (var pot in pots)
        {
            Console.WriteLine($"    Šešir {pot.Key}:");
            foreach (var team in pot.Value)
            {
                Console.WriteLine($"        {team.Team} - Grupa: {team.GroupName}");
            }
        }

        Random rnd = new Random();
        List<Tuple<Timovi, Timovi>> quarterfinals = new List<Tuple<Timovi, Timovi>>();

        Console.WriteLine("\nFormiranje četvrtfinala:");
        while (pots["D"].Count > 0 && pots["G"].Count > 0)
        {
            if (pots["D"].Count == 0 || pots["G"].Count == 0)
            {
                Console.WriteLine("Nema dovoljno timova za formiranje mečeva u grupama D i G.");
                break;
            }

            Timovi teamD = pots["D"][rnd.Next(pots["D"].Count)];
            Timovi teamG = pots["G"][rnd.Next(pots["G"].Count)];

            if (teamD.Team != teamG.Team)
            {
                quarterfinals.Add(Tuple.Create(teamD, teamG));
                pots["D"].Remove(teamD);
                pots["G"].Remove(teamG);
                Console.WriteLine($"    {teamD.Team} vs {teamG.Team}");
            }
            else
            {
                // Ova linija može dovesti do problema ako su u istom potu
                Console.WriteLine($"    Izbegnuta grupa rematch: {teamD.Team} i {teamG.Team} - Grupa: {teamD.GroupName}");
                break;
            }
        }

        while (pots["E"].Count > 0 && pots["F"].Count > 0)
        {
            if (pots["E"].Count == 0 || pots["F"].Count == 0)
            {
                Console.WriteLine("Nema dovoljno timova za formiranje mečeva u grupama E i F.");
                break;
            }

            Timovi teamE = pots["E"][rnd.Next(pots["E"].Count)];
            Timovi teamF = pots["F"][rnd.Next(pots["F"].Count)];

            if (teamE.Team != teamF.Team)
            {
                quarterfinals.Add(Tuple.Create(teamE, teamF));
                pots["E"].Remove(teamE);
                pots["F"].Remove(teamF);
                Console.WriteLine($"    {teamE.Team} vs {teamF.Team}");
            }
            else
            {
                // Ova linija može dovesti do problema ako su u istom potu
                Console.WriteLine($"    Izbegnuta grupa rematch: {teamE.Team} i {teamF.Team} - Grupa: {teamE.GroupName}");

                break;
            }
        }

        Console.WriteLine("\nEliminaciona faza:");
        Dictionary<string, List<Timovi>> eliminationMatches = new Dictionary<string, List<Timovi>>();
        char matchLabel = 'A';

        foreach (var match in quarterfinals)
        {
            eliminationMatches[$"Match {matchLabel}"] = new List<Timovi> { match.Item1, match.Item2 };
            matchLabel++;
        }

        return eliminationMatches;
    }

    static void SimulateEliminationPhase(Dictionary<string, List<Timovi>> eliminationMatches)
    {
        Console.WriteLine("\nČetvrtfinale:");

        List<Tuple<Timovi, Timovi, int, int>> quarterFinalResults = new List<Tuple<Timovi, Timovi, int, int>>();
        List<Timovi> quarterFinalWinners = new List<Timovi>();

        foreach (var match in eliminationMatches)
        {
            var teams = match.Value;
            var (winner, team1Points, team2Points) = SimulateMatchWithScores(teams[0], teams[1]);

            quarterFinalResults.Add(Tuple.Create(teams[0], teams[1], team1Points, team2Points));
            quarterFinalWinners.Add(winner);

            Console.WriteLine($"    {teams[0].Team} - {teams[1].Team} ({team1Points}:{team2Points})");
        }

        Console.WriteLine("\nPobednici četvrtfinala:");
        foreach (var winner in quarterFinalWinners)
        {
            Console.WriteLine($"    {winner.Team}");
        }

        SimulateSemiFinals(quarterFinalResults);
    }


    static void SimulateSemiFinals(List<Tuple<Timovi, Timovi, int, int>> quarterFinalResults)
    {
        Console.WriteLine("\nPolufinale:");

        List<Timovi> semiFinalWinners = new List<Timovi>();
        List<Timovi> semiFinalLosers = new List<Timovi>();
        List<Timovi> semiFinalPobeda = new List<Timovi>();

        foreach (var result in quarterFinalResults)
        {
            var (team1, team2, team1Points, team2Points) = result;
            var winner = team1Points > team2Points ? team1 : team2;


            semiFinalWinners.Add(winner);

        }

        if (semiFinalWinners.Count < 4)
        {
            Console.WriteLine("Nema dovoljno pobednika za polufinale.");
            return;
        }

        var semiFinalMatches = new List<Tuple<Timovi, Timovi>>();

        semiFinalMatches.Add(Tuple.Create(semiFinalWinners[0], semiFinalWinners[1]));
        semiFinalMatches.Add(Tuple.Create(semiFinalWinners[2], semiFinalWinners[3]));

        List<Tuple<Timovi, Timovi, int, int>> semiFinalResults = new List<Tuple<Timovi, Timovi, int, int>>();

        foreach (var match in semiFinalMatches)
        {

            var (team1, team2) = match;
            var (winner, team1Points, team2Points) = SimulateMatchWithScores(team1, team2);

            semiFinalResults.Add(Tuple.Create(team1, team2, team1Points, team2Points));
            Console.WriteLine($"    {team1.Team} - {team2.Team} ({team1Points}:{team2Points})");
            var pobeda = team1Points > team2Points ? team1 : team2;
            semiFinalPobeda.Add(pobeda);
         
             var loser = team1Points > team2Points ? team2 : team1;
            semiFinalLosers.Add(loser);
        }

        var thirdPlaceResults = SimulateThirdPlaceMatch(semiFinalLosers);

        var finalResults = SimulateFinal(semiFinalPobeda);

        DisplayMedalWinners(finalResults.Item1, finalResults.Item2, thirdPlaceResults.Item1);
    }


 


    static Tuple<Timovi, Timovi, int, int> SimulateThirdPlaceMatch(List<Timovi> semiFinalLosers)
    {
        Console.WriteLine("\nUtakmica za treće mesto:");

        var (team1, team2) = (semiFinalLosers[0], semiFinalLosers[1]);
        var (winner, team1Points, team2Points) = SimulateMatchWithScores(team1, team2);

        Console.WriteLine($"    {team1.Team} - {team2.Team} ({team1Points}:{team2Points})");

        return Tuple.Create(team1Points > team2Points ? team1 : team2, team1Points > team2Points ? team2 : team1, team1Points, team2Points);
    }
    static Tuple<Timovi, Timovi, int, int> SimulateFinal(List<Timovi> semiFinalLosers)
    {
        Console.WriteLine("\nFinale:");

       
        var (team1, team2) = (semiFinalLosers[0], semiFinalLosers[1]);
        var (winner, team1Points, team2Points) = SimulateMatchWithScores(team1, team2);

        Console.WriteLine($"    {team1.Team} - {team2.Team} ({team1Points}:{team2Points})");

        return Tuple.Create(team1Points > team2Points ? team1 : team2, team1Points > team2Points ? team2 : team1, team1Points, team2Points);
    }
    /*
    static Tuple<Timovi, Timovi, int, int> SimulateFinal(List<Tuple<Timovi, Timovi, int, int>> semiFinalResults)
    {
        Console.WriteLine("\nFinale:");

        // Assuming the final is the first match in the list
        var (team1, team2, _, _) = semiFinalResults[0];
        var (winner, team1Points, team2Points) = SimulateMatchWithScores(team1, team2);

        Console.WriteLine($"    {team1.Team} - {team2.Team} ({team1Points}:{team2Points})");

        return Tuple.Create(team1Points > team2Points ? team1 : team2, team1Points > team2Points ? team2 : team1, team1Points, team2Points);
    }
    */

    static void DisplayMedalWinners(Timovi gold, Timovi silver, Timovi bronze)
    {
        Console.WriteLine("\nMedalje:");
        Console.WriteLine($"    1. {gold.Team}");
        Console.WriteLine($"    2. {silver.Team}");
        Console.WriteLine($"    3. {bronze.Team}");
    }

    static Tuple<Timovi, int, int> SimulateMatchWithScores(Timovi team1, Timovi team2)
    {
        Random rnd = new Random();
        int team1Points = GenerateMatchPoints(team1.FIBARanking);
        int team2Points = GenerateMatchPoints(team2.FIBARanking);

        var winner = team1Points > team2Points ? team1 : team2;

        return Tuple.Create(winner, team1Points, team2Points);
    }



    static Timovi SimulateMatch(Timovi team1, Timovi team2)
    {
        Random rnd = new Random();
        int team1Points = GenerateMatchPoints(team1.FIBARanking);
        int team2Points = GenerateMatchPoints(team2.FIBARanking);

        return team1Points > team2Points ? team1 : team2;
    }




}
