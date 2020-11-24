using System;

namespace DIOwpf
{
    // Error codes
    public enum MessageError : byte
    {
        TooUserName = Constants.BadUsername,
        TooPassword = Constants.BadPassword,
        Exists = Constants.UserExists,
        NoExists = Constants.UserNoExists,
        WrongPassword = Constants.WrongPassword
    }


    // Error event
    public class MessageErrorEventArgs : EventArgs
    {
        public MessageErrorEventArgs(MessageError error)
        {
            Error = error;
        }

        public MessageError Error { get; private set; }
    }


    // Is user connected
    public class MessageAvailEventArgs : EventArgs
    {
        public MessageAvailEventArgs(string user, bool avail)
        {
            UserName = user;
            IsAvailable = avail;
        }

        public string UserName { get; private set; }

        public bool IsAvailable { get; private set; }
    }


    // receiving a message
    public class MessageReceivedEventArgs : EventArgs
    {
        public MessageReceivedEventArgs(string user, string msg)
        {
            From = user;
            Message = msg;
        }

        public string From { get; private set; }

        public string Message { get; private set; }
    }


    // Receiving a file
    public class FileReceivedEventArgs : EventArgs
    {
        public FileReceivedEventArgs(string user, string fileName, int size)
        {
            From = user;
            FileName = fileName;
            Size = size;
        }

        public string From { get; private set; }
        public string FileName { get; private set; }
        public int Size { get; private set; }
    }


    public delegate void MessageErrorEventHandler(object sender, MessageErrorEventArgs e);
    public delegate void MessageAvailEventHandler(object sender, MessageAvailEventArgs e);
    public delegate void MessageReceivedEventHandler(object sender, MessageReceivedEventArgs e);
    public delegate void FileReceivedEventHandler(object sender, FileReceivedEventArgs e);
}
