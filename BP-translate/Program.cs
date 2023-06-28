using System.Net;
using System.Net.Sockets;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace BPtranslate {
    public sealed class Program {
        static X509Certificate2 serverCertificate = new X509Certificate2(Path.Combine(Directory.GetCurrentDirectory(), "bpmasterdata.pfx"));
        static string masterDataIp;
        static string hostsFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "system32/drivers/etc/hosts");

        const string redirectEntry = "127.0.0.1 masterdata-main.aws.blue-protocol.com";

        static void RunServer() {
            TcpListener listener = new TcpListener(IPAddress.Loopback, 443);
            listener.Start();

            new Task(() => {
                while (true) {
                    TcpClient client = listener.AcceptTcpClient();
                    new Task(() => {
                        SslStream clientStream = new SslStream(client.GetStream(), false);
                        clientStream.AuthenticateAsServer(serverCertificate, clientCertificateRequired: false, checkCertificateRevocation: false);

                        SslStream serverStream = new SslStream(new TcpClient(masterDataIp, 443).GetStream(), false);
                        serverStream.AuthenticateAsClient("masterdata-main.aws.blue-protocol.com");

                        ProxyConnection(clientStream, serverStream, true);
                        ProxyConnection(serverStream, clientStream, false);
                    }).Start();
                }
            }).Start();
        }

        static void ProxyConnection(SslStream src, SslStream output, bool toServer) {
            new Task(() => {
                byte[] message = new byte[4096];
                int clientBytes;
                while (true) {
                    try {
                        clientBytes = src.Read(message, 0, 4096);
                    } catch {
                        break;
                    }
                    if (clientBytes == 0) {
                        break;
                    }

                    if (toServer) {
                        string request = Encoding.UTF8.GetString(message);
                        string[] reqLines = request.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                        string firstLine = reqLines[0];

                        if (firstLine.StartsWith("GET /apiext/texts/ja_JP")) {
                            string loc = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "loc.json"));
                            string response = "HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\nContent-Length: " + loc.Length + "\r\nConnection: keep-alive\r\n\r\n" + loc;
                            src.Write(Encoding.ASCII.GetBytes(response));

                            Console.WriteLine("Translation sent, the program will close in 30 seconds");
                            RemoveRedirectFromHosts();
                            Thread.Sleep(TimeSpan.FromSeconds(30));
                            RemoveCertificate();
                            Environment.Exit(0);
                            continue;
                        }
                    }
                    output.Write(message, 0, clientBytes);
                }
                src.Close();
            }).Start();
        }

        public static int Main(string[] args) {
            handler = new ConsoleEventDelegate(ConsoleEventCallback);
            SetConsoleCtrlHandler(handler, true);

            using (X509Store store = new X509Store(StoreName.Root, StoreLocation.LocalMachine)) {
                store.Open(OpenFlags.ReadWrite);
                store.Add(serverCertificate);
            }

            if (File.ReadAllText(hostsFile).Contains(redirectEntry)) {
                RemoveRedirectFromHosts();
            }
            UpdateMasterDataRealIp();
            AddRedirectToHosts();

            RunServer();

            Console.WriteLine("Waiting for game start");

            while(true) Console.ReadKey();
        }

        static void AddRedirectToHosts() {
            //Check if the file already ends with a new line or we need to add it
            bool addNewLine = true;
            using (FileStream fs = new FileStream(hostsFile, FileMode.Open))
            using (BinaryReader rd = new BinaryReader(fs)) {
                fs.Position = fs.Length - 1;
                int last = rd.Read();
                if (last == 10) addNewLine = false;
            }

            string toAppend = (addNewLine ? Environment.NewLine + redirectEntry : redirectEntry);
            File.AppendAllText(hostsFile, toAppend);
        }

        static void RemoveRedirectFromHosts() {
            File.WriteAllText(hostsFile, File.ReadAllText(hostsFile).Replace(redirectEntry, ""));
        }

        static void RemoveCertificate() {
            using (X509Store store = new X509Store(StoreName.Root, StoreLocation.LocalMachine)) {
                store.Open(OpenFlags.ReadWrite);
                store.Remove(serverCertificate);
            }
        }

        static void UpdateMasterDataRealIp() {
            masterDataIp = Dns.GetHostEntry("masterdata-main.aws.blue-protocol.com").AddressList[0].ToString();
        }

        //Handle console close
        static bool ConsoleEventCallback(int eventType) {
            if (eventType == 2) {
                RemoveRedirectFromHosts();
                RemoveCertificate();
            }
            return false;
        }
        static ConsoleEventDelegate handler;
        private delegate bool ConsoleEventDelegate(int eventType);
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetConsoleCtrlHandler(ConsoleEventDelegate callback, bool add);
    }
}