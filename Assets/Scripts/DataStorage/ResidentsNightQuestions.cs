// ReSharper disable StringLiteralTypo

using System;
using System.Collections.Generic;
using System.Linq;

namespace DataStorage
{
    public static class ResidentsNightQuestions
    {
        /// <summary>
        /// Gets random question and corresponding answers from the predefined set
        /// </summary>
        /// <returns>KeyValuePair where the Key is the question, and Value is the list of the corresponding answers</returns>
        public static KeyValuePair<string, List<string>> GetRandomQuestionWithAnswers()
        {
            var random = new Random();
            var randomIndex = random.Next(0, QuestionToAnswers.Count);
            var randomQuestion = QuestionToAnswers.ElementAt(randomIndex);
            return randomQuestion;
        }

        private static readonly Dictionary<string, List<string>> QuestionToAnswers =
            new()
            {
                {
                    "Who’s the best musician from The Sodomy Boys?",
                    new List<string> { "Mike", "Jacob", "Aaron", "Bob" }
                },
                {
                    "Are you going to The Sodomy Boys’ concert?",
                    new List<string> { "Yes", "No", "Didn’t they cancel it?", "Who are they?" }
                },
                {
                    "Pick the best meal from the Russian Datcha to compete in Hell Master Chef Kitchen with Ramon Gorday",
                    new List<string>
                    {
                        "Roasted duck with cranberry sauce", "Chicken nuggets plated on mustard cream",
                        "Steamed seasonal vegetables risotto", "Tomato cream soup"
                    }
                },
                {
                    "Do you have any experience as a babysitter?",
                    new List<string>
                    {
                        "Yes, I love children", "No, but willing to gain experience", "No, not interested",
                        "Animals preferred"
                    }
                },
                {
                    "Where would you like the new tram route to go (road works estimated time: 2 years)?",
                    new List<string>
                    {
                        "Złotowscy’s Park - Three Head Theater", "2nd Primary School - Tram Depot",
                        "University of Sylva - Town’s Hall", "Beer Hall - Russian Datcha"
                    }
                },
                {
                    "Do you feel that it is appropriate to let ex-celebrities be teachers at Primary School?",
                    new List<string>
                    {
                        "Yes, why not", "Yes, as long as it is Michalina Starkowska",
                        "No, we should keep children away from such influences",
                        "No, teachers must have appropriate education"
                    }
                },
                {
                    "What should be the most overhyped matter to be voted on in our city?",
                    new List<string>
                    {
                        "Driver course dedicated for women", "More places at nurseries",
                        "Police investigation on The Sodomy Boys", "Ostracisation of Dr Brzęczek"
                    }
                },
                {
                    "Pick the most unnecessary social initiative this year:",
                    new List<string>
                    {
                        "Easter Egg Festival", "French Bread Pizza Eating Competition",
                        "Opening of ‘The Honeymooners’ Beer Hall", "“Tax Scandal” protest"
                    }
                },
                {
                    "Who’s the most promising young artist from our city to represent us at Eurovision?",
                    new List<string> { "The Sodomy Boys", "B00gies", "Fha", "Susanah" }
                },
                {
                    "What would be your preferred location for the new coworking space that will be used as a phone charging station and teenager’s hangout spot?",
                    new List<string>
                    {
                        "New the Złotowscy’ Park", "In my basement", "Near the tram stops of 64 line route",
                        "By the Saint Venice"
                    }
                },
                {
                    "What would you guess is the depth of Liberty Lake?",
                    new List<string> { "211 meters", "69 meters", "12 meters", "242 meters" }
                },
                {
                    "What are your thoughts on the mural work of Antonin Borek?",
                    new List<string>
                    {
                        "It’s a work of art (confusing one)", "It’s okay", "It’s vandalism, why is it even allowed?",
                        "I could do it cheaper and better (and with no artistic experience)"
                    }
                },
                {
                    "What kind of community event would you like to be organized so that you can have an excuse to leave work early?",
                    new List<string>
                    {
                        "Neighbourhood Day", "Charity Triathlon", "Movie Night under the night sky", "Farmer’s Market"
                    }
                },
                {
                    "Are you planning to participate in the Stanisława Walasiewicz’s Third Marathon?",
                    new List<string>
                    {
                        "Training since last year", "Probably", "No, only as a watcher",
                        "No, it only generates unnecessary traffic"
                    }
                },
                {
                    "After which cocktail at Russian Datcha did you commit the most regrettable decisions? (Answers are collected for the ‘Best Summer Drink’ questionnaire)",
                    new List<string>
                    {
                        "Tom Collins’ Adventure (includes magic powder)",
                        "Mary, Mary, Forgive Me (Modern Margarita Twist)",
                        "Old Town Old Fashioned (older than Janusz Borek)",
                        "Longest Island (3 different flavoured vodkas involved)"
                    }
                },
                {
                    "What’s the hottest gossip from the last local government debate?",
                    new List<string>
                    {
                        "Prices of real estate market (landlords really need 4 month vacations from not doing anything)",
                        "School’s toilet paper affair - why do we pay 6 monthly yearly and yet there is not one piece?",
                        "Free prostate exam for men", "Hostile architecture, do we really need spikes on the pavements?"
                    }
                },
                {
                    "Ever thought about joining the Anarchists Student Club?",
                    new List<string>
                    {
                        "Been a member since 99’", "They are a bunch of idiots", "Sound like a Scientologist’s Church",
                        "Where are the teachers? Where are the parents?"
                    }
                },
                {
                    "Estimate the number of burbits the mayor has in his office:",
                    new List<string> { "Around 2000", "359", "Like 6", "Six 10-piece packets" }
                },
                {
                    "Vote for new mandatory class at the University of Salva:",
                    new List<string>
                    {
                        "Telekinesis", "Astrology with minor in Chinese Zodiac Signs",
                        "Techno Etiquette 101 with practical exam", "Mixology (no one knows what it means)"
                    }
                },
                {
                    "What is your go-to excuse for being late?",
                    new List<string>
                    {
                        "Tram line 64 derailed again (for the 12th time this year)", "Another marathon-related traffic",
                        "My dog was crying for me leaving him at the dog daycare", "The pavement spiked me"
                    }
                },
                {
                    "What was your favourite PE class at the local High School?",
                    new List<string>
                    {
                        "Snorkelling in the Town Hall Fountain", "Puppy yoga", "Techno moves, advanced",
                        "Avoiding PE was a sport itself"
                    }
                },
                {
                    "What is the best landmark in town?",
                    new List<string>
                    {
                        "The Haunted Carousel", "Upside-Down House", "Whispering Well that gives great love advice",
                        "The Potato Statue"
                    }
                },
                {
                    "How should the town deal with the invasion of raccoons?",
                    new List<string>
                    {
                        "Training them like dogs", "Raccoon yoga", "Charity raccoon-walking group", "Adoption program"
                    }
                },
                {
                    "What community garden initiative would you support to promote sustainable living?",
                    new List<string>
                    {
                        "Vertical Gardening Workshops", "Seed Exchange Program", "Beekeeping and Pollinator Gardens",
                        "Composting Education Sessions"
                    }
                },
                {
                    "What city-sponsored class or workshop would you be interested in attending?",
                    new List<string>
                    {
                        "Home Gardening 101", "Financial Planning for Students", "Yoga in the Park",
                        "DIY Home Repair Basics"
                    }
                },
                {
                    "What annual event brings the most joy and unity to the community?",
                    new List<string>
                    {
                        "Community Picnic in the Park", "New Year’s Eve Countdown", "National Night Against Crime",
                        "Family Barbecue at the Rooftop of the Airport"
                    }
                },
                {
                    "Which local restaurant, in your opinion, serves the best comfort food?",
                    new List<string> { "Peach Mama", "Cozy Corner", "Nana’s Kitchen", "Wookame" }
                },
                {
                    "What would be the official snack of the city’s movie nights in the park?",
                    new List<string>
                    {
                        "Popcorn drizzled with caramel and peanuts", "Pickle-flavoured lollipops",
                        "Nachos topped with cheese and onion", "Ice cream sandwiches with hot sauce"
                    }
                },
                {
                    "If aliens landed in our city and demanded a tour guide, who would be the best choice?",
                    new List<string>
                    {
                        "Crazy Carla, the girl who talks to butterflies", "Disco Denis, the dancing fireman",
                        "Funny Hunny, the stand-up comedian at the local petrol station",
                        "Grandma Eugene, the scooter-driving daredevil"
                    }
                },
                {
                    "What should be the official city anthem?",
                    new List<string>
                    {
                        "“The Ballad of the Shopping Mall”", "“Rats in the pipes”", "“The Great Pigeon Race”",
                        "“The Sound of Hot-dogs Eating Contest ”"
                    }
                },
                {
                    "If you were a flavor of ice cream, what would you be?",
                    new List<string>
                        { "Avant-Garde Almond", "Spoiled Strawberry", "Wacky Watermelon", "Chill Cherries" }
                },
                {
                    "What would be the most absurd event in the city’s Olympics?",
                    new List<string>
                    {
                        "Shopping Cart Racing", "Competitive Parallel Parking", "Extreme School Chair Jousting",
                        "Freestyle Treadmill Texting"
                    }
                },
                {
                    "If the mayor decided to have a pet as an official city spokesperson, which animal should it be?",
                    new List<string>
                    {
                        "A parrot with a talent for public speaking",
                        "A turtle with a slow but steady approach to governance",
                        "A chimpanzee that communicates with sign language",
                        "A fox with a knack for political commentary"
                    }
                },
                {
                    "If the city hosted a “Worst Haircut” competition, who would win?",
                    new List<string>
                    {
                        "Frizzy Diana with the accidental mohawk", "Curly John with the asymmetrical french crop",
                        "Wild Veronica with green mullet", "Bald Rob with the questionable wig"
                    }
                },
                {
                    "What should be the punishment for jaywalking in the city?",
                    new List<string>
                    {
                        "Forced participation in a crosswalk dance-off",
                        "Wearing a cow costume while crossing a street",
                        "Attending a mandatory 'Traffic Etiquette' course",
                        "Becoming the official city crosswalk flag waver for the day"
                    }
                },
                {
                    "If the city decided to build a monument to honor its quirkiest resident, what should it be?",
                    new List<string>
                    {
                        "A giant metal goose statue", "A monument shaped like a talking traffic cone",
                        "A sculpture of the world’s largest soap bubble",
                        "A statue of the infamous 'Rat Whisperer' in action"
                    }
                },
                {
                    "Who is the leading actor in the upcoming movie “In the City Shadows”?",
                    new List<string> { "John Anderson", "Eve Roberts", "Hans Sullivan", "Sarah Roleson" }
                },
                {
                    "Which theme should be chosen for this year’s city carnival?",
                    new List<string>
                        { "Fairy Tale", "Time Travel Extravaganza", "Galactic Carnival", "Historical Heroes Parade" }
                },
                {
                    "What is the best pizza topping at Pizzarella Palace?",
                    new List<string>
                        { "Pepperoni and Mushrooms", "BBQ Chicken and Pineapple", "Extra Cheese", "Veggie Delight" }
                },
                {
                    "In the event of a zombie apocalypse, where would you prefer the city’s evacuation point to be?",
                    new List<string>
                        { "Abandoned techno club", "Sports stadium", "Shopping mall", "Underground channels" }
                },
                {
                    "What’s the best way to promote environmental awareness in schools?",
                    new List<string>
                    {
                        "Introduce a mandatory eco-friendly curriculum", "Organize tree-planting events",
                        "Conduct regular environmental workshops", "Implement a school garden project"
                    }
                },
                {
                    "Who should be honored with the “Citizen of the Year” award?",
                    new List<string>
                    {
                        "Camilla Thomal for community service", "Enrique Lesias for artistic contributions",
                        "Dr. Hillal for academic achievements", "Officer Ramirez for outstanding bravery"
                    }
                },
                {
                    "Which local artist should design the new city flag?",
                    new List<string>
                    {
                        "Roxanne Rivera (Abstract Art)", "James Baxville (Photorealism)",
                        "Helena Fernandez (Sculpture)",
                        "Hen Chan (Digital Art)"
                    }
                },
                {
                    "What’s the ideal location for the new city library?",
                    new List<string>
                    {
                        "Uptown Square", "Hillside Park", "Old Town Quarters", "Next to the Technical University Campus"
                    }
                },
                {
                    "What do you think about the physical appearance of the most famous seducer - George Flowercup?",
                    new List<string> { "He’s fine", "He’s very attractive", "Meh", "About who?" }
                },
                {
                    "Which parenting style is the best?",
                    new List<string> { "Permissive", "Authoritative", "Neglectful", "Authoritarian" }
                },
                {
                    "Which Latin phrase suits you the most?",
                    new List<string>
                    {
                        "De gustibus non est disputandum (in matters of taste, there can be no disputes)",
                        "Cogito, ergo sum (I think, therefore I am)", "In vino veritas (in wine, there is truth)",
                        "Persona non grata (an unwelcome person)"
                    }
                },
                {
                    "Which Zodiac sign is the most stubborn?",
                    new List<string> { "Taurus", "Pisces", "Scorpio", "Capricorn" }
                },
                {
                    "Which cafe in the city is your favorite?",
                    new List<string> { "Cuatro Perros", "CD Cafe", "Starmax Cafe", "Sweet Girl" }
                },
                {
                    "What is the height of Srzelista Mountain?",
                    new List<string> { "1123 m", "3000 m", "4414 m", "2700 m" }
                }
            };
    }
}