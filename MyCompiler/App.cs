using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace MyCompiler
{
    public static class App
    {
        public static List<char> letters = new List<char>();
        public static List<char> digits = new List<char>();
        public static List<int> FollowE = new List<int>();
        public static List<int> FollowT = new List<int>();
        public static List<int> FollowL = new List<int>();
        public static List<int> SINGLE = new List<int>();
        public static List<int> @double = new List<int>();
        public static List<int> literacy = new List<int>();

        public static string tmp;

        static void Main(string[] args)
        {
            string filename;
            if (args.Length == 1)
                filename = args[0];
            else
            {
                Console.Write("Enter source filename: ");
                filename = Console.ReadLine();
            }
            if (!File.Exists(filename))
            {
                Console.WriteLine("Input file not found");
                Console.ReadKey();
            }

            Console.WriteLine("STEP 1: Creating ASM code...");
            SyntaxParser sp = new SyntaxParser(filename);
            sp.Parse();
            sp.Dispose();


            Console.WriteLine("STEP 2: Compiling...");
            string masmPath = @"C:\masm32";
            ConsoleProcess(masmPath + @"\bin\ml", @"/c /Cp /coff /Fo src\map.obj src\code.asm");


            Console.WriteLine("STEP 3: Linking...");
            ConsoleProcess(masmPath + @"\bin\link", @"src\map.obj /SUBSYSTEM:CONSOLE /LIBPATH:" + masmPath + @"\lib /OUT:src\program.exe");

            Console.WriteLine("Done!");
            Console.ReadKey();
        }

        private static void ConsoleProcess(string filename, string arguments)
        {
            using (Process p = new Process())
            {
                p.StartInfo.EnvironmentVariables["INCLUDE"] = @"C:\masm32\include";

                p.StartInfo.FileName = filename;
                p.StartInfo.Arguments = arguments;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.Start();
                Console.WriteLine(p.StandardOutput.ReadToEnd());
                p.WaitForExit();
            }
        }
    }

}
