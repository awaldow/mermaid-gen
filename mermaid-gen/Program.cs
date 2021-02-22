using System;
using System.IO;

namespace mermaid_gen
{
    public class Program
    {
        static int Main(string[] args)
        {
            (ExitCode exitCode, string parseResponseString) = ArgsParser.TryParse(args, out ArgsParser argsParsed);
            if (exitCode == ExitCode.Success)
            {

                switch (argsParsed.Command)
                {
                    case "generate":
                        {
                            Console.WriteLine($"Loading assembly from {Path.Combine(Directory.GetCurrentDirectory(), argsParsed.InputAssemblyPath)}");
                            if (!string.IsNullOrEmpty(Path.GetDirectoryName(argsParsed.OutputPath)) && !Directory.Exists(Path.GetDirectoryName(argsParsed.OutputPath)))
                            {
                                Console.WriteLine($"Creating directory {Path.GetDirectoryName(argsParsed.OutputPath)}");
                                Directory.CreateDirectory(Path.GetDirectoryName(argsParsed.OutputPath));
                            }

                            return (int)exitCode;
                        }
                    case "_generate":
                        {
                            return (int)exitCode;
                        }
                    case "--help":
                        {
                            Console.WriteLine("Usage -- mermaid-gen {--help|generate {args}}");
                            Console.WriteLine("\t{args}:");
                            Console.WriteLine("\t\t-a,--input-assembly-file\tpath to dll");
                            Console.WriteLine("\t\t-t,--diagram-type\t\ttype of Mermaid diagram to generate (options are {er | class})");
                            return (int)ExitCode.Success;
                        }
                    default:
                        {
                            Console.WriteLine(parseResponseString);
                            return (int)ExitCode.Error;
                        }
                }
            }
            else
            {
                Console.WriteLine(parseResponseString);
                return (int)exitCode;
            }
        }
    }
}
