using System;
using TwitchLib;
using TwitchLib.Client;
using TwitchLib.Client.Models;
using TwitchLib.Client.Events;
using TwitchLib.Client.Extensions;
using TwitchLib.Api;
using TwitchLib.Api.Models.Helix.Users.GetUsersFollows;
using TwitchLib.Api.Models.v5.Subscriptions;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Configuration;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Threading;
using System.Text.RegularExpressions;
using MySql.Data.MySqlClient;


namespace EleunameBotConsole
{
    class Program
    {
        #region Variables
        public static TwitchClient client;
        public static List<int> dailybonuses = new List<int>(new int[] { 250, 500, 100, 50, 75, 1, 2, 420, -1, -100, 5, -5, -50, 111, 42, 1337, 250, 500, 100, 50, 75, 1, 2, 420, -1, -100, 5, -5, -50, 111, 42, 250, 500, 100, 50, 75, 1, 2, 420, -1, -100, 5, -5, -50, 111, 42, 250, 500, 100, 50, 75, 1, 2, 420, -1, -100, 5, -5, -50, 111, 42, 1337 }); //array where dailybonus value is taken. Values are repeated to give a more accurate sense of randomness.
        public static ConnectionCredentials credentials = new ConnectionCredentials("id", "token"); //Credentials for the Bot login
        public static int subscriptionbonus = 2500;
        public static Random rnd = new Random();
        #endregion
        static void Main(string[] args)
        {
            client = new TwitchClient();
            client.Initialize(credentials, "channel");
            client.OnJoinedChannel += onJoinedChannel;
            client.OnMessageReceived += onMessageReceived;
            client.OnUserJoined += onUserJoined;
            client.OnGiftedSubscription += new EventHandler<OnGiftedSubscriptionArgs>(onGiftedSubscription);
            client.OnWhisperReceived += onWhisperReceived;
            client.OnChatCommandReceived += onCommandReceived;
            client.OnNewSubscriber += onNewSubscriber;
            client.OnReSubscriber += new EventHandler<OnReSubscriberArgs>(onReSubscription);
            client.Connect();
            Console.WriteLine("Successfully connected to chat");
            Console.ReadLine();
        }
        #region TwitchLib Functions


        private static void onJoinedChannel(object sender, OnJoinedChannelArgs e)
        {
            client.SendMessage(e.Channel, "Successfully connected, hello!");
        }
        private static void onMessageReceived(object sender, OnMessageReceivedArgs e)
        {
            Console.WriteLine($"{e.ChatMessage.Username}: {e.ChatMessage.Message}");
            if (e.ChatMessage.Message.Equals(Trivia.CurrentAnswer, StringComparison.CurrentCultureIgnoreCase))
            {
                if (e.ChatMessage.IsSubscriber) //Subs trivia points multiplier, if winner is a sub then he gets 2x points.
                {
                    Trivia.answerPoints = 500; 
                }
                else
                {
                    Trivia.answerPoints = 250;
                }
                Trivia.AnswerQuestion(e.ChatMessage.Username.ToLower());
            }
        }


        private static void onWhisperReceived(object sender, OnWhisperReceivedArgs e)
        {
        }

        private static void onNewSubscriber(object sender, OnNewSubscriberArgs e)
        {
            if (e.Subscriber.SubscriptionPlanName.Equals("prime", StringComparison.CurrentCultureIgnoreCase))
            {
                client.SendMessage(client.JoinedChannels[0], $"{e.Subscriber.DisplayName} has just subscribed using Twitch Prime! Thank you and welcome to the family! You have received 2500 points for subscribing.");
                DatabaseHandler.ExecuteNonQuery($"INSERT IGNORE INTO Users (username,points) values ('{e.Subscriber.DisplayName.ToLower()}', {subscriptionbonus}); UPDATE Users SET points = points + {subscriptionbonus} WHERE username='{e.Subscriber.DisplayName.ToLower()}'");
            }
            else
            {
                client.SendMessage(client.JoinedChannels[0], $"{e.Subscriber.DisplayName} has just subscribed! Thank you and welcome to the family! You have received 2500 points for subscribing.");
                DatabaseHandler.ExecuteNonQuery($"INSERT IGNORE INTO Users (username,points) values ('{e.Subscriber.DisplayName.ToLower()}', {subscriptionbonus}); UPDATE Users SET points = points + {subscriptionbonus} WHERE username='{e.Subscriber.DisplayName.ToLower()}'");
            }
        }

