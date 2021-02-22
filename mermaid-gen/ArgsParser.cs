using System;
using System.Linq;

namespace mermaid_gen
{
    public class ArgsParser
    {
        public string Command { get; set; }
        public MermaidDiagramType DiagramType { get; set; }
        public string InputAssemblyPath { get; set; }
        public string OutputPath { get; set; }
        public GenerationType GenerationType { get; set; }

        public static readonly string[] acceptedCommands = { "generate", "_generate", "--help" };
        private static readonly string[] acceptedArgs = { "-a", "--input-assembly-path", "-t", "--diagram-type", "-o", "--output-path", "-g", "--generation-type" };

        public static (ExitCode, string) TryParse(string[] args, out ArgsParser argsParser)
        {
            argsParser = new ArgsParser();
            if (!ArgsParser.acceptedCommands.Any(a => a == args[0]))
            {
                return (ExitCode.CommandUnknown, $"Command {args[0]} not found");
            }
            else
            {
                argsParser.Command = args[0];
            }

            try
            {
                for (int i = 1; i < args.Length; i += 2)
                {
                    switch (args[i])
                    {
                        case "-a":
                        case "--input-assembly-path":
                            if (acceptedArgs.Contains(args[i + 1]))
                                return (ExitCode.Error, $"");
                            argsParser.InputAssemblyPath = args[i + 1];
                            break;
                        case "-o":
                        case "--output-path":
                            if (acceptedArgs.Contains(args[i + 1]))
                                return (ExitCode.Error, $"");
                            argsParser.OutputPath = args[i + 1];
                            break;
                        case "-t":
                        case "--diagram-type":
                            if (acceptedArgs.Contains(args[i + 1]))
                                return (ExitCode.Error, $"");
                            switch (args[i + 1])
                            {
                                case "er":
                                    argsParser.DiagramType = MermaidDiagramType.ER;
                                    break;
                                case "class":
                                    argsParser.DiagramType = MermaidDiagramType.Class;
                                    break;
                                default: // If we got here, someone is requesting an unsupported DiagramType
                                    return (ExitCode.Error, $"");
                            }
                            break;
                        case "-g":
                        case "--generation-type":
                            if (acceptedArgs.Contains(args[i + 1]))
                                return (ExitCode.Error, $"");
                            switch (args[i + 1])
                            {
                                case "no-fluent":
                                    argsParser.GenerationType = GenerationType.NonFluent;
                                    break;
                                case "fluent":
                                    argsParser.GenerationType = GenerationType.Fluent;
                                    break;
                                default: // If we got here, someone is requesting an unsupported DiagramType
                                    return (ExitCode.Error, $"");
                            }
                            break;
                        default: // If we got here, we have an unbalanced set of args
                            return (ExitCode.Error, $"");
                    }
                }
            }
            catch (Exception e)
            {
                // TODO: Handle unbalanced args array here
                Console.WriteLine(e.Message);
                return (ExitCode.Error, $"");
            }

            return (ExitCode.Success, $"");
        }
    }

    public enum ExitCode
    {
        Success = 0,
        CommandUnknown = 1,
        Error = 2
    }

    public enum GenerationType
    {
        NonFluent = 0,
        Fluent = 1
    }

    public enum MermaidDiagramType
    {
        ER = 0,
        Class = 1
    }
}