using Cfg.Net.Contracts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Cfg.Net.Readers.FileSystemWatcherReader {
   public class FileFinder {

      public HashSet<string> Folders = new HashSet<string>();
      public FileFinder() {
         Folders.Add(Directory.GetCurrentDirectory());
         Folders.Add(AppDomain.CurrentDomain.BaseDirectory);
         Folders.Add(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
      }

      /// <summary>
      /// Finds full name and path of file.  Use if file name is not rooted.
      /// </summary>
      /// <param name="fileName"></param>
      /// <param name="logger"></param>
      /// <returns></returns>
      public string Find(string fileName, ILogger logger) {
         FileInfo fileInfo;
         foreach (var folder in Folders) {
            fileInfo = new FileInfo(Path.Combine(folder, fileName));
            if (!fileInfo.Exists) {
               logger.Warn($"file {fileInfo.FullName} not found...");
               continue;
            }
            return fileInfo.FullName;
         }
         return fileName;
      }
   }
}

