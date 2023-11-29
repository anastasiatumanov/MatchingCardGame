// Author: Anastasia Tumanov
// File Name: Main.cs
// Project Name: AnastasiaTPASS1
// Creation Date: September 13th, 2022
// Modified Date: September 29rd, 2022
// Description: This game allows user to play on their own or with a partner. The goal is to match as many cards using their ranks as possible.

using System;
using System.IO;
using System.Collections.Generic;

namespace PASS1Testing
{
    class MainClass
    {
        // File I/O streamwriter/reader
        static StreamWriter outFile;
        static StreamReader inFile;

        // Random number generator
        static Random rng = new Random();

        // Deck of cards
        static List<int> DECK = new List<int>();

        // Multi dimensional array of cards (rows and columns)
        static int[,] CARDS = new int[4, 13];

        // Card states
        static char[,] GAME_STATE = new char[4, 13];

        // Total number of cards
        static int DECKNUM = 52;

        // Placeholders for card 1 (saved card)
        static char CARD1 = ' ';
        static int CARD1X = -1;
        static int CARD1Y = -1;

        // Used to determine whether the card is first or second to be picked
        static int STEP = 0;

        // Statistics
        static int MATCHES1 = 0;
        static int FLIPS1 = 0;
        static int MATCHES2 = 0;
        static int FLIPS2 = 0;

        // Boolean for players turn 
        static bool PLAYER1 = true;

        // String containing current player information
        static string PLAYER_TURN = "Player 1";

        // Used to determine single or multi player modes
        static string MODE = "";

        // Game statistics
        static int TOTAL_SINGLE_GAMES;
        static int TOTAL_SINGLE_FLIPS;
        static int FASTEST_WIN;
        static int TOTAL_MULTI_GAMES;
        static int PLAYER_1_WINS;
        static int PLAYER_2_WINS;

        public static void Main(string[] args)
        {
            // Used to display suit symbols
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            // Read in file statistics
            ReadStats();

            // Users menu choice
            string userOption = "";

            // Loop until user chooses an invalid option
            while (userOption != "4")
            {
                // Display Menu
                DisplayMenu();

                // Read in user option
                userOption = Console.ReadLine();

                // Create, shuffle and deal cards
                CreateDeck();
                ShuffleDeck();
                DealCards();

                // Based on user option, switch playing mode
                switch (userOption)
                {
                    case "1":
                        // Single player mode
                        MODE = "Single";
                        SinglePlayer();
                        break;
                    case "2":
                        // Multi player mode
                        MODE = "Multi";
                        MultiPlayer();
                        break;
                    case "3":
                        // Draw statistics 
                        DrawStatsScreen();
                        break;
                }
            }
        }

        //Pre: None
        //Post: None
        //Description: Single Player mode
        private static void SinglePlayer()
        {
            // Clear screen
            Console.Clear();

            // Reset stat variables
            FLIPS1 = 0;
            MATCHES1 = 0;

            // Display empty board
            DisplayBoard();

            // Game continues until all cards have been paired
            while (MATCHES1 < 26)
            {
                CheckCard();
            }

            // Clear screen
            Console.Clear();

            // Display ending message
            Console.WriteLine("Congratulations! You got all 26 matches! Press ENTER to return to menu.");
            Console.ReadLine();

            // Update statistics 
            TOTAL_SINGLE_GAMES += 1;
            TOTAL_SINGLE_FLIPS += FLIPS1;

            // If current number of flips is smaller
            if (FLIPS1 < FASTEST_WIN || FLIPS1 == 0)
            {
                // Update file statistics
                FASTEST_WIN = FLIPS1;
            }

            // Write statistics
            WriteStats();
        }
        private static void MultiPlayer()
        {
            // Reset stat variables
            FLIPS1 = 0;
            MATCHES1 = 0;
            FLIPS2 = 0;
            MATCHES2 = 0;
            PLAYER1 = true;
            PLAYER_TURN = "Player 1";

            // Display board
            DisplayBoard();

            // While the total number of matches is less than 26
            while ((MATCHES1 + MATCHES2) < 26)
            {
                // If it is player 1s turn 
                if (PLAYER1)
                {
                    // Flip card and check match
                    CheckCard();

                    // Flip card and check match
                    CheckCard();

                }
                else if (!PLAYER1)
                {
                    // Flip card and check match
                    CheckCard();

                    // Flip card and check match
                    CheckCard();
                }
            }

            // Based on who has more matches, display a specific winner message
            if (MATCHES1 > MATCHES2)
            {
                // Display winner message
                Console.Clear();
                Console.WriteLine("Congratulations! Player 1 Won! Press ENTER to return to menu.");
                Console.ReadLine();

                // Increase player 1 wins
                PLAYER_1_WINS += 1;
            }
            else if (MATCHES2 > MATCHES1)
            {
                // Display winner message
                Console.Clear();
                Console.WriteLine("Congratulations! Player 2 Won! Press ENTER to return to menu.");
                Console.ReadLine();

                // Increase player 2 wins
                PLAYER_2_WINS += 1;
            }
            else if (MATCHES1 == MATCHES2)
            {
                // Display tied message
                Console.Clear();
                Console.WriteLine("Game was tied! Press ENTER to return to menu.");
                Console.ReadLine();

            }

            // Update total multiplayer games played
            TOTAL_MULTI_GAMES += 1;

            // Write to statistics file
            WriteStats();
        }

