using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;

namespace LMS2.components
{

    public class LicenseManager
    {
        private Dictionary<string, SessionInfo> sessions = new Dictionary<string, SessionInfo>();
        private License license = new License();

        public bool IsCurrentSessionLicensed(string sessionID)
        {
            return true;
        }

        public bool IsLicenseValid
        {
            get
            {
                return true;
            }
        }

        public void ConnectSession(string sessionID, string user, string clientName, string clientAddr, string clientAgent)
        {
            if (sessions.Count < license.NumberOfSessions)
            {
                sessions[sessionID] = new SessionInfo { ConnectTime = DateTime.Now, SessionID = sessionID, User = user, ClientName = clientName, ClientAddr = clientAddr /*, ClientAgent = clientAgent */ };
            }
        }

        public void DisconnectSession(string sessionID)
        {
            if (sessions.ContainsKey(sessionID))
                sessions.Remove(sessionID);
        }

        public SessionInfo[] ActiveSessions
        {
            get { return sessions.Values.ToArray(); }
        }


        public class SessionInfo
        {
            public DateTime ConnectTime { get; set; }
            public string SessionID { get; set; }
            public string User { get; set; }
            public string ClientName { get; set; }
            public string ClientAddr { get; set; }
            //public string ClientAgent { get; set; }
        }

        public class License
        {
            public int NumberOfSessions { get { return numberOfSessions; } }
            public string ServerMAC { get { return ServerMAC; } }
            public DateTime Expiration { get { return Expiration; } }

            private int numberOfSessions = int.MaxValue;
            private string serverMAC = "";
            private DateTime expiration = DateTime.MaxValue;

            public License()
            {
                string licenseFile = AppDomain.CurrentDomain.BaseDirectory + "license.txt";
                string licenseString = "";
                try
                {
                    if (File.Exists(licenseFile))
                        licenseString = File.ReadAllText(licenseFile);
                }
                catch { }

                if (licenseString.Length > 0)
                    ProcessLicenseString(licenseString);
                else
                    AppError.LogError("LicenseManager", "License file is missing or cannot be read.");

            }


            private void ProcessLicenseString(string licenseString)
            {
            }


        }


    }

}