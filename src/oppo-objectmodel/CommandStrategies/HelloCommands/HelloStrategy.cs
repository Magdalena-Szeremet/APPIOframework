﻿using System.Collections.Generic;

namespace Oppo.ObjectModel.CommandStrategies.HelloCommands
{
    public class HelloStrategy : ICommand<ObjectModel>
    {
        public string Name => Constants.CommandName.Hello;

        public CommandResult Execute(IEnumerable<string> inputParams)
        {
            var outputMessages = new List<KeyValuePair<string, string>>();
            outputMessages.Add(new KeyValuePair<string, string>(Constants.HelloString, string.Empty));            
            return new CommandResult(true, outputMessages);
        }

        public string GetHelpText()
        {
            return Resources.text.help.HelpTextValues.HelloCommand;
        }
    }
}