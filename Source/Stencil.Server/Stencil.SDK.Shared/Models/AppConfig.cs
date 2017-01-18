using System;
using System.Collections.Generic;
using System.Text;

namespace Stencil.SDK.Models
{
    public class AppConfig
    {
        public AppConfig()
        {
            this.AppConfigIntervalHours = 24;
            this.AppVersionCheckIntervalHours = 16;
            this.AccountVerifyIntervalHours = 8;
            this.TranslationIntervalHours = 72;

            this.ImageUploadSize_Width = 2048;
            this.ImageUploadSize_Height = 2048;

            this.Widget = new EndpointConfig()
            {
                StaleSeconds = 60, // not used
                PageSize = 10,
                ScrollThresholdSize = 100,
                ScrollThresholdCount = 4
            };
        }
        public int ImageUploadSize_Width { get; set; }
        public int ImageUploadSize_Height { get; set; }
        public int AppConfigIntervalHours { get; set; }
        public int TranslationIntervalHours { get; set; }
        public int AppVersionCheckIntervalHours { get; set; }
        public int AccountVerifyIntervalHours { get; set; }
        public EndpointConfig Widget { get; set; }
    }
}