        //Pre: None
        //Post: None
        //Description: Display card deck
        private static void DisplayBoard()
        {
            // Clear screen and draw title
            Console.Clear();
            DrawTitle();

            // Write column headers
            Console.WriteLine("      A         B         C         D         E         F         G         H         I         J         K         L         M");

            // For the number of rows
            for (int row = 0; row < CARDS.GetLength(0); row++)
            {
                // For the number of columns
                for (int column = 0; column < CARDS.GetLength(1); column++)
                {
                    Console.Write("   ╔═════╗");
                }

                Console.WriteLine();

                // For the number of columns
                for (int column = 0; column < CARDS.GetLength(1); column++)
                {
                    // If card was matched
                    if (GAME_STATE[row, column] == 'm')
                    {
                        // Change colour and print rank
                        Console.Write("   ║");
                        Console.ForegroundColor = ConsoleColor.Magenta;
                        Console.Write(DetermineRank(row, column));
                        Console.ResetColor();
                        Console.Write("    ║");
                    }
                    else
                    {
                        // Print rank
                        Console.Write("   ║" + DetermineRank(row, column) + "    ║");
                    }
                }

                Console.WriteLine();

                // Display the row numbers
                Console.Write(row + 1);

                // For the number of columns
                for (int column = 0; column < CARDS.GetLength(1); column++)
                {
                    // If card was matched
                    if (GAME_STATE[row, column] == 'm')
                    {
                        // Change colour and print suit
                        Console.Write("  ║  ");
                        Console.ForegroundColor = ConsoleColor.Magenta;
                        Console.Write(DetermineSuit(row, column));
                        Console.ResetColor();
                        Console.Write("  ║ ");
                    }
                    else
                    {
                        // Print rank
                        Console.Write("  ║  " + DetermineSuit(row, column) + "  ║ ");
                    }
                }

                Console.WriteLine();

                // For the number of columns
                for (int column = 0; column < CARDS.GetLength(1); column++)
                {
                    // If card was matched
                    if (GAME_STATE[row, column] == 'm')
                    {
                        // Change colour and print rank
                        Console.Write("   ║    ");
                        Console.ForegroundColor = ConsoleColor.Magenta;
                        Console.Write(DetermineRank(row, column));
                        Console.ResetColor();
                        Console.Write("║");
                    }
                    else
                    {
                        // Print rank
                        Console.Write("   ║    " + DetermineRank(row, column) + "║");
                    }
                }

                Console.WriteLine();

                // For the number of columns
                for (int column = 0; column < CARDS.GetLength(1); column++)
                {
                    Console.Write("   ╚═════╝");
                }

                Console.WriteLine();
            }

            // Depending on if the mode is single or multi, display statistics
            if (MODE == "Single")
            {
                DisplayStatsSingle();
            }
            else if (MODE != "Single")
            {
                DisplayStatsMulti();
            }
        }

        //Pre: None
        //Post: None
        //Description: Create total card deck
        private static void CreateDeck()
        {
            // For the number of cards in a typical deck
            for (int i = 0; i < DECKNUM; i++)
            {
                // Add cards to array
                DECK.Add(i);
            }
        }

