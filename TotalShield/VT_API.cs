using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows.Forms;
using VirusTotalNet;
using VirusTotalNet.Objects;
using VirusTotalNet.ResponseCodes;
using VirusTotalNet.Results;


namespace TotalShield
{
    public static class VT_API
    {
        static readonly int bigtimeout = 30000;

        private static readonly HttpClient client = new HttpClient();

        private static readonly string vt_report_url = "https://www.virustotal.com/ui/files/";
        private static readonly string vt_report_useragent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/84.0.4147.135 Safari/537.36 OPR/70.0.3728.189";
        

        public static string BytesToHexString(byte[] bytes)
        {
            string result = "";
            foreach (byte b in bytes) result += b.ToString("x2");
            return result;
        }

        public static async Task<string> GetReportData(string hash)
        {
            try
            {
                client.DefaultRequestHeaders.UserAgent.ParseAdd(vt_report_useragent);
                string responseString = await client.GetStringAsync(vt_report_url + hash);
                return responseString;
            }
            catch
            {
                return null;
            }
        }

        public static bool IsThreat(AV_Report report)
        {
            foreach (var item in report.av_results)
            {
                if (!item.result.Equals("Clean"))
                    return true;
            }

            return false;
        }

        public static async Task<AV_Report> ScanFile(string filepath, string key, List<string> active_avs, bool queued, bool ispremium, Scan scanform)
        {

            AV_Report avreport = new AV_Report();

            VirusTotal virusTotal = new VirusTotal(key);
            List<AV_Result> results = new List<AV_Result>();
            virusTotal.UseTLS = true;
            byte[] filebytes = File.ReadAllBytes(filepath);
            string filehash;
            bool report_fail;
            using (SHA256 sha256obj = SHA256.Create())
            {

                filehash = BytesToHexString(sha256obj.ComputeHash(filebytes));
            }
            FileReport obj_report = null;
            dynamic json_report = null;



            while (true)
            {
                try
                {
                    if (ispremium)
                    {
                        obj_report = await virusTotal.GetFileReportAsync(filebytes);
                        report_fail = obj_report.ResponseCode != FileReportResponseCode.Present;
                    }
                    else
                    {
                        string response_str = await GetReportData(filehash);
                    

                        if (response_str != null)
                            json_report = JsonConvert.DeserializeObject(response_str);

                        report_fail = response_str == null || json_report.error != null;
                    }



                    if (report_fail)
                    {

                        if (queued)
                        {
                            await Task.Delay(bigtimeout);
                            continue;
                        }
                        else
                        {

                            await virusTotal.ScanFileAsync(filepath);
                            return null;
                        }
                    }


                    avreport.file = filepath;
                    avreport.hash = filehash;
                    var culture = new CultureInfo("ro-RO");
                    DateTime localDate = DateTime.Now;

                    avreport.time = localDate.ToString(culture);


                    if (ispremium)
                    {
                        foreach (var item in active_avs)
                        {


                            ScanEngine scan = obj_report.Scans[item];


                            AV_Result result = new AV_Result();

                            result.av_name = item;


                            if (scan.Detected)
                            {
                                result.result = scan.Result;
                            }
                            else
                            {
                                result.result = "Clean";
                            }

                            results.Add(result);

                        }
                    }
                    else
                    {
                        dynamic scanresults = json_report.data.attributes.last_analysis_results;

                        foreach (var item in active_avs)
                        {
                            dynamic avresult = scanresults[item];

                            AV_Result result = new AV_Result();

                            result.av_name = item;


                            if (avresult.category == "malicious" && avresult.result != null)
                            {
                                result.result = avresult.result;
                            }
                            else
                            {
                                result.result = "Clean";
                            }

                            results.Add(result);

                        }
                    }
                    avreport.av_results = results;
                    return avreport;
                }
                catch (Exception)
                {

                    bool res = await Settings.IsInternetAvailable();
                    if (!res)
                    {
                        if (scanform.net_msg != null)
                        {
                            scanform.net_msg.Close();
                        }
                        scanform.net_msg = new TotalMessage("Scan Error", "No internet connection :( ", MessageBoxButtons.OK);
                        scanform.net_msg.ShowDialog();
                        scanform.net_msg = null;
                    }

                    await Task.Delay(bigtimeout);
                    continue;
                }


            }


        }


        public static async Task<bool> IsKeyValid(string key, bool premium)
        {
            try
            {

                VirusTotal virusTotal = new VirusTotal(key);
                virusTotal.UseTLS = true;
                byte[] virus = new byte[1];
                virus[0] = 0x20;


                FileReport report = await virusTotal.GetFileReportAsync(virus);
                if (premium == true)
                {
                    FileReport report1 = await virusTotal.GetFileReportAsync(virus);
                    FileReport report2 = await virusTotal.GetFileReportAsync(virus);
                    FileReport report3 = await virusTotal.GetFileReportAsync(virus);
                    FileReport report4 = await virusTotal.GetFileReportAsync(virus);
                    FileReport report5 = await virusTotal.GetFileReportAsync(virus);
                }
                return true;


            }
            catch (Exception)
            {
                return false;
            }

        }


    }
}
