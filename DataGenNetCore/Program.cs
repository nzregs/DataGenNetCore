using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Microsoft.Azure.EventHubs;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.CommandLineUtils;

namespace DataGenNetCore
{
    class Program
    {
        static AppSecrets secrets = new AppSecrets();
        static List<string> users = SampleData.Users();
        static List<TelcoMessage.Website> websites = SampleData.Websites();
        static List<TelcoMessage.DataSpecial> dataspecials = SampleData.DataSpecials();
        static List<TelcoMessage.Tower> towers = SampleData.Towers();
        static List<TelcoMessage.Subscriber> subscribers = SampleData.Subscribers();
        static EventHubClient hubClient;
        static Random random = new Random();

        private static int TimeOfDayWait()
        {
            // simulate quiet/busy times of the day returning an additional delay time in milliseconds
            TimeZoneInfo userZone = TimeZoneInfo.FindSystemTimeZoneById("New Zealand Standard Time");
            int hour = TimeZoneInfo.ConvertTime(DateTime.UtcNow, userZone).Hour;
            if (hour >= 22 || hour <= 5)
            {
                return 9;
            }
            else if (hour >= 6 && hour <= 8)
            {
                return 4;
            }
            else if (hour >= 9 && hour <= 11)
            {
                return 3;
            }
            else if (hour >= 12 && hour <= 14)
            {
                return 2;
            }
            else if (hour >= 15 && hour <= 19)
            {
                return 4;
            }
            else
            {
                return 5;
            }
        }


        private static async Task sendMessages(string velocity, string messagetype)
        {
            Boolean keepProcessing = true;
            Int32 delay = 1000;
            DateTime timeStamp = DateTime.Now;
            Int32 messageCount = 0;

            switch (velocity)
            {
                case "slow":
                    timeStamp = DateTime.Now;
                    delay = 5 * 1000;
                    //docker run error: "Cannot see if a key has been pressed when either application does not have a console or when console input has been redirected from a file. Try Console.In.Peek."
                    while (!(Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape))
                    {
                        await sendMessage(messagetype);
                        messageCount += 1;
                        if (timeStamp.AddSeconds(15) <= DateTime.Now)
                        {
                            var diffInSeconds = (DateTime.Now - timeStamp).TotalSeconds;
                            Console.WriteLine("Sent {0} messages in {1} seconds at a rate of {2} messages/min", messageCount, diffInSeconds, Math.Round((messageCount / diffInSeconds) * 60));
                            timeStamp = DateTime.Now;
                            messageCount = 0;
                        }
                        Thread.Sleep(delay + (TimeOfDayWait()*100));
                    }
                    break;
                case "fast":
                    timeStamp = DateTime.Now;
                    delay = 0;

                    //docker run error: "Cannot see if a key has been pressed when either application does not have a console or when console input has been redirected from a file. Try Console.In.Peek."
                    while (!(Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape))
                    {
                        await sendMessage(messagetype);
                        messageCount += 1;
                        if (timeStamp.AddSeconds(15) <= DateTime.Now)
                        {
                            var diffInSeconds = (DateTime.Now - timeStamp).TotalSeconds;
                            Console.WriteLine("Sent {0} messages in {1} seconds at a rate of {2} messages/min", messageCount, diffInSeconds, Math.Round((messageCount / diffInSeconds) * 60));
                            timeStamp = DateTime.Now;
                            messageCount = 0;
                        }
                        Thread.Sleep(delay + (TimeOfDayWait() * 100));
                    }
                    break;
                case "faster":
                    timeStamp = DateTime.Now;
                    delay = 0;
                    //docker run error: "Cannot see if a key has been pressed when either application does not have a console or when console input has been redirected from a file. Try Console.In.Peek."
                    while (!(Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape))
                    {
                        int parallelCount = 10;
                        Parallel.For(0, parallelCount, index =>
                        {
                            sendMessage(messagetype);
                            Thread.Sleep(delay + TimeOfDayWait() );
                            Interlocked.Add(ref messageCount, 1);
                        });
                        if (timeStamp.AddSeconds(5) <= DateTime.Now)
                        {
                            var diffInSeconds = (DateTime.Now - timeStamp).TotalSeconds;
                            Console.WriteLine("Sent {0} messages in {1} seconds at a rate of {2} messages/sec", messageCount, diffInSeconds, Math.Round(messageCount / diffInSeconds));
                            timeStamp = DateTime.Now;
                            messageCount = 0;
                        }
                    }
                    break;
                case "insane":
                    timeStamp = DateTime.Now;
                    delay = 0;
                    //docker run error: "Cannot see if a key has been pressed when either application does not have a console or when console input has been redirected from a file. Try Console.In.Peek."
                    while (!(Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape))
                    {
                        int parallelCount = 20;
                        Parallel.For(0, parallelCount, async index =>
                        {
                            await sendMessage(messagetype);
                            Interlocked.Add(ref messageCount, 1);
                        });
                        if (timeStamp.AddSeconds(5) <= DateTime.Now)
                        {
                            var diffInSeconds = (DateTime.Now - timeStamp).TotalSeconds;
                            Console.WriteLine("Sent {0} messages in {1} seconds at a rate of {2} messages/sec", messageCount, diffInSeconds, Math.Round(messageCount / diffInSeconds));
                            timeStamp = DateTime.Now;
                            messageCount = 0;
                        }
                    }
                    break;
                default:
                    Console.WriteLine("No sendmessages velocity specified, no messages sent.");
                    break;
            }
        }
       
