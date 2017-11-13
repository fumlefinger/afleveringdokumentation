using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Threading;

namespace ConsoleApp1

{
    /// <summary>
    /// Genere hænder for både spiller, spillerens splittede hånd og dealers hånd
    /// </summary>
    class Player
    {
        public bool handStand = false;
        public bool handBust = false;
        public string handShow = "";
        public int handValue = 0;
        public List<string> hand = new List<string>();



    }



    class Program
    {

        //generere de 3 spilbare hænder og assigner dem til de individuelle classes
        public static Player player = new Player();
        public static Player playerSplit = new Player();
        public static Player dealer = new Player();

        //Laver arrays til de 52 kort og de 4 kulørere
        static string[] cards = new string[52];
        static string[] suits = new string[4];

        //mine statiske bools der skal tilgåes fra flere funktioner.
        static bool playerDidSplit = false;
        static bool choosing = true;
        static bool splitActive = false;

        // mine statiske ints der skal tilgåes fra flere funktioner.
        static int playerBet = 0;
        static int valuta = 0;

        //random number generator brugt til at vælge et tilfældigt kort
        static Random cardPicker = new Random();

        //Sql connection informationer samt username variabel der bruges til at logge ind i spil samt trække informationer ud af sql table
        static string username = "";
        static string sql = "SELECT * FROM brugere";
        static string mySqlConnectionString = "datasource=127.0.0.1;port=3306;username=root;password=;database=blackjack";
        static MySqlConnection databaseConnection = new MySqlConnection(mySqlConnectionString);
        static MySqlCommand cmd = new MySqlCommand(sql, databaseConnection);

        static void Main(string[] args)
        {
            

            //gemmer Brugernavne og Passwords og valuta ned fra database, da jeg ikke kan finde på en bedre måde at checke på dem i c#
            List<string> usernames = new List<string>();
            List<string> passwords = new List<string>();
            List<int> balance = new List<int>();

            //navngiver de 4 kort arter, Hjerter Spar Klør(Clubs) Ruder(Diamonds)
            suits[0] = "h";
            suits[1] = "s";
            suits[2] = "c";
            suits[3] = "d";

           
            //laver en række variabler der skal bruges i spillet
            string password = "";
            string newuser = "";
            string newpass = "";
            string playerAction = "";

            bool newUserCheck = true;
            bool logedIn = false;
            bool game = true;
            bool splitCheck = true;
            
            int wrongCounter = 0;
            

                
            //hiver brugernavne, Passwords og Valuta ned og gemmer dem i deres lists
            databaseConnection.Open();

            MySqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                usernames.Add(reader.GetString("brugernavn"));
                passwords.Add(reader.GetString("kodeord"));
                balance.Add(int.Parse(reader.GetString("valuta")));
            }

            databaseConnection.Close();


            //generer decket og bruges hver gang decket skal "blandes"
            ShuffleDeck();
            


            //login loop
            while (!logedIn)
            {

                Console.WriteLine("You need to log in to play this game please enter a username, or type (new) to create an account");
                username = Console.ReadLine();

                if (username == "new")
                {
                    while (newUserCheck)
                    {

                        Console.WriteLine("Please enter your desired username, type exit to return");
                        username = Console.ReadLine();

                        if (username == "exit")
                        {
                            username = "";
                            break;
                        }
                        else if (!usernames.Contains(username))
                        {
                            newUserCheck = false;
                            Console.WriteLine("Please enter the desired password");
                        }
                        else
                        {
                            Console.WriteLine("That username is already taken please choose a different one");
                        }

                    }
                    password = Console.ReadLine();

                    //Generer sql kommando til at indsætte brugernavn og kodeord i database
                    sql = "INSERT INTO brugere(brugernavn,kodeord,valuta) VALUES ('" + username + "','" + password + "','" + 500 + "')";
                    MySqlCommand insertion = databaseConnection.CreateCommand();

                    //tilføjer brugernavn, kode og penge til deres lists så spillet ikke skal genstartes efter oprettelse
                    usernames.Add(username);
                    passwords.Add(password);
                    balance.Add(500);

                    //kører den tidligere generede sql kommando og gemmer bruger, kode og penge i database for long term storage
                    insertion.CommandText = sql;
                    databaseConnection.Open();
                    insertion.ExecuteNonQuery();
                    databaseConnection.Close();
                    username = "";

                }

                //tjekker om der er skrevet noget i username
                if (username != "")
                {

                    wrongCounter = 0;
                    Console.WriteLine("Please enter your Password: ");
                    password = Console.ReadLine();

                    //for loop der iterere gennem alle brugernavne og kodeord for at finde et match, hvis alle forsøg fejler sendes fejlmeddelse
                    for (int i = 0; i < usernames.Count(); i++)
                    {
                        if (username == usernames[i] && password == passwords[i])
                        {
                            logedIn = true;
                            valuta = balance[i];
                            break;
                        }
                        else
                        {
                            wrongCounter++;
                        }
                    }

                    if (wrongCounter == usernames.Count())
                    {
                        Console.Clear();
                        Console.WriteLine("You have entered a wrong user name or password");
                    }
                }
            }

