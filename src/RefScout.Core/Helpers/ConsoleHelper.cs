using System;

namespace RefScout.Core.Helpers;

public static class ConsoleHelper
{
    public static void WriteLine(object message, ConsoleColor? color = null)
    {
        if (color.HasValue)
        {
            Console.ForegroundColor = color.Value;
        }

        Console.WriteLine(message);
        Console.ResetColor();
    }

    public static void Write(object message, ConsoleColor? color = null)
    {
        if (color.HasValue)
        {
            Console.ForegroundColor = color.Value;
        }

        Console.Write(message);
        Console.ResetColor();
    }
}