        private static int GetRandomEventNum(int num_choices, List<int> choice_weight)
        {
            int sum_of_weight = 0;
            for (int i = 0; i < num_choices; i++)
            {
                sum_of_weight += choice_weight[i];
            }
            int rnd = random.Next(sum_of_weight);
            for (int i = 0; i < num_choices; i++)
            {
                if (rnd < choice_weight[i])
                    return i;
                rnd -= choice_weight[i];
            }
            return 1;
        }

        private static async Task<string> BuildSimpleMessage()
        {
            Guid messageId = Guid.NewGuid();
            Int32 randomUser = 0;

            Int32.TryParse(users[random.Next(0, users.Count)], out randomUser);
            var message = new Message
            {
                messageDate = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                messageId = messageId.ToString(),
                userId = randomUser,
                value = random.Next(1, 65536)
            };
            var messagestringjson = JsonConvert.SerializeObject(message, new JsonSerializerSettings() { ContractResolver = new CamelCasePropertyNamesContractResolver() });

            return messagestringjson;
        }

        private static async Task<string> BuildTelcoMessage()
        {
            var callTypes = new List<string>() { "call", "sms", "data" };
            var callTypeWeight = new List<int>() { 8, 27, 65 };

            var randomCallType = callTypes[GetRandomEventNum(callTypes.Count, callTypeWeight)];

            TelcoMessage.CallDetailRecord callDetailRecord;

            switch (randomCallType)
            {
                case "call":
                    callDetailRecord = new TelcoMessage.CallDetailRecord
                    {
                        EventDate = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                        TowerId = towers[random.Next(0, towers.Count)].TowerId.ToString(),
                        FromNumber = subscribers[random.Next(0, subscribers.Count)].SubscriberNumber.ToString(),
                        ToNumber = subscribers[random.Next(0, subscribers.Count)].SubscriberNumber.ToString(),
                        BillingType = "call",
                        Duration = random.Next(1, 60),
                        IMEI = ""
                    };
                    break;
                case "sms":
                    callDetailRecord = new TelcoMessage.CallDetailRecord
                    {
                        EventDate = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                        TowerId = towers[random.Next(0, towers.Count)].TowerId.ToString(),
                        FromNumber = subscribers[random.Next(0, subscribers.Count)].SubscriberNumber.ToString(),
                        ToNumber = subscribers[random.Next(0, subscribers.Count)].SubscriberNumber.ToString(),
                        BillingType = "sms",
                        Duration = 0,
                        IMEI = ""
                    };
                    break;
                default:
                    callDetailRecord = new TelcoMessage.CallDetailRecord
                    {
                        EventDate = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                        TowerId = towers[random.Next(0, towers.Count)].TowerId.ToString(),
                        FromNumber = subscribers[random.Next(0, subscribers.Count)].SubscriberNumber.ToString(),
                        BillingType = "data",
                        Duration = 0,
                        Bytes = random.Next(1, 1024),
                        Protocol = 6,
                        Port = 80,
                        IMEI = "",
                        Uri = websites[random.Next(0, websites.Count)].Uri.ToString()
                    };
                    break;
            }

            var messagestringjson = JsonConvert.SerializeObject(callDetailRecord, new JsonSerializerSettings() { ContractResolver = new CamelCasePropertyNamesContractResolver() });
            return messagestringjson;
        }