            //selve spillets loop
            while (game)
            {
                Console.ReadLine();
                Console.Clear();
                Console.WriteLine("Welcome to BlackJack " + username + " You currently have: " + valuta + " coins to bet");
                Console.WriteLine("The house will always hit on 16, and stand on 17");
                Console.WriteLine("please enter the amount you would like to bet: ");


                //loop der kører indtil spiller har valgt et beløb at satse
                while (playerBet == 0)
                {

                    //try catch der fanger bets under minimum indsats eller forsøg på at satse mere end der er på kontoen
                    //samt sørger for at der kun kan indtastes tal
                    try
                    {
                        playerBet = int.Parse(Console.ReadLine());
                        if (playerBet > valuta || playerBet < 10)
                        {
                            playerBet = 0;
                            Console.WriteLine("please only enter an amount between 10 and your total money count");
                        }
                    }
                    catch (FormatException es)
                    {
                        playerBet = 0;
                        Console.WriteLine("please only enter numbers into the bet");


                    }
                }

                Console.Clear();
                Console.WriteLine("Your bet of " + playerBet + " has been accepted, dealing hands");

                //funktion kaldes med argumentet new der giver spiller og dealer 2 kort hver
                DealHand("new");

                //propper de 2 kort som spiller har fået i den string der skrives ud så man kan se sin hånd
                for (int i = 0; i < player.hand.Count(); i++)
                {
                    player.handShow += player.hand[i];
                    player.handShow += " ";
                }

                Console.WriteLine(player.handShow);

                //det samme gøres her for dealer dog kun med 1 kort da dealers kort 2 er skjult i blackjack
                dealer.handShow += dealer.hand[0];
                

                Console.WriteLine(dealer.handShow);

                splitCheck = true;

                //while loop der kører så længe spilleren ikke har "standed"
                while (choosing)
                {
                    
                    //tjekker at spilleren ikke har standed og om spilleren kun har 2 kort på hånden for at give mulighed for double
                    if (player.handStand != true && player.hand.Count() == 2)
                    {
                        Console.WriteLine("Please Choose your next action (stand) (hit) (double)");
                        playerAction = Console.ReadLine();

                        //de forskellige if statements til de forskellige valg spilleren har
                        if (playerAction == "stand")
                        {
                            Stand(player);
                            player.handStand = true;
                            choosing = false;
                        }
                        else if (playerAction == "hit")
                        {
                            RepeatCard(player);
                            player.handShow += " ";
                            player.handShow += player.hand[player.hand.Count() - 1];
                            
                        }
                        else if (playerAction == "double")
                        {
                            RepeatCard(player);
                            player.handShow += " ";
                            player.handShow += player.handShow[player.hand.Count() - 1];
                            Stand(player);
                        }
                        else
                        {
                            Console.WriteLine("That was not a valid option please try agin");
                            playerAction = "";
                        }
                        Console.WriteLine(player.handShow);

                    }

                    // samme som ovenover bare uden check på håndens størrelse og uden double mulighed
                    else if (player.handStand != true)
                    {
                        while (player.handStand == false)
                        {
                            Console.WriteLine("Please Choose your next action (stand) (hit)");
                            playerAction = Console.ReadLine();
                            if (playerAction == "stand")
                            {
                                Stand(player);
                                player.handStand = true;
                                choosing = false;
                            }
                            else if (playerAction == "hit")
                            {
                                RepeatCard(player);
                                player.handShow += " ";
                                player.handShow += player.hand[player.hand.Count() - 1];
                                Console.WriteLine(player.handShow);
                            }
                            else
                            {
                                Console.WriteLine("That was not a valid option please try agin");
                                playerAction = "";
                            }
                            
                        }
                    }


                    //begyndende kode til split funktionen der ikke er skrevet færdig endnu
                    /*
                    if (splitActive == true && playerSplit.handStand != true)
                    {
                        while (playerSplit.handStand == true)
                        {
                            Console.WriteLine("Please choose your action for hand 2 (stand) (hit)");
                            playerAction = Console.ReadLine();
                            if (playerAction == "stand")
                            {
                                Stand(playerSplit);
                                choosing = false;
                                playerSplit.handStand = false;
                            }
                            else if (playerAction == "hit")
                            {
                                RepeatCard(playerSplit);
                                playerSplit.handShow += " ";
                                playerSplit.handShow += playerSplit.hand[playerSplit.hand.Count() - 1];
                                Console.WriteLine(player.handShow);
                            }
                            else
                            {
                                Console.WriteLine("That was not a valid option please try agin");
                                playerAction = "";
                            }

                        }
                    } */
                }

                //spiller er færdig med at spille, så dealerens hånd køres igennem
                DealerPlays();

                //kalkulere om spiller har vundet eller tabt sin hånd
                CalculateWin();

                //blander kortene og starter spillet forfra
                ShuffleDeck();

            }
        }

