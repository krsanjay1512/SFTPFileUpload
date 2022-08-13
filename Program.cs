using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Renci.SshNet;

namespace SFTPFileUpload
{   

    class Program
    {

        static void UploadFileOnSFTP(string clinetName)
        {
            #region Emails related variable
            string toEmail = ConfigurationManager.AppSettings[clinetName + "ToEmail"];
            string FromEmail = ConfigurationManager.AppSettings[clinetName + "FromEmail"];
            string BccEmail = ConfigurationManager.AppSettings[clinetName + "BccEmail"];
            string Strsmtp = ConfigurationManager.AppSettings[clinetName + "SMTP"];
            #endregion

            #region SFTP related variable
            string sftpRoot_Path = ConfigurationManager.AppSettings[clinetName + "SFTP_path"];
            string mySourceLocation = ConfigurationManager.AppSettings["mySourceLocation"];
            string myServer = ConfigurationManager.AppSettings[clinetName + "Server"];
            //int myPort = int.Parse(ConfigurationManager.AppSettings["Port"]);
            string myUser = ConfigurationManager.AppSettings[clinetName + "User"];
            string myPassword = ConfigurationManager.AppSettings[clinetName + "Password"];
            #endregion

            string[] folders = Directory.GetFileSystemEntries(mySourceLocation, "*.", SearchOption.AllDirectories);
            //string[] txtfiles = Directory.GetFileSystemEntries(mySourceLocation, "*.txt", SearchOption.AllDirectories);
            string[] allFiles = Directory.GetFiles(mySourceLocation, "*.*", SearchOption.AllDirectories);

            string currentDateTm = DateTime.Now.Date.ToString("yyyyMMdd");
            string logPath = ConfigurationManager.AppSettings["logPath"] + currentDateTm + "_" + clinetName;

            int totalCount_folders = folders.Length;
            int totalCount_entries = allFiles.Length;
            //int totalCount_txtfiles = txtfiles.Length;
            Logger.WriteLog("Found #" + totalCount_entries + " elements which #" + totalCount_folders + " are directories and #" + totalCount_entries + " are files.\n", logPath, "_SuccessLogs.txt");

            Logger.WriteLog("Initializing connection to SFTP\n", logPath, "_SuccessLogs.txt");
            try
            {

                using (SftpClient sftp = new SftpClient(myServer, myUser, myPassword))
                {
                    sftp.Connect();
                    if (sftp.IsConnected)
                    {
                        Logger.WriteLog("Connected to SFTP Server is: \n" + myServer + " and UserName : " + myUser, logPath, "_SuccessLogs.txt");
                        foreach (string folder in folders)
                        {
                            string folderDirectory = Path.GetDirectoryName(folder);
                            folderDirectory = folderDirectory.Replace(mySourceLocation, "").Replace("\\", "/") + "/";
                            string folderName = Path.GetFileName(folder);
                            try
                            {
                                if (folderDirectory == "/")
                                {
                                    //if (sftp.Exists(sftpRoot_Path + folderDirectory + sftpRoot_Path + folderName))
                                    if (sftp.Exists(sftpRoot_Path + folderName))
                                    {
                                        Logger.WriteLog("Directory already exists on SFTP.\n" + folderDirectory + "/" + folderName, logPath, "_SuccessLogs.txt");
                                    }
                                    else
                                    {
                                        string folderDirectorio = sftpRoot_Path + folderName;
                                        // string DirectorioProcesados = "/Put" + folderDirectory + folderName;

                                        sftp.CreateDirectory(folderDirectorio);
                                        Logger.WriteLog("Creating directory on SFTP: \n" + folderDirectory + folderName + "\n", logPath, "_SuccessLogs.txt");
                                        Logger.WriteLog(folderDirectory + folderName + "\n", logPath, "_NewFolderLogs.txt");
                                    }
                                }
                                else
                                {
                                    if (sftp.Exists(sftpRoot_Path + folderDirectory + folderName))
                                    {
                                        Logger.WriteLog("Directory already exists on SFTP.\n" + folderDirectory + "/" + folderName, logPath, "_SuccessLogs.txt");
                                    }
                                    else
                                    {
                                        string folderDirectorio = sftpRoot_Path + folderDirectory + folderName;
                                        // string DirectorioProcesados = "/Put" + folderDirectory + folderName;

                                        sftp.CreateDirectory(folderDirectorio);
                                        Logger.WriteLog("Creating directory on SFTP: \n" + folderDirectory + folderName + "\n", logPath, "_SuccessLogs.txt");
                                        Logger.WriteLog(folderDirectory + folderName + "\n", logPath, "_NewFolderLogs.txt");
                                    }
                                }
                            }
                            catch (Exception exp)
                            {
                                Logger.WriteLog("Error, can't connect to the server " + myServer + exp.Message + ".\n", logPath, "_ErrorLogs.txt");
                                Logger.WriteLog("Error, can't connect to the server " + myServer + exp.ToString() + ".\n", logPath, "_DetailsErrorLogs.txt");
                            }
                        }
                        foreach (string allfile in allFiles)
                        {
                            FileInfo fileInfo = new FileInfo(allfile);
                            var created = fileInfo.CreationTime;
                            //var directory = new DirectoryInfo(mySourceLocation);
                            //var lastmodifiedFile = directory.GetFiles().OrderByDescending(f => f.LastWriteTime).First();
                            var currentDate = DateTime.Now;
                            //var lastUploaded = fileInfo.LastAccessTime;

                            var lastmodified = fileInfo.LastWriteTime;
                            string txtDirectory = Path.GetDirectoryName(allfile);
                            string txtDirectoryReal = txtDirectory.Replace(mySourceLocation, "").Replace("\\", "/") + "/";
                            string txtName = Path.GetFileName(allfile);
                            var lastUploaded = GetLastRunEntry(clinetName);
                            DateTime lastUploaded1 = new DateTime(1900, 01, 01);
                            try
                            {
                                if (lastUploaded != "")
                                {
                                    lastUploaded1 = Convert.ToDateTime(lastUploaded);
                                }

                                if (txtDirectoryReal == "/")
                                {
                                    if ((lastmodified > lastUploaded1) || (lastUploaded == ""))
                                    {
                                        using (FileStream fileStream = new FileStream(allfile, FileMode.OpenOrCreate))
                                        {
                                            string fnameToUpload = sftpRoot_Path + txtName;//$"{DateTime.Now:yyyy-MM-dd_HH-mm-ss-fff}"
                                            sftp.BufferSize = 4 * 1024;
                                            sftp.UploadFile(fileStream, fnameToUpload, true, null);
                                            Logger.WriteLog("Writing file in : " + fnameToUpload + " and File Size in Bytes :" + fileInfo.Length + "\n", logPath, "_SuccessLogs.txt");
                                            Logger.WriteLog(fnameToUpload + "\n", logPath, "_NewFilesLogs.txt");
                                        }
                                    }

                                    else //(sftp.Exists(sftpRoot_Path + txtDirectoryReal + sftpRoot_Path + txtName))//$"{DateTime.Now:yyyy-MM-dd_HH-mm-ss-fff}"
                                    {
                                        Logger.WriteLog("File already exists on SFTP is\n " + txtName + " and File Size in Bytes: " + fileInfo.Length, logPath, "_SuccessLogs.txt");
                                    }

                                }
                                else
                                {
                                    if ((lastmodified > lastUploaded1) || (lastUploaded == ""))
                                    {
                                        using (FileStream fileStream = new FileStream(allfile, FileMode.OpenOrCreate))
                                        {
                                            string fnameToUpload = sftpRoot_Path + txtDirectoryReal + txtName;//$"{DateTime.Now:yyyy-MM-dd_HH-mm-ss-fff}"
                                            sftp.BufferSize = 4 * 1024;
                                            sftp.UploadFile(fileStream, fnameToUpload, true, null);
                                            Logger.WriteLog("Writing file in : " + fnameToUpload + " and File Size in Bytes :" + fileInfo.Length + "\n", logPath, "_SuccessLogs.txt");
                                            Logger.WriteLog(fnameToUpload, logPath, "_NewFilesLogs.txt");
                                        }
                                    }

                                    else //(sftp.Exists(sftpRoot_Path + txtDirectoryReal + sftpRoot_Path + txtName))//$"{DateTime.Now:yyyy-MM-dd_HH-mm-ss-fff}"
                                    {
                                        Logger.WriteLog("File already exists on SFTP is\n " + txtName + " and File Size in Bytes: " + fileInfo.Length, logPath, "_SuccessLogs.txt");
                                    }

                                }
                            }
                            catch (Exception exp)
                            {
                                Logger.WriteLog("Error, can't connect to the server " + myServer + exp.Message + ".\n", logPath, "_ErrorLogs.txt");
                                Logger.WriteLog("Error, can't connect to the server " + myServer + exp.ToString() + ".\n", logPath, "_DetailsErrorLogs.txt");
                            }
                        }
                        Logger.WriteLog("Files uploaded successfully.\n", logPath, "_SuccessLogs.txt");
                        Logger.WriteLog("Disconnecting from server.\n", logPath, "_SuccessLogs.txt");
                        sftp.Disconnect();
                    }
                    else
                    {
                        Logger.WriteLog("Error, can't connect to the server.\n", logPath, "_SuccessLogs.txt");
                        Logger.WriteLog("Error, can't connect to the server " + myServer + ".\n", logPath, "_ErrorLogs.txt");
                    }
                }
                PutLastRunEntry(clinetName);
                string strEmlBody = Logger.EmailSuccessBody(clinetName);
                Logger.SendEmail(strEmlBody, "Sync(Upload) ran Succesfully for " + clinetName, Strsmtp, FromEmail, toEmail, BccEmail);
            }
            catch (Exception ex)
            {
                //Logger.ErrorLog(ex.Message + ".\n");
                Logger.WriteLog("Error, can't connect to the server " + myServer + ex.Message + ".\n", logPath, "_SuccessLogs.txt");
                Logger.WriteLog("Error, can't connect to the server " + myServer + ex.Message + ".\n", logPath, "_ErrorLogs.txt");
                Logger.WriteLog("Error, can't connect to the server " + myServer + ex.ToString() + ".\n", logPath, "_DetailsErrorLogs.txt");
                Logger.SendEmail("There is Some issue for " + clinetName + "\n" + ex.Message + "< br > < br > Thanks \n < br > Axway FileTransfer Team \n", "Error in SFTP Sync for " + clinetName, Strsmtp, FromEmail, toEmail, BccEmail);
            }

        }

