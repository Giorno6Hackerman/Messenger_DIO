namespace DIOwpf
{
    public static class Constants
    {
        // const
        public const int ShakeHands = 666;      
        public const byte ItsOK = 0;           // Works properly
        public const byte Login = 1;        // Request to login
        public const byte Register = 2;     // Request to register
        public const byte BadUsername = 3;  
        public const byte BadPassword = 4;  
        public const byte UserExists = 5;       // Already exists
        public const byte UserNoExists = 6;     // Doesn't exists
        public const byte WrongPassword = 7;    // Wrong password
        public const byte UserAvailable = 8;  // Is user available?
        public const byte SendMessage = 9;        // Send message
        public const byte ReceivedMessage = 10;    // Message received
        public const byte SendFile = 11;    // Send file
        public const byte ReceivedFile = 12;    // Receive file
        public const byte LoadImage = 13;    // Load new image on server
        public const byte GetImage = 12;    // Get image from server data
    }
}
