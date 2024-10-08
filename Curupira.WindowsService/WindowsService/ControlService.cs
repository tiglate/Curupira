﻿using System.ServiceProcess;

namespace Curupira.WindowsService.WindowsService
{
    public partial class ControlService : ServiceBase
    {
        public ControlService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Program.StartServer();
        }

        protected override void OnStop()
        {
            Program.StopServer();
        }
    }
}