        #region Get&SetLastRunEntry
        static void PutLastRunEntry(string clientCode)
        {
            string currentDateTm = DateTime.Now.Date.ToString("yyyyMMdd");
            string LastRunEntryPath = ConfigurationManager.AppSettings["LastRunEntry"] + clientCode + "_LastRunEntry.txt";

            try
            {
                //string LastRunEntryPath = ConfigurationManager.AppSettings["LastRunEntry"];

                if (File.Exists(LastRunEntryPath))
                {
                    File.Delete(LastRunEntryPath);
                }
                using (StreamWriter writer = new StreamWriter(LastRunEntryPath))
                {
                    writer.WriteLine($"{DateTime.Now}");
                }
            }
            catch (Exception exp)
            {
                Logger.WriteLog("Some issue PutLastRunEntry function  " + exp.Message + ".\n", LastRunEntryPath, "currentDateTm_ErrorLogs.txt");
            }


        }

        static string GetLastRunEntry(string clientCode)
        {
            string currentDateTm = DateTime.Now.Date.ToString("yyyyMMdd");
            string LastRunEntryPath = ConfigurationManager.AppSettings["LastRunEntry"] + clientCode + "_LastRunEntry.txt";
            try
            {
                if (File.Exists(LastRunEntryPath))
                {
                    // Read entire text file content in one string    
                    string text = File.ReadAllText(LastRunEntryPath);
                    return text;
                    Console.WriteLine(text);
                }
                else
                {
                    return "";
                }
            }
            catch (Exception exp)
            {
                Logger.WriteLog("Some issue GetLastRunEntry function  " + exp.Message + ".\n", LastRunEntryPath, "currentDateTm_ErrorLogs.txt");
                return "";
            }
        }
        #endregion

        static void ClientList()
        {
            string clintList = ConfigurationManager.AppSettings["ClientList"];
            string[] arrclientList = clintList.Split(',');
            foreach (string name in arrclientList)
            {
                Console.WriteLine(name);
                UploadFileOnSFTP(name);
                //string strBd = Logger.EmailSuccessBody(name);
            }
        }

        static void Main(string[] args)
        {

            ClientList();
            // testReadFile();


            Console.Write("Press any key to close .\n");
            Console.ReadLine();
        }
    }
}

