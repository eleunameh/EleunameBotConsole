using System;
using System.Collections.Generic;
using System.Text;
using TwitchLib;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using MySql.Data.MySqlClient;
using System.IO;
using System.Linq;
using TwitchLib.Client.Extensions;

namespace EleunameBotConsole.Features
{
    public class Commands
    {
        public static void testCommandHandler(OnChatCommandReceivedArgs e)
        {
            Console.WriteLine("SYSTEM: test command was called!");
            Program.client.SendMessage(Program.client.JoinedChannels[0], "TestCommand was called!");
        }
        #region Interactive commands
        public static void punchCommand(OnChatCommandReceivedArgs e)
        {
            Console.WriteLine("SYSTEM: kms command was called!");
            if (e.Command.ArgumentsAsList.Count == 0)
            {
                Program.client.SendMessage(Program.client.JoinedChannels[0], "You gotta target someone first, dummy!");
            }
            else

            {
                Program.client.SendMessage(Program.client.JoinedChannels[0], $"Woah, {e.Command.ChatMessage.Username} has punched {e.Command.ArgumentsAsList[0]} in the face");
            }
        }
        public static void hugCommand(OnChatCommandReceivedArgs e)
        {
            Console.WriteLine("SYSTEM: hug command was called!");
            if (e.Command.ArgumentsAsList.Count == 0)
            {
                Program.client.SendMessage(Program.client.JoinedChannels[0], "You gotta target someone first, dummy!");
            }
            else

            {
                Program.client.SendMessage(Program.client.JoinedChannels[0], $"Awwww, {e.Command.ChatMessage.Username} just gave {e.Command.ArgumentsAsList[0]} a really warm hug!");
            }
        }
        #endregion
        #region Points commands
        public static void dailybonusCommand(OnChatCommandReceivedArgs e)
        {
            int r = Program.rnd.Next(Program.dailybonuses.Length);
            int dailybonus = Program.dailybonuses[r];
            int record = 0;
            string sql = $"SELECT timestamp FROM Users WHERE username = '{e.Command.ChatMessage.Username.ToLower()}';";
            record = int.Parse(DatabaseHandler.ScalarCommand(sql));
            if (DateTimeOffset.UtcNow.ToUnixTimeSeconds() - record >= 86400)
            {
                if (dailybonus < 0)
                {
                    Program.client.SendMessage(Program.client.JoinedChannels[0], $"Oof. Lady Luck decided that today is not a lucky day for you! You lost {dailybonus} points! Better luck tomorrow :c");
                }
                else
                {
                    Program.client.SendMessage(Program.client.JoinedChannels[0], $"{e.Command.ChatMessage.Username.ToLower()} - Good news! The Lady Luck decided that your bonus for today is {dailybonus} points!");

                }
                string text = $"UPDATE Users SET timestamp = {DateTimeOffset.UtcNow.ToUnixTimeSeconds()} WHERE username='{e.Command.ChatMessage.Username.ToLower()}'; UPDATE Users SET points = points + {dailybonus} WHERE username='{e.Command.ChatMessage.Username.ToLower()}';";
                DatabaseHandler.ExecuteNonQuery(text);
            }
        }
        public static void givepointsCommand(OnChatCommandReceivedArgs e)
        {
            bool check = Array.Exists(Program.highmods, element => element == e.Command.ChatMessage.Username);
            if (e.Command.ChatMessage.IsBroadcaster && e.Command.ArgumentsAsList.Count > 1 || check == true && e.Command.ArgumentsAsList.Count > 1 )
                    {
                    int given;
                    if (int.TryParse(e.Command.ArgumentsAsList[1], out given))
                    {
                        if (given < 0)
                        {
                        Console.WriteLine("COMMANDS: Points have been taken, is it okay?");
                            Program.client.SendMessage(Program.client.JoinedChannels[0], $"{e.Command.ArgumentsAsList[1]} points have been taken from {e.Command.ArgumentsAsList[0]}");
                            DatabaseHandler.ExecuteNonQuery($"INSERT IGNORE INTO Users (username,points) values ('{e.Command.ArgumentsAsList[0].ToLower()}', {e.Command.ArgumentsAsList[1]}); UPDATE Users SET points = points + {e.Command.ArgumentsAsList[1]} WHERE username='{e.Command.ArgumentsAsList[0]}'");
                        }
                        else
                        {
                        Console.WriteLine("COMMANDS: Points have been given, is it okay?");
                        Program.client.SendMessage(Program.client.JoinedChannels[0], $"{e.Command.ArgumentsAsList[1]} points have been awarded to {e.Command.ArgumentsAsList[0]}");
                        DatabaseHandler.ExecuteNonQuery($"INSERT IGNORE INTO Users (username,points) values ('{e.Command.ArgumentsAsList[0].ToLower()}', {e.Command.ArgumentsAsList[1]}); UPDATE Users SET points = points + {e.Command.ArgumentsAsList[1]} WHERE username='{e.Command.ArgumentsAsList[0]}'");
                    }
                    }
                    else
                    {
                        Console.WriteLine("ERROR: You must provide a number as parameter, noob.");
                    }
            }
        }
        public static void getpointsCommand(OnChatCommandReceivedArgs e)
        {
            if (e.Command.ArgumentsAsList.Count != 0)
            {
                if (!e.Command.ArgumentsAsList[0].Contains(";"))
                {
                    int points = 0;
                    points = int.Parse(DatabaseHandler.ScalarCommand($"SELECT points FROM Users WHERE username = '{MySqlHelper.EscapeString(e.Command.ArgumentsAsList[0].ToLower())}';"));
                    Program.client.SendMessage(Program.client.JoinedChannels[0], $"{e.Command.ArgumentsAsList[0].ToLower()} has {points} points.");
               }
                else
                {
                    int points = 0;
                    points = int.Parse(DatabaseHandler.ScalarCommand($"SELECT points FROM Users WHERE username = '{e.Command.ChatMessage.Username.ToLower()}';"));
                    Program.client.SendMessage(Program.client.JoinedChannels[0], $"{e.Command.ChatMessage.Username.ToLower()} has " + points + " points.");
                }
            }
        }
        #endregion
        #region Text commands
        public static void discordCommand(OnChatCommandReceivedArgs e)
        {
            Console.WriteLine("SYSTEM: discord command was called!");   
            Program.client.SendMessage(Program.client.JoinedChannels[0], "Join Emanuele's cool Discord community filled with friendly shibas here: https://discord.gg/rcyfK8Y");
        }
        public static void helpCommand(OnChatCommandReceivedArgs e)
        {
            Console.WriteLine("SYSTEM: help command was called!");
            Program.client.SendMessage(Program.client.JoinedChannels[0], "You can find this bot's command list at https://uploads.eleuna.me/commands.txt");
        }
        public static void pingCommand(OnChatCommandReceivedArgs e)
        {
            Console.WriteLine("SYSTEM: ping command was called!");
            Program.client.SendMessage(Program.client.JoinedChannels[0], "Pong!");
        }
        #endregion
        public static void songrequestCommand(OnChatCommandReceivedArgs e)
        {
            Console.WriteLine("SYSTEM: A song has been requested.");
            if (e.Command.ArgumentsAsList.Count == 1)
            {
                int points = 0;
                points = int.Parse(DatabaseHandler.ScalarCommand($"SELECT points FROM Users WHERE username = '{e.Command.ChatMessage.Username.ToLower()}';"));
                if (points < 10000)
                {
                    Program.client.SendMessage(Program.client.JoinedChannels[0], $"{e.Command.ChatMessage.Username} - Sorry but you need 10000 points and you currently have {points} points");
                }
                else
                {
                    if (e.Command.ArgumentsAsList[0].StartsWith("https://www.youtube.com"))
                    {
                        Program.client.SendMessage(Program.client.JoinedChannels[0], $"{e.Command.ChatMessage.Username} has requested {YoutubeParse.GetTitle(e.Command.ArgumentsAsList[0])}");
                        File.AppendAllText("songrequests.txt", $"{e.Command.ArgumentsAsList[0]} has been requested by {e.Command.ChatMessage.Username}" + Environment.NewLine);
                    }
                    else
                    {
                        Program.client.SendMessage(Program.client.JoinedChannels[0], $"{e.Command.ChatMessage.Username} has requested {e.Command.ArgumentsAsList[0]}");
                        File.AppendAllText("songrequests.txt", $"{e.Command.ArgumentsAsList[0]} has been requested by {e.Command.ChatMessage.Username}" + Environment.NewLine);
                    }

                    string text = $"UPDATE Users SET points = points - 10000 WHERE username='{e.Command.ChatMessage.Username.ToLower()}';";
                    DatabaseHandler.ExecuteNonQuery(text);
                }
            }
        }
        public static void addhighmodCommand(OnChatCommandReceivedArgs e)
        {
            if(e.Command.ChatMessage.IsBroadcaster)
            {
                Console.WriteLine($"SYSTEM: {e.Command.ArgumentsAsList[0]} is being added as new high mod!");
                File.AppendAllText("highmods.txt", $",{e.Command.ArgumentsAsList[0]}");
                Console.WriteLine("SYSTEM: Refreshing high mods list...");
                Program.highmods = Program.GetHighMods("highmods.txt");
                Console.WriteLine($"SYSTEM: High mods list has successfully been refreshed and now has {Program.highmods.Length} members.");
                Program.client.SendMessage(Program.client.JoinedChannels[0],$"{e.Command.ArgumentsAsList[0]}has successfully been added as a high mod!");
            }
        }
        public static void subtestCommand(OnChatCommandReceivedArgs e )
        {
            if (e.Command.ChatMessage.IsBroadcaster)
            {
                Console.WriteLine("SYSTEM: subtest command was called!");
                Program.client.InvokeNewSubscriber(new System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<string, string>>(), "", System.Drawing.Color.AliceBlue, e.Command.ArgumentsAsList[0], "", "", "", "", "", "", TwitchLib.Client.Enums.SubscriptionPlan.NotSet, "", "", "", false, false, false, false, "", TwitchLib.Client.Enums.UserType.Admin, "", "eleuname");
            }
           
        }

    }
}

