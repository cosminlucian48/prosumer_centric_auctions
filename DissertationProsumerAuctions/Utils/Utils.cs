﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace DissertationProsumerAuctions
{
    public class Utils
    {
        public static int NoTurns = 10;
        public static int EnergyLoadRateNumberOfDelays = Convert.ToInt32(ConfigurationManager.AppSettings.Get("EnergyLoadRateNumberOfDelays"));
        public static int EnergyGenerationRateNumberOfDelays  = Convert.ToInt32(ConfigurationManager.AppSettings.Get("EnergyGenerationRateNumberOfDelays"));
        public static int EnergyPriceNumberOfDelays = Convert.ToInt32(ConfigurationManager.AppSettings.Get("EnergyPriceNumberOfDelays"));
        public static int NumberOfProsumers = Convert.ToInt32(ConfigurationManager.AppSettings.Get("NumberOfProsumers"));
        public static int EnergyMarketParticipantsSignUpInterval = Convert.ToInt32(ConfigurationManager.AppSettings.Get("EnergyMarketParticipantsSignUpInterval"));

        public static int Delay = 1500;
        public static Random RandNoGen = new Random();

        public static void ParseMessage(string content, out string action, out string parameters)
        {
            string[] t = content.Split();

            action = t[0];

            parameters = "";

            if (t.Length > 1)
            {
                for (int i = 1; i < t.Length - 1; i++)
                    parameters += t[i] + " ";
                parameters += t[t.Length - 1];
            }
        }

        public static string Str(object p1, object p2)
        {
            return string.Format("{0} {1}", p1, p2);
        }

        public static string Str(object p1, object p2, object p3)
        {
            return string.Format("{0} {1} {2}", p1, p2, p3);
        }

        public static string Str(object p1, object p2, object p3, object p4)
        {
            return string.Format("{0} {1} {2} {3}", p1, p2, p3, p4);
        }
    }
}
