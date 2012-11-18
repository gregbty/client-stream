using System;
using System.Windows.Forms;
using ClientStream.Forms;

namespace ClientStream
{
    internal static class Program
    {
        public static MainForm MainForm { get; private set; }

        /// <summary>
        ///     The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            MainForm = new MainForm();

            Application.Run(MainForm);
        }
    }
}