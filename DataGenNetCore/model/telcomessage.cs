using System;
using System.Collections.Generic;
using System.Text;

namespace DataGenNetCore
{
    public class TelcoMessage
    {
        public class CallDetailRecord
        {
            public string TowerId { get; set; }
            public string EventDate { get; set; }
            public string IMEI { get; set; }
            public string FromNumber { get; set; }
            public string ToNumber { get; set; }
            public string BillingType { get; set; }
            public int Duration { get; set; }
            public int Bytes { get; set; }
            public int Protocol { get; set; }
            public int Port { get; set; }
            public string Uri { get; set; }
            public decimal Cost { get; set; }
        }
        public class Tower
        {
            public string TowerId { get; set; }            
            public string Name { get; set; }            
            public string Description { get; set; }           
            public string Address { get; set; }           
            public string City { get; set; }            
            public string Longitude { get; set; }
            public string Latitude { get; set; }
        }
        public class Website
        {
            public string Uri { get; set; }
            public string Name { get; set; }
        }
        public class Subscriber
        {
            public string SubscriberNumber { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string AddressCity { get; set; }
            public string AddressCountry { get; set; }          
        }
        public class DataSpecial
        {
            public string Uri { get; set; }
            public string Name { get; set; }
            public string Offer { get; set; }
            public string CostPerMB { get; set; }
        }
    }
    
}
