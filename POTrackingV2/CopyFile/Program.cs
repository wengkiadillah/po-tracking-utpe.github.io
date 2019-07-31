using System;
using System.Collections.Generic;
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
                string source = @WebConfigurationManager.AppSettings["Source"];
                string destination = @WebConfigurationManager.AppSettings["Destination"];
                string filename = @WebConfigurationManager.AppSettings["FileName"];

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

                var directory = new DirectoryInfo(source);
                var target = new DirectoryInfo(destination);
                var myFile = (from f in directory.GetFiles()
                              orderby f.LastWriteTime descending
                              select f).First();
                
                //foreach (FileInfo fi in directory.GetFiles())
                //{
                //    Console.WriteLine(@"Read File {0}\{1}", target.FullName, fi.Name);
                //    //fi.CopyTo(Path.Combine(target.ToString(), fi.Name), true);
                //}

                string destFile = Path.Combine(destination, filename);
                Console.WriteLine(@"Copying {0}\{1}", target.FullName, myFile.Name);
                System.IO.File.Copy(myFile.FullName, destFile, true);

                // For FTP 
                //CopyFile("test.csv", "test.csv", "administrator", "P@s5w0rd");

                Console.WriteLine(@"Success !");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Source + ex.StackTrace);
                //Console.ReadLine();
            }
        }
        

        public static bool CopyFile(string fileName, string FileToCopy, string userName, string password)
        {
            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://10.48.10.116/MCS" + fileName);
                request.Method = WebRequestMethods.Ftp.DownloadFile;

                request.Credentials = new NetworkCredential(userName, password);
                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                Stream responseStream = response.GetResponseStream();
                Upload("ftp://10.48.10.116/MCS" + FileToCopy, ToByteArray(responseStream), userName, password);
                responseStream.Close();
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
