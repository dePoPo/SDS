//-----------------------------------------------------------------------
//  Path="C:\Users\bno.CORP\OneDrive\Git\SDS\sds\sds"
//  File="CopyScanner.cs" 
//  Modified="zaterdag 26 februari 2022" 
//  Author: H.P. Noordam
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.IO;

namespace sds
{
    internal class CopyScanner
    {
        private readonly bool _haltOnError;
        private readonly LogBuilder _logger;
        private readonly string _source;
        private readonly string _target;

        public int CopyCount { get; set; }

        public int UpdateCount { get; set; }

        public CopyScanner(string source, string target, LogBuilder logBuilder, bool haltOnErrors) {
            _source = source;
            _target = target;
            CopyCount = 0;
            UpdateCount = 0;
            _logger = logBuilder;
            _haltOnError = haltOnErrors;
        }

        public void CopyFiles(bool skipHidden) {
            TreeScanner sourceScanner = new TreeScanner(_source, _logger);
            List<string> sourceList = sourceScanner.GetFileTree(skipHidden);
            _logger.Say($"checking {sourceList.Count} files.");
            //
            // Loop source files
            foreach (string s in sourceList) {
                string sourcePartial = s.Substring(_source.Length);
                string targetPath = $"{_target}{sourcePartial}";
                string targetDir = Path.GetDirectoryName(targetPath);
                //
                // new files
                if (!Directory.Exists(targetDir)) {
                    _logger.Say($"dnew:\t{Common.DisplayString(targetDir)}");
                    Directory.CreateDirectory(targetDir);
                }
                bool isHandled = false;
                //
                // new files
                if (!File.Exists(targetPath)) {
                    try {
                        _logger.Say($"fnew:\t{Common.DisplayString(s)}");
                        File.Copy(s, targetPath);
                        CopyCount++;
                    } catch (Exception ex) {
                        _logger.Say($"error copying new file: {Common.DisplayString(s)}\r\n{ex.Message}");
                        if (_haltOnError) {
                            Common.HaltOnError();
                        }
                    }
                    isHandled = true;
                }
                //
                // changed files
                if (!isHandled) {
                    try {
                        CheckFileUpdated(s, targetPath);
                    } catch (Exception ex) {
                        _logger.Say($"error copying updated file: {Common.DisplayString(s)}\r\n{ex.Message}");
                        if (_haltOnError) {
                            Common.HaltOnError();
                        }
                    }
                }
            }
        }

        private void CheckFileUpdated(string s, string targetPath) {
            long sSize = new FileInfo(s).Length;
            long tSize = new FileInfo(targetPath).Length;
            DateTime dtSrc = new FileInfo(s).LastWriteTime;
            DateTime dtTar = new FileInfo(targetPath).LastWriteTime;
            if (sSize != tSize) {
                _logger.Say($"fchg:\t{Common.DisplayString(s)}");
                File.Copy(s, targetPath, true);
                UpdateCount++;
            } else {
                TimeSpan span = new TimeSpan(dtSrc.Ticks - dtTar.Ticks);
                if (span.TotalSeconds > 60) {
                    _logger.Say($"dchg:\t{Common.DisplayString(s)}");
                    File.Copy(s, targetPath, true);
                    UpdateCount++;
                }
            }
        }
    }
}