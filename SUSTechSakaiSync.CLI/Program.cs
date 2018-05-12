using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using WebDav;

namespace SUSTechSakaiSync.CLI
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var configContent = await File.ReadAllTextAsync("SyncConfig.xml");
            var config = SyncConfig.FromConfigXml(configContent);
            var syncList = new ConcurrentBag<SyncItem>();

            Console.WriteLine("Generating sync list");
            var syncListGeneratorTasks = config.Resources.Select(res => Task.Run(async () =>
              {
                  var serverFiles = await GetServerFilesAsync(res, config.GetCredentials());
                  var localFiles = await GetLocalFilesAsync(res.LocalRoot);
                  var sync = from sf in serverFiles
                             join lf in localFiles on sf.LocalPath equals lf.RealtivePath into fg
                             from lf in fg.DefaultIfEmpty()
                             where lf == null || sf.LastModified > lf.LastModified
                             select new SyncItem { Uri = sf.Uri, LocalPath = lf?.Path ?? res.LocalRoot + sf.LocalPath };
                  foreach (var item in sync)
                      syncList.Add(item);
              }));
            await Task.WhenAll(syncListGeneratorTasks);
            await Task.WhenAll(syncList.Select(async item =>
            {
                Console.WriteLine($"Sync start: {item}");
                await SyncFile(item, config.GetCredentials());
                Console.WriteLine($"Sync completed: {item}");
            }));
        }

        private static async Task SyncFile(SyncItem item, ICredentials credentials)
        {
            var param = new WebDavClientParams
            {
                Credentials = credentials
            };
            using (var client = new WebDavClient(param))
            {
                var response = await client.GetRawFile(item.Uri);
                if (response.IsSuccessful)
                {
                    var dir = Path.GetDirectoryName(item.LocalPath);
                    if (!Directory.Exists(dir))
                        Directory.CreateDirectory(dir);

                    using (var fs = new FileStream(item.LocalPath, FileMode.OpenOrCreate, FileAccess.Write))
                    {
                        await response.Stream.CopyToAsync(fs);
                    }
                }
                else
                {
                    throw new WebException(response.Description);
                }
            }
        }

        private static Task<IEnumerable<LocalFileInfo>> GetLocalFilesAsync(string localRoot)
        => Task.FromResult(
            Directory.GetFiles(localRoot, "*", SearchOption.AllDirectories)
                .Select(p => new FileInfo(p))
                .Select(fi => new LocalFileInfo
                {
                    Path = fi.FullName,
                    RealtivePath = fi.FullName.Substring(localRoot.Length),
                    LastModified = fi.LastWriteTime
                })
            );


        private static async Task<IEnumerable<ServerFileInfo>> GetServerFilesAsync(ConfigResource res, ICredentials credentials)
        {
            var param = new WebDavClientParams
            {
                Credentials = credentials
            };
            using (var client = new WebDavClient(param))
            {
                const string sakaiServerUrl = "https://sakai.sustc.edu.cn";
                var fileList = new List<ServerFileInfo>();
                var dirQueue = new Queue<string>();
                dirQueue.Enqueue(sakaiServerUrl + res.ServerRoot);
                while (dirQueue.Any())
                {
                    var result = await client.Propfind(dirQueue.Dequeue());
                    foreach (var item in result.Resources.Skip(1))// the first one is the root directory itself
                    {
                        if (res.Excludes.Any(ex => item.Uri.StartsWith(res.ServerRoot + ex)))
                            continue;
                        if (item.IsCollection)
                            dirQueue.Enqueue(sakaiServerUrl + item.Uri);
                        else
                        {
                            var uri = Uri.UnescapeDataString(sakaiServerUrl + item.Uri);
                            fileList.Add(new ServerFileInfo
                            {
                                Uri = uri,
                                LocalPath = uri.Replace(sakaiServerUrl + res.ServerRoot, "").Replace("/", "\\"),
                                LastModified = item.LastModifiedDate.GetValueOrDefault()
                            });
                        }
                    }
                }
                return fileList;
            }
        }
    }
}
