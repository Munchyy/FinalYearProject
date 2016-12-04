using System;
using System.Windows.Forms;

using System.Net;
using System.Threading;

namespace FinalYearProject
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
           /* using (Game1 game = new Game1())
            {
                game.Run();
            }*/

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
#endif
}

