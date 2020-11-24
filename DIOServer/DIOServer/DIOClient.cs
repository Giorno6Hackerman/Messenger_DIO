using System;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Net.Security;
using System.IO;
using System.Security.Authentication;

namespace DIOServer
{
    public class DIOClient
    {
        private Program prog;
        public TcpClient client;

        public NetworkStream netStream;
        public SslStream ssl;
        public BinaryReader bReader;
        public BinaryWriter bWriter;
        UserInfo userInfo = null;


        // новый поток для клиента
        public DIOClient(Program program, TcpClient tcpClient)
        {
            prog = program;
            client = tcpClient;
        }


        // установка соединения с клиентом
        public void SetupConnection()  
        {
            try
            {
                Console.WriteLine("[{0}] New connection.", DateTime.Now);
                netStream = client.GetStream();
                ssl = new SslStream(netStream, false);
                ssl.AuthenticateAsServer(prog.cert, false, SslProtocols.Tls, true);
                Console.WriteLine("[{0}] Connection authenticated.", DateTime.Now);

                bReader = new BinaryReader(ssl, Encoding.UTF8);
                bWriter = new BinaryWriter(ssl, Encoding.UTF8);

                // рукопожатие
                
                int hello = bReader.ReadInt32();
                if (hello == Constants.ShakeHands)
                {
                    
                    byte logMode = bReader.ReadByte();
                    string userName = bReader.ReadString();
                    string password = bReader.ReadString();

                    Thread.Sleep(10);
                    bWriter.Write(Constants.ShakeHands);
                    bWriter.Flush();
                    
                    if (userName.Length > 0)
                    {
                        if (password.Length > 0)
                        {
                            switch (logMode)
                            {
                                case Constants.Register: // если идёт регистрация клиента
                                    if (CheckRegister(userName, password))
                                    {
                                        Receiver();
                                    }
                                    else
                                        bWriter.Write(Constants.UserExists);  // ник уже занят
                                    break;
                                case Constants.Login:  // если идёт авторизация клиента
                                    if (CheckLogin(userName, password))
                                    {
                                         Receiver();
                                    }
                                    break;
                                default:
                                    break;
                            }
                        }
                        else
                            bWriter.Write(Constants.BadPassword);
                    }
                    else
                        bWriter.Write(Constants.BadUsername);
                }
                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                if (!client.Connected)
                    CloseConnection();
            }

            
        }


        // получает все пакеты
        private void Receiver()  
        {
            Console.WriteLine("[{0}] User \"{1}\" logged in.", DateTime.Now, userInfo.UserName);
            userInfo.LoggedIn = true;

            try
            {
                while (client.Connected)
                {
                    byte type = bReader.ReadByte();  // тип входящего пакета

                    switch (type)
                    {
                        case Constants.UserAvailable:
                            CheckUserAvailable();
                            break;
                        case Constants.SendMessage:
                            SendMessage();
                            break;
                        case Constants.SendFile:
                            SendFile();
                            break;
                        default:
                            break;
                    }
                }
            }
            catch(IOException ex) 
            {
                Console.WriteLine(ex.Message);
            } 

            userInfo.LoggedIn = false;
            Console.WriteLine("[{0}] User \"{1}\" logged out", DateTime.Now, userInfo.UserName);
        }


        private bool CheckLogin(string userName, string password)
        {
            if (prog.users.TryGetValue(userName, out userInfo))
            {
                if (password == userInfo.Password)
                {
                    // разрыв соединения другого зарегестрированного клиента
                    if (userInfo.LoggedIn)
                        userInfo.Connection.CloseConnection();

                    userInfo.Connection = this;
                    bWriter.Write(Constants.ItsOK);
                    bWriter.Flush();

                    return true;
                }
                else
                    bWriter.Write(Constants.WrongPassword);  // не тот пароль
            }
            else
                bWriter.Write(Constants.UserNoExists);  // нет такого юзера
            
            return false;
        }


        private bool CheckRegister(string userName, string password)
        {
            if (!prog.users.ContainsKey(userName))
            {
                // ник не занят
                userInfo = new UserInfo(userName, password, this);
                prog.users.Add(userName, userInfo);
                bWriter.Write(Constants.ItsOK);
                bWriter.Flush();
                Console.WriteLine("[{0}] Registered new user \"{1}\".", DateTime.Now, userName);
                prog.SaveUsers();

                return true;
            }

            return false;
        }


        private void CheckUserAvailable()
        {
            string who = bReader.ReadString();
            bWriter.Write(Constants.UserAvailable);
            bWriter.Write(who);

            UserInfo info;
            if (prog.users.TryGetValue(who, out info))
            {
                if (info.LoggedIn)
                    bWriter.Write(true);
                else
                    bWriter.Write(false);
            }
            else
                bWriter.Write(false);

            bWriter.Flush();
        }


        private void SendMessage()
        {
            string to = bReader.ReadString();
            string msg = bReader.ReadString();

            UserInfo recipient;
            if (prog.users.TryGetValue(to, out recipient))
            {
                if (recipient.LoggedIn)
                {
                    recipient.Connection.bWriter.Write(Constants.ReceivedMessage);
                    recipient.Connection.bWriter.Write(userInfo.UserName);  // от кого
                    recipient.Connection.bWriter.Write(msg);                // само сообщение
                    recipient.Connection.bWriter.Flush();
                    Console.WriteLine("[{0}] ({1} -> {2}) Message sent.", DateTime.Now, userInfo.UserName, recipient.UserName);
                }
            }
        }



        private void SendFile()
        {
            string to = bReader.ReadString();
            string fileName = bReader.ReadString();
            int fileSize = bReader.ReadInt32();
            byte[] buffer = new byte[2048];

            UserInfo recipient;
            if (prog.users.TryGetValue(to, out recipient))
            {
                if (recipient.LoggedIn)
                {
                    recipient.Connection.bWriter.Write(Constants.ReceivedFile);
                    recipient.Connection.bWriter.Write(userInfo.UserName);  // от кого
                    recipient.Connection.bWriter.Write(fileName);
                    recipient.Connection.bWriter.Write(fileSize);

                    while (fileSize > 0)
                    {
                        int count = buffer.Length > fileSize ? fileSize : buffer.Length;
                        buffer = bReader.ReadBytes(count);
                        recipient.Connection.bWriter.Write(buffer, 0, count);
                        fileSize -= count;
                    }

                    recipient.Connection.bWriter.Flush();
                    Console.WriteLine("[{0}] ({1} -> {2}) File sent.", DateTime.Now, userInfo.UserName, recipient.UserName);
                }
            }
        }

        // закрытие соединения
        private void CloseConnection() 
        {
            try
            {
                if(userInfo != null)
                    userInfo.LoggedIn = false;
                bReader.Close();
                bWriter.Close();
                ssl.Close();
                netStream.Close();
                client.Close();
                Console.WriteLine("[{0}] End of the connection.", DateTime.Now);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }



    }
}
