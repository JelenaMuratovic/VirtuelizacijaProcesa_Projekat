using Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    public class Program
    {
        static void Main(string[] args)
        {
            ChannelFactory<IDroneService> factory = new ChannelFactory<IDroneService>("DroneService");

            IDroneService proxy = factory.CreateChannel();
            string metaHeader = "LinearAccelerationX,LinearAccelerationY,LinearAccelerationZ,WindSpeed,WindAngle,Time";
            string status = proxy.StartSession(metaHeader);
            Console.WriteLine("Server returned: " + status);
            SendData(proxy);

        }
        private static void SendData(IDroneService proxy)
        {
            string csvPath = ("flight_values.csv");
            if (!File.Exists(csvPath))
            {
                Console.WriteLine("CSV file not found: " + csvPath);
                return;
            }

            int maxRows = 100; // koliko redova učitavamo
            int count = 0;

            using (var reader = new StreamReader(csvPath))
            {
                // preskoci header
                string headerLine = reader.ReadLine();

                while (!reader.EndOfStream && count < maxRows)
                {
                    string line = reader.ReadLine();
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        Console.WriteLine("Nevalidan red");
                        continue;
                    }

                    string[] parts = line.Split(',');

                    try
                    {
                        // Parsiramo podatke u DroneSample
                        DroneSample sample = new DroneSample
                        {
                            Time = double.Parse(parts[0], CultureInfo.InvariantCulture),
                            WindSpeed = double.Parse(parts[1], CultureInfo.InvariantCulture),
                            WindAngle = double.Parse(parts[2], CultureInfo.InvariantCulture),
                            LinearAccelerationX = double.Parse(parts[18], CultureInfo.InvariantCulture),
                            LinearAccelerationY = double.Parse(parts[19], CultureInfo.InvariantCulture),
                            LinearAccelerationZ = double.Parse(parts[20], CultureInfo.InvariantCulture),
                        };

                        // Šaljemo sample serveru
                        string sampleStatus = proxy.PushSample(sample);
                        Console.WriteLine($"Sample {count + 1} sent. Server status: {sampleStatus}");
                    }
                    catch (FormatException)
                    {
                        Console.WriteLine($"Invalid data at line {count + 2}: {line}");
                    }

                    count++;
                }
            }
        }
    }
}
