using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Zork
{
    public class Game
    {
        [JsonProperty]
        private string WelcomeMessage = null;

        [JsonIgnore]
        public static Game Instance { get; private set; }
        public World World { get; set; }
        
        [JsonIgnore]
        public Player Player { get; private set; }

        [JsonIgnore]
        private bool IsRunning { get; set; }

        [JsonIgnore]
        public CommandManager CommandManager { get; }
        public Game(World world, Player player)
        {
            World = world;
            Player = player;
        }

        public Game() => CommandManager = new CommandManager();
        

        private static readonly string ScriptDirectory = "Scripts";
        private static readonly string ScriptFileExtension = "*.csx";

        private bool mIsRunning;
        private bool mIsRestarting;

        private static readonly Random Random = new Random();
        private void DisplayWelcomeMessage() => Console.WriteLine(WelcomeMessage);
        public static void Start(string gameFileName)
        {
            if (!File.Exists(gameFileName))
            {
                throw new FileNotFoundException("Expected file.", gameFileName);
            }

            while (Instance == null || Instance.mIsRestarting)
            {
                Instance = Load(gameFileName);
                Instance.LoadCommands();
                Instance.LoadScripts();
                Instance.DisplayWelcomeMessage();
                Instance.Run();
            }
        }
        public void Run()
        {
            mIsRunning = true;
            Room previousRoom = null;
            while (mIsRunning)
            {
                if (previousRoom != Player.Location)
                {
                    Console.WriteLine(Player.Location);
                    CommandManager.PerformCommand(this, "LOOK");
                    previousRoom = Player.Location;
                }

                Console.Write("\n> ");
                if (CommandManager.PerformCommand(this, Console.ReadLine().Trim()))
                {
                    
                }
                else
                {
                    Console.WriteLine("That's not a verb I recognize.");
                }
            }
        }

        public void Restart()
        {
            mIsRunning = false;
            mIsRestarting = true;
            Console.Clear();
        }

        public void Quit()
        {
            mIsRunning = false;
        }
        public static Game Load(string fileName)
        {
            Game game = JsonConvert.DeserializeObject<Game>(File.ReadAllText(fileName));
            game.Player = game.World.SpawnPlayer();

            return game;
        }

        private void LoadCommands()
        {
            Type[] types = Assembly.GetExecutingAssembly().GetTypes();
            
            foreach (Type type in types)
            {
                CommandClassAttributes commandClassAttributes = type.GetCustomAttribute<CommandClassAttributes>();
                if (commandClassAttributes != null)
                {
                    MethodInfo[] methods = type.GetMethods();
                    foreach (MethodInfo method in methods)
                    {
                        CommandAttribute commandAttribute = method.GetCustomAttribute<CommandAttribute>();
                        if (commandAttribute != null)
                        {
                            Command command = new Command(commandAttribute.CommandName, commandAttribute.Verbs,
                                (Action<Game, CommandContext>)Delegate.CreateDelegate(typeof(Action<Game, CommandContext>), method));
                            CommandManager.AddCommand(command);
                        }
                    }
                }
            }
        }

        private void LoadScripts()
        {
            foreach (string file in Directory.EnumerateFiles(ScriptDirectory, ScriptFileExtension))
            {
                try
                {
                    var scriptOptions = ScriptOptions.Default.AddReferences(Assembly.GetExecutingAssembly());

                    scriptOptions = scriptOptions.WithEmitDebugInformation(true).WithFilePath(new FileInfo(file).FullName).WithFileEncoding(Encoding.UTF8);

                    string script = File.ReadAllText(file);
                    CSharpScript.RunAsync(script, scriptOptions).Wait();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error compiling script: {file} Error: {ex.Message}");
                }
            }
        }

        public bool ConfirmAction(string prompt)
        {
            Console.Write(prompt);

            while (true)
            {
                string response = Console.ReadLine().Trim().ToUpper();
                if (response == "YES" || response == "Y")
                {
                    return true;
                }
                else if (response == "NO" || response == "N")
                {
                    return false;
                }
                else
                {
                    Console.Write("Yes or no only!");
                }
            }
        }
    }
    
    
}
