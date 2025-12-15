using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Web;

namespace api_amanda.Helpers
{
    public class RawPrinterHelper
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public class DOCINFOA
        {
            [MarshalAs(UnmanagedType.LPStr)]
            public string pDocName;
            [MarshalAs(UnmanagedType.LPStr)]
            public string pOutputFile;
            [MarshalAs(UnmanagedType.LPStr)]
            public string pDataType;
        }

        [DllImport("winspool.Drv", EntryPoint = "OpenPrinterA", SetLastError = true)]
        public static extern bool OpenPrinter(string szPrinter, out IntPtr hPrinter, IntPtr pd);

        [DllImport("winspool.Drv", EntryPoint = "ClosePrinter")]
        public static extern bool ClosePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "StartDocPrinterA", SetLastError = true)]
        public static extern bool StartDocPrinter(IntPtr hPrinter, int level, [In] DOCINFOA di);

        [DllImport("winspool.Drv", EntryPoint = "EndDocPrinter")]
        public static extern bool EndDocPrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "StartPagePrinter")]
        public static extern bool StartPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "EndPagePrinter")]
        public static extern bool EndPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "WritePrinter", SetLastError = true)]
        public static extern bool WritePrinter(IntPtr hPrinter, IntPtr pBytes, int dwCount, out int dwWritten);

        public static bool SendStringToPrinter(string printerName, string data)
        {
            IntPtr pPrinter;
            DOCINFOA docInfo = new DOCINFOA
            {
                pDocName = "Ticket POS",
                pDataType = "RAW"
            };

            if (!OpenPrinter(printerName, out pPrinter, IntPtr.Zero))
                return false;

            if (!StartDocPrinter(pPrinter, 1, docInfo))
            {
                ClosePrinter(pPrinter);
                return false;
            }

            StartPagePrinter(pPrinter);

            IntPtr pBytes = Marshal.StringToCoTaskMemAnsi(data);
            WritePrinter(pPrinter, pBytes, data.Length, out _);
            Marshal.FreeCoTaskMem(pBytes);

            EndPagePrinter(pPrinter);
            EndDocPrinter(pPrinter);
            ClosePrinter(pPrinter);

            return true;
        }
    }
}