// ReSharper disable StringLiteralTypo

using System;
using System.Collections.Generic;

namespace DataStorage
{
    public static class Executions
    {
        /// <summary>
        /// Returns random execution message from the predefined list
        /// </summary>
        /// <param name="playerName">Player's name to be put into the execution message</param>
        /// <returns>Random execution message with inputted player's name</returns>
        public static string GetRandomDeathMessage(string playerName = "Anonym")
        {
            var capitalisedPlayerName = char.ToUpper(playerName[0]) + playerName[1..];
            var random = new Random();
            var randomIndex = random.Next(0, ListOfExecutions.Count);
            var randomDeathMessage = ListOfExecutions[randomIndex];
            randomDeathMessage = randomDeathMessage.Replace("{PlayerName}", capitalisedPlayerName);
            return randomDeathMessage;
        }

        private static readonly List<string> ListOfExecutions = new()
        {
            "Viva la revolución! Or not? The revolution has eaten its child. {PlayerName} was executed.",
            "{PlayerName} met their demise in a tragic accident. May they rest in peace.",
            "The notorious killer claimed another victim. This time it was {PlayerName}.",
            "In a shocking turn of events, {PlayerName} was found lifeless. The cause of death remains unknown.",
            "{PlayerName} succumbed to their injuries after a fierce battle. A true warrior until the end.",
            "Rumors circulate about the mysterious death of {PlayerName}. Authorities are investigating.",
            "{PlayerName} was eliminated in a strategic move. The game continues without them.",
            "The Grim Reaper visited the virtual realm and took {PlayerName} with them.",
            "An unfortunate incident claimed the life of {PlayerName}. The gaming community mourns the loss.",
            "{PlayerName} faced an unexpected demise. Friends and allies are in disbelief.",
            "In a twist of fate, {PlayerName} was taken from us. Their digital legacy lives on."
        };
    }
}