using System;
using System.Collections.Generic;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiskRemover
{
    public class AppSetting
    {
        public static string Path = "appsettings.json";
        public string? SaveDirectory { get; set; }
        public int? Days { get; set; }
        public static AppSetting Load()
        {
            try
            {
                if (!System.IO.File.Exists(Path))
                {
                    var setting =  new AppSetting();
                    setting.Save();
                    return setting;
                }
                var json = System.IO.File.ReadAllText(Path);
                return System.Text.Json.JsonSerializer.Deserialize<AppSetting>(json);
            }
            catch (Exception)
            {
                return new AppSetting();
            }
        }
        public void Save()
        {
            var json = System.Text.Json.JsonSerializer.Serialize(this, new System.Text.Json.JsonSerializerOptions() { WriteIndented=true});
            System.IO.File.WriteAllText(Path, json);
        }
    }
}
