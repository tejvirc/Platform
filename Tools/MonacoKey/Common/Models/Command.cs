namespace Common.Models
{
    using Common.Utils;
    using System.Collections.Generic;
    using System.Xml.Serialization;

    // Used for serialization and deserialzation of array of commands
    // .NET prefers to serialize a class, not an array alone
    public class CommandData
    {
        public List<Command> Commands = new List<Command> { };
        public CommandData() { }
    }

    public class Command : NotifyPropertyChanged
    {
        private string _description = "";
        private string _id = "";
        private string _name = "";
        private string _volumeName = "";
        private string _parameters = "";
        private bool _restartEGM = false;
        private string _script = "";
        private string _scriptFileName = "";
        public Command() { }

        public Command(string id, string name, string description, bool restartEGM, string scriptFileName, string parameters, string volumeName)
        {
            Id = id;
            Name = name;
            Description = description;
            ScriptFileName = scriptFileName;
            RestartEGM = restartEGM;
            Parameters = parameters;
            VolumeName = volumeName;
        }

        public string Description
        {
            get
            {
                return _description;
            }
            set
            {
                _description = value;
                OnPropertyChanged(nameof(Description));
            }
        }
        public string Id
        {
            get
            {
                return _id;
            }
            set
            {
                _id = value;
                OnPropertyChanged(nameof(Id));
            }
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
        public string Parameters
        {
            get
            {
                return _parameters;
            }
            set
            {
                _parameters = value;
                OnPropertyChanged(nameof(Parameters));
            }
        }
        public bool RestartEGM
        {
            get
            {
                return _restartEGM;
            }
            set
            {
                _restartEGM = value;
                OnPropertyChanged(nameof(RestartEGM));
            }
        }
        [XmlIgnore]
        public string Script
        {
            get
            {
                return _script;
            }
            set
            {
                _script = value;
                OnPropertyChanged(nameof(Script));
            }
        }
        public string ScriptFileName
        {
            get
            {
                return _scriptFileName;
            }
            set
            {
                _scriptFileName = value;
                OnPropertyChanged(nameof(ScriptFileName));
            }
        }
        public string VolumeName
        {
            get
            {
                return _volumeName;
            }
            set
            {
                _volumeName = value;
                OnPropertyChanged(nameof(VolumeName));
            }
        }
    }
}