        private static void onReSubscription(object sender, OnReSubscriberArgs e)
        {
            int resubpoints = e.ReSubscriber.Months * subscriptionbonus;
            client.SendMessage(client.JoinedChannels[0], $"{e.ReSubscriber.Login} has resubscribed for {e.ReSubscriber.Months} months. You have received {resubpoints} points for re-subscribing. Included message: {e.ReSubscriber.ResubMessage}");
            DatabaseHandler.ExecuteNonQuery($"INSERT IGNORE INTO users (username,points) values ('{e.ReSubscriber.Login}', {resubpoints}); UPDATE users SET points = points + {resubpoints} WHERE username='{e.ReSubscriber.Login}'";);
        }
        private static void onGiftedSubscription(object sender, OnGiftedSubscriptionArgs e)
        {
            client.SendMessage(client.JoinedChannels[0], $"Wow! {e.GiftedSubscription.Login} has gifted a subscription to {e.GiftedSubscription.MsgParamRecipientUserName}! Thank you so much for your generosity! You have been awarded 5000 points for being so kind :)");
          
            DatabaseHandler.ExecuteNonQuery($"INSERT IGNORE INTO users (username,points) values ('{e.GiftedSubscription.Login}', {5000}); UPDATE users SET points = points + {5000} WHERE username='{e.GiftedSubscription.Login}'";);
        }
        private static void onUserJoined(object sender, OnUserJoinedArgs e)
        {

        }

        #endregion

