using System;
using System.IO;
using System.Linq;
using System.Reflection;
using mermaid_gen.generators;

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

                            var convertedAssemblyPath = Path.Combine(Directory.GetCurrentDirectory(), argsParsed.InputAssemblyPath);
                            Assembly startupAssembly = Assembly.LoadFrom(convertedAssemblyPath);

                            switch (argsParsed.DiagramType)
                            {
                                case MermaidDiagramType.ER:
                                    {
                                        var erGenerator = new ErGenerator(startupAssembly.GetTypes().ToList());
                                        erGenerator.Generate();
                                        Console.WriteLine($"Mermaid ER diagram generated from {argsParsed.InputAssemblyPath}");
                                        try
                                        {
                                            File.WriteAllText(argsParsed.OutputPath, erGenerator.ErDiagram);
                                        }
                                        catch (Exception e)
                                        {
                                            Console.WriteLine(e.Message);
                                            return (int)ExitCode.Error;
                                        }
                                        return (int)exitCode;
                                    }
                                default:
                                    {
                                        Console.WriteLine($"No valid diagram type generator found for {argsParsed.DiagramType.ToString()}");
                                        return (int)ExitCode.Error;
                                    }
                            }
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
