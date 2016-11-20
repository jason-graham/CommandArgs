//---------------------------------------------------------------------------- 
//
//  Copyright (C) CSharp Labs.  All rights reserved.
// 
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
// 
//  The above copyright notice and this permission notice shall be included in
//  all copies or substantial portions of the Software.
// 
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//  THE SOFTWARE.
// 
// History
//  05/13/13    Created 
//
//---------------------------------------------------------------------------

namespace System.Diagnostics
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;

    /// <summary>
    /// The CommandArgs class parses and formats command line arguments.
    /// </summary>
    public sealed class CommandArgs
    {
        #region Constant Fields
        private const string Prefix = "Prefix";
        private const string Key = "Key";
        private const string Value = "Value";
        private const string RegexString = "(?<" + Prefix + ">[-/]{1}){0,1}(?([\\w\\d]+[:=]{1})(?<" + Key + ">[\\w\\d]+)[:=]{1}(?(\"{1})\"{1}(?<" + Value + ">[^\"]+)\"{1}|(?<" + Value + ">[^\"\\s]+))|(?(\"{1})\"{1}(?<" + Value + ">[^\"]+)\"{1}|(?<" + Value + ">[^\"\\s]+)))";
        #endregion

        #region Static Fields
        /// <summary>
        /// Lazy initialization of current command arguments.
        /// </summary>
        private static Lazy<CommandArgs> current = new Lazy<CommandArgs>(() => new CommandArgs(Environment.CommandLine));
        #endregion

        #region Current Command Arguments
        /// <summary>
        /// Gets an instance of the CommandArgs with the current command line.
        /// </summary>
        public static CommandArgs Current
        {
            get
            {
                return current.Value;
            }
        }
        #endregion

        #region Fields
        /// <summary>
        /// Defines the comparison operation to use while parsing.
        /// </summary>
        public readonly StringComparer Comparer = StringComparer.OrdinalIgnoreCase;
        /// <summary>
        /// Collection of command line values.
        /// </summary>
        private string[] values;
        /// <summary>
        /// Collection of command line switches.
        /// </summary>
        private string[] switches;
        /// <summary>
        /// Collection of command line flags.
        /// </summary>
        private string[] flags;
        /// <summary>
        /// Collection of command line switch value pairs.
        /// </summary>
        private CommandArgumentKeyPair[] switchPairs;
        /// <summary>
        /// Collection of command line flag value pairs.
        /// </summary>
        private CommandArgumentKeyPair[] flagPairs;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes the CommandArgs class and populates the collections with the specified command line arguments.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public CommandArgs(string args)
        {
            Process(args);
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets a collection of values which had no prefix.
        /// </summary>
        public ReadOnlyCollection<string> Values
        {
            get
            {
                return new ReadOnlyCollection<string>(values);
            }
        }
        /// <summary>
        /// Gets a collection of switches.
        /// </summary>
        public ReadOnlyCollection<string> Switches
        {
            get
            {
                return new ReadOnlyCollection<string>(switches);
            }
        }
        /// <summary>
        /// Gets a collection of flags.
        /// </summary>
        public ReadOnlyCollection<string> Flags
        {
            get
            {
                return new ReadOnlyCollection<string>(flags);
            }
        }
        /// <summary>
        /// Gets a collection of flag value pairs.
        /// </summary>
        public ReadOnlyCollection<CommandArgumentKeyPair> FlagPairs
        {
            get
            {
                return new ReadOnlyCollection<CommandArgumentKeyPair>(flagPairs);
            }
        }
        /// <summary>
        /// Gets a collection of switch value pairs.
        /// </summary>
        public ReadOnlyCollection<CommandArgumentKeyPair> SwitchPairs
        {
            get
            {
                return new ReadOnlyCollection<CommandArgumentKeyPair>(switchPairs);
            }
        }

        /// <summary>
        /// Gets the command line arguments.
        /// </summary>
        public string CommandLine { get; private set; }
        #endregion

        #region Methods
        /// <summary>
        /// Populates the collections with the specified command line arguments.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public void Process(string args)
        {
            args = args ?? string.Empty;

            HashSet<string> values = new HashSet<string>(Comparer);
            HashSet<string> switches = new HashSet<string>(Comparer);
            HashSet<string> flags = new HashSet<string>(Comparer);
            Dictionary<string, CommandArgumentKeyPair> switchPairs = new Dictionary<string, CommandArgumentKeyPair>(Comparer);
            Dictionary<string, CommandArgumentKeyPair> flagPairs = new Dictionary<string, CommandArgumentKeyPair>(Comparer);

            CommandLine = args;

            if (args != string.Empty)
            {
                Regex reg = new Regex(RegexString);

                if (reg.IsMatch(args))
                {
                    foreach (Match m in reg.Matches(args))
                    {
                        switch (m.Groups[Prefix].Value)
                        {
                            case "-":
                                if (m.Groups[Key].Value == string.Empty)
                                {
                                    flags.Add(m.Groups[Value].Value);
                                }
                                else
                                {
                                    string key = m.Groups[Key].Value;
                                    if (!flagPairs.ContainsKey(key))
                                        flagPairs.Add(key, new CommandArgumentKeyPair(key, m.Groups[Value].Value));
                                }
                                break;
                            case "/":
                                if (m.Groups[Key].Value == string.Empty)
                                {
                                    switches.Add(m.Groups[Value].Value);
                                }
                                else
                                {
                                    string key = m.Groups[Key].Value;
                                    if (!switchPairs.ContainsKey(key))
                                        switchPairs.Add(key, new CommandArgumentKeyPair(key, m.Groups[Value].Value));
                                }
                                break;
                            case "":
                                string value = m.Groups[Value].Value;

                                if (!Paths.FilePathsEqual(value, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, AppDomain.CurrentDomain.FriendlyName))) //ignores application and VS debugging process
                                    values.Add(value);
                                break;
                        }
                    }
                }
            }

            this.values = values.ToArray();
            this.switches = switches.ToArray();
            this.flags = flags.ToArray();
            this.switchPairs = switchPairs.Values.ToArray();
            this.flagPairs = flagPairs.Values.ToArray();
        }
        #endregion
    }
}
