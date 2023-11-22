// ReSharper disable StringLiteralTypo

using System;
using System.Collections.Generic;

namespace DataStorage
{
    public static class DefaultAlibis
    {
        /// <summary>
        /// Returns random alibi from the predefined list
        /// </summary>
        /// <returns>Random alibi</returns>
        public static string GetRandomAlibi()
        {
            var random = new Random();
            var randomIndex = random.Next(0, ListOfAlibis.Count);
            var randomAlibi = ListOfAlibis[randomIndex];
            return randomAlibi;
        }

        private static readonly List<string> ListOfAlibis = new()
        {
            "I was in the vespers.",
            "I was at the Sodomy Boys concert.",
            "I was studying for an exam on Lithuania’s history at the library.",
            "I was at the vet appointment with my hamster.",
            "I was watching the theater show “From the smog begotten”",
            "I attend evening studies and I was just at the lecture.",
            "I got some kip after an exhausting day.",
            "The porter would’ve seen if someone had left my apartment.",
            "I’m literally 10 years old",
            "The Vth - You shall not kill.",
            "Until yesterday’s evening I was in quarantine.",
            "I was in the sauna, which can be easily verified by the reception desk.",
            "My roof was leaking, so I couldn’t even leave the house.",
            "I was painting the walls in the hallway.",
            "I had a house makeover.",
            "The whooping cough got me.",
            "It was Britney Spears’ Night at the club!",
            "My parents grounded me.",
            "I got stuck working over hours again.",
            "I brewed.",
            "I went on a date to “Russian datcha”",
            "I had Czech lessons.",
            "I had yoga classes.",
            "I had math tutoring.",
            "They still didn’t take my cast off.",
            "My long-distance friend visited me.",
            "I was looking at the stars (it was the Big Dipper).",
            "I didn’t even have the strength to get out of bed.",
            "I wasn’t even close to the crime scene.",
            "There was a fire on my lot.",
            "I was writing an article about risks coming from the Ancient Europe philosophy.",
            "It was me that reported the death!",
            "The murder took place at X, back then I was stuck in a traffic jam.",
            "I’ve heard the thud of a fight and was scared to leave the room.",
            "A mad diarrhea got me.",
            "I am too weak of a person.",
            "I got issued a speeding ticket back then.",
            "I had a Project X type of house party back then."
        };
    }
}