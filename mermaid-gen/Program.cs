using System;

namespace mermaid_gen
{
    public class Program
    {
        static void Main(string[] args)
        {
            (ExitCode exitCode, string parseResponseString) = ArgsParser.TryParse(args, out ArgsParser argsParsed);
            if (exitCode == ExitCode.Success)
            {
                
                switch (argsParsed.Command)
                {

                }
            }
        }
    }
}