        private static void onCommandReceived(object sender, OnChatCommandReceivedArgs e)
        {
            switch (e.Command.CommandText)
            {

                #region Points stuff
                case "givepoints":
                    if (e.Command.ChatMessage.IsBroadcaster)
                    {
                        if (e.Command.ArgumentsAsList.Count != 0)
                        {
                            int given = int.Parse(e.Command.ArgumentsAsList[1]);
                            if (given < 0)
                            {
                                client.SendMessage(client.JoinedChannels[0], $"{e.Command.ArgumentsAsList[1]} points have been taken from {e.Command.ArgumentsAsList[0]}");
                                DatabaseHandler.ExecuteNonQuery($"INSERT IGNORE INTO Users (username,points) values ('{e.Command.ArgumentsAsList[0].ToLower()}', {e.Command.ArgumentsAsList[1]}); UPDATE Users SET points = points + {e.Command.ArgumentsAsList[1]} WHERE username='{e.Command.ArgumentsAsList[0]}'");
                            }
                            else
                            {
                                client.SendMessage(client.JoinedChannels[0], $"{e.Command.ArgumentsAsList[1]} points have been awarded to {e.Command.ArgumentsAsList[0]}");
                                DatabaseHandler.ExecuteNonQuery($"INSERT IGNORE INTO Users (username,points) values ('{e.Command.ArgumentsAsList[0].ToLower()}', {e.Command.ArgumentsAsList[1]}); UPDATE Users SET points = points + {e.Command.ArgumentsAsList[1]} WHERE username='{e.Command.ArgumentsAsList[0]}'");
                            }
                        }
                    }
                    break;
                case "getpoints":
                    {
                        if (e.Command.ArgumentsAsList.Count != 0)
                        {
                            if (!e.Command.ArgumentsAsList[0].Contains(";"))
                            {
                                MySqlConnection cnn;
                                cnn = new MySqlConnection(DatabaseHandler.myConnectionString);
                                cnn.Open();
                                int points = 0;
                                points = int.Parse(DatabaseHandler.ScalarCommand($"SELECT points FROM Users WHERE username = '{e.Command.ArgumentsAsList[0].ToLower()}';"));
                                client.SendMessage(client.JoinedChannels[0], $"{e.Command.ArgumentsAsList[0].ToLower()} has " + points + " points.");
                                cnn.Close();
                            }
                        }
                        else
                        {
                            MySqlConnection cnn;
                            cnn = new MySqlConnection(DatabaseHandler.myConnectionString);
                            cnn.Open();
                            int points = 0;
                            points = int.Parse(DatabaseHandler.ScalarCommand($"SELECT points FROM Users WHERE username = '{e.Command.ChatMessage.Username.ToLower()}';"));
                            client.SendMessage(client.JoinedChannels[0], $"{e.Command.ChatMessage.Username.ToLower()} has " + points + " points.");
                            cnn.Close();
                        }
                    }
                    break;

                case "dailybonus":
                    int r = rnd.Next(dailybonuses.Count); //gets random value from the dailybonuses array
                    int dailybonus = dailybonuses[r];
                    int record = 0;
                    string sql = $"SELECT timestamp FROM Users WHERE username = '{e.Command.ChatMessage.Username.ToLower()}';";
                    record = int.Parse(DatabaseHandler.ScalarCommand(sql));
                    if (DateTimeOffset.UtcNow.ToUnixTimeSeconds() - record >= 86400) //checks if the user has taken a dailybonus in the last 24 hours
                    {
                        if (dailybonus < 0)
                        {
                            client.SendMessage(client.JoinedChannels[0], $"Oof. Lady Luck decided that today is not a lucky day for you! You lost {dailybonus} points! Better luck tomorrow :c");
                        }
                        else
                        {
                            client.SendMessage(client.JoinedChannels[0], $"{e.Command.ChatMessage.Username.ToLower()} - Good news! The Lady Luck decided that your bonus for today is {dailybonus} points!");

                        }
                        string text = $"UPDATE Users SET timestamp = {DateTimeOffset.UtcNow.ToUnixTimeSeconds()} WHERE username='{e.Command.ChatMessage.Username.ToLower()}'; UPDATE Users SET points = points + {dailybonus} WHERE username='{e.Command.ChatMessage.Username.ToLower()}';";
                        DatabaseHandler.ExecuteNonQuery(text);
                    }
                    break;
                #endregion

                #region Text command (these commands are relative to your own stream)
                case "discord":
                    client.SendMessage(client.JoinedChannels[0], "Join Emanuele's cool Discord here!");
                    break;
                case "help":
                    client.SendMessage(client.JoinedChannels[0], "You can find this bot's command list at");
                    break;
                case "ping":
                    if (e.Command.ChatMessage.IsBroadcaster)
                    {
                        client.SendMessage(client.JoinedChannels[0], "Pong");
                    }
                    break;
                case "emote":
                    client.SendMessage(client.JoinedChannels[0], "eleunaPout");
                    break;
                #endregion
                #region Song related commands
                case "playlist":
                    client.SendMessage(client.JoinedChannels[0], "Currently using a custom playlist, please use !song to know the title of the currently playing song!"); //!song is a command available on the desktop-side of this bot, because it reads spotify metadata.
                    break;
                #endregion

                #region Points rewards
                case "songrequest":
                    if (e.Command.ArgumentsAsList.Count == 1)
                    {
                        int points = 0; 
                        points = int.Parse(DatabaseHandler.ScalarCommand($"SELECT points FROM Users WHERE username = '{e.Command.ChatMessage.Username.ToLower()}';"));
                        if (points < 10000)
                        {
                            client.SendMessage(client.JoinedChannels[0], $"{e.Command.ChatMessage.Username} - Sorry but you need 10000 points and you currently have {points} points");
                        }
                        else
                        {
                            if (e.Command.ArgumentsAsList[0].StartsWith("https://www.youtube.com")) //if the url starts with youtube link, then it gets the title through the YoutubeParse class, else just displays the link.
                                {
                                client.SendMessage(client.JoinedChannels[0], $"{e.Command.ChatMessage.Username} has requested {YoutubeParse.GetTitle(e.Command.ArgumentsAsList[0])}");
                            }
                            else
                            {
                                client.SendMessage(client.JoinedChannels[0], $"{e.Command.ChatMessage.Username} has requested {e.Command.ArgumentsAsList[0]}");

                            }

                            string text = $"UPDATE Users SET points = points - 10000 WHERE username='{e.Command.ChatMessage.Username.ToLower()}';";
                            DatabaseHandler.ExecuteNonQuery(text);
                        }


                    }
                    if (e.Command.ArgumentsAsList.Count == 0)
                    {
                        client.SendMessage(client.JoinedChannels[0], $"{e.Command.ChatMessage.Username} - You have to send me a Youtube link to request a song!");
                    }
                    break;
                #endregion
                #region Interactive commands
                case "kms":
                    client.SendMessage(client.JoinedChannels[0], $"{e.Command.ChatMessage.Username} has killed himself :c");
                    break;
                case "punch":
                    if (e.Command.ArgumentsAsList.Count == 0)
                    {
                        client.SendMessage(client.JoinedChannels[0], "You gotta target someone first, dummy!");
                    }
                    else

                    {
                        client.SendMessage(client.JoinedChannels[0], $"Woah, {e.Command.ChatMessage.Username} has punched {e.Command.ArgumentsAsList[0]} in the face");
                    }
                    break;
                case "hug":
                    if (e.Command.ArgumentsAsList.Count == 0)
                    {
                        client.SendMessage(client.JoinedChannels[0], "You gotta target someone first, dummy!");
                    }
                    else

                    {
                        client.SendMessage(client.JoinedChannels[0], $"Awwww, {e.Command.ChatMessage.Username} just gave {e.Command.ArgumentsAsList[0]} a really warm hug!");
                    }
                    break;
                #endregion
                #region Broadcaster stuff
                    case "triviastart":
                    if(e.Command.ChatMessage.IsBroadcaster)
                    {
                        Trivia.Load();
                        Trivia.Start();
                        Console.WriteLine("Loaded " + Trivia.TotalQuestions + " questions.");
                        client.SendMessage(client.JoinedChannels[0], "A new trivia session has been started!");
                    }
                    break;
                case "triviastop":
                    if (e.Command.ChatMessage.IsBroadcaster)
                    {
                        Trivia.Running = false;
                        Trivia.Timer.Stop();
                        client.SendMessage(client.JoinedChannels[0], "Trivia has ended for today! Thanks for playing and see you next stream!");
                        Console.WriteLine("Trivia session has been successfully closed");
                    }
                    break;
                case "subtest":
                    if (e.Command.ChatMessage.IsBroadcaster)
                    {
                        client.InvokeNewSubscriber(new System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<string, string>>(), "", System.Drawing.Color.AliceBlue, e.Command.ArgumentsAsList[0], "", "", "", "", "", "", TwitchLib.Client.Enums.SubscriptionPlan.NotSet, "", "", "", false, false, false, false, "", TwitchLib.Client.Enums.UserType.Admin, "", "eleuname");
                    }
                    break;
                    #endregion


            }
        }
    }
}
