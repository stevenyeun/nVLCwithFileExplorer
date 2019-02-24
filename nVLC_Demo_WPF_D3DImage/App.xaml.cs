using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace nVLC_Demo_WPF_D3DImage
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public string DirPath { get; set; }
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (e.Args.Length == 1)
            {
                try
                {
                    DirPath = e.Args[0];
                }
                catch (Exception ee)
                {
                    MessageBox.Show("명령 인수를 올바르게 입력하여주세요!");
                }
            }
            else
            {
                MessageBox.Show("명령 인수가 없습니다!");
            }
        }
    }
}