        //Pre: None
        //Post: None
        //Description: Shuffle card deck
        private static void ShuffleDeck()
        {
            // Create indexes for rng
            int idx1;
            int idx2;

            // Loop 1000 times
            for (int i = 0; i < 1000; i++)
            {
                // Generate two new positions
                idx1 = rng.Next(DECKNUM);
                idx2 = rng.Next(DECKNUM);

                // Switch card places (shuffling)
                (DECK[idx1], DECK[idx2]) = (DECK[idx2], DECK[idx1]);
            }
        }

        //Pre: None
        //Post: None
        //Description: Deal cards from list and store to array
        private static void DealCards()
        {
            // For the number of rows
            for (int i = 0; i < CARDS.GetLength(0); i++)
            {
                // For the number of columns
                for (int j = 0; j < CARDS.GetLength(1); j++)
                {
                    // Removes the top int from the List and places it in the 2D array
                    CARDS[i, j] = DECK[0];
                    DECK.RemoveAt(0);

                    // Unflip all cards
                    GAME_STATE[i, j] = 'u';
                }
            }
        }

        //Pre: None
        //Post: None
        //Description: Flip card based on player input
        private static void FlipCard(string playerInput)
        {
            int number;
            char letter;
            int letterTo = 0;

            // Get numerical value of first index of player input
            number = (int)Char.GetNumericValue(playerInput[0]);

            // Convert second index of player input to upper case
            letter = Char.ToUpper(playerInput[1]);

            // Based on alphabetical letter, convert to numerical value
            if (letter == 'A')
            {
                letterTo = 0;
            }
            else if (letter == 'B')
            {
                letterTo = 1;
            }
            else if (letter == 'C')
            {
                letterTo = 2;
            }
            else if (letter == 'D')
            {
                letterTo = 3;
            }
            else if (letter == 'E')
            {
                letterTo = 4;
            }
            else if (letter == 'F')
            {
                letterTo = 5;
            }
            else if (letter == 'G')
            {
                letterTo = 6;
            }
            else if (letter == 'H')
            {
                letterTo = 7;
            }
            else if (letter == 'I')
            {
                letterTo = 8;
            }
            else if (letter == 'J')
            {
                letterTo = 9;
            }
            else if (letter == 'K')
            {
                letterTo = 10;
            }
            else if (letter == 'L')
            {
                letterTo = 11;
            }
            else if (letter == 'M')
            {
                letterTo = 12;
            }

            // If current card is not matched
            if (GAME_STATE[number - 1, letterTo] != 'm')
            {
                // Flip card over 
                GAME_STATE[number - 1, letterTo] = 'f';
            }
        }

        //Pre: None
        //Post: None
        //Description: Determine suit based on coordinates
        private static string DetermineSuit(int X, int Y)
        {
            // Create empty suit string
            string suit = "";

            // If card is chosen, flipped or matched
            if (GAME_STATE[X, Y] == 'f' || GAME_STATE[X, Y] == 'm' || GAME_STATE[X, Y] == 'c')
            {
                // Based on card position, assign suit symbol
                if (CARDS[X, Y] >= 0 && CARDS[X, Y] <= 12)
                {
                    suit = "♥";
                }
                else if (CARDS[X, Y] >= 13 && CARDS[X, Y] <= 25)
                {
                    suit = "♠";
                }
                else if (CARDS[X, Y] >= 26 && CARDS[X, Y] <= 38)
                {
                    suit = "♦";
                }
                else if (CARDS[X, Y] >= 39 && CARDS[X, Y] <= 51)
                {
                    suit = "♣";
                }
            }
            else
            {
                suit = " ";
            }

            return suit;
        }

