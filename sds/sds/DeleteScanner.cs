//-----------------------------------------------------------------------
//  Path="C:\Users\bno.CORP\OneDrive\Git\SDS\sds\sds"
//  File="DeleteScanner.cs" 
//  Modified="zaterdag 26 februari 2022" 
//  Author: H.P. Noordam
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace sds
{
    internal class DeleteScanner
    {
        private readonly bool _haltOnError;
        private readonly LogBuilder _logger;
        private readonly string _source;
        private readonly string _target;

        public DeleteScanner(string source, string target, LogBuilder logbuilder, bool haltOnError) {
            _source = source;
            _target = target;
            _logger = logbuilder;
            _haltOnError = haltOnError;
        }

        public int DeleteFiles(bool skipHidden) {
            int deletecount = 0;
            TreeScanner targetScanner = new TreeScanner(_target, _logger);
            List<string> targetList;
            try {
                targetList = targetScanner.GetFileTree(skipHidden);
            } catch (IOException ex) {
                _logger.Say(ex.Message);
                return 0;
            }
            _logger.Say($"checking {targetList.Count} files.");
            //
            // Check for files that are on the target, but are no longer on the source
            foreach (string s in targetList) {
                //
                // Target file to check
                string targetPartial = s.Substring(_target.Length);
                string check = $"{_source}{targetPartial}";
                if (!File.Exists(check)) {
                    //
                    // Not present on source
                    _logger.Say($"fdel:\t{Common.DisplayString(s)}");
                    try {
                        FileInfo info = new FileInfo(s);
                        if (info.IsReadOnly) {
                            info.IsReadOnly = false;
                        }
                        File.Delete(s);
                        deletecount++;
                    } catch (Exception ex) {
                        _logger.Say($"{ex.Message} deleting file {Common.DisplayString(s)}");
                    }
                    //
                    // If the remaining directory is empty, remove it
                    string targetPath = Path.GetDirectoryName(s);
                    DirectoryInfo checkFiles = new DirectoryInfo(targetPath);
                    FileInfo[] content = checkFiles.GetFiles("*.*");
                    DirectoryInfo[] dcontent = checkFiles.GetDirectories();
                    if (!content.Any()
                        &&
                        !dcontent.Any()) {
                        _logger.Say($"ddel:\t{Common.DisplayString(targetPath)}");
                        try {
                            Directory.Delete(targetPath);
                        } catch (Exception ex) {
                            _logger.Say($"{ex.Message} deleting directory {Common.DisplayString(targetPath)}");
                            if (_haltOnError) {
                                Common.HaltOnError();
                            }
                        }
                    }
                }
            }
            return deletecount;
        }
    }
}