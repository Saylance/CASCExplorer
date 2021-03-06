﻿using System.IO;
using System.Net;

namespace CASCExplorer
{
    public class SyncDownloader
    {
        AsyncAction progressReporter;

        public SyncDownloader(AsyncAction progressReporter)
        {
            this.progressReporter = progressReporter;
        }

        public void DownloadFile(string url, string path)
        {
            HttpWebRequest request = WebRequest.CreateHttp(url);
            HttpWebResponse resp = (HttpWebResponse)request.GetResponse();

            using (Stream stream = resp.GetResponseStream())
            using (Stream fs = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                CopyToStream(stream, fs, resp.ContentLength);
            }
        }

        public MemoryStream OpenFile(string url)
        {
            HttpWebRequest request = WebRequest.CreateHttp(url);
            HttpWebResponse resp = (HttpWebResponse)request.GetResponse();

            using (Stream stream = resp.GetResponseStream())
            {
                MemoryStream ms = new MemoryStream();

                CopyToStream(stream, ms, resp.ContentLength);

                ms.Position = 0;
                return ms;
            }
        }

        private void CopyToStream(Stream src, Stream dst, long len)
        {
            long done = 0;

            byte[] buf = new byte[0x1000];

            int count;
            do
            {
                if (progressReporter != null && progressReporter.IsCancellationRequested)
                    return;

                count = src.Read(buf, 0, buf.Length);
                dst.Write(buf, 0, count);

                done += count;

                if (progressReporter != null)
                    progressReporter.ReportProgress((int)((float)done / (float)len * 100.0f));
            } while (count > 0);
        }
    }
}