        //Pre: None
        //Post: None
        //Description: Determine rank based on coordinates
        private static char DetermineRank(int X, int Y)
        {
            // Empty char value holder
            char value = ' ';

            // If card is chosen, flipped or matched
            if (GAME_STATE[X, Y] == 'f' || GAME_STATE[X, Y] == 'm'
                || GAME_STATE[X, Y] == 'c')
            {
                // Based on modulus result, assign a card rank
                if (CARDS[X, Y] % 13 == 0)
                {
                    value = 'A';
                }
                else if (CARDS[X, Y] % 13 == 1)
                {
                    value = '2';
                }
                else if (CARDS[X, Y] % 13 == 2)
                {
                    value = '3';
                }
                else if (CARDS[X, Y] % 13 == 3)
                {
                    value = '4';
                }
                else if (CARDS[X, Y] % 13 == 4)
                {
                    value = '5';
                }
                else if (CARDS[X, Y] % 13 == 5)
                {
                    value = '6';
                }
                else if (CARDS[X, Y] % 13 == 6)
                {
                    value = '7';
                }
                else if (CARDS[X, Y] % 13 == 7)
                {
                    value = '8';
                }
                else if (CARDS[X, Y] % 13 == 8)
                {
                    value = '9';
                }
                else if (CARDS[X, Y] % 13 == 9)
                {
                    value = 'T';
                }
                else if (CARDS[X, Y] % 13 == 10)
                {
                    value = 'J';
                }
                else if (CARDS[X, Y] % 13 == 11)
                {
                    value = 'Q';
                }
                else if (CARDS[X, Y] % 13 == 12)
                {
                    value = 'K';
                }
            }
            else
            {
                value = ' ';
            }

            return value;
        }

        //Pre: None
        //Post: None
        //Description: Draw title used in menu and during gameplay
        private static void DrawTitle()
        {
            // Change foreground color 
            Console.ForegroundColor = ConsoleColor.Magenta;

            // Display title
            Console.WriteLine("                                                     _             _   _             ");
            Console.WriteLine("                       ___ ___  _ __   ___ ___ _ __ | |_ _ __ __ _| |_(_) ___  _ __");
            Console.WriteLine("                      / __/ _ \\| '_ \\ / __/ _ \\ '_ \\| __| '__/ _` | __| |/ _ \\| '_ \\ ");
            Console.WriteLine("                     | (_| (_) | | | | (_|  __/ | | | |_| | | (_| | |_| | (_) | | | |");
            Console.WriteLine("                      \\___\\___/|_| |_|\\___\\___|_| |_|\\__|_|  \\__,_|\\__|_|\\___/|_| |_|");
            Console.WriteLine("");
            Console.WriteLine("");

            // Reset to original colours
            Console.ResetColor();
        }

        //Pre: None
        //Post: None
        //Description: Display menu options
        private static void DisplayMenu()
        {
            // Clear screen
            Console.Clear();

            // Draw Concentration title
            DrawTitle();

            // Change foreground color
            Console.ForegroundColor = ConsoleColor.Blue;

            // Draw menu and options
            Console.WriteLine("\t\t\t\t\t\tMENU");
            Console.WriteLine("\t\t\t\t\t\t----");
            Console.WriteLine("\t\t\t\t\t1. Single Player Mode");
            Console.WriteLine("\t\t\t\t\t2. Two Player Mode");
            Console.WriteLine("\t\t\t\t\t3. View Statistics");
            Console.WriteLine("\t\t\t\t\t4. Exit");
            Console.WriteLine("");
            Console.Write("\t\t\t\t\t      Option: ");

            // Reset to original colours
            Console.ResetColor();
        }

