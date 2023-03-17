using Files_cloud_manager.Client.Models.States;

namespace Files_cloud_manager.Client.Services.Interfaces
{
    internal interface IProgramListCaretaker
    {
        ProgramListMemento Memento { get; set; }

        ProgramListMemento LoadFromFile();
        void SaveToFile();
    }
}