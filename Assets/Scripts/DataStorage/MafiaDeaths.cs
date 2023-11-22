// ReSharper disable StringLiteralTypo

using System;
using System.Collections.Generic;

namespace DataStorage
{
    public static class MafiaDeaths
    {
        /// <summary>
        /// Generates a random death message related to mafia activities with the specified player name.
        /// </summary>
        /// <param name="playerName">The name of the player who will be put in the message.</param>
        /// <returns>A random mafia death message with the player's name.</returns>
        public static string GetRandomMafiaDeathMessage(string playerName)
        {
            var capitalizedPlayerName = char.ToUpper(playerName[0]) + playerName[1..];
            var random = new Random();
            var randomIndex = random.Next(0, ListOfDeathMessages.Count);
            var randomDeathMessage = ListOfDeathMessages[randomIndex];
            randomDeathMessage = randomDeathMessage.Replace("{PlayerName}", capitalizedPlayerName);
            return randomDeathMessage;
        }

        private static readonly List<string> ListOfDeathMessages = new()
        {
            "The mafia has silenced another soul. {PlayerName} will be remembered in whispers.",
            "In the dark underworld, {PlayerName} met their untimely end. The streets are watching.",
            "A notorious hitman claimed {PlayerName}'s life. The city trembles in fear.",
            "{PlayerName} crossed the wrong gang, paying the ultimate price.",
            "A shadowy figure took {PlayerName} out of the game. Retribution is inevitable.",
            "The don has spoken. {PlayerName} sleeps with the fishes now.",
            "An underworld dispute ended with {PlayerName}'s life in the balance.",
            "The streets are unforgiving. {PlayerName} fell victim to the lawless night.",
            "{PlayerName} was marked for death, and the mafia delivered.",
            "In the criminal underworld, loyalty is scarce. {PlayerName} paid the price for betrayal."
        };
    }
}