        //Pre: None
        //Post: None
        //Description: Attempt to match cards, update all needed statistics
        private static void TryMatching()
        {
            if (PLAYER1)
            {
                PLAYER_TURN = "Player 1";
            }
            else if (!PLAYER1)
            {
                PLAYER_TURN = "Player 2";
            }

            // If first card being chosen
            if (STEP == 0)
            {
                // Increase step variable
                STEP += 1;

                // Nested loop (rows and columns) which finds flipped cards
                for (int i = 0; i < GAME_STATE.GetLength(0); i++)
                {
                    for (int j = 0; j < GAME_STATE.GetLength(1); j++)
                    {
                        // If card is flipped
                        if (GAME_STATE[i, j] == 'f')
                        {
                            // Change game state to "chosen"
                            GAME_STATE[i, j] = 'c';

                            // Determine rank of card 1
                            CARD1 = DetermineRank(i, j);

                            // Set X and Y coordinates of the chosen card
                            CARD1X = i;
                            CARD1Y = j;
                        }
                    }
                }
            }
            else if (STEP == 1)
            {
                // Nested loop (rows and columns) which finds flipped cards
                for (int i = 0; i < GAME_STATE.GetLength(0); i++)
                {
                    for (int j = 0; j < GAME_STATE.GetLength(1); j++)
                    {
                        // If card is flipped
                        if (GAME_STATE[i, j] == 'f')
                        {
                            // If cards are equal in rank
                            if (CheckMatch(CARD1, DetermineRank(i, j)) == true)
                            {
                                // Output success message, wait for user to enter
                                Console.Write("Matched! Press ENTER to continue");
                                Console.ReadLine();

                                // Change game state to matched
                                GAME_STATE[i, j] = 'm';
                                GAME_STATE[CARD1X, CARD1Y] = 'm';

                                // Depending on mode, update all needed stats
                                if (MODE == "Single")
                                {
                                    // Update statistics
                                    FLIPS1 += 1;
                                    MATCHES1 += 1;
                                }
                                else if (MODE == "Multi")
                                {
                                    // Increase statistics based on current player
                                    if (PLAYER1)
                                    {
                                        FLIPS1 += 1;
                                        MATCHES1 += 1;
                                        PLAYER1 = true;
                                    }
                                    else if (!PLAYER1)
                                    {
                                        FLIPS2 += 1;
                                        MATCHES2 += 1;
                                        PLAYER1 = false;
                                    }
                                }

                            }
                            else if (CheckMatch(CARD1, DetermineRank(i, j)) == false)
                            {
                                // Display message, wait for user to hit enter
                                Console.Write("No Match. Press ENTER to continue");
                                Console.ReadLine();

                                // Change card states to unflipped
                                GAME_STATE[i, j] = 'u';
                                GAME_STATE[CARD1X, CARD1Y] = 'u';

                                // Depending on mode, update all needed stats
                                if (MODE == "Single")
                                {
                                    FLIPS1 += 1;
                                }
                                else if (MODE == "Multi")
                                {
                                    // Increase statistics based on current player
                                    if (PLAYER1)
                                    {
                                        FLIPS1 += 1;
                                        PLAYER1 = false;
                                    }
                                    else if (!PLAYER1)
                                    {
                                        FLIPS2 += 1;
                                        PLAYER1 = true;
                                    }
                                }
                            }
                        }
                    }
                }

                // Reset step variable
                STEP = 0;
            }
        }

        //Pre: None
        //Post: None
        //Description: Compare the ranks of two cards to see whether they match
        private static bool CheckMatch(char card1, char card2)
        {
            // Determine whether the two cards match
            if (Char.Equals(card1, card2))
            {
                // Cards do match, return true
                return true;

            }
            else
            {
                // Cards do not match, return false
                return false;
            }
        }

        //Pre: None
        //Post: None
        //Description: Display statistics during game when in single player mode
        private static void DisplayStatsSingle()
        {
            // Display statistics
            Console.WriteLine("");
            Console.WriteLine("Full flips: " + FLIPS1);
            Console.WriteLine("Matches: " + MATCHES1);
            Console.WriteLine("");
        }

        //Pre: None
        //Post: None
        //Description: Display statistics during game when in multiplayer mode
        private static void DisplayStatsMulti()
        {
            // Display statistics of both players
            Console.WriteLine("");
            Console.WriteLine("Current Turn: " + PLAYER_TURN);
            Console.WriteLine("Player 1 Flips: " + FLIPS1 + "         Player 2 Flips: " + FLIPS2);
            Console.WriteLine("Player 1 Matches: " + MATCHES1 + "       Player 2 Matches: " + MATCHES2);
            Console.WriteLine("");
        }

