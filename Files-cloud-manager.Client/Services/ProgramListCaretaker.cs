using Files_cloud_manager.Client.Configs;
using Files_cloud_manager.Client.Models.States;
using Files_cloud_manager.Client.Services.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Files_cloud_manager.Client.Services
{
    internal class ProgramListCaretaker : IProgramListCaretaker
    {
        public ProgramListMemento Memento { get; set; }

        private string _pathToSaveFile;

        public ProgramListCaretaker(ProgramListCaretakerConfig caretakerConfig)
        {
            _pathToSaveFile = caretakerConfig.PathToSaveFile;
            LoadFromFile();
        }

        public void SaveToFile()
        {
            JsonSerializer jsonSerializer = new JsonSerializer();
            using (FileStream stream = new FileStream(_pathToSaveFile, FileMode.OpenOrCreate, FileAccess.Write))
            using (StreamWriter writer = new StreamWriter(stream))
            {
                jsonSerializer.Serialize(writer, Memento);
            }
        }

        public ProgramListMemento LoadFromFile()
        {
            JsonSerializer jsonSerializer = new JsonSerializer();
            using (FileStream stream = new FileStream(_pathToSaveFile, FileMode.OpenOrCreate, FileAccess.Read))
            using (StreamReader reader = new StreamReader(stream))
            {
                Memento = (jsonSerializer.Deserialize(reader, typeof(ProgramListMemento)) as ProgramListMemento) ?? new ProgramListMemento();
            }
            return Memento;
        }
    }
}
