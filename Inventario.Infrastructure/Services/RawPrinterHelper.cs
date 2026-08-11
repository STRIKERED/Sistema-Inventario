using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Inventario.Infrastructure.Services;

/// <summary>
/// Envía bytes crudos (RAW) a una impresora instalada en Windows a través de winspool.drv,
/// sin pasar por el subsistema de impresión GDI. Es la forma estándar de enviar comandos
/// ESC/POS a una impresora térmica de tickets desde .NET en Windows.
/// </summary>
[SupportedOSPlatform("windows")]
internal static class RawPrinterHelper
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    private struct DOCINFOA
    {
        [MarshalAs(UnmanagedType.LPStr)]
        public string pDocName;

        [MarshalAs(UnmanagedType.LPStr)]
        public string? pOutputFile;

        [MarshalAs(UnmanagedType.LPStr)]
        public string pDataType;
    }

    [DllImport("winspool.drv", EntryPoint = "OpenPrinterA", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true)]
    private static extern bool OpenPrinter(string szPrinter, out IntPtr hPrinter, IntPtr pd);

    [DllImport("winspool.drv", EntryPoint = "ClosePrinter", SetLastError = true, ExactSpelling = true)]
    private static extern bool ClosePrinter(IntPtr hPrinter);

    [DllImport("winspool.drv", EntryPoint = "StartDocPrinterA", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true)]
    private static extern bool StartDocPrinter(IntPtr hPrinter, int level, ref DOCINFOA di);

    [DllImport("winspool.drv", EntryPoint = "EndDocPrinter", SetLastError = true, ExactSpelling = true)]
    private static extern bool EndDocPrinter(IntPtr hPrinter);

    [DllImport("winspool.drv", EntryPoint = "StartPagePrinter", SetLastError = true, ExactSpelling = true)]
    private static extern bool StartPagePrinter(IntPtr hPrinter);

    [DllImport("winspool.drv", EntryPoint = "EndPagePrinter", SetLastError = true, ExactSpelling = true)]
    private static extern bool EndPagePrinter(IntPtr hPrinter);

    [DllImport("winspool.drv", EntryPoint = "WritePrinter", SetLastError = true, ExactSpelling = true)]
    private static extern bool WritePrinter(IntPtr hPrinter, byte[] pBytes, int dwCount, out int dwWritten);

    /// <summary>Envía <paramref name="datos"/> tal cual (RAW) a la impresora <paramref name="nombreImpresora"/>.</summary>
    public static void EnviarBytes(string nombreImpresora, byte[] datos, string nombreDocumento = "Ticket")
    {
        if (!OpenPrinter(nombreImpresora, out var hPrinter, IntPtr.Zero))
        {
            throw new InvalidOperationException(
                $"No se pudo abrir la impresora '{nombreImpresora}'. Código de error: {Marshal.GetLastWin32Error()}.");
        }

        try
        {
            var docInfo = new DOCINFOA
            {
                pDocName = nombreDocumento,
                pOutputFile = null,
                pDataType = "RAW"
            };

            if (!StartDocPrinter(hPrinter, 1, ref docInfo))
            {
                throw new InvalidOperationException(
                    $"No se pudo iniciar el documento en la impresora '{nombreImpresora}'. Código de error: {Marshal.GetLastWin32Error()}.");
            }

            try
            {
                if (!StartPagePrinter(hPrinter))
                {
                    throw new InvalidOperationException(
                        $"No se pudo iniciar la página en la impresora '{nombreImpresora}'. Código de error: {Marshal.GetLastWin32Error()}.");
                }

                try
                {
                    if (!WritePrinter(hPrinter, datos, datos.Length, out var bytesEscritos) || bytesEscritos != datos.Length)
                    {
                        throw new InvalidOperationException(
                            $"No se pudieron escribir todos los bytes en la impresora '{nombreImpresora}'. Código de error: {Marshal.GetLastWin32Error()}.");
                    }
                }
                finally
                {
                    EndPagePrinter(hPrinter);
                }
            }
            finally
            {
                EndDocPrinter(hPrinter);
            }
        }
        finally
        {
            ClosePrinter(hPrinter);
        }
    }
}
