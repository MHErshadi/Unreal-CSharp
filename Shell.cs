using System;

static class Shell
{
    static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            SD.executeMode = 2;
            if (Console.CursorTop != 0)
                Console.WriteLine();
            Console.WriteLine(UD.VERSION);
            Console.WriteLine("Terminal verified as UnrealTerminal version 1.0.0");
            Console.WriteLine("Core verified as UnrealCore version 1.0.0\n");
            while (true)
            {
                WriteMessage("Unreal >> ", ConsoleColor.DarkGreen);
                string code = Console.ReadLine();
                RunCode("<stdin>", code);
            }
        }
        (string result, string error) = CMDHandler.HandleCMDInput(args);
        if (error != null)
            WriteLineMessage(error, ConsoleColor.DarkRed);
        else if (result != null)
            Console.WriteLine(result);
    }
    public static bool RunCode(string fn, string code)
    {
        if (code.Trim() == "")
            return false;
        (string result, string error) = Core.CodeResult(fn, code);
        if (error != null)
            WriteLineMessage(error, ConsoleColor.DarkRed);
        else if (result != null && SD.executeMode == 2)
        {
            if (Console.CursorLeft == 0)
                WriteMessage("Out: ", ConsoleColor.DarkYellow);
            else
                WriteMessage("\nOut: ", ConsoleColor.DarkYellow);
            Console.WriteLine(result);
        }
        return error != null;
    }
    static void WriteLineMessage(string message, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.WriteLine(message);
        Console.ResetColor();
    }
    static void WriteMessage(string message, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.Write(message);
        Console.ResetColor();
    }
}