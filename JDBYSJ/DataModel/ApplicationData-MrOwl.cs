using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace JDBYSJ.Data
{
    class ApplicationData_MrOwl
    {
        public async static void ReadLocalSetting()
        {
            try
            {
                ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;//存储单一配置的句柄

                Object value = localSettings.Values["SelfChannelID"];//a setting
                if (value != null)
                {
                    App.SelfChannelID = value as String;
                    App.SelfChannelName = await NewsChannelsDataSource.GetChannelNameByChannelId(App.SelfChannelID);
                }
            }
            catch (Exception ex)
            { }
            //ApplicationDataCompositeValue composite =
            //   (ApplicationDataCompositeValue)localSettings.Values["exampleCompositeSetting"];

            //if (composite == null)
            //{
            //    // No data
            //}
            //else
            //{
            //    // Access data in composite["intVal"] and composite["strVal"]
            //}

            //bool hasContainer = localSettings.Containers.ContainsKey("exampleContainer");
            //bool hasSetting = false;

            //if (hasContainer)
            //{
            //    hasSetting = localSettings.Containers["exampleContainer"].Values.ContainsKey("exampleSetting");
            //}
        }
    }
}
