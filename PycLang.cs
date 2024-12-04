using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace VovaScript
{
    public static class VovaScript2
    {
        public static bool Tokens = false;
        public static bool PrintVariablesInDebug = false;   //
        public static bool PrintVariablesAfterDebug = false;//
        public static bool PrintProgram = false;            //
        public static bool Debug = false;                   //
        public static bool TimePrint = true;
        public static string Directory = "";

        public static void LogTokens(ref Token[] tokens)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(string.Join("|", tokens.Select(t => t.View).ToArray()));
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(string.Join("|", tokens.Select(t => Convert.ToString(t.Value)).ToArray()));
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine(string.Join("|", tokens.Select(t => Convert.ToString(t.Type.GetStringValue())).ToArray()));
            Console.ForegroundColor = ConsoleColor.Yellow;
        }

        public static void PrintVariables(bool printVariablesInDebug = false)
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            if (printVariablesInDebug)
                foreach (var variable in Objects.Variables)
                {
                    if (variable.Value.GetType().ToString() == "System.Collections.Generic.List`1[System.Object]")
                      //  Console.WriteLine($"{variable.Key} = {PrintStatement.ListString((List<object>)variable.Value)}; тип <<{TypePrint.Pyc(variable.Value)}>>; ");
                   // else
                        Console.WriteLine($"{variable.Key} = {variable.Value}; тип <<{TypePrint.Pyc(variable.Value)}>>; ");
                }
            Console.ResetColor();
        }

        public static void PrintError(Exception error)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            if (error is FileNotFoundException)
                Console.WriteLine("НЕ БЫЛА НАЙДЕННА НЕКАЯ БИБЛИОТЕКА ДЛЯ РАБОТЫ ЯЗЫКА, ВОЗМОЖНО ЭТО БЫЛА Newtonsoft.Json.dll");

            else if (error is ReturnStatement)
            {
                ReturnStatement ret = error as ReturnStatement;
                Console.WriteLine($"КАК-ТО НЕ ТАК ВЕРНУЛ НО ВЕРНУЛО: <{ret.GetResult()}>");
            }

            else if (error is BreakStatement)
                Console.WriteLine("ЗАЧЕМ-ТО БЫЛО ИСПОЛЬЗОВАННО <выйти> ВНЕ ЦИКЛА");

            else if (error is ContinueStatement)
                Console.WriteLine("ЗАЧЕМ-ТО БЫЛО ИСПОЛЬЗОВАННО <продолжить> ВНЕ ЦИКЛА");

            else
                Console.WriteLine(error.Message);

            Console.ResetColor();
        }

        public static void PycOnceLoad(string code, string dir)
        {
            Directory = dir;
           // try {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                var tokens = new Tokenizator(code).Tokenize();
                if (Tokens) LogTokens(ref tokens);

                IStatement program = new Parser(tokens).Parse();
                if (PrintProgram) Console.WriteLine(program);
                program.Execute();

                stopwatch.Stop();
                if (TimePrint) Console.WriteLine(stopwatch.Elapsed);
          //  } catch (Exception e) { PrintError(e); } 
          
            PrintVariables(PrintVariablesAfterDebug);
        }

        public static void Pyc()
        {
            while (true)
            {
                Console.ResetColor();
                Console.Write("> ");
                string code = Console.ReadLine() ?? "";
                PycOnceLoad(code, Directory);
            }
        }
    }
}
