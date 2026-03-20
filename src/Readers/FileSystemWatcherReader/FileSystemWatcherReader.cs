using Cfg.Net.Contracts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Cfg.Net.Readers.FileSystemWatcherReader {

   public class FileSystemWatcherReader : IActiveReader, IDisposable {

      public event EventHandler<CfgEventArgs> Changed;
      private FileSystemWatcher _fileSystemWatcher;
      private readonly object _locker = new object();
      private string _source;
      private IDictionary<string, string> _parameters;

      public string Read(string fileName, IDictionary<string, string> parameters, ILogger logger) {

         if (string.IsNullOrEmpty(fileName)) {
            logger.Error("Your configuration file name is null or empty.");
            return null;
         }

         // This allows for consumer to pass in URL query parameters with file name
         var queryStringIndex = fileName.IndexOf('?');
         if (queryStringIndex > 0) {
            if (parameters == null) {
               parameters = new Dictionary<string, string>();
            }
            var urlParameters = HttpUtility.ParseQueryString(fileName.Substring(queryStringIndex + 1));
            foreach (var pair in urlParameters) {
               parameters[pair.Key] = pair.Value;
            }
            fileName = fileName.Substring(0, queryStringIndex);
         }

         var lastPart = fileName.Split('\\').Last();
         var intersection = lastPart.Intersect(Path.GetInvalidFileNameChars()).ToArray();

         if (intersection.Any()) {
            logger.Error("Your configuration file name contains invalid characters: " + string.Join(", ", intersection) + ".");
            return null;
         }

         if (Path.HasExtension(fileName)) {
            FileInfo fileInfo;
            if (!Path.IsPathRooted(fileName)) {
               fileName = new FileFinder().Find(fileName, logger);
            }

            fileInfo = new FileInfo(fileName);

            if (_fileSystemWatcher == null) {
               _fileSystemWatcher = new FileSystemWatcher {
                  Path = fileInfo.DirectoryName,
                  Filter = fileInfo.Name,
                  NotifyFilter = NotifyFilters.LastWrite
               };
               _fileSystemWatcher.Changed += OnFileChanged;
               _fileSystemWatcher.EnableRaisingEvents = true;
            }

            _source = fileName;
            _parameters = parameters;

            try {
               return ReadAllText(fileInfo.FullName);
            } catch (Exception ex) {
               logger.Error("Can not read file. {0}", ex.Message);
               return null;
            }
         }

         logger.Error("Invalid file name: {0}.  File must have an extension (e.g. xml, json, etc)", fileName);
         return null;

      }
 
      private void OnFileChanged(object sender, FileSystemEventArgs e) {
         try {
            lock (_locker) {
               _fileSystemWatcher.EnableRaisingEvents = false;
               Changed?.Invoke(this, new CfgEventArgs() { Source = _source, Parameters = _parameters });
            }
         } finally {
            _fileSystemWatcher.EnableRaisingEvents = true;
         }
      }

      public void Dispose() {
         Changed = null;  // removes any subscribers
         if(_fileSystemWatcher != null) {
            _fileSystemWatcher.Dispose();
         }
      }

      private string ReadAllText(string fullName) {
         using (var fs = new FileStream(fullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 0x1000)) {
            using (var sr = new StreamReader(fs)) {
               var contents = sr.ReadToEnd();
               return contents;
            }
         }
      }

   }
}
