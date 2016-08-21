using Newtonsoft.Json;
using ServiceStack.Data;
using ServiceStack.OrmLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EDShoppingList
{
    public class Repository
    {
        private readonly HttpClient httpClient = new HttpClient();
        private readonly IDbConnectionFactory _connectionFactory =
            new OrmLiteConnectionFactory("data source = db.s3db; Version=3;", SqliteDialect.Provider);

        public async Task OpenAsync(IProgress<string> progress)
        {
            try
            {
                progress.Report("Accessing database");
                using (var db = _connectionFactory.Open())
                {
                    using (var dbTrans = db.OpenTransaction())
                    {
                        if (db.CreateTableIfNotExists<JsonSystem>())
                        {
                            var json = await LoadDataAsync<List<JsonSystem>>(@"systems_populated.json", progress);
                            progress.Report("Adding systems to database");
                            await db.InsertAllAsync(json);
                        }

                        if (db.CreateTableIfNotExists<JsonStation>())
                        {
                            var stations = await LoadDataAsync<List<JsonStation>>(@"stations.json", progress);
                            progress.Report("Adding stations to database");
                            await db.InsertAllAsync(stations);

                            progress.Report("Creating index of stations <-> modules, this may take a few minutes !");
                            db.DropAndCreateTable<StationModule>();
                            var stationModules = (
                                from station in stations
                                from module in station.selling_modules
                                select new StationModule
                                {
                                    StationId = station.id,
                                    ModuleId = module
                                }).ToList();
                            await db.InsertAllAsync(stationModules);
                        }

                        if (db.CreateTableIfNotExists<JsonModule>())
                        {
                            var json = await LoadDataAsync<List<JsonModule>>(@"modules.json", progress);
                            progress.Report("Adding modules to database");
                            await db.InsertAllAsync(json);
                        }

                        dbTrans.Commit();
                    }
                }
            }
            catch (Exception ex)
            {
                progress.Report($"Error: {ex.Message}");
                throw;
            }
        }

        public IEnumerable<JsonSystem> GetSystems()
        {
            using (var db = _connectionFactory.Open())
            {
                return db.Select<JsonSystem>();
            }
        }

        public IEnumerable<JsonModule> GetModules()
        {
            using (var db = _connectionFactory.Open())
            {
                return db.Select<JsonModule>();
            }
        }

        // http://stackoverflow.com/questions/21169573/how-to-implement-progress-reporting-for-portable-httpclient
        private async Task<string> DownloadStringAsync(string url, Action<double> progress, CancellationToken token = default(CancellationToken))
        {
            var stringBuilder = new StringBuilder();
            var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, token);

            response.EnsureSuccessStatusCode();

            var total = response.Content.Headers.ContentLength.HasValue ? response.Content.Headers.ContentLength.Value : -1L;
            var canReportProgress = total != -1 && progress != null;

            using (var stream = await response.Content.ReadAsStreamAsync())
            {
                var totalRead = 0L;
                var buffer = new byte[4096];
                var isMoreToRead = true;

                do
                {
                    token.ThrowIfCancellationRequested();

                    var read = await stream.ReadAsync(buffer, 0, buffer.Length, token);

                    if (read == 0)
                    {
                        isMoreToRead = false;
                    }
                    else
                    {
                        var data = new byte[read];
                        buffer.ToList().CopyTo(0, data, 0, read);

                        stringBuilder.Append(Encoding.ASCII.GetString(data));

                        totalRead += read;

                        if (canReportProgress)
                        {
                            progress((totalRead * 1d) / (total * 1d) * 100);
                        }
                    }
                } while (isMoreToRead);
            }
            return stringBuilder.ToString();
        }

        private async Task<TData> LoadDataAsync<TData>(string path, IProgress<string> progress, CancellationToken token = default(CancellationToken), bool forceOnlineDownload = false)
        {
            var serializer = new JsonSerializer();
            string data;
            if (!File.Exists(path) || forceOnlineDownload)
            {
                progress.Report($"Loading {path} from EDDB");

                data = await DownloadStringAsync($"https://eddb.io/archive/v4/" + path, p => progress.Report($"Loading {path} from EDDB: {p:N0} %"), token);
            }
            else
            {
                progress.Report($"Loading {path} from file");
                using (var file = File.OpenText(path))
                {
                    data = await file.ReadToEndAsync();
                }
            }
            return JsonConvert.DeserializeObject<TData>(data);
        }

        private static bool Contains(string source, string toCheck, StringComparison comp)
        {
            return source != null && toCheck != null && source.IndexOf(toCheck, comp) >= 0;
        }

        private static double DistanceBetweenSystems(JsonSystem first, JsonSystem second)
        {
            if (first == null || second == null)
            {
                return 0.0;
            }

            double deltaX = second.x - first.x;
            double deltaY = second.y - first.y;
            double deltaZ = second.z - first.z;

            return Math.Sqrt(deltaX * deltaX + deltaY * deltaY + deltaZ * deltaZ);
        }

        public async Task<IEnumerable<ModuleSearchResponse>> SearchModules(
            IEnumerable<ModuleSearchQuery> searchQuery,
            double maxDistTo,
            string sys,
            IProgress<string> progress,
            CancellationToken token = default(CancellationToken))
        {
            return await Task.Run(async () =>
            {
                using (var db = _connectionFactory.Open())
                {
                    var modules = new List<JsonModule>();

                    progress.Report("Finding matching modules");
                    foreach (var module in await db.SelectAsync<JsonModule>())
                    {
                        if (searchQuery.Any(q =>
                        {
                            var match = Contains(module.group.name, q.Name, StringComparison.OrdinalIgnoreCase);
                            if (match && q.Class.HasValue)
                            {
                                match = match && (q.Class.Value == module.@class);
                            }
                            if (match && !string.IsNullOrWhiteSpace(q.Rating))
                            {
                                match = match && (q.Rating.Equals(module.rating, StringComparison.OrdinalIgnoreCase));
                            }
                            return match;
                        }))
                        {
                            modules.Add(module);
                        }
                    }

                    progress.Report("Listing all stations selling the matched modules");
                    JsonSystem targetSystem = null;
                    if (Math.Abs(maxDistTo) > double.Epsilon && !string.IsNullOrWhiteSpace(sys))
                    {
                        targetSystem = await db.SingleAsync<JsonSystem>(s => s.name == sys);
                    }

                    var resultCollection = new List<ModuleSearchResponse>();
                    foreach (var module in modules)
                    {
                        var stationModules = db.From<StationModule>()
                                               .Join<StationModule, JsonStation>((sm, station) => sm.StationId == station.id)
                                               .Join<JsonStation, JsonSystem>((station, system) => station.system_id == system.id)
                                               .Where(sm => sm.ModuleId == module.id)
                                               ;

                        if (targetSystem != null)
                        {
                            stationModules = stationModules.And<JsonSystem>
                                (
                                    system =>
                                    (
                                        (targetSystem.x - system.x) * (targetSystem.x - system.x) +
                                        (targetSystem.y - system.y) * (targetSystem.y - system.y) +
                                        (targetSystem.z - system.z) * (targetSystem.z - system.z)
                                    ) < maxDistTo * maxDistTo
                                );
                        }

                        var results = db.SelectMulti<StationModule, JsonStation, JsonSystem>(stationModules);
                        foreach (var tuple in results)
                        {
                            resultCollection.Add(new ModuleSearchResponse
                            {
                                Class = module.@class,
                                Rating = module.rating,
                                Module = module.@group.name,
                                Category = module.@group.category,
                                Price = module.price,

                                Station = tuple.Item2.name,
                                DistanceToStar = tuple.Item2.distance_to_star,

                                System = tuple.Item3.name,
                                DistanceToSystem = DistanceBetweenSystems(targetSystem, tuple.Item3),
                            });
                        }
                    }

                    progress.Report("Filtering the results");
                    return resultCollection
                                  .OrderBy(r => r.DistanceToSystem)
                                  .ThenBy(r => r.DistanceToStar)
                                  .ThenBy(r => r.Module)
                                  .ThenBy(r => r.Class)
                                  .ThenBy(r => r.Rating)
                                  .ThenBy(r => r.Price).AsParallel().AsEnumerable();
                }
            });
        }
    }
}