        private static async Task sendMessage(string messagetype)
        {
            string messagestringjson = "";

            if (messagetype == null)
            {
                messagestringjson = await BuildSimpleMessage();
            }
            else
            {
                messagestringjson = await BuildTelcoMessage();
            }

            EventData data = new EventData(Encoding.UTF8.GetBytes(messagestringjson));
            try
            {
                await hubClient.SendAsync(data);
            }
            catch (EventHubsException e)
            {
                if (!e.IsTransient) 
                Console.Write($"Fatal error occurred while sending to event hubs: {e.Message}");
                throw e;
            }
        }       


        public static void Main(string[] args)
        {
            MainAsync(args).GetAwaiter().GetResult();
        }

        private static async Task MainAsync(string[] args)
        {
            var result = 0;
            
            Console.WriteLine("Message Sender Data Generator: usage: dotnet DataGenNetCore.dll sendmessages -t <default|telco> <slow|fast|faster|insane>");
            Console.WriteLine("Press ESC to stop");

            var app = new Microsoft.Extensions.CommandLineUtils.CommandLineApplication(throwOnUnexpectedArg: false);

            app.Name = "Event Hub Data Generator";           
            app.HelpOption("-? | -h | --help");
            
            var sendmessages = app.Command("sendmessages", s => {
                s.OnExecute(() => {
                    s.ShowHelp(); //show help for sendmessages
                    return 1; //return error since we didn't do anything
                });
                s.HelpOption("-? | -h | --help"); //show help on --help

            });
            var optionMessageType = sendmessages.Option("-t | --message-type", "-t --message-type simple|telco", CommandOptionType.SingleValue);

            sendmessages.Command("help", help => {
                help.Description = "get help!";
                help.OnExecute(() => {
                    sendmessages.ShowHelp("sendmessages");
                    return 1;
                });
            });
            sendmessages.Command("slow", slow => {                
                slow.Description = "run message sending with 1 second thread sleep plus time of day delay (slow)";
                slow.HelpOption("-? | -h | --help");
                slow.OnExecute(async() => {
                    Console.WriteLine("using slow mode (1 message every ~five seconds, plus time of day delay, using thread.sleep)");
                    await sendMessages("slow", optionMessageType.Value());              
                    return 0;
                });
            });
            sendmessages.Command("fast", fast => {                
                fast.Description = "run message sending with time of day delay only (fast)";
                fast.HelpOption("-? | -h | --help");
                fast.OnExecute(async() => {
                    Console.WriteLine("using fast mode (single thread, time of day delay)");
                    await sendMessages("fast", optionMessageType.Value());
                    return 0;
                });
            });
            sendmessages.Command("faster", faster => {
                faster.Description = "run parallel for loop message sending (fastest)";
                faster.HelpOption("-? | -h | --help");
                faster.OnExecute(async () => {
                    Console.WriteLine("using faster mode (10 threads via parallel.for, time of day delay)");
                    await sendMessages("faster", optionMessageType.Value());
                    return 0;
                });
            });
            sendmessages.Command("insane", insane => {
                insane.Description = "run parallel for loop message sending (insane)";
                insane.HelpOption("-? | -h | --help");
                insane.OnExecute(async () => {
                    Console.WriteLine("using insane mode (20 threads via parallel.for, no delay)");
                    await sendMessages("insane", optionMessageType.Value());
                    return 0;
                });
            });

            
            
            try
            {
                var connectionStringBuilder = new EventHubsConnectionStringBuilder(secrets.EventHubConnectionString())
                {
                    EntityPath = secrets.EventHubPath()
                };

                hubClient = EventHubClient.CreateFromConnectionString(connectionStringBuilder.ToString());               
                app.Execute(args);
            }
            catch (Exception e)
            {
                app.HelpOption("-? | -h | --help");
                Console.WriteLine("Error occurred: {0}", e.Message);            
            }
            Console.WriteLine("press any key to exit");
            Console.ReadKey();
            Environment.Exit(result);
        }       
    }
}
