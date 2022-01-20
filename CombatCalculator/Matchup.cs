using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace CombatCalculator
{
    public static class Matchup
    {
        [FunctionName("Matchup")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("The Matchup function processed a request.");
        try
            {
                int defenseDivisor = 0;
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                JObject data = JsonConvert.DeserializeObject(requestBody) as JObject;
                
                int i = 0;
                var players = new List<Player>();
                foreach (var obj in data)
                {
                    if (i == 0) defenseDivisor = obj.Value.ToObject<int>();
                    else players.Add(obj.Value.ToObject<Player>());
                    i++;
                }

                string results = CalculateFight(players, defenseDivisor);
                var response = new Dictionary<string, string>(){{"outcome", results }};

                log.LogInformation(response.ToString());
                return new OkObjectResult(response);
            }
            catch (Exception e)
            {
                return new BadRequestObjectResult(e);
            }
        }
        private static string CalculateFight(List<Player> players, int defenseDivisor)
        {
            DetermineAdvantage(players);
            string results = Attack(players, defenseDivisor);
            return results;
        }

        private static void DetermineAdvantage(List<Player> players)        
        {
            //create list of styles
            List<string> styles = players.Select(o => o.Style).ToList();

            //check player's style against styles list for advantage
            for (int i = 0; i < styles.Count; i++)
            {
                //TODO: to enable team battles multiplier property will need to be a dictionary
                //with the index of the player they're strong against
                if (players[i].Style == "MAGIC" && styles.Contains("MELEE"))
                    players[i].Multiplier = 2;
                if (players[i].Style == "MELEE" && styles.Contains("RANGE"))
                    players[i].Multiplier = 2;
                if (players[i].Style == "RANGE" && styles.Contains("MAGIC"))
                    players[i].Multiplier = 2;
            }
        }

        private static string Attack(List<Player> players, int defenseDivisor)
        {
            string results = "";
            float c1_total_time = 0;
            float c2_total_time = 0;
            Queue<Player> battle_queue = new Queue<Player>();

            foreach (var player in players)
                battle_queue.Enqueue(player);

            var combatant_1 = battle_queue.Dequeue();
            var combatant_2 = battle_queue.Dequeue();

            //solve combatant 1
            float c1_damage_solved = (.01f * combatant_1.Accuracy) * ((combatant_1.Strength * combatant_1.Multiplier) - (combatant_2.Defense / defenseDivisor));
            if (c1_damage_solved <= 0)
            {
                results += "Combatant_1 the "+combatant_1.Style+ " is too weak to damage Combatant_2. @ ";
                c1_total_time = -1;
            }
            else
            {
                c1_damage_solved += combatant_1.BaseRate;
                float c1_hits_required = combatant_2.Health / c1_damage_solved;
                c1_total_time = c1_hits_required * (1 / combatant_1.AttackSpeed);
                results += string.Format("Combatant_1 the {0} would take {1} seconds to defeat Combatant_2 the {2} @ ", combatant_1.Style, c1_total_time, combatant_2.Style);
            }
            
            //solve combatant 2
            float c2_damage_solved = (.01f * combatant_2.Accuracy) * ((combatant_2.Strength * combatant_2.Multiplier) - (combatant_1.Defense / defenseDivisor));
            if (c2_damage_solved <= 0)
            {
                results += "Combatant_2 the " + combatant_2.Style + " is too weak to damage Combatant_1. @ ";
                c2_total_time = -1;
            }
            else
            {
                c2_damage_solved += combatant_2.BaseRate;
                float c2_hits_required = combatant_1.Health / c2_damage_solved;
                c2_total_time = c2_hits_required * (1 / combatant_2.AttackSpeed);
                results += string.Format("Combatant_2 the {0} would take {1} seconds to defeat Combatant_1 the {2} @ ", combatant_2.Style, c2_total_time, combatant_1.Style);
            }

            //determine winner
            if (c1_total_time < c2_total_time || c2_total_time <= 0)
                results += "Combatant 1 Wins!";
            else if (c1_total_time == c2_total_time)
                results += "Draw!";
            else
                results += "Combatant 2 Wins!";
            return results;
        }
    }
}
