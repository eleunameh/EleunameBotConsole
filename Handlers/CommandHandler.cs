using System;
using System.Collections.Generic;
using System.Text;
using TwitchLib;
using TwitchLib.Client;
using TwitchLib.Client.Events;

namespace EleunameBotConsole.Handlers
{
    class CommandHandler
    {
        Dictionary<string, Action<OnChatCommandReceivedArgs>> registeredCommands = new Dictionary<string, Action<OnChatCommandReceivedArgs>>();

        public void RegisterCommand(string textToTriggerCommand, Action<OnChatCommandReceivedArgs> callback)
        {
            if (registeredCommands.ContainsKey(textToTriggerCommand))
            {
                Console.WriteLine("ERROR: The command you were trying to add was already registered!");
            }   
            else
            {
                //Register the command by adding it to the RegisteredCommands dictionary
                registeredCommands.Add(textToTriggerCommand, callback);
            }
        }

        public void OnCommandReceived(OnChatCommandReceivedArgs args)
        {
            if (registeredCommands.ContainsKey(args.Command.CommandText))
            {
                Action<OnChatCommandReceivedArgs> callback = registeredCommands[args.Command.CommandText]; //get the callback for the command that was registered under this command
                callback(args); //call the callback
            }
        }
    }
}

