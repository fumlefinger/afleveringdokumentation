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

        public static Player player = new Player();
        public static Player playerSplit = new Player();
        public static Player dealer = new Player();

        static string[] cards = new string[52];
        static string[] suits = new string[4];

        static bool playerDidSplit = false;
        static bool choosing = true;
        static bool splitActive = false;

        static int playerBet = 0;
        static int valuta = 0;

        static Random cardPicker = new Random();

        static string username = "";
        static string sql = "SELECT * FROM brugere";
        static string mySqlConnectionString = "datasource=127.0.0.1;port=3306;username=root;password=;database=blackjack";
        static MySqlConnection databaseConnection = new MySqlConnection(mySqlConnectionString);
        static MySqlCommand cmd = new MySqlCommand(sql, databaseConnection);

        static void Main(string[] args)
        {

            //Opsæt af database forbindelse
            

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
                    sql = "INSERT INTO brugere(brugernavn,kodeord,valuta) VALUES ('" + username + "','" + password + "','" + 500 + "')";
                    MySqlCommand insertion = databaseConnection.CreateCommand();
                    usernames.Add(username);
                    passwords.Add(password);
                    insertion.CommandText = sql;
                    databaseConnection.Open();
                    insertion.ExecuteNonQuery();
                    databaseConnection.Close();
                    username = "";

                }

                if (username != "")
                {
                    wrongCounter = 0;
                    Console.WriteLine("Please enter your Password: ");
                    password = Console.ReadLine();

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

                while (playerBet == 0)
                {
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
                DealHand("new");

                for (int i = 0; i < player.hand.Count(); i++)
                {
                    player.handShow += player.hand[i];
                    player.handShow += " ";
                }

                Console.WriteLine(player.handShow);

                dealer.handShow += dealer.hand[0];
                

                Console.WriteLine(dealer.handShow);

                splitCheck = true;
                while (choosing)
                {
                    
                    if (player.handStand != true && player.hand.Count() == 2)
                    {
                        Console.WriteLine("Please Choose your next action (stand) (hit) (double)");
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
                    }
                }
                DealerPlays();
                CalculateWin();
                ShuffleDeck();

            }
        }


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
            sql = "UPDATE brugere SET valuta = "+valuta+" WHERE brugernavn = '"+username+"'";
            MySqlCommand insertion = databaseConnection.CreateCommand();
            insertion.CommandText = sql;
            databaseConnection.Open();
            insertion.ExecuteNonQuery();
            databaseConnection.Close();
        }



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

        static void Split(Player who)
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
