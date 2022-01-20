using System;
using System.Collections.Generic;
using System.Text;

namespace CombatCalculator
{
    class Player
    {
        public string Style { get; set; }
        public int Health { get; set; }
        public int Strength { get; set; }
        public float AttackSpeed { get; set; }
        public int Defense { get; set; }
        public int Multiplier { get; set; }
        public int BaseRate { get; set; }
        public int Accuracy { get; set; }

        public Player(string style, int health, int strength, int attackSpeed, int defense, int multiplier, int baseRate, int accuracy)
        {
            Style = style;
            Health = health;
            Strength = strength;
            AttackSpeed = attackSpeed;
            Defense = defense;
            Multiplier = multiplier;
            BaseRate = baseRate;
            Accuracy = accuracy;
        }
    }
}
