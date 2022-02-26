//-----------------------------------------------------------------------
//  Path="C:\Users\bno.CORP\OneDrive\Git\SDS\sds\sds"
//  File="TreeScanner.cs" 
//  Modified="zaterdag 26 februari 2022" 
//  Author: H.P. Noordam
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.IO;

namespace sds
{
    internal class TreeScanner
    {
        private readonly string _basedirectory; // The directory to start scanning from
        private int _count;
        private int _countDir;
        private readonly LogBuilder _logger;
        private readonly string _pattern; // Optional, pattern *.jpg, etc

        /// <summary>
        ///     Initiates a treescanner object from the given directory returning all files
        /// </summary>
        public TreeScanner(string basedirectory, LogBuilder logBuilder) {
            _basedirectory = basedirectory;
            _count = 0;
            _countDir = 0;
            _logger = logBuilder;
        }

        /// <summary>
        ///     Initiates a treescanner object from the given directory returning a specific pattern
        /// </summary>
        public TreeScanner(string basedirectory, string pattern) {
            _basedirectory = basedirectory;
            _pattern = pattern;
        }

        /// <summary>
        ///     Return all files in the filesystem tree starting at the base directory
        /// </summary>
        /// <returns></returns>
        public List<string> GetFileTree(bool skipHidden) {
            int prevLength = 0;
            List<string> result = new List<string>();
            Stack<string> worklist = new Stack<string>();
            worklist.Push(_basedirectory);
            _countDir++;
            while(worklist.Count > 0) {
                // Get a workitem
                bool skip = false;
                string workfolder = worklist.Pop();
                DirectoryInfo dirinfo = new DirectoryInfo(workfolder);
                try {
                    //
                    // Add all subfolders in the current workdirectory to the stack of items to be processed
                    try {
                        DirectoryInfo[] subfolders = dirinfo.GetDirectories();
                        foreach(DirectoryInfo subfolder in subfolders) {
                            worklist.Push(subfolder.FullName);
                            _countDir++;
                        }
                    } catch(UnauthorizedAccessException) {
                        _logger.Say($"skip (no access) : {workfolder}");
                        skip = true;
                    }
                    //
                    // Add all files in the currect workdirectory to the result list
                    if(!skip) {
                        FileInfo[] filelist = string.IsNullOrEmpty(_pattern) ? dirinfo.GetFiles() : dirinfo.GetFiles(_pattern);
                        foreach(FileInfo fileInfo in filelist) {
                            if((fileInfo.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden) {
                                //
                                // Normal file
                                result.Add(fileInfo.FullName);
                            } else {
                                //
                                // Hidden file
                                if(!skipHidden) {
                                    //
                                    // skip is not defined, so we copy hidden files
                                    result.Add(fileInfo.FullName);
                                } else {
                                    //
                                    // skip hidden file 
                                    _logger.Say($"skip hidden file {fileInfo.FullName}");
                                }
                            }

                            _count++;
                            string displayString = fileInfo.FullName;
                            int maxLine = Common.GetConsoleWidth() - 4;
                            if(maxLine < 70) {
                                maxLine = 70;
                            }
                            if(displayString.Length > maxLine) {
                                int offset = displayString.Length - maxLine;
                                displayString = $"...{displayString.Substring(offset)}";
                            }
                            int length = displayString.Length;
                            Console.Write(displayString);
                            if(length < prevLength) {
                                for(int i = 0; i < prevLength - length; i++) {
                                    Console.Write(" ");
                                }
                            }
                            Console.Write("\r");
                            prevLength = length;
                        }
                    }
                } catch(PathTooLongException) {
                    // Long paths are possible, but a possible compat. nightmare, so we dont return long results. see:
                    // http://msdn.microsoft.com/en-us/library/system.io.pathtoolongexception.aspx
                    // http://blogs.msdn.com/b/bclteam/archive/2007/02/13/long-paths-in-net-part-1-of-3-kim-hamilton.aspx
                }
            }
            for(int i = 0; i < prevLength; i++) {
                Console.Write(" ");
            }
            Console.Write("\r");
            _logger.Say($"located {_countDir} directory's and {_count} files.");
            return result;
        }
    }
}