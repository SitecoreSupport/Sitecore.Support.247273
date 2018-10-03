using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using Microsoft.Extensions.DependencyInjection;
using Sitecore.Data.Items;
using Sitecore.XA.Feature.Media;
using Sitecore.XA.Feature.Media.Models;
using Sitecore.XA.Feature.Media.Repositories;
using Sitecore.XA.Foundation.SitecoreExtensions.Extensions;

namespace Sitecore.Support.XA.Feature.Media.Repositories
{
  public class GalleryRepository: Sitecore.XA.Feature.Media.Repositories.GalleryRepository
  {
    protected override string GetVideoThumbnailUrlFromVideoProvider(Item item)
    {
      string result = null;
      switch (item.Fields[Templates.GalleryVideo.Fields.VideoProvider].ToEnum<VideoProvider>())
      {
        case VideoProvider.YouTube:
        {
          Match match = Regex.Match(item[Templates.GalleryVideo.Fields.VideoID], "^(?:https?\\:\\/\\/)?(?:www\\.)?(?:youtu\\.be\\/|youtube\\.com\\/(?:embed\\/|v\\/|watch\\?v\\=))?([\\w-]{10,12})(?:$|\\&|\\?\\#).*");
          if (match.Success)
          {
            result = $"http://img.youtube.com/vi/{match.Groups[1]}/hqdefault.jpg";
          }
          break;
        }
        case VideoProvider.Dailymotion:
        {
          Match match = Regex.Match(item[Templates.GalleryVideo.Fields.VideoID], "^(?:https?\\:\\/\\/)?(?:www\\.)?(?:dailymotion\\.com\\/)?(?:video\\/([^_]+))?[^#]*(?:#video=([^_&]+))?");
          for (int num = 2; num > 0; num--)
          {
            if (match.Success && match.Groups[num].Success)
            {
              result = $"http://www.dailymotion.com/thumbnail/video/{match.Groups[num].Value}";
            }
          }
          break;
        }
      }
      return result;
    }
  }


  public class RegisterDependencies: Sitecore.DependencyInjection.IServicesConfigurator
  {
    public void Configure(IServiceCollection serviceCollection)
    {
      serviceCollection.AddTransient<IGalleryRepository, GalleryRepository>();
    }
  }

}