using System;
using System.IO;
using System.Linq;
using System.Threading;

static class CMDHandler
{
    static string[] argsPassedFromCMD;
    public static (string, string) HandleCMDInput(string[] args)
    {
        argsPassedFromCMD = args;
        string firstArg = argsPassedFromCMD[0];
        if (firstArg.EndsWith(".un"))
        {
            string error = CheckTooManyArgsPassed(3);
            if (error != null)
                return (null, error);
            if (!File.Exists(firstArg))
                return (null, $"'{firstArg}' doesn't exist");
            return RunCodeFromFile(firstArg);
        }
        return RunCommands(firstArg);
    }
    static readonly string[] exitConditions = new string[3]
    { "EXIT-INSTANTLY", "WAIT-FOR-EXIT", "WAIT-IF-ERROR" };
    static (string, string) RunCodeFromFile(string filePath)
    {
        string exitCondition = null;
        if (argsPassedFromCMD.Length >= 2)
        {
            exitCondition = argsPassedFromCMD[1];
            if (exitConditions.Contains(exitCondition)) { }
            else if (exitCondition.StartsWith("EXIT-AFTER-TIME:")) { }
            else
                return (null, $"Invalid run condition '{argsPassedFromCMD[1]}'");
        }
        if (argsPassedFromCMD.Length == 3)
        {
            string executeMode = argsPassedFromCMD[2];
            if (!new string[3] { "1", "2", "3" }.Contains(executeMode))
                return (null, "Mode must be 1, 2 or 3");
            SD.executeMode = byte.Parse(executeMode);
        }
        bool codeHasError = Shell.RunCode(filePath, string.Join("\n", File.ReadAllLines(filePath)));
        if (exitCondition != null)
        {
            if (exitCondition.StartsWith("EXIT-AFTER-TIME:"))
            {
                string timeToSleepStr = exitCondition.Split(':')[1];
                if (!int.TryParse(timeToSleepStr, out int timeToSleep))
                    return (null, "Time must be int");
                Thread.Sleep(timeToSleep);
            }
            else
                switch (exitCondition)
                {
                    case "EXIT-INSTANTLY":
                        break;
                    case "WAIT-FOR-EXIT":
                        Console.WriteLine("\nPress any key to conitnue...");
                        Console.ReadKey();
                        break;
                    case "WAIT-IF-ERROR":
                        if (codeHasError)
                        {
                            Console.WriteLine("\nPress any key to conitnue...");
                            Console.ReadKey();
                        }
                        break;
                }
        }
        return (null, null);
    }
    static (string, string) RunCommands(string firstArg)
    {
        string error;
        switch (firstArg)
        {
            case "version":
                error = CheckTooManyArgsPassed(1);
                if (error != null)
                    return (null, error);
                return (UD.VERSION, null);
            case "DEBUG":
                error = CheckTooManyArgsPassed(2);
                if (error != null)
                    return (null, error);
                if (argsPassedFromCMD.Length != 2)
                    return (null, "File path not given");
                string filePath = argsPassedFromCMD[1];
                if (!File.Exists(filePath))
                    return (null, $"'{filePath}' doesn't exist");
                Core.Debug(filePath);
                break;
            default:
                return (null, $"Unknown command '{firstArg}'");
        }
        return (null, null);
    }
    static string CheckTooManyArgsPassed(int argCount)
    {
        return argsPassedFromCMD.Length > argCount ? $"{argsPassedFromCMD.Length - argCount} too many args passed" : null;
    }
}