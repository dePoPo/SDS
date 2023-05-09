//-----------------------------------------------------------------------
//  Path="C:\Users\bno.CORP\OneDrive\Git\SDS\sds\sds"
//  File="Program.cs" 
//  Modified="zaterdag 26 februari 2022" 
//  Author: H.P. Noordam
//-----------------------------------------------------------------------
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace sds
{
    internal class Program
    {
        private static void Main(string[] args) {
            bool optionHaltOnError = false;
            bool optionCreateTarget = false;
            bool optionSkipHiddenFiles = false;

            //
            // Init
            LogBuilder logger = new LogBuilder();
            string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            logger.Say($"simple directory sync (sds) v {version} (c) 2014-2022 Bob Noordam (https://bobnoordam.nl)");
            //
            // Validate
            if(args.Count() < 2) {
                logger.Say("usage: sds [-options] [sourcedirectory] [targetdirectory]");
                logger.Say("   -mail:logrecipient sends an email log to the set recipient");
                logger.Say("   -smtp:your.smtp.server the smtp server to use (must accept local subnet mail)");
                logger.Say("   -sender:sds@localhost.localdomain use a custom sernder address for the log file");
                logger.Say("   -d create target directory if it does not exists");
                logger.Say("   -h halt on errors until the error is confirmed.");
                logger.Say("   -sh skip hidden files");
                return;
            }
            int argCount = args.Count();
            string sourcePath = args[argCount - 2];
            string targetPath = args[argCount - 1];
            while(targetPath.EndsWith(@"\")) {
                targetPath = targetPath.Substring(0, targetPath.Length - 1);
            }
            while(sourcePath.EndsWith(@"\")) {
                sourcePath = sourcePath.Substring(0, sourcePath.Length - 1);
            }
            string sender = "sds@localhost.localdomain";
            string smtpserver = "localhost";
            string rcpt = string.Empty;
            if(Common.HasArgument("-d", args)) {
                logger.Say("option -d create target if it does not exists");
                optionCreateTarget = true;
            }
            if(Common.HasArgument("-h", args)) {
                logger.Say("option -h halt on errors until confirmation");
                optionHaltOnError = true;
            }
            if(Common.HasArgument("-sh", args)) {
                logger.Say("option -sh skipp hidden files");
                optionSkipHiddenFiles = true;
            }
            //
            // Check if the -smtp parameter is passed and set the smtp server if needed
            if(Common.GetArgument(@"-smtp:", args) != string.Empty) {
                smtpserver = Common.GetArgument(@"-smtp:", args);
                logger.Say($"using smtp server {smtpserver}");
            }
            //
            // Check if the -sender parameter is passed and set the smtp server if needed
            if(Common.GetArgument(@"-sender:", args) != string.Empty) {
                sender = Common.GetArgument(@"-sender:", args);
                logger.Say($"using log sender {sender}");
            }
            //
            // Check if the -mail parameter is passed and send the log file is requested
            if(Common.GetArgument(@"-mail:", args) != string.Empty) {
                rcpt = Common.GetArgument(@"-mail:", args);
                logger.Say($"using log receiver {rcpt}");
            }
            //
            // Execute
            logger.Say($"source: {sourcePath}");
            logger.Say($"target: {targetPath}");
            //
            // Controleer of het bron path toegangkelijk is
            bool validPaths = true;
            if(!Directory.Exists(sourcePath)) {
                validPaths = false;
                logger.Say($"source path {sourcePath} cannot be found");
                if(optionHaltOnError) {
                    Common.HaltOnError();
                }
            }
            //
            // Controleer of het doel path toegangkelijk is
            if(optionCreateTarget) {
                try {
                    Directory.CreateDirectory(targetPath);
                } catch(Exception ex) {
                    logger.Say(ex.Message);
                    if(optionHaltOnError) {
                        Common.HaltOnError();
                    }
                }
            }
            if(!Directory.Exists(targetPath)) {
                validPaths = false;
                logger.Say($"destination path {targetPath} cannot be found");
                if(optionHaltOnError) {
                    Common.HaltOnError();
                }
            }
            if(validPaths) {
                //
                // Delete check
                logger.Say("checking for files no longer present on the source");
                DeleteScanner delScanner = new DeleteScanner(sourcePath, targetPath, logger, optionHaltOnError);
                int deleteCount = delScanner.DeleteFiles(optionSkipHiddenFiles);
                //
                // New files
                logger.Say("checking for new and changed files");
                CopyScanner newScanner = new CopyScanner(sourcePath, targetPath, logger, optionHaltOnError);
                newScanner.CopyFiles(optionSkipHiddenFiles);
                int updateCount = newScanner.UpdateCount;
                int copyCount = newScanner.CopyCount;
                logger.Say($"{copyCount} new, {updateCount} updated, {deleteCount} deleted.");
            }
            //
            // Mail logfile if rcpt is set
            if(!string.IsNullOrEmpty(rcpt)) {
                try {
                    logger.MailLog(smtpserver, rcpt, sender);
                } catch(Exception ex) {
                    logger.Say($"ERROR SENDING EMAIL FROM {sender} TO {rcpt} WITH {smtpserver}");
                    logger.Say(ex.Message);
                    if(optionHaltOnError) {
                        Common.HaltOnError();
                    }
                }
            }
#if DEBUG
            Console.Write("debug build. enter to terminate >>");
            Console.ReadLine();
#endif
        }
    }
}