        //Pre: None
        //Post: None
        //Description: Display all statistics
        private static void DrawStatsScreen()
        {
            // Clear console
            Console.Clear();

            // Display statistics 
            Console.WriteLine("STATISTICS");
            Console.WriteLine("~~~~~~~~~~");
            Console.WriteLine("");
            Console.WriteLine("Total Single Player Games Played: " + TOTAL_SINGLE_GAMES);
            Console.WriteLine("Total Single Player Full Flips: " + TOTAL_SINGLE_FLIPS);
            Console.WriteLine("Fastest Single Player Mode Win: " + FASTEST_WIN);
            Console.WriteLine("");
            Console.WriteLine("Total Multi Player Games Played: " + TOTAL_MULTI_GAMES);

            if (TOTAL_MULTI_GAMES > 0)
            {
                Console.WriteLine("Player 1 Win %: " + PLAYER_1_WINS/TOTAL_MULTI_GAMES);
                Console.WriteLine("Player 2 Win %: " + PLAYER_2_WINS/TOTAL_MULTI_GAMES);
            }
            else
            {
                Console.WriteLine("Player 1 Win %: 0");
                Console.WriteLine("Player 2 Win %: 0");
            }
            Console.WriteLine("Total Tied Wins: " + (TOTAL_MULTI_GAMES - PLAYER_1_WINS - PLAYER_2_WINS));

            Console.WriteLine(" ");

            // Wait for the user to press enter before returning to menu
            Console.Write("Press ENTER to return to MENU");
            Console.ReadLine();

        }

        //Pre: None
        //Post: None
        //Description: 1 card flipped and matching is attempted. 
        private static void CheckCard()
        {
            // Player input string
            string input = "";

            // Prompt user to enter a row and column and read in line
            Console.Write("Select a card by entering a row number and column letter (ex: 2F): ");
            input = Console.ReadLine();

            // If input length is equal to 2
            if (input.Length == 2)
            {
                // If row number is between 1 and 4 (inclusive)
                if (Convert.ToInt32(Convert.ToString(input[0])) > 0 && Convert.ToInt32(Convert.ToString(input[0])) < 5)
                {
                    // Convert to uppercase
                    Char.ToUpper(input[1]);

                    // Second index must be one of the letters included
                    if (input[1] == 'A' || input[1] == 'B' || input[1] == 'C'
                        || input[1] == 'D' || input[1] == 'E' || input[1] == 'F'
                        || input[1] == 'G' || input[1] == 'H' || input[1] == 'I'
                        || input[1] == 'J' || input[1] == 'K' || input[1] == 'L' || input[1] == 'M')
                    {
                        // Flip card chosen
                        FlipCard(input);

                        // Display board
                        DisplayBoard();

                        // Try matching cards
                        TryMatching();

                        // Display Board
                        DisplayBoard();
                    }
                    else
                    {
                        // Error message
                        Console.WriteLine("Input was not in the correct format. Try again.");
                    }
                }
                else
                {
                    // Error message
                    Console.WriteLine("Input was not in the correct format. Try again.");
                }
            }
            else
            {
                // Error message
                Console.WriteLine("Input was not in the correct format. Try again.");
            }
        }

        //Pre: None
        //Post: None
        //Description: Write statistics to text file
        private static void WriteStats()
        {
            try
            {
                // Create text file
                outFile = File.CreateText("stats.txt");

                // Write statistics to file
                outFile.WriteLine(TOTAL_SINGLE_GAMES);
                outFile.WriteLine(TOTAL_SINGLE_FLIPS);
                outFile.WriteLine(FASTEST_WIN);
                outFile.WriteLine(TOTAL_MULTI_GAMES);
                outFile.WriteLine(PLAYER_1_WINS);
                outFile.WriteLine(PLAYER_2_WINS);

                // Close file
                outFile.Close();
            }
            catch (FileNotFoundException fnf)
            {
                Console.WriteLine(fnf.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        //Pre: None
        //Post: None
        //Description: Read statistics from file
        private static void ReadStats()
        {
            try
            {
                // Open text file
                inFile = File.OpenText("stats.txt");

                TOTAL_SINGLE_GAMES = Convert.ToInt32(inFile.ReadLine());
                TOTAL_SINGLE_FLIPS = Convert.ToInt32(inFile.ReadLine());
                FASTEST_WIN = Convert.ToInt32(inFile.ReadLine());
                TOTAL_MULTI_GAMES = Convert.ToInt32(inFile.ReadLine());
                PLAYER_1_WINS = Convert.ToInt32(inFile.ReadLine());
                PLAYER_2_WINS = Convert.ToInt32(inFile.ReadLine());

                // Close file
                inFile.Close();
            }
            catch (FormatException fe)
            {
                Console.WriteLine(fe.Message);
            }
            catch (FileNotFoundException fnf)
            {
                Console.WriteLine(fnf.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}