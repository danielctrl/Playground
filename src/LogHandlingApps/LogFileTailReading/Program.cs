using System;
using System.IO;
using System.Text;
using System.Threading;

namespace LogFileTailReading
{
    class Program
    {
        private static long totalReadBytes = 0;

        static void Main(string[] args)
        {
            if(args == null || args.Length <= 0 || string.IsNullOrWhiteSpace(args[0]))
                throw new ArgumentException("The log's full file name must be provided");
            
            //Getting the directory path and file name
            var fullFileName = args[0];
            var directoryPath = GetDirectoryFromFullFileName(fullFileName);
            var fileName = GetFileNameFromFullFileName(fullFileName);

            using (var watcher = new FileSystemWatcher())
            {
                //Informing the full file path
                //This code is meant to work only with a single file. it enable it to work observing multiple files, the totalReadBytes static and the way it's being outputed need to be refactored
                watcher.Path = directoryPath;
                watcher.Filter = fileName;

                //Adding the watching only when the file is being writting
                watcher.NotifyFilter = NotifyFilters.LastWrite;

                //Adding the events
                watcher.Changed += ReadNewTextInFile;
                watcher.Created += ResetTotalReadBytes;
                watcher.Deleted += ResetTotalReadBytes;

                //Activating the events raising
                watcher.EnableRaisingEvents = true;

                //Console.WriteLine($"It is observing the file {directoryPath}\\{fileName} and all new text will be reported here.");
                System.Threading.Thread.Sleep(Timeout.Infinite);
            }
        }

        private static string GetDirectoryFromFullFileName(string fullFileName)
        {
            return fullFileName.Replace("/", "\\").Substring(0, fullFileName.LastIndexOf("\\"));
        }
        
        private static string GetFileNameFromFullFileName(string fullFileName)
        {
            return fullFileName.Replace("/", "\\").Substring(fullFileName.LastIndexOf("\\"));
        }

        /// <summary>
        /// Read only what wasn't read before on the file
        /// </summary>
        private static void ReadNewTextInFile(object source, FileSystemEventArgs e)
        {
            Console.Write(ReadFileIncrementingOffset(e.FullPath, ref totalReadBytes)?.ToString() ?? "");
        }

        /// <summary>
        /// Reset the totalReadBytes
        /// </summary>
        private static void ResetTotalReadBytes(object source, FileSystemEventArgs e)
        {
            totalReadBytes = 0;
        }


        /// <summary>
        /// Read a file offseting the amount of bytes given the param offset. It also increments the offset with the amount of bytes read.
        /// </summary>
        /// <param name="fileFullPath"></param>
        /// <param name="offset"></param>
        /// <param name="encode"></param>
        /// <returns></returns>
        public static StringBuilder ReadFileIncrementingOffset(string fileFullPath, ref long offset, Encoding encode = null)
        {
            encode = encode ?? new UTF8Encoding();

            try
            {
                using (FileStream fs = File.Open(fileFullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    var newBytesToReadLength = fs.Length - offset;

                    //If some text were removed it resets the total read bytes to the current amount of bytes in the file and interrupts the flow
                    if (newBytesToReadLength < 0)
                    {
                        offset = fs.Length;
                        return null;
                    }

                    //Command to skip some bytes
                    fs.Seek(offset, SeekOrigin.Begin);

                    //Initializing variables
                    byte[] bytes = new byte[newBytesToReadLength];
                    var sb = new StringBuilder();

                    //Reading the file until it has nothing else to be read
                    while (true)
                    {
                        //Read the file
                        var numReadBytes = fs.Read(bytes, 0, bytes.Length);
                        
                        //Adds what was read into offset
                        offset += numReadBytes;

                        if (numReadBytes == 0)
                            break;
                            
                        sb.Append(encode.GetString(bytes));
                    }
                    
                    return sb;
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex);
            }

            return null;
        }
    }
}