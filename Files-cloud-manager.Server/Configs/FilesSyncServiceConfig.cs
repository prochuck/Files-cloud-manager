namespace Files_cloud_manager.Server.Configs
{
    public class FilesSyncServiceConfig
    {
        /// <summary>
        /// Путь к папке, в которую будут сохраняться файлы.
        /// </summary>
        public string BaseFilesPath { get; set; }
        /// <summary>
        /// Путь к папке, в которую будут сохраняться временные файлы.
        /// Желательно что-бы она находилась на одном диске с BaseFilePath.
        /// </summary>
        public string TmpFilesPath { get; set; }
    }
}
