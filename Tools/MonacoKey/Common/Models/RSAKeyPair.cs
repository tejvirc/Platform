namespace Common.Models
{
    using Utils;
    using System.Security.Cryptography;

    public class RSAKeyPair : NotifyPropertyChanged
    {
        private string _name = null;
        private RSAParameters? _publicKey;
        private RSAParameters? _privateKey;

        public RSAKeyPair() { }
        public RSAKeyPair(string name, RSAParameters? publicKey, RSAParameters? privateKey)
        {
            Name = name;
            PublicKey = publicKey;
            PrivateKey = privateKey;
        }

        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }
        public RSAParameters? PublicKey
        {
            get
            {
                return _publicKey;
            }
            set
            {
                _publicKey = value;
                OnPropertyChanged(nameof(PublicKey));
            }
        }
        public RSAParameters? PrivateKey
        {
            get
            {
                return _privateKey;
            }
            set
            {
                _privateKey = value;
                OnPropertyChanged(nameof(PrivateKey));
            }
        }
    }
}
