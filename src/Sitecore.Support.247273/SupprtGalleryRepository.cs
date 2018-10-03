using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Sitecore.Data.Items;
using Sitecore.XA.Feature.Media;
using Sitecore.XA.Feature.Media.Models;
using Sitecore.XA.Feature.Media.Repositories;
using Sitecore.XA.Foundation.SitecoreExtensions.Extensions;

namespace Sitecore.Support.XA.Feature.Media.Repositories
{
  public class GalleryRepository : Sitecore.XA.Feature.Media.Repositories.GalleryRepository
  {
    protected override string GetVideoThumbnailUrlFromVideoProvider(Item item)
    {
      string result = null;
      switch (item.Fields[Templates.GalleryVideo.Fields.VideoProvider].ToEnum<VideoProvider>())
      {
        case VideoProvider.YouTube:
        {
          Match match = Regex.Match(item[Templates.GalleryVideo.Fields.VideoID],
            "^(?:https?\\:\\/\\/)?(?:www\\.)?(?:youtu\\.be\\/|youtube\\.com\\/(?:embed\\/|v\\/|watch\\?v\\=))?([\\w-]{10,12})(?:$|\\&|\\?\\#).*");
          if (match.Success)
          {
            result = $"http://img.youtube.com/vi/{match.Groups[1]}/hqdefault.jpg";
          }

          break;
        }
        case VideoProvider.Dailymotion:
        {
          Match match = Regex.Match(item[Templates.GalleryVideo.Fields.VideoID],
            "^(?:https?\\:\\/\\/)?(?:www\\.)?(?:dailymotion\\.com\\/)?(?:video\\/([^_]+))?[^#]*(?:#video=([^_&]+))?");
          for (int num = 2; num > 0; num--)
          {
            if (match.Success && match.Groups[num].Success)
            {
              WebRequest request =
                WebRequest.Create(
                  $"https://api.dailymotion.com/video/{match.Groups[num].Value}?fields=thumbnail_large_url");
              WebResponse response = request.GetResponse();
              Stream responseStream = response.GetResponseStream();
              if (responseStream != null)
              {
                StreamReader reader = new StreamReader(responseStream);
                string responseFromServer = reader.ReadToEnd();
                JObject json = JObject.Parse(responseFromServer);
                result = json["thumbnail_large_url"].Value<string>();
                response.Close();
              }
              else
              {
                response.Close();
              }
            }
          }

          break;
        }
        case VideoProvider.Vimeo:
        {
          Match match = Regex.Match(item[Templates.GalleryVideo.Fields.VideoID],
            "^(?:https?\\:\\/\\/)?(?:www\\.)?(?:vimeo\\.com\\/)?(?:.*#|.*/videos/)?([0-9]+)");
          if (match.Success)
          {
            WebRequest request = WebRequest.Create($"http://vimeo.com/api/v2/video/{match.Groups[1]}.json");
            WebResponse response = request.GetResponse();
            Stream responseStream = response.GetResponseStream();
            if (responseStream != null)
            {
              StreamReader reader = new StreamReader(responseStream);
              string responseFromServer = reader.ReadToEnd();
              //remove redundant [ and ] which breaks parsing to JSON
              responseFromServer = responseFromServer.Replace('[', ' ').Replace(']', ' ');
              JObject json = JObject.Parse(responseFromServer);
              result = json["thumbnail_large"].Value<string>();
              response.Close();
            }
            else
            {
              response.Close();
            }

          }

          break;
        }
      }

      return result;
    }
  }
  public class RegisterDependencies : Sitecore.DependencyInjection.IServicesConfigurator
  {
    public void Configure(IServiceCollection serviceCollection)
    {
      serviceCollection.AddTransient<IGalleryRepository, GalleryRepository>();
    }
  }
}