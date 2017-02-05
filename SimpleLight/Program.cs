using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace SimpleLight
{
    class Program
    {
      
        #region WinApi
        public static void WriteByte(int pOffset, byte pBytes, IntPtr handle)
        {
            WriteMem(pOffset, BitConverter.GetBytes(pBytes), handle);
        }

        public static void WriteMem(int pOffset, byte[] pBytes, IntPtr handle)
        {
            WriteProcessMemory(handle, pOffset, pBytes, pBytes.Length, 0);
        }

        public static int ReadInt(int pOffset, IntPtr handle)
        {
            return BitConverter.ToInt32(ReadMem(pOffset, 4, handle), 0);
        }

        public static byte[] ReadMem(int pOffset, int pSize, IntPtr handle)
        {
            byte[] buffer = new byte[pSize];
            ReadProcessMemory(handle, pOffset, buffer, pSize, 0);
            return buffer;
        }

        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(IntPtr hProcess, int lpBaseAddress, byte[] buffer, int size,
            int lpNumberOfBytesRead);

        [DllImport("kernel32.dll")]
        public static extern bool WriteProcessMemory(IntPtr hProcess, int lpBaseAddress, byte[] buffer, int size,
            int lpNumberOfBytesWritten);
#endregion
        static void Main(string[] args)
        {
            Process mediviaOgl = Process.GetProcessesByName("Medivia_OGL").FirstOrDefault();

            if (mediviaOgl == null)
            {
                Process mediviaCheck = Process.GetProcessesByName("Medivia_D3D").FirstOrDefault();
                string errorMessage = mediviaCheck != null
                    ? "You seem to be running the directx version of medivia, please use the OpenGl version."
                    : "Failed to find the Medivia client!";
                Console.WriteLine(errorMessage);
                Console.ReadKey();
                return;
            }

            Console.WriteLine("Successfully attached to a Medivia client.");
            int tibiaBase = mediviaOgl.MainModule.BaseAddress.ToInt32();
            IntPtr handle = mediviaOgl.Handle;
            int start = tibiaBase + 0x57442C;
            int playerBase = ReadInt(start, handle);

            CancellationTokenSource lightHackCancellationTokenSource = new CancellationTokenSource();
            CancellationToken lightHackCancellationToken = lightHackCancellationTokenSource.Token;
            Task lightHackTask = new Task(() =>
            {
                while (!lightHackCancellationToken.IsCancellationRequested)
                {
                    playerBase = ReadInt(start, handle);
                    WriteByte(playerBase + 0xA0, 15, handle);
                    Thread.Sleep(250);
                }
            }, lightHackCancellationToken);
         
            lightHackTask.Start();
            Console.WriteLine("Lighthack is now active, press any key to stop.");
            Console.ReadKey();
           
            lightHackCancellationTokenSource.Cancel();
            WriteByte(playerBase + 0xA0, 0, handle);

        }
    }
}
