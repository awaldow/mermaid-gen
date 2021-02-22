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
                                        switch (argsParsed.GenerationType)
                                        {
                                            case GenerationType.NonFluent:
                                                {
                                                    var erGenerator = new ErNonFluentGenerator(startupAssembly.GetTypes().ToList());
                                                    erGenerator.Generate();
                                                    Console.WriteLine($"Mermaid ER diagram generated from {argsParsed.InputAssemblyPath}");
                                                    if (argsParsed.OutputPath == "stdout")
                                                    {
                                                        Console.WriteLine(erGenerator.ErDiagram);
                                                        return (int)ExitCode.Success;
                                                    }
                                                    else
                                                    {
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
                                                }
                                            case GenerationType.Fluent:
                                                {
                                                    Console.WriteLine("Fluent generation is not yet supported");
                                                    return (int)ExitCode.CommandUnknown;
                                                }
                                            default:
                                                {
                                                    Console.WriteLine($"Unknown generationType {argsParsed.GenerationType.ToString()}");
                                                    return (int)ExitCode.CommandUnknown;
                                                }
                                        }
                                    }
                                default:
                                    {
                                        Console.WriteLine($"No valid diagram type generator found for {argsParsed.DiagramType.ToString()}");
                                        return (int)ExitCode.Error;
                                    }
                            }
                        }
                    case "--help":
                        {
                            Console.WriteLine("Usage -- mermaid-gen {--help|generate {args}}");
                            Console.WriteLine("\t{args}:");
                            Console.WriteLine("\t\t-a,--input-assembly-file\tpath to dll");
                            Console.WriteLine("\t\t-t,--diagram-type\t\ttype of Mermaid diagram to generate (options are {er | class}). Defaults to er");
                            Console.WriteLine("\t\t-o,--output-path\t\toutput path for mermaid diagram, including file name. provide the string 'stdout' to get command line output");
                            Console.WriteLine("\t\t-g,--generation-type\t\tgeneration type (options are {no-fluent | fluent}). If you are using any fluent configuration, use fluent; otherwise, use no-fluent. Defaults to no-fluent");
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