        /// <summary>
        /// Funktion der spiller dealerens hånd. tjekker om dealer skal hit eller stand
        /// </summary>
        static void DealerPlays()
        {
            bool dealerPlaying = true;
            while (dealerPlaying)
            {
                dealer.handValue = 0;
                dealer.handShow += " ";
                dealer.handShow += dealer.hand[dealer.hand.Count - 1];
                Stand(dealer);

                if (dealer.handValue <= 16 )
                {
                    RepeatCard(dealer);
                }

                if (dealer.handValue > 16)
                {
                    dealerPlaying = false;
                }
                
                Console.WriteLine(dealer.handShow);
            }
            
        }


        /// <summary>
        /// tjekker de forskellige værdier på hænderne op mod hinanden, og udbetaler gevinst eller tager indsats
        /// </summary>
        static void CalculateWin()
        {
            if (player.handBust == true)
            {
                valuta -= playerBet;
                Console.WriteLine("Sorry your hand busted");
            }


            if (playerSplit.handBust == true)
            {
                valuta -= playerBet;
                Console.WriteLine("sorry your second hand busted");
            }

            if (playerDidSplit == true && playerSplit.handBust != true)
            {
                if (playerSplit.handValue > dealer.handValue && dealer.handBust != true)
                {
                    valuta += playerBet;
                    Console.WriteLine("Congratulations your second hand won");
                }
                else if (playerSplit.handValue < dealer.handValue && dealer.handBust == false && playerSplit.handBust == false)
                {
                    valuta -= playerBet;
                    Console.WriteLine("sorry the dealer beat your second hand");
                }
                else if (playerSplit.handBust == false && dealer.handBust == true)
                {
                    Console.WriteLine("congratulations the dealer busted");
                }
            }
            else
            {
                if (player.handValue > dealer.handValue && player.handBust == false)
                {
                    valuta += playerBet;
                    Console.WriteLine("Congratulations your hand won");
                }
                else if (player.handValue < dealer.handValue && dealer.handBust == false && player.handBust == false)
                {
                    valuta -= playerBet;
                    Console.WriteLine("sorry the dealer beat your hand");
                }
                else if (player.handBust == false && dealer.handBust == true)
                {
                    valuta += playerBet;
                    Console.WriteLine("the dealer busted you won your hand");
                }
            }
            if (player.handValue == dealer.handValue)
            {
                Console.WriteLine("equal hands, you push");
            }
            if (playerSplit.handValue == dealer.handValue)
            {
                Console.WriteLine("your second hand is equal you push");
            }

            //opdatere spillerens pengepung i databasen så de gemmes ved spil luk
            sql = "UPDATE brugere SET valuta = "+valuta+" WHERE brugernavn = '"+username+"'";
            MySqlCommand insertion = databaseConnection.CreateCommand();
            insertion.CommandText = sql;
            databaseConnection.Open();
            insertion.ExecuteNonQuery();
            databaseConnection.Close();
        }


