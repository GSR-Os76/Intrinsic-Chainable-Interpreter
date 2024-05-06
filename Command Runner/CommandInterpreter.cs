﻿using System.Text.RegularExpressions;

namespace GSR.CommandRunner
{
    public class CommandInterpreter : ICommandInterpreter
    {
        private const string FUNCTION_ASSIGN_TYPE = "fa";
        private const string ASSIGN_TYPE = "a";
        private const string STRING_LITERAL_TYPE = "sl";

        private const string META_COMMAND_START_REGEX = @"^~\s*.\s*";
        private const string MEMBER_NAME_REGEX = @"^(_a-zA-Z)+(_0-9a-zA-z)*";
        private const string UNTIL_END_QUOTE = @"^([^\\""]|\\""|\\\\)*(?="")";

        private static readonly ICommandSet s_metaCommands = new CommandSet(typeof(CommandInterpreter));
        private readonly ICommandSet m_commands;
        private readonly  ISessionContext m_sessionContext;
        private int m_uniqueNumber = 0;



        public CommandInterpreter(ICommandSet defaultCommands) : this(defaultCommands, new SessionContext()) { } // end constructor

        public CommandInterpreter(ICommandSet defaultCommands, ISessionContext sessionContext) 
        {
            m_commands = defaultCommands;
            m_sessionContext = sessionContext;
        } // end constructor



        public ICommand Evaluate(string input) 
        {
            string parse = input.Trim();
            if (parse[0].Equals('$')) 
            {
                parse = parse[1..];
                string varName = Regex.Match(parse, MEMBER_NAME_REGEX).Value;
                parse = parse.Replace(MEMBER_NAME_REGEX, "").TrimStart();

                if (parse[..2].Equals("=>"))
                {
                    parse = parse[2..].TrimStart();
                    ICommand val = ReadCommand(parse);
                    return CommandFor(FUNCTION_ASSIGN_TYPE, () => m_sessionContext.SetValue(varName, val)); 
                }
                else if (parse[0].Equals('='))
                {
                    parse = parse[1..].TrimStart();
                    ICommand val = ReadCommand(parse);
                    return CommandFor(ASSIGN_TYPE, () => m_sessionContext.SetValue(varName, val.Execute(Array.Empty<object>())));
                }
                parse = $"${varName}{parse}";
            }
            
            return ReadCommand(parse);
        } // end Evaluate()


        private ICommand ReadCommand(string input) 
        {
            string parse = input.Trim();
            // if 0-9 numeric
            // if " string
            if (parse[0].Equals('"'))
            {
                parse = parse[1..];
                string value = Regex.Match(parse, UNTIL_END_QUOTE).Value.Replace("\\\"", "\"").Replace("\\\\", "\\");
                parse = Regex.Replace(parse, UNTIL_END_QUOTE, "")[1..].TrimStart();
                if (parse.Equals(""))
                    return CommandFor(STRING_LITERAL_TYPE, typeof(string), () => value);
                else if (parse[0].Equals('.'))
                {
# warning, begin method chain.
                }
                else
                    throw new InvalidOperationException($"Couldn't interpret value: \"{input}\"");
            }
            else if (parse[0].Equals('$'))
            {
                parse = parse[1..];
                string varName = Regex.Match(parse, MEMBER_NAME_REGEX).Value;
                parse = parse.Replace(MEMBER_NAME_REGEX, "").TrimStart();

                object? val = m_sessionContext.GetValue(varName, typeof(object));
                // try return value, or if invoked holding command execute command.

            }

            /*else if (Regex.IsMatch(parse, META_COMMAND_START_REGEX))
                {
                    parse = Regex.Replace(input, META_COMMAND_START_REGEX, "");
                    return ReadMetaCommand(parse);
                    string cName = Regex.Match(input, MEMBER_NAME_REGEX).Groups[0].Value;

                }*/
            return null;
        } // end ReadCommand()



        private ICommand CommandFor(string type, Action value) => new Command($"{type}_{++m_uniqueNumber}", typeof(void), Array.Empty<Type>(), (x) => { value(); return null; });

        private ICommand CommandFor(string type, Type returnType, Func<object?> value) => new Command($"{type}_{++m_uniqueNumber}", returnType, Array.Empty<Type>(), (x) => value());

    } // end class
} // end namespace