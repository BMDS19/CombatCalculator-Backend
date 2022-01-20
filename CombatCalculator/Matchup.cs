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
                {
                    /*
                    string name = req.Query["name"];
                     = Convert.ToInt32(req.Query["defenseDivisor"]);

                    string p1_style = req.Query["p1_style"];
                    int p1_health = Convert.ToInt32(req.Query["p1_health"]);
                    int p1_strength = Convert.ToInt32(req.Query["p1_strength"]);
                    int p1_attackSpeed = Convert.ToInt32(req.Query["p1_attackSpeed"]);
                    int p1_defense = Convert.ToInt32(req.Query["p1_defense"]);
                    int p1_baseRate = Convert.ToInt32(req.Query["p1_baseRate"]);
                    int p1_accuracy = Convert.ToInt32(req.Query["p1_accuracy"]);

                    string p2_style = req.Query["p1_style"];
                    int p2_health = Convert.ToInt32(req.Query["p2_health"]);
                    int p2_strength = Convert.ToInt32(req.Query["p2_strength"]);
                    int p2_attackSpeed = Convert.ToInt32(req.Query["p2_attackSpeed"]);
                    int p2_defense = Convert.ToInt32(req.Query["p2_defense"]);
                    int p2_baseRate = Convert.ToInt32(req.Query["p2_baseRate"]);
                    int p2_accuracy = Convert.ToInt32(req.Query["p2_accuracy"]);
                    */
                }

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                JObject data = JsonConvert.DeserializeObject(requestBody) as JObject;
                

                int i = 0;
                List<object> players = new List<object>();
                foreach (var obj in data)
                {
                    if (i == 0) defenseDivisor = obj.Value.ToObject<int>();
                    else players.Add(obj);
                    i++;
                }

                //TODO: iterate through players

                //Player p1 = new Player(p1_style, p1_health, p1_strength, p1_attackSpeed, p1_defense, 1, p1_baseRate, p1_accuracy);
                //Player p2 = new Player(p2_style, p2_health, p2_strength, p2_attackSpeed, p2_defense, 1, p2_baseRate, p2_accuracy);

                //string results = CalculateFight(p1, p2, defenseDivisor);

                var response = new Dictionary<string, string>()
            {
                //{"outcome", results }
            };

                //log.LogInformation(response.ToString());
                return (ActionResult)new OkObjectResult(defenseDivisor.ToString());
                //return name != null
                //    ? (ActionResult)new OkObjectResult(i.ToString())
                //    : new BadRequestObjectResult("Please pass a name on the query string or in the request body");
            }
            catch (Exception e)
            {
                return new BadRequestObjectResult(e);
            }
        }
        private static string CalculateFight(Player p1, Player p2, int defenseDivisor)
        {
            DetermineAdvantage(p1, p2);
            float p1_results = Attack(p1,p2,defenseDivisor);
            float p2_results = Attack(p2,p1,defenseDivisor);

            if (p1_results == -1 && p2_results == -1)
                return "They're both too weak to damage each other.";
            if (p1_results == -1)
                return "P2 " + p2.Style + " wins!";
            if (p2_results == -1)
                return "P2 " + p1.Style + " wins!";

            if (p1_results > p2_results)
                return p2.Style + " wins!";
            else if(p1_results == p2_results)
                return "Draw " + p1_results.ToString() + " " + p2_results.ToString();
            else
                return p1.Style + " wins!";
        }

        private static void DetermineAdvantage(Player p1, Player p2)        
        {
            if (p1.Style == "MAGIC" && p2.Style == "MELEE")
                p1.Multiplier = 2;
            if (p1.Style == "RANGE" && p2.Style == "MAGIC")
                p1.Multiplier = 2;
            if (p1.Style == "MELEE" && p2.Style == "RANGE")
                p1.Multiplier = 2;

            if (p2.Style == "MAGIC" && p1.Style == "MELEE")
                p2.Multiplier = 2;
            if (p2.Style == "RANGE" && p1.Style == "MAGIC")
                p2.Multiplier = 2;
            if (p2.Style == "MELEE" && p1.Style == "RANGE")
                p2.Multiplier = 2;
        }

        private static float Attack(Player player, Player opponent, int defenseDivisor)
        {
            float damage_solved = (.01f * player.Accuracy) * ((player.Strength * player.Multiplier) - (opponent.Defense / defenseDivisor));
            if (damage_solved <= 0)
            {
                //too weak to damage enemy
                return -1;
            }
            damage_solved += player.BaseRate;
            float hits_required = opponent.Health / damage_solved;
            float total_time = hits_required * (1 / player.AttackSpeed);

            return total_time;
        }
    }
}
