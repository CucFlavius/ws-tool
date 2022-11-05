using OpenTK;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectWS
{
    public class Debug
    {
        const ConsoleColor LOG_COLOR = ConsoleColor.White;
        const ConsoleColor WARNING_COLOR = ConsoleColor.Yellow;
        const ConsoleColor ERROR_COLOR = ConsoleColor.DarkRed;
        const ConsoleColor EXCEPTION_COLOR = ConsoleColor.Red;

        public static void Log(string text, ConsoleColor color = LOG_COLOR)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ResetColor();
        }

        public static void Log(object anything, ConsoleColor color = LOG_COLOR)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(anything.ToString());
            Console.ResetColor();
        }

        public static void Log(bool value, ConsoleColor color = LOG_COLOR)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(value.ToString());
            Console.ResetColor();
        }

        public static void Log(int value, ConsoleColor color = LOG_COLOR)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(value);
            Console.ResetColor();
        }

        public static void Log(uint value, ConsoleColor color = LOG_COLOR)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(value);
            Console.ResetColor();
        }

        public static void Log(float value, ConsoleColor color = LOG_COLOR)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(value);
            Console.ResetColor();
        }

        public static void Log(Matrix4 value, ConsoleColor color = LOG_COLOR)
        {
            Console.ForegroundColor = color;
            Console.WriteLine("Matrix4 " + value.ToString());
            Console.ResetColor();
        }

        public static void Log(string format, object arg0)
        {
            Console.ForegroundColor = LOG_COLOR;
            Console.WriteLine(format, arg0);
            Console.ResetColor();
        }

        public static void LogWarning(string text)
        {
            Console.ForegroundColor = WARNING_COLOR;
            Console.WriteLine(text);
            Console.ResetColor();
        }

        public static void LogError(string text)
        {
            Console.ForegroundColor = ERROR_COLOR;
            Console.WriteLine(text);
            Console.ResetColor();
        }

        public static void LogException(Exception e)
        {
            Console.ForegroundColor = EXCEPTION_COLOR;
            Console.WriteLine(e.Message);
            Console.WriteLine(e.StackTrace);
            Console.ResetColor();
        }

        public static void Log()
        {
            Console.WriteLine();
        }
    }
}