        /// <summary>
        /// udregner værdien på den hånd der bliver send som argument
        /// </summary>
        /// <param name="who">navnet på den spiller der stander</param>
        static void Stand(Player who)
        {
            int cardcount = 0;
            int acecounter = 0;
            bool aces = false;


            cardcount = who.hand.Count();

            for (int i = 0; i < cardcount; i++)
            {

                if (who.hand[i] == "1")
                {
                    who.handValue += 10;
                    acecounter++;
                    aces = true;
                }

                try
                {
                    who.handValue += int.Parse(who.hand[i]);
                }
                catch (FormatException es)
                {
                    who.handValue += 10;
                }
            }

            while (aces == true)
            {
                if (who.handValue > 21)
                {
                    who.handValue -= 10;
                    acecounter--;


                    if (acecounter < 1)
                    {
                        aces = false;
                    }
                }
                else
                {
                    break;
                }
            }

            if (who.handValue > 21)
            {
                who.handBust = true;
            }
        }


        /// <summary>
        /// split funktion der ikke er lavet endnu
        /// </summary>
        static void Split()
        {
            playerSplit.hand.Add(player.hand[1]);
            player.hand.RemoveAt(1);
            player.handShow = "Your hand is: ";
            playerSplit.handShow = "Your second hand is: ";
            RepeatCard(player);
            RepeatCard(playerSplit);

            for (int i = 0; i < 2; i++)
            {
                player.handShow += player.hand[i];
                player.handShow += " ";
                playerSplit.handShow += playerSplit.hand[i];
                playerSplit.handShow += " ";
            }

            splitActive = true;
        }

        /// <summary>
        /// Denne funktion dealer kort ud til spiller og dealer og fjerner ligeledes kort fra (cards) listen
        /// </summary>
        static void DealHand(string state)
        {

            int cardChosen = 0;

            if (state == "new")
            {
                cardChosen = cardPicker.Next(0, cards.Count());
                player.hand.Add(cards[cardChosen]);
                cards[cardChosen] = "";
                RepeatCard(dealer);
                RepeatCard(player);
                RepeatCard(dealer);

            }
        }



        /// <summary>
        /// Checker om det kort der forsøges delt allerede er delt, og vælger et nyt hvis det er tilfældet
        /// </summary>
        /// <param name="who">who er enten player eller dealer</param>
        static void RepeatCard(Player who)
        {
            int cardChosen;


            while (true)
            {
                cardChosen = cardPicker.Next(0, cards.Count());
                if (cards[cardChosen] != "")
                {
                    who.hand.Add(cards[cardChosen]);
                    cards[cardChosen] = "";
                    break;
                }
            }


        }

        /// <summary>
        /// Generere en ny hel liste af kort, da der konstant 
        /// slettes kort fra decket for at give en mere præcis gengivelse af at deale en hånd
        /// </summary>
        static void ShuffleDeck()

        {

            

            int indexer = 0;

            player.handValue = 0;
            playerSplit.handValue = 0;
            dealer.handValue = 0;
            playerBet = 0;

            splitActive = false;
            player.handStand = false;
            playerSplit.handStand = false;
            player.handBust = false;
            playerSplit.handBust = false;
            dealer.handBust = false;
            playerDidSplit = false;
            choosing = true;

            player.hand.Clear();
            playerSplit.hand.Clear();
            dealer.hand.Clear();
            player.handShow = "Your hand is: ";
            playerSplit.handShow = "Your hand is: ";
            dealer.handShow = "The Dealers hand is: ";


            for (int i = 1; i < 14; i++)
            {
                for (int j = indexer; j < indexer + 4; j++)
                {


                    if (i > 12)
                    {
                        cards[j] = "K";
                    }
                    else if (i > 11)
                    {
                        cards[j] = "D";
                    }
                    else if (i > 10)
                    {
                        cards[j] = "J";
                    }
                    else
                    {
                        cards[j] = i.ToString();
                    }
                }
                indexer += 4;
            }
        }
    
    }
}
