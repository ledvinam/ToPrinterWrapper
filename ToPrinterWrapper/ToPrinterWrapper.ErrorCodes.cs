using System.ComponentModel;

namespace ToPrinterWrapper
{
    public static class ErrorCodes
    {
        [Description("Ok")]
        public const int Ok = 0;
        [Description("Error")]
        public const int Error = 1;
        [Description("Printer not found")]
        public const int PrinterNotFound = 1000;
        [Description("Printer Offline")]
        public const int PrinterOffline = 1001;
        [Description("All actions failed")]        
        public const int AllActionsFailed = 1094796908;
        [Description("Access denied")]
        public const int AccessDenied = 1094927438;
        [Description("Append file(s) missing")]
        public const int AppendFilesMissing = 1095126377;
        [Description("User abort")]
        public const int UserAbort = 1096970868;
        [Description("All documents print error")]
        public const int AllDocumentsPrintError = 1097101381;
        [Description("Application is not installed")]
        public const int ApplicationNotInstalled = 1097879113;
        [Description("Bad filename")]
        public const int BadFilename = 1111903841;
        [Description("Bad or missing settings file")]
        public const int BadOrMissingSettingsFile = 1112493395;
        [Description("Bad document file")]
        public const int BadDocumentFile = 1113670726;
        [Description("Bad file list")]
        public const int BadFileList = 1113671276;
        [Description("Bad data")]
        public const int BadData = 1113678916;
        [Description("Bad image file")]
        public const int BadImageFile = 1113678921;
        [Description("Bad number of parameters")]
        public const int BadNumberOfParameters = 1113678926;
        [Description("Bad open")]
        public const int BadOpen = 1113678927;
        [Description("Bad image thumbnail")]
        public const int BadImageThumbnail = 1113678932;
        [Description("Border not found")]
        public const int BorderNotFound = 1114787430;
        [Description("Can't create file")]
        public const int CantCreateFile = 1128482409;
        [Description("Can't create output folder")]
        public const int CantCreateOutputFolder = 1128484678;
        [Description("Condition is false")]
        public const int ConditionIsFalse = 1128875621;
        [Description("Condition is true")]
        public const int ConditionIsTrue = 1128879205;
        [Description("Connect failed")]
        public const int ConnectFailed = 1131300460;
        [Description("Canceled by user")]
        public const int CanceledByUser = 1131307884;
        [Description("COM Server creation failed")]
        public const int ComServerCreationFailed = 1131375939;
        [Description("COM exception")]
        public const int ComException = 1131375941;
        [Description("COM Server configuration failed")]
        public const int ComServerConfigurationFailed = 1131375943;
        [Description("COM Server lost")]
        public const int ComServerLost = 1131375955;
        [Description("Delete document error")]
        public const int DeleteDocumentError = 1145324914;
        [Description("Document conversion error")]
        public const int DocumentConversionError = 1147366981;
        [Description("DDE error")]
        public const int DdeError = 1147430213;
        [Description("Different permission")]
        public const int DifferentPermission = 1147758160;
        [Description("Disabled feature")]
        public const int DisabledFeature = 1147761478;
        [Description("Create dispatch error")]
        public const int CreateDispatchError = 1148416069;
        [Description("End of input stream")]
        public const int EndOfInputStream = 1162824019;
        [Description("Execute script error")]
        public const int ExecuteScriptError = 1163096645;
        [Description("Empty list")]
        public const int EmptyList = 1164801100;
        [Description("Empty range")]
        public const int EmptyRange = 1164801106;
        [Description("Exe launch error")]
        public const int ExeLaunchError = 1165511749;
        [Description("File already exists")]
        public const int FileAlreadyExists = 1178682744;
        [Description("File access denied")]
        public const int FileAccessDenied = 1178690372;
        [Description("File converting failed")]
        public const int FileConvertingFailed = 1178814049;
        [Description("File extension isn't in list")]
        public const int FileExtensionNotInList = 1178945865;
        [Description("File format not supported")]
        public const int FileFormatNotSupported = 1179012691;
        [Description("File has zero length")]
        public const int FileHasZeroLength = 1179146828;
        [Description("File is encrypted but page isn't")]
        public const int FileIsEncryptedButPageIsnt = 1179206978;
        [Description("File IO error")]
        public const int FileIoError = 1179209541;
        [Description("File loading failed")]
        public const int FileLoadingFailed = 1179403873;
        [Description("File list is empty")]
        public const int FileListIsEmpty = 1179404613;
        [Description("Failed to load product DLL")]
        public const int FailedToLoadProductDll = 1179406404;
        [Description("File not exists")]
        public const int FileNotExists = 1179534712;
        [Description("File not found")]
        public const int FileNotFound = 1179534916;
        [Description("Files not found")]
        public const int FilesNotFound = 1179864646;
        [Description("File type disabled")]
        public const int FileTypeDisabled = 1179927657;
        [Description("Folder watcher error")]
        public const int FolderWatcherError = 1181505349;
        [Description("Ftp error")]
        public const int FtpError = 1182036037;
        [Description("Http error")]
        public const int HttpError = 1215590469;
        [Description("Incompatible app version")]
        public const int IncompatibleAppVersion = 1229025366;
        [Description("Invalid conversion")]
        public const int InvalidConversion = 1229155958;
        [Description("Invalid data exchange ptr")]
        public const int InvalidDataExchangePtr = 1229210960;
        [Description("Image data missing")]
        public const int ImageDataMissing = 1229213033;
        [Description("Internal error")]
        public const int InternalError = 1229288050;
        [Description("Unsupported format for app")]
        public const int UnsupportedFormatForApp = 1229342273;
        [Description("Input file(s) missing")]
        public const int InputFilesMissing = 1229344105;
        [Description("Invalid file size")]
        public const int InvalidFileSize = 1229345658;
        [Description("Item not found")]
        public const int ItemNotFound = 1229866606;
        [Description("Invalid printer name")]
        public const int InvalidPrinterName = 1229999713;
        [Description("Incorrect password")]
        public const int IncorrectPassword = 1230009207;
        [Description("Internet error")]
        public const int InternetError = 1231971653;
        [Description("Invalid operation")]
        public const int InvalidOperation = 1231976015;
        [Description("Invalid parameter")]
        public const int InvalidParameter = 1231976016;
        [Description("Installation error")]
        public const int InstallationError = 1232303173;
        [Description("Launch script error")]
        public const int LaunchScriptError = 1280537157;
        [Description("Move from input folder error")]
        public const int MoveFromInputFolderError = 1296451910;
        [Description("Maximum file size exceeded")]
        public const int MaximumFileSizeExceeded = 1296454469;
        [Description("Maximum file size exceeded & truncated")]
        public const int MaximumFileSizeExceededTruncated = 1297303124;
        [Description("Not initialized")]
        public const int NotInitialized = 1313435241;
        [Description("No pages in range")]
        public const int NoPagesInRange = 1313884498;
        [Description("No printers found")]
        public const int NoPrintersFound = 1313895270;
        [Description("Needs password")]
        public const int NeedsPassword = 1313895287;
        [Description("Object is off")]
        public const int ObjectIsOff = 1330204518;
        [Description("Out of memory")]
        public const int OutOfMemory = 1330472301;
        [Description("Open printer error")]
        public const int OpenPrinterError = 1330660722;
        [Description("Output stream overflow")]
        public const int OutputStreamOverflow = 1330859862;
        [Description("Object busy")]
        public const int ObjectBusy = 1331839609;
        [Description("Plugin launch error")]
        public const int PluginLaunchError = 1347175794;
        [Description("Plugin may be damaged")]
        public const int PluginMayBeDamaged = 1347240516;
        [Description("Plugin restart request")]
        public const int PluginRestartRequest = 1347572337;
        [Description("Plugin terminated unexpectedly")]
        public const int PluginTerminatedUnexpectedly = 1347704185;
        [Description("Plugin internal error")]
        public const int PluginInternalError = 1348946245;
        [Description("Plugin parameters error")]
        public const int PluginParametersError = 1348948037;
        [Description("Plugin timeout")]
        public const int PluginTimeout = 1348949097;
        [Description("Process busy")]
        public const int ProcessBusy = 1349665397;
        [Description("Printer error")]
        public const int PrinterError = 1349666162;
        [Description("Print error")]
        public const int PrintError = 1349676613;
        [Description("Queue extract error")]
        public const int QueueExtractError = 1363494258;
        [Description("Queue is empty")]
        public const int QueueIsEmpty = 1363768133;
        [Description("Rules chain break by condition")]
        public const int RulesChainBreakByCondition = 1380139586;
        [Description("Warning: Rule disabled")]
        public const int WarningRuleDisabled = 1380215127;
        [Description("Read error")]
        public const int ReadError = 1380282994;
        [Description("Warning: Rule idle")]
        public const int WarningRuleIdle = 1380541548;
        [Description("Rule load error")]
        public const int RuleLoadError = 1380730226;
        [Description("Reject not encrypt file password")]
        public const int RejectNotEncryptFilePassword = 1380861254;
        [Description("Warning: Rule paused")]
        public const int WarningRulePaused = 1381201008;
        [Description("User retry")]
        public const int UserRetry = 1383363193;
        [Description("Some actions failed")]
        public const int SomeActionsFailed = 1396786796;
        [Description("Solid border color")]
        public const int SolidBorderColor = 1396863555;
        [Description("Shell exec error")]
        public const int ShellExecError = 1397048690;
        [Description("Stamp file loading failed")]
        public const int StampFileLoadingFailed = 1397115974;
        [Description("Size or position mismatch")]
        public const int SizeOrPositionMismatch = 1397706829;
        [Description("Set selected printer error")]
        public const int SetSelectedPrinterError = 1397968965;
        [Description("Some documents print error")]
        public const int SomeDocumentsPrintError = 1399091269;
        [Description("Thumbnail missing")]
        public const int ThumbnailMissing = 1414359411;
        [Description("Thumbnail not supported")]
        public const int ThumbnailNotSupported = 1414360691;
        [Description("Unsupported app version")]
        public const int UnsupportedAppVersion = 1430351958;
        [Description("Undefined error")]
        public const int UndefinedError = 1430614642;
        [Description("Unsupported Windows version")]
        public const int UnsupportedWindowsVersion = 1431197526;
        [Description("Unsupported encoding")]
        public const int UnsupportedEncoding = 1432634734;
        [Description("Unsupported document type")]
        public const int UnsupportedDocumentType = 1433289812;
        [Description("Unknown encoding")]
        public const int UnknownEncoding = 1433290094;
        [Description("Unknown plugin")]
        public const int UnknownPlugin = 1433292903;
        [Description("Undefined action")]
        public const int UndefinedAction = 1433297985;
        [Description("Unsupported feature")]
        public const int UnsupportedFeature = 1433301830;
        [Description("Unsupported file type")]
        public const int UnsupportedFileType = 1433618004;
        [Description("Write error")]
        public const int WriteError = 1464169074;
        [Description("Plugin settings file read error")]
        public const int PluginSettingsFileReadError = 1885819730;
        [Description("Plugin settings file write error")]
        public const int PluginSettingsFileWriteError = 1885819735;

        public static string ToDescription(this int errorCode)
        {
            var type = typeof(ErrorCodes);
            foreach (var field in type.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static))
            {
                if (field.FieldType == typeof(int))
                {
                    var value = field.GetValue(null);
                    if (value is int intValue && intValue == errorCode)
                    {
                        var attr = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;
                        if (attr != null)
                            return attr.Description;
                    }
                }
            }
            return $"Unknown error code: {errorCode}";
        }
    }
}
