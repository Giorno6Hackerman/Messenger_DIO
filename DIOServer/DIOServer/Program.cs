using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace DIOServer
{
    public class Program
    {
        // изменить потом на свой
        public IPAddress serverIP = IPAddress.Parse("127.0.0.1");
        public int port = 6666;
        public bool isRunning = true;
        // сам сервак
        public TcpListener server;
        // инфа о юзерах и их подключениях
        public Dictionary<string, UserInfo> users = new Dictionary<string, UserInfo>();


        // Получение ip-адреса.
        //public IPAddress serverIP = Dns.GetHostEntry(Dns.GetHostName()).AddressList[0].MapToIPv4();
        //public IPAddress serverIP = IPAddress.Parse("192.168.43.121");

        // для ssl
        public X509Certificate2 cert;


        // подготовка и запуск сервера
        public Program()
        {
            Console.Title = "DIO Messenger Server";
            Console.WriteLine("******** DIO Messenger Server ********");
            Console.WriteLine(Dns.GetHostName());
            Console.WriteLine(serverIP.ToString());
            LoadUsers();
            Console.WriteLine("[{0}] Starting server...", DateTime.Now);
            cert = new X509Certificate2("server.pfx", "instant");

            server = new TcpListener(serverIP, port);
            server.Start();
            Console.WriteLine("[{0}] Server is running properly.", DateTime.Now);
            Listen();
        }


        static void Main(string[] args)
        {
            Program prog = new Program();
            Console.WriteLine();
            Console.WriteLine("Press enter to close the program.");
            Console.ReadLine();
        }


        // слушает подключения
        private void Listen()  
        {
            try
            {
                while (isRunning)
                {
                    TcpClient tcpClient = server.AcceptTcpClient();
                    DIOClient client = new DIOClient(this, tcpClient);

                    Thread clientThread = new Thread(new ThreadStart(client.SetupConnection));
                    clientThread.Start();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }


        string usersFileName = Environment.CurrentDirectory + "\\users.dat";
        
        // сохранение инфы о юзерах
        public void SaveUsers()  
        {
            try
            {
                Console.WriteLine("[{0}] Saving users...", DateTime.Now);
                BinaryFormatter binForm = new BinaryFormatter();
                FileStream userFile = new FileStream(usersFileName, FileMode.Create, FileAccess.Write);
                binForm.Serialize(userFile, users.Values.ToArray());  
                userFile.Close();
                Console.WriteLine("[{0}] Users saved.", DateTime.Now);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }


        // загрузка инфы о юзерах
        public void LoadUsers()  
        {
            try
            {
                Console.WriteLine("[{0}] Loading users...", DateTime.Now);
                BinaryFormatter binForm = new BinaryFormatter();
                FileStream userFile = new FileStream(usersFileName, FileMode.Open, FileAccess.Read);
                UserInfo[] infos = (UserInfo[])binForm.Deserialize(userFile);      
                userFile.Close();
                users = infos.ToDictionary((x) => x.UserName, (x) => x);
                Console.WriteLine("[{0}] Users loaded, count = {1}", DateTime.Now, users.Count);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
