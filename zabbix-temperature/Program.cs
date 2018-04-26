using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Threading;

namespace zabbix_temperature
{
    class Program
    {
        static private SerialPort _serialPort = new SerialPort();
        static private int _baudRate = 9600;
        static private int _dataBits = 8;
        static private Handshake _handshake = Handshake.None;
        static private Parity _parity = Parity.None;
        static private StopBits _stopBits = StopBits.One;
        static private string _portName;

        static readonly private string metricTemperature = "it_temperature";
        static readonly private string metricHumidity = "it_humidity";

        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                _portName = args[0];
            }
            else
            {
                string[] ports = SerialPort.GetPortNames();
                _portName = ports[0];
            }
            if (Open())
            {
                do
                {
                    Thread.Sleep(2000);
                } while (true);
            }
            Console.ReadLine();
        }

        public int BaudRate { get { return _baudRate; } set { _baudRate = value; } }
        public int DataBits { get { return _dataBits; } set { _dataBits = value; } }
        public Handshake Handshake { get { return _handshake; } set { _handshake = value; } }
        public Parity Parity { get { return _parity; } set { _parity = value; } }
        public string PortName { get { return _portName; } set { _portName = value; } }
        static public bool Open()
        {
            try
            {
                _serialPort.BaudRate = _baudRate;
                _serialPort.DataBits = _dataBits;
                _serialPort.Handshake = _handshake;
                _serialPort.Parity = _parity;
                _serialPort.PortName = _portName;
                _serialPort.StopBits = _stopBits;
                _serialPort.DataReceived += new SerialDataReceivedEventHandler(_serialPort_DataReceived);
                _serialPort.Open();
                Console.WriteLine("Open: " + _portName);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error open: " + _portName + " : " + ex.Message);
                return false;
            }
            return true;
        }

        static void toZabbix(string name, string value)
        {
            value = value.Replace(',', '.');
            string zabbixSender = @"c:\zabbix\zabbix_sender.exe";
            String zabbixParams = String.Format(" -z {0} -p 10051 -s {1} -k {2} -o {3}", "zabbix.megapage.ru", "TEMP-SQL", name, value);
            Console.WriteLine(zabbixParams);
            Process p = new Process();
            // Redirect the output stream of the child process.
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.FileName = zabbixSender;
            p.StartInfo.Arguments = zabbixParams;
            p.Start();
            string output = p.StandardOutput.ReadToEnd();
            Console.WriteLine(output);
            p.WaitForExit();
            p.Close();
        }

        static void _serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            string line = _serialPort.ReadLine();
            line = line.Trim().Replace("\r", string.Empty);

            string[] values = line.Replace('.', ',').Split(';');
            foreach (string item in values)
            {
                if (item.StartsWith("t"))
                {
                    toZabbix(metricTemperature, item.Substring(2));
                }
                else if (item.StartsWith("h"))
                {
                    toZabbix(metricHumidity, item.Substring(2));
                }
            }
        }
    }
}
