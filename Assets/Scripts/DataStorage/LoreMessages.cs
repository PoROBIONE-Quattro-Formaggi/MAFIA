// ReSharper disable StringLiteralTypo

using System;
using System.Collections.Generic;

namespace DataStorage
{
    public static class LoreMessages
    {
        /// <summary>
        /// Returns random lore message from the predefined list
        /// </summary>
        /// <returns>Random lore message</returns>
        public static string GetRandomMessage()
        {
            var random = new Random();
            var randomIndex = random.Next(0, ListOfMessages.Count);
            var randomMessage = ListOfMessages[randomIndex];
            return randomMessage;
        }

        private static readonly List<string> ListOfMessages = new()
        {
            "The Sodomy Boys have canceled their concert! “It’s about safety of the residents!”",
            "What a Sodom! New record from The Sodomy Boys on 15.04 [LISTEN TO THE FIRST SINGLE]",
            "The oldest man in the world tells us a secret of a long life.",
            "Tasty, not so sweet? We visited the “The Honeymooners” beer hall.",
            "X over the years. [SEE PHOTOS]",
            "A true Pole should score at least 13/20 on this history quiz.",
            "We’ve said goodbye to the longest living resident of our city, Janusz Borek.",
            "This inconspicuous fruit has great nutritional value . You surely have it in your house!",
            "The Sodomy Boys concert tickets are selling out!",
            "Ex manager about the boy band: “The police should investigate them”",
            "The citizens harsh words about the mayor: “GTFO!”",
            "The pupils of 2nd Primary School named after Bruno Jasieński in the finals of the III National Tournament for the Golden Linden",
            "Złotowscy’s Park in the ranking of the most beautiful parks in the whole Europe!",
            "Attention drivers! Road renovations have started in the center…",
            "Trains announced the safest form of transportation. [SEE THE STATISTICS]",
            "Entrepreneurs: “We are short of labor”",
            "What is worth eating at Russian Datcha?",
            "Attention! Incoming changes in the route of line 64 tram.",
            "Have you seen them? Report to the police immediately!",
            "Rising prices? Simple steps on how to save up on the most significant expenses.",
            "Tips from Maria Wasilewska for soon-to-be fathers.",
            "Szczepan Wyszycki about his wife’s romance: “Unfortunately, I did see it coming.”",
            "The history of the first Student’s Anarchist Club",
            "“From the smog begotten” returns to the Three Heads Theater stage.",
            "Student! Are you aware of your rights?",
            "Recruitment has started to the University of Sylva",
            "Are you hungry for more? Still got an appetite? Take part in French bread pizza eating competition",
            "III Stanisława Wasilewicz’s Marathon will take place on the streets of our city",
            "Are you smarter than a 5-year old? Do this simple test!",
            "Grzegorz Walczak  advises: “There is no such thing as too much of sauerkraut”",
            "Babysitter for a 2-year old needed!",
            "Men after 40! Sign for a doctor check-up!",
            "Woman behind the steering wheel? Impossible, or so we thought! Become a driver",
            "It is for sure now. Easter Egg Festival will not take place after all.",
            "“I invite back to the primary school” - Dr Brzęczek’s words about “Tax Scandal”",
            "Shocking! Check how the value of the dollar has changed over the years.",
            "”Ms. or “Mrs.”? The linguist responds to the editorial.",
            "One in five residents has already changed their heating system!",
            "Which computer is best to purchase?",
            "Take part in the competition for the most charming cafe in the city.",
            "Sex, drugs and rock’n’roll: Shocking stories about The Sodomy Boys",
            "The mayor refuses to comment.",
            "Kulewska on AI: “Do androids dream of electronic sheep?!”",
            "The mystery of “haunted floor” solved: “It was just a meowing cat!”.",
            "Women absolutely ADORE that! Not so many men know about it…",
            "She used to be a sexbomb… today, she might be teaching your children.",
            "Parents, come to your senses! 30 tips for parents of first-graders",
            "Veni, vidi, vici, in other words, all about the scout rally on Strzelista Mountain.",
            "Horoscope for Scorpio in May. You definitely didn't expect that!",
            "Saint Venice is launching massive promotions. Get a deal!"
        };
    }
}