using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace SFTPFileUpload
{
    
    public static class Logger
    {

        public static void WriteLog(string message, string path, string logFilename)
        {
            string clintList = ConfigurationManager.AppSettings["ClientList"];
            string toEmail = ConfigurationManager.AppSettings[clintList + "ToEmail"];
            string FromEmail = ConfigurationManager.AppSettings[clintList + "FromEmail"];
            string BccEmail = ConfigurationManager.AppSettings[clintList + "BccEmail"];
            string Strsmtp = ConfigurationManager.AppSettings[clintList + "SMTP"];
            try
            {
                string pathNM = path + logFilename;
                using (StreamWriter writer = new StreamWriter(pathNM, true))
                {
                    writer.WriteLine($"{DateTime.Now} ~ {message}");
                }
            }
            catch (Exception exp)
            {
                // SendEmail(exp.ToString(), "Error writing Logs!");
                SendEmail("There is Some issue for WriteLog \n" + exp.Message, "Error writing Logs! " + clintList, Strsmtp, FromEmail, toEmail, BccEmail);
            }
        }

        public static void ErrorLog(string message)
        {
            string errorLogPath = ConfigurationManager.AppSettings["errorLogPath"];
            using (StreamWriter writer = new StreamWriter(errorLogPath, true))
            {
                writer.WriteLine($"{DateTime.Now} ~ {message}");
            }
        }
        public static void SendEmail(string msgBody, string subject, string SmtpName, string fromEmail, string ToEmail, string bccEmail)
        {
            string currentDateTm = DateTime.Now.Date.ToString("yyyyMMdd");
            string logPath = ConfigurationManager.AppSettings["logPath"] + currentDateTm + "_";
            try
            {
                //MailMessage message = new MailMessage();

                MailMessage message = new MailMessage(fromEmail, ToEmail, subject, msgBody);
                AlternateView htmlView = AlternateView.CreateAlternateViewFromString(msgBody, null, "text/html");
                message.AlternateViews.Add(htmlView);
                SmtpClient client = new SmtpClient(SmtpName);

                message.Bcc.Add(bccEmail);

                client.UseDefaultCredentials = true;
                client.Send(message);

            }
            catch (Exception exp)
            {
                Logger.WriteLog(exp.Message, logPath, "EmailErrorLogs.txt");
            }
        }

        public static string EmailSuccessBody(string clientCode)
        {
            string currentDateTm = DateTime.Now.Date.ToString("yyyyMMdd");
            string logErPath = ConfigurationManager.AppSettings["logPath"] + currentDateTm + "_";
            string logPath = ConfigurationManager.AppSettings["logPath"] + currentDateTm + "_" + clientCode + "_NewFilesLogs.txt";
            //string logPath = "D:\\Pradeep\\SFTPLogs\\20220208_ContractPodAI_NewFilesLogs.txt";
            try
            {


                DataTable dt = new DataTable();
                string messageBody = "";
                if (File.Exists(logPath))
                {

                    using (System.IO.TextReader tr = File.OpenText(logPath))
                    {
                        string line;
                        while ((line = tr.ReadLine()) != null)
                        {

                            string[] items = line.Trim().Split('~');
                            if (dt.Columns.Count == 0)
                            {
                                // Create the data columns for the data table based on the number of items
                                // on the first line of the file
                                for (int i = 0; i < items.Length; i++)
                                    dt.Columns.Add(new DataColumn("Column" + i, typeof(string)));
                            }

                            if (line == "")
                            { }//this is for ignoring blank line, if we will not ignore this error will come while adding in datatable
                            else
                            {
                                Console.WriteLine(items[1].ToString());
                                dt.Rows.Add(items);
                            }

                        }

                    }
                    //string messageBody = "";
                    if (dt.Rows.Count == 0)
                    {
                        messageBody = "<font>No new or updated files or folder found !!! </font><br><br>";
                        return messageBody;
                    }
                    else
                    {
                        messageBody = "<font>The following are the records: </font><br><br>";
                        string htmlTableStart = "<table style=\"border-collapse:collapse; text-align:center;\" >";
                        string htmlTableEnd = "</table>";
                        string htmlHeaderRowStart = "<tr style=\"background-color:#6FA1D2; color:#ffffff;\">";
                        string htmlHeaderRowEnd = "</tr>";
                        string htmlTrStart = "<tr style=\"color:#555555;\">";
                        string htmlTrEnd = "</tr>";
                        string htmlTdStart = "<td style=\" border-color:#5c87b2; border-style:solid; border-width:thin; padding: 5px;\">";
                        string htmlTdEnd = "</td>";
                        messageBody += htmlTableStart;
                        messageBody += htmlHeaderRowStart;
                        messageBody += htmlTdStart + "Date" + htmlTdEnd;
                        messageBody += htmlTdStart + "Files Name" + htmlTdEnd;
                        messageBody += htmlHeaderRowEnd;

                        //Loop all the rows from grid vew and added to html td 

                        foreach (DataRow dr in dt.Rows)
                        {
                            messageBody = messageBody + htmlTrStart;
                            messageBody = messageBody + htmlTdStart + dr[0].ToString() + htmlTdEnd; //adding Date  
                            messageBody = messageBody + htmlTdStart + dr[1].ToString() + htmlTdEnd; //adding Files Name  

                            messageBody = messageBody + htmlTrEnd;

                        }

                        messageBody = messageBody + htmlTableEnd;
                        messageBody = messageBody + "<br> Thanks \n <br>Axway FileTransfer Team \n";
                        return messageBody;
                    }
                }
                else
                {
                    return "Today As of now No File found for Upload !!! <br> <br>Thanks \n <br> Axway FileTransfer Team \n";
                }
            }
            catch (Exception exp)
            {
                Logger.WriteLog(exp.Message, logErPath, "EmailErrorLogs.txt");
                return "";
            }


        }
    }
}
