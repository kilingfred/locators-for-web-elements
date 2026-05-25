using System;
using System.IO;
using System.Threading;

namespace Base.Utils
{
    public static class FileHelper
    {
        public static bool WaitForFileCreatedAndStable(string filePath, TimeSpan timeout)
        {
            var startTime = DateTime.Now;
            long lastSize = -1;

            while (DateTime.Now - startTime < timeout)
            {
                if (File.Exists(filePath))
                {
                    // Check if file size is stable (indicates download is finished)
                    long currentSize = new FileInfo(filePath).Length;
                    if (currentSize > 0 && currentSize == lastSize)
                    {
                        return true;
                    }
                    lastSize = currentSize;
                }
                Thread.Sleep(500); // Poll every half-second
            }
            return false;
        }
    }
}