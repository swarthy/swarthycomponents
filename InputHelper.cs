//Author: Alexander Mochalin
//Copyright (c) 2013-2014 All Rights Reserved

using System;
namespace SwarthyComponents.Input
{
    public static class StringExtentions
    {
        public static bool TryParse<T>(this string value, out T newValue, T defaultValue = default(T))
            where T : struct, IConvertible
        {
            newValue = defaultValue;
            try
            {
                newValue = (T)Convert.ChangeType(value, typeof(T));
            }
            catch
            {
                return false;
            }
            return true;
        }

        public static T Parse<T>(this string value)
            where T : struct, IConvertible
        {
            return (T)Convert.ChangeType(value, typeof(T));
        }
    }
    public static class InputHelper
    {
        public static T Promt<T>(string msg, T defaultValue = default(T)) where T : struct, IConvertible
        {
            Console.Write("{0} [{1}]: ", msg, defaultValue);
            T val = defaultValue;
            var inp = Console.ReadLine();
            if (inp.Length == 0)
                return defaultValue;
            while (!inp.TryParse(out val))
            {
                Console.WriteLine("Ошибка ввода. Повторите попытку.");
                inp = Console.ReadLine();
            }
            return val;
        }
    }
}
