using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Configuration;

namespace CopyFile
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                //string source = @WebConfigurationManager.AppSettings["Source"];
                //string destination = @WebConfigurationManager.AppSettings["Destination"];
                string filename = @WebConfigurationManager.AppSettings["FileName"];
                string ftpSource = @WebConfigurationManager.AppSettings["FTPSource"];
                string ftpDestination = @WebConfigurationManager.AppSettings["FTPDestination"];
                string syncDestination = @WebConfigurationManager.AppSettings["SyncDestination"];
                string fileToCopy = @WebConfigurationManager.AppSettings["FileToCopy"];

                //private string iisAppName = WebConfigurationManager.AppSettings["IISAppName"];

                //string newFolder = "NewlyAdded";
                //string path = System.IO.Path.Combine(
                //   Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                //   newFolder
                //);

                //if (!System.IO.Directory.Exists(path))
                //{
                //    System.IO.Directory.CreateDirectory(path);
                //}

                //if (!System.IO.Directory.Exists(destination))
                //{
                //    System.IO.Directory.CreateDirectory(destination);
                //}

                // Copy Files
                //var directory = new DirectoryInfo(source);
                //var target = new DirectoryInfo(destination);
                //var myFile = (from f in directory.GetFiles()
                //              orderby f.LastWriteTime descending
                //              select f).First();

                //foreach (FileInfo fi in directory.GetFiles())
                //{
                //    Console.WriteLine(@"Read File {0}\{1}", target.FullName, fi.Name);
                //    //fi.CopyTo(Path.Combine(target.ToString(), fi.Name), true);
                //}

                // Copy Files (2)
                //string destFile = Path.Combine(destination, filename);
                //Console.WriteLine(@"Copying {0}\{1}", target.FullName, myFile.Name);
                //System.IO.File.Copy(myFile.FullName, destFile, true);

                // Versioning
                int fileVersion = Int32.Parse(@WebConfigurationManager.AppSettings["FileVersion"]);
                fileVersion = fileVersion + 1;

                string appPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                string configFile = System.IO.Path.Combine(appPath, "CopyFile.exe.config");
                ExeConfigurationFileMap configFileMap = new ExeConfigurationFileMap();
                configFileMap.ExeConfigFilename = configFile;
                System.Configuration.Configuration config = ConfigurationManager.OpenMappedExeConfiguration(configFileMap, ConfigurationUserLevel.None);

                config.AppSettings.Settings["FileVersion"].Value = fileVersion.ToString();
                config.Save();

                // For FTP 
                CopyFile(filename, fileToCopy, "administrator", "P@s5w0rd", fileVersion.ToString(), ftpSource, ftpDestination, syncDestination);

                Console.WriteLine(@"Success !");
                //Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Source + ex.StackTrace);
                //Console.ReadLine();
            }
        }


        public static bool CopyFile(string fileName, string fileToCopy, string userName, string password, string fileVersion,string ftpSource, string ftpDestination, string syncDestination)
        {
            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftpSource + fileName);
                request.Method = WebRequestMethods.Ftp.DownloadFile;
                request.Credentials = new NetworkCredential(userName, password);

                string fileNameWithoutExetension = fileName.Split('.').First();

                using (Stream ftpStream = request.GetResponse().GetResponseStream())
                {
                    // Copy to History
                    using (Stream fileStream = File.Create($"{ftpDestination}{fileNameWithoutExetension}_{fileVersion}.csv"))
                    {
                        ftpStream.CopyTo(fileStream);
                    }
                }

                // Copy to Data
                var directory = new DirectoryInfo(ftpDestination);
                var target = new DirectoryInfo(syncDestination);
                var myFile = (from f in directory.GetFiles()
                              orderby f.LastWriteTime descending
                              select f).First();

                string destFile = Path.Combine(syncDestination, fileToCopy);
                System.IO.File.Copy(myFile.FullName, destFile, true);

                //Upload("ftp://10.48.10.116/MCS" + FileToCopy, ToByteArray(responseStream), userName, password);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public static Byte[] ToByteArray(Stream stream)
        {
            MemoryStream ms = new MemoryStream();
            byte[] chunk = new byte[4096];
            int bytesRead;
            while ((bytesRead = stream.Read(chunk, 0, chunk.Length)) > 0)
            {
                ms.Write(chunk, 0, bytesRead);
            }

            return ms.ToArray();
        }

        public static bool Upload(string FileName, byte[] Image, string FtpUsername, string FtpPassword)
        {
            try
            {
                System.Net.FtpWebRequest clsRequest = (System.Net.FtpWebRequest)System.Net.WebRequest.Create(FileName);
                clsRequest.Credentials = new System.Net.NetworkCredential(FtpUsername, FtpPassword);
                clsRequest.Method = System.Net.WebRequestMethods.Ftp.UploadFile;
                System.IO.Stream clsStream = clsRequest.GetRequestStream();
                clsStream.Write(Image, 0, Image.Length);

                clsStream.Close();
                clsStream.Dispose();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
