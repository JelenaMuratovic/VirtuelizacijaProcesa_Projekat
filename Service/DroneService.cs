using Common;
using Service.Subscriber;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace Service
{
    public class DroneService : IDroneService
    {
        private StreamWriter measurementsWriter;
        private StreamWriter rejectsWriter;

        private double previousAnorm = 0;
        private double Amean = 0;
        private double Anorm = 0;
        private double deltaA = 0;
        private double AnormSum = 0;
        private double Weffect = 0;
        private int samplesCount = 0;

        public delegate void TransferEventHandler(object sender, EventArgs e);
        public event TransferEventHandler TransferStartedEvent;
        public event TransferEventHandler TransferCompletedEvent;
        public event TransferEventHandler SampleReceivedEvent;


        public delegate void WarningEventHandler(object sender, WarningEventArguments e);
        public event WarningEventHandler AccelerationSpikeEvent;
        public event WarningEventHandler OutOfBandWarningEvent;
        public event WarningEventHandler WindSpikeEvent;

        TransferCompletedObserver transferCompleted = new TransferCompletedObserver();
        TransferStartedObserver transferStarted = new TransferStartedObserver();
        SampleReceiveObserver sampleReceived = new SampleReceiveObserver();
        AccelerationSpikeObserver accelerationSpike = new AccelerationSpikeObserver();
        OutOfBandWarningObserver outOfBandWarning = new OutOfBandWarningObserver();
        WindSpikeObserver windSpikeWarning = new WindSpikeObserver();

        public DroneService()
        {
            TransferStartedEvent += transferStarted.OnTransferStarted;
            TransferCompletedEvent += transferCompleted.OnTransferCompleted;
            SampleReceivedEvent += sampleReceived.OnSampleReceived;
            AccelerationSpikeEvent += accelerationSpike.OnAccelerationSpike;
            OutOfBandWarningEvent += outOfBandWarning.OnOutOfBandWarning;
            WindSpikeEvent += windSpikeWarning.OnWindSpike;
        }

        public string EndSession()
        {
            TransferCompletedEvent(this, EventArgs.Empty);

            measurementsWriter.Dispose();
            rejectsWriter.Dispose();

            return "ACK: COMPLETED";
        }

        public string PushSample(DroneSample sample)
        {
            //validacija polja
            bool isValidFormat = ValidateFormat(sample);
            bool isValidValues = ValidateValues(sample);
            if (!isValidFormat)
            {
                throw new FaultException<DataFormatFault>(
                   new DataFormatFault($"Polja nisu validnog formata."),
                   "DataFormatFault");
            }
            if (!isValidValues)
            {

                //upisi u rejects.csv
                rejectsWriter.WriteLine(sample.ToString());

                throw new FaultException<DataValidationFault>(
                   new DataValidationFault($"Vrednosti polja nisu u validnom opsegu."),
                   "DataValidationFault");
            }
            Console.WriteLine("Transfer in progress...");

            //upis u measurements.csv
            measurementsWriter.WriteLine(sample.ToString());

            samplesCount++;
            //poziv eventa sample received
            SampleReceivedEvent(this, EventArgs.Empty);
            Console.WriteLine(samplesCount);

            Anorm = Math.Sqrt(Math.Pow((double)sample.LinearAccelerationX, 2) + Math.Pow((double)sample.LinearAccelerationY, 2) + Math.Pow((double)sample.LinearAccelerationZ, 2));
            AnormSum += Anorm;
            Amean = AnormSum / samplesCount;

            //pracenje odstupanja od tekuceg proseka
            double odstupanje = double.Parse(System.Configuration.ConfigurationManager.AppSettings["odstupanje"]);
            if (Anorm < (1.0 - odstupanje) * Amean)
                OutOfBandWarningEvent(this, new WarningEventArguments("below"));
            else if (Anorm > (1.0 + odstupanje) * Amean)
                OutOfBandWarningEvent(this, new WarningEventArguments("above"));

            //racunanje promene vetra
            double W_threshold = double.Parse(System.Configuration.ConfigurationManager.AppSettings["W_threshold"]);
            Weffect = Math.Abs((double)sample.WindSpeed * Math.Sin((double)sample.WindAngle));
            if (Weffect > W_threshold)
                WindSpikeEvent(this, new WarningEventArguments("above"));
            else if (Weffect < -W_threshold)
                WindSpikeEvent(this, new WarningEventArguments("below"));

            //provera da li je dron naglo ubrzao
            if (samplesCount > 1)
            {
                deltaA = Anorm - previousAnorm;
                double A_threshold = double.Parse(System.Configuration.ConfigurationManager.AppSettings["A_threshold"]);
                if (deltaA > A_threshold)
                    AccelerationSpikeEvent(this, new WarningEventArguments("above"));
                else if (deltaA < -A_threshold)
                    AccelerationSpikeEvent(this, new WarningEventArguments("below"));
            }
            previousAnorm = Anorm;

            Console.WriteLine("Transfer completed\n");
            return "ACK: WORKING";
        }
        private bool ValidateFormat(DroneSample sample)
        {
            if (sample.LinearAccelerationX == null || sample.LinearAccelerationY == null || sample.LinearAccelerationZ == null ||
                sample.WindSpeed == null || sample.WindAngle == null || sample.Time == null)
                return false;
            else
                return true;
        }
        private bool ValidateValues(DroneSample sample)
        {
            if (sample.WindSpeed <= 0 || sample.WindAngle <= 0 || sample.Time < 0)
                return false;
            else
                return true;
        }

        public string StartSession(string metaHeader)
        {
            TransferStartedEvent(this, EventArgs.Empty);

            //prebaciti u app.config
            string filePathMeasurements = "measurements_session.csv";
            string filePathReject = "rejects.csv";

            measurementsWriter = new StreamWriter(filePathMeasurements);
            measurementsWriter.WriteLine(metaHeader);

            rejectsWriter = new StreamWriter(filePathReject);
            rejectsWriter.WriteLine(metaHeader);

            //resetuj
            previousAnorm = 0;
            Amean = 0;
            samplesCount = 0;

            return "ACK: IN_PROGRESS";
        }
    }
}
