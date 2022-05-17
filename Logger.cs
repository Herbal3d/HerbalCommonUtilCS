/*
 * Copyright (c) 2017 Robert Adams
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using System;
using System.Collections.Generic;

using NLog;

namespace org.herbal3d.cs.CommonUtil {

    public class LogParameters {
        public static string LogToConsole = "LogToConsole";     // boolean
        public static string LogToFile = "LogToFile";           // boolean
        public static string LogBaseFilename = "LogBaseFilename"; // boolean
    }

    public interface BLogger {
        void SetLogLevel(LogLevels pLevel);
        void Trace(string pMsg, params Object[] pArgs);
        void Debug(string pMsg, params Object[] pArgs);
        void Info(string pMsg, params Object[] pArgs);
        void Warn(string pMsg, params Object[] pArgs);
        void Error(string pMsg, params Object[] pArgs);
    }

    public enum LogLevels {
        Trace = 0,
        Debug,
        Information,
        Warning,
        Error
    }

    // Logger that eats all log message
    public class BLoggerNull : BLogger {
        void BLogger.Debug(string pMsg, params object[] pArgs) { }
        void BLogger.Error(string pMsg, params object[] pArgs) { }
        void BLogger.Info(string pMsg, params object[] pArgs) { }
        void BLogger.SetLogLevel(LogLevels pLevel) { }
        void BLogger.Trace(string pMsg, params object[] pArgs) { }
        void BLogger.Warn(string pMsg, params object[] pArgs) { }
    }

    // Logger that does a raw console output
    public class BLoggerConsole : BLogger {
        protected LogLevels _logLevel = LogLevels.Information;
        public BLoggerConsole() {
        }
        public void SetLogLevel(LogLevels pLevel) {
            _logLevel = pLevel;
        }
        private void DoLog(LogLevels pLevel, string pMsg, params Object[] pArgs) {
            Console.WriteLine(pMsg, pArgs);
        }
        public void Trace(string pMsg, params Object[] pArgs) {
            if (_logLevel == LogLevels.Trace || _logLevel == LogLevels.Debug) {
                DoLog(LogLevels.Trace, pMsg, pArgs);
            }
        }
        public void Debug(string pMsg, params Object[] pArgs) {
            if (_logLevel == LogLevels.Debug) {
                DoLog(LogLevels.Debug, pMsg, pArgs);
            }
        }
        public void Info(string pMsg, params Object[] pArgs) {
            DoLog(LogLevels.Information, pMsg, pArgs);
        }
        public void Warn(string pMsg, params Object[] pArgs) {
            DoLog(LogLevels.Warning, pMsg, pArgs);
        }
        public void Error(string pMsg, params Object[] pArgs) {
            DoLog(LogLevels.Error, pMsg, pArgs);
        }
    }

    public class BLoggerNLog: BLogger {
        protected LogLevels _logLevel = LogLevels.Information;
        protected NLog.Logger _logger;

        public static Dictionary<LogLevels, NLog.LogLevel> LogLevelMap = new Dictionary<LogLevels, LogLevel>() {
            { LogLevels.Trace, NLog.LogLevel.Trace },
            { LogLevels.Debug, NLog.LogLevel.Debug },
            { LogLevels.Information, NLog.LogLevel.Info },
            { LogLevels.Warning, NLog.LogLevel.Warn },
            { LogLevels.Error, NLog.LogLevel.Error },
        };

        public BLoggerNLog(string logBaseFilename, bool logToConsole = false, bool logToFile = false) {
            var config = new NLog.Config.LoggingConfiguration();

            if (logToConsole) {
                var logconsole = new NLog.Targets.ConsoleTarget("logconsole") {
                    // Remove the long name of this modules from the output
                    // Layout = "${longdate}|${level: uppercase = true}||${message: withexception = true}"
                    Layout = "${longdate}|${level: uppercase = true}||${message}"
                };
                config.AddRule(LogLevel.Trace, LogLevel.Fatal, logconsole);
            }

            if (logToFile) {
                // https://github.com/NLog/NLog/wiki/Configure-from-code
                var logfile = new NLog.Targets.FileTarget("logfile") {
                    // https://github.com/nlog/nlog/wiki/File-target
                    FileName = logBaseFilename ?? "${basedir}/Logs/logfile.log",
                    CreateDirs = true,
                    // Layout = "${longdate}|${level: uppercase = true}||${message: withexception = true}",
                    Layout = "${longdate}|${level: uppercase = true}||${message}",
                    LineEnding = NLog.Targets.LineEndingMode.LF,
                    // https://github.com/nlog/nlog/wiki/FileTarget-Archive-Examples#archive-numbering-examples
                    MaxArchiveFiles = 5,
                    ArchiveFileName = "logfile.{#}.log",
                    ArchiveNumbering = NLog.Targets.ArchiveNumberingMode.DateAndSequence,
                    ArchiveEvery = NLog.Targets.FileArchivePeriod.Day,
                    ArchiveAboveSize = 1000,
                    ArchiveDateFormat = "yyyyMMdd"
                };
                // Rules for mapping loggers to targets
                config.AddRule(LogLevel.Trace, LogLevel.Fatal, logfile);
            }

            NLog.LogManager.Configuration = config;

            _logger = NLog.LogManager.GetCurrentClassLogger();

        }

        public void SetLogLevel(LogLevels pLevel) {
            _logLevel = pLevel;
        }
        private void DoLog(LogLevels pLevel, string pMsg, params Object[] pArgs) {
            _logger.Log(BLoggerNLog.LogLevelMap[pLevel], pMsg, pArgs);
        }
        public void Trace(string pMsg, params Object[] pArgs) {
            if (_logLevel == LogLevels.Trace || _logLevel == LogLevels.Debug) {
                DoLog(LogLevels.Trace, pMsg, pArgs);
            }
        }
        public void Debug(string pMsg, params Object[] pArgs) {
            if (_logLevel == LogLevels.Debug) {
                DoLog(LogLevels.Debug, pMsg, pArgs);
            }
        }
        public void Info(string pMsg, params Object[] pArgs) {
            DoLog(LogLevels.Information, pMsg, pArgs);
        }
        public void Warn(string pMsg, params Object[] pArgs) {
            DoLog(LogLevels.Warning, pMsg, pArgs);
        }
        public void Error(string pMsg, params Object[] pArgs) {
            DoLog(LogLevels.Error, pMsg, pArgs);
        }
    }
}
