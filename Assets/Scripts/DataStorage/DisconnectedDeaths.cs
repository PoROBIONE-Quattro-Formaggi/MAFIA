// ReSharper disable StringLiteralTypo

using System;
using System.Collections.Generic;

namespace DataStorage
{
    public static class DisconnectedDeaths
    {
        /// <summary>
        /// Returns random disconnected death message from the predefined list
        /// </summary>
        /// <returns>Random disconnected death message</returns>
        public static string GetRandomDisconnectedDeath()
        {
            var random = new Random();
            var randomIndex = random.Next(0, ListOfDisconnectedDeaths.Count);
            var randomNewsHeadline = ListOfDisconnectedDeaths[randomIndex];
            return randomNewsHeadline;
        }

        private static readonly List<string> ListOfDisconnectedDeaths = new()
        {
            "Kraszewski’s pond. The police have recovered a drowned body. They suspect a suicide.",
            "40-year old Caucasian male arrested in connection to the fatal incident.",
            "Another fatal case of Black Death in our city.",
            "Classical Music’s Week. A man was seriously rammed.",
            "A scourge of heart attacks has taken from us another citizen.",
            "Have you seen this person? Please contact the Missing Person’s Office, Waleczna St, 36.",
            "Corpse in the city swimming pool/bath. The administration: “a leak of chlorine has occurred”",
            "[X!] The waitress recalls: “the guy just like, you know, fainted, and then he never got up again…”",
            "What has actually happened? Unexplained circumstances of death in the Maria Czubaszek’s Library.",
            "Suicide or a scream for help? Conversation with Doctor Warek about recent events."
        };
    }
}