using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Html2Pdf.Lib
{
    public static class TheMagic
    {
        public static async Task<byte[]> Go(string url, int timeoutInSeconds = 30, string pathToExe = null)
        {
            if (pathToExe == null)
            {
                pathToExe = $@"{Path.GetDirectoryName(typeof(TheMagic).Assembly.Location)}\wkhtmltopdf.exe";
                if (!File.Exists(pathToExe))
                {
                    pathToExe = HttpContext.Current.Server.MapPath("~/bin/wkhtmltopdf.exe");
                }
            }

            var timeout = DateTime.UtcNow.AddSeconds(timeoutInSeconds);
            var savePdfTo = Path.GetTempFileName();
            var t = Task.Run(() => GeneratePdf(url, savePdfTo, pathToExe));
            while (!t.IsCompleted)
            {
                if (timeout < DateTime.UtcNow)
                {
                    break;
                }
                await Task.Delay(250);
            }
            while (!File.Exists(savePdfTo))
            {
                if (timeout < DateTime.UtcNow)
                {
                    break;
                }
                await Task.Delay(250);
            }
            while (File.GetLastWriteTimeUtc(savePdfTo).AddSeconds(2) >= DateTime.UtcNow)
            {
                if (timeout < DateTime.UtcNow)
                {
                    break;
                }
                await Task.Delay(250);
            }
            var bytes = File.ReadAllBytes(savePdfTo);
            try
            {
                File.Delete(savePdfTo);
            }
            catch
            {
                // oh well we tried
            }
            return bytes;
        }

        private static void GeneratePdf(string url, string targetLocation, string pathToExe)
        {
            ExecuteCommand(pathToExe, $@"""{url}"" ""{targetLocation}""");
        }

        public static string ExecuteCommand(string pathToExe, string args)
        {
            try
            {
                System.Diagnostics.ProcessStartInfo procStartInfo = new System.Diagnostics.ProcessStartInfo(pathToExe, args);
                procStartInfo.UseShellExecute = false;
                procStartInfo.CreateNoWindow = true;
                System.Diagnostics.Process proc = new System.Diagnostics.Process();
                procStartInfo.RedirectStandardOutput = true;
                proc.StartInfo = procStartInfo;
                proc.Start();
                proc.WaitForExit();
            }
            catch
            {
            }
            return null;
        }
    }
}
