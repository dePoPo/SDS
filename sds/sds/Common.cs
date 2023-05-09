//-----------------------------------------------------------------------
//  Path="C:\Users\bno.CORP\OneDrive\Git\SDS\sds\sds"
//  File="Common.cs" 
//  Modified="zaterdag 26 februari 2022" 
//  Author: H.P. Noordam
//-----------------------------------------------------------------------
using System;

namespace sds
{
    internal class Common
    {
        public static string DisplayString(string msg) {
            int maxCol = GetConsoleWidth();
            if(msg.Length > maxCol) {
                string msgleft = msg.Substring(0, maxCol / 2);
                string msgright = msg.Substring(msg.Length - maxCol / 2);
                msg = $"{msgleft}...{msgright}";
            }
            return msg;
        }

        public static string GetArgument(string argument, string[] args) {
            string result = string.Empty;
            foreach(string s in args) {
                if(s.IndexOf(argument) != -1) {
                    int breakPoint = argument.Length;
                    result = s.Substring(breakPoint);
                }
            }
            return result;
        }

        public static int GetConsoleWidth() {
            int retVal = 80;
            try {
                retVal = Console.WindowWidth;
            } catch(Exception) {
                // we cannot access the console window in faceless operation (eg vs build process)
            }
            if(retVal < 80) {
                retVal = 80;
            }
            return retVal;
        }

        public static void HaltOnError() {

            string buffer = string.Empty;
            while(buffer != "Y" && buffer != "N")
            {
                Console.WriteLine("Continue after error (Y/N)");
                buffer = Console.ReadLine().Trim().ToUpper();
                if (buffer == "N")
                {
                    Environment.Exit(99);
                }
            }

        }

        public static bool HasArgument(string argument, string[] args) {
            foreach(string s in args) {
                if(s == argument) {
                    return true;
                }
            }
            return false;
        }
    }
}
