using System.Globalization;
using System.Text.RegularExpressions;
using System.Web;
using CalendarSync.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using Directory = System.IO.Directory;
using File = System.IO.File;

internal class Program
{
    private static async Task Sync()
    {
        var builder = new ConfigurationBuilder().AddJsonFile($"appsettings.json", true, true);
        var config = builder.Build();

        // Configure the MSAL client to get tokens
        var pcaOptions = new PublicClientApplicationOptions
        {
            ClientId = config["AppClientId"],
            AadAuthorityAudience = AadAuthorityAudience.AzureAdAndPersonalMicrosoftAccount,
            RedirectUri = "http://localhost",
        };

        var pca = PublicClientApplicationBuilder
            .CreateWithApplicationOptions(pcaOptions).Build();

        // The permission scope required for Microsoft Graph
        var graphScopes = new string[]
        {
            "https://graph.microsoft.com/Calendars.ReadWrite",
            "https://graph.microsoft.com/Calendars.ReadWrite.Shared",
            "https://graph.microsoft.com/User.Read"
        };

        using var db = new EventContext();

        Microsoft.Graph.Event? currentEvent = null;

        try
        {
            // Make the interactive token request
            var authResult = await pca.AcquireTokenInteractive(graphScopes).ExecuteAsync();

            // Configure the GraphClient with the access token
            var graphClient = new GraphServiceClient(new DelegateAuthenticationProvider((requestMessage) =>
            {
                requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authResult.AccessToken);
                return Task.CompletedTask;
            }));

            var syncState = db.SyncStates.FirstOrDefault(s => s.Principal == authResult.Account.Username);
            if (syncState == null)
            {
                syncState = new SyncState();
                syncState.Principal = authResult.Account.Username;
                db.SyncStates.Add(syncState);
            }

            var startDateTime = DateTime.Now.Date - TimeSpan.FromDays(365);
            var endDateTime = DateTime.Now.Date + TimeSpan.FromDays(365 * 2);

            var queryOptions = new List<QueryOption>();
            if (string.IsNullOrEmpty(syncState.DeltaUrl) || endDateTime - DateTime.Now < TimeSpan.FromDays(365))
            {
                queryOptions.Add(new QueryOption("startDateTime", startDateTime.ToString("o")));
                queryOptions.Add(new QueryOption("endDateTime", endDateTime.ToString("o")));
                syncState.StartWindow = startDateTime;
                syncState.EndWindow = endDateTime;
            }
            else
            {
                var deltaUri = new Uri(syncState.DeltaUrl);
                var deltaToken = HttpUtility.ParseQueryString(deltaUri.Query).Get("$deltatoken");
                queryOptions.Add(new QueryOption("$deltatoken", deltaToken));
            }

            var eventsRequest = graphClient.Me.Calendar.CalendarView
                .Delta()
                .Request(queryOptions)
                .Select("id,iCalUId,start,end,showAs,subject,isAllDay,isCancelled,location,onlineMeetingUrl,body");

            var events = eventsRequest;
            
            int count = 0;
            do
            {
                var page = await events.GetAsync();

                foreach (var e in page)
                {
                    Console.WriteLine(++count);
                    currentEvent = e;

                    if (e.Subject == null)
                    {
                        continue;
                    }

                    var isCancelled = false;
                    if (e.IsCancelled.HasValue)
                    {
                        isCancelled = e.IsCancelled.Value;
                    }

                    var existingEvent = db.Events
                        .Include(e => e.EventBody)
                        .Include(e => e.EventLocation)
                        .FirstOrDefault(ee => ee.ICalUid == e.ICalUId && ee.Uid == e.Id);
                    if (existingEvent == null && !isCancelled)
                    {
                        var dbEvent = new CalendarSync.Model.Event
                        {
                            ICalUid = e.ICalUId,
                            Uid = e.Id,
                            Start = DateTime.Parse(e.Start.DateTime, CultureInfo.CreateSpecificCulture(e.Start.TimeZone)),
                            End = DateTime.Parse(e.End.DateTime, CultureInfo.CreateSpecificCulture(e.End.TimeZone)),
                            IsAllDay = e.IsAllDay.HasValue ? e.IsAllDay.Value : false,
                            EventBody = new EventBody
                            {
                                ContentType = e.Body.ContentType.ToString(),
                                Content = e.Body.Content,
                                /*AdditionalDatas = e.Body.AdditionalData.Select(a => new AdditionalData {
                                    Key = a.Key,
                                    Value = a.Value == null ? "" : a.Value.ToString()
                                }).ToList()*/
                            },
                            Subject = e.Subject,
                            OnlineMeetingUrl = e.OnlineMeetingUrl,
                            EventLocation = new EventLocation
                            {
                                DisplayName = e.Location.DisplayName,
                                LocationUrl = e.Location.LocationUri,
                                /*AdditionalDatas = e.Location.AdditionalData.Select(a => new AdditionalData {
                                    Key = a.Key,
                                    Value = a.Value == null ? "" : a.Value.ToString()
                                }).ToList()*/
                            },
                            ShowAs = e.ShowAs.ToString(),
                            IsCancelled = e.IsCancelled.HasValue ? e.IsCancelled.Value : false,
                            /*AdditionalDatas = e.AdditionalData.Select(a => new AdditionalData {
                                Key = a.Key,
                                Value = a.Value == null ? "" : a.Value.ToString()
                            }).ToList()*/
                        };

                        db.Events.Add(dbEvent);
                        db.SaveChanges();
                    }
                    else if (existingEvent != null && isCancelled)
                    {
                        if (existingEvent.EventBody != null) db.EventBodies.Remove(existingEvent.EventBody);
                        if (existingEvent.EventLocation != null) db.EventLocations.Remove(existingEvent.EventLocation);
                        db.Events.Remove(existingEvent);
                        db.SaveChanges();
                    }
                    else if (existingEvent != null)
                    {
                        existingEvent.Start = DateTime.Parse(e.Start.DateTime, CultureInfo.CreateSpecificCulture(e.Start.TimeZone));
                        existingEvent.End = DateTime.Parse(e.End.DateTime, CultureInfo.CreateSpecificCulture(e.End.TimeZone));
                        existingEvent.IsAllDay = e.IsAllDay.HasValue ? e.IsAllDay.Value : false;
                        existingEvent.Subject = e.Subject;
                        existingEvent.OnlineMeetingUrl = e.OnlineMeetingUrl;
                        existingEvent.ShowAs = e.ShowAs.ToString();
                        existingEvent.IsCancelled = e.IsCancelled.HasValue ? e.IsCancelled.Value : false;

                        existingEvent.EventBody.ContentType = e.Body.ContentType.ToString();
                        existingEvent.EventBody.Content = e.Body.Content;

                        existingEvent.EventLocation.DisplayName = e.Location.DisplayName;
                        existingEvent.EventLocation.LocationUrl = e.Location.LocationUri;

                        db.SaveChanges();
                    }
                }

                if (page.NextPageRequest != null)
                {
                    events = page.NextPageRequest;
                }
                else
                {
                    object? deltaLink;
                    if (page.AdditionalData.TryGetValue("@odata.deltaLink", out deltaLink))
                    {
                        syncState.DeltaUrl = deltaLink.ToString();
                    }
                    
                    events = null;
                }
            } while (events != null);

            db.SaveChanges();


            Console.WriteLine("Done");
        }
        catch (MsalException ex)
        {
            Console.WriteLine($"Error acquiring access token : {ex}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex}");
            if (currentEvent != null)
            {
                var ces = JsonConvert.SerializeObject(currentEvent);
                System.Console.WriteLine(ces);
            }
        }

        if (System.Diagnostics.Debugger.IsAttached)
        {
            Console.WriteLine("Hit any key to exit...");
            Console.ReadKey();
        }
    }

    private static void PurgeInPath(string path)
    {
        var files = Directory.GetFiles(path, "*.md", SearchOption.AllDirectories);
        var dateRegex = new Regex(@"^(\d{4}-\d{2}-\d{2}) (.*).md$");
        foreach (var file in files)
        {
            var filename = Path.GetFileName(file);
            var matches = dateRegex.Matches(filename);
            if (matches.Count > 0)
            {
                var dateString = matches[0].Groups[1].Value;
                var subject = matches[0].Groups[2].Value;

                if (IsGeneratedAndUntouched(file))
                {
                    System.Console.WriteLine($"Deleting {filename}...");
                    File.Delete(file);
                }
            }
        }
    }

    private static void PurgeEmptyDailyNotesInPath(string path)
    {
        var files = Directory.GetFiles(path, "*.md", SearchOption.AllDirectories);
        var dateRegex = new Regex(@"^(\d{4}-\d{2}-\d{2}).md$");
        foreach (var file in files)
        {
            var filename = Path.GetFileName(file);
            var matches = dateRegex.Matches(filename);
            if (matches.Count > 0)
            {
                var dateString = matches[0].Groups[1].Value;

                if (IsUntouchedDaily(file))
                {
                    System.Console.WriteLine($"Deleting {filename}...");
                    File.Delete(file);
                }
            }
        }
    }

    private static List<string> CleanFilesInPath(string path, DateTime oldestDate, DateTime newestDate)
    {
        var files = Directory.GetFiles(path, "*.md", SearchOption.AllDirectories);
        var dateRegex = new Regex(@"^(\d{4}-\d{2}-\d{2}) (.*).md$");
        var existingFiles = new List<string>();
        foreach (var file in files)
        {
            var filename = Path.GetFileName(file);
            var matches = dateRegex.Matches(filename);
            if (matches.Count > 0)
            {
                var dateString = matches[0].Groups[1].Value;
                var subject = matches[0].Groups[2].Value;

                existingFiles.Add(filename);

                var fileDate = DateTime.Parse(dateString);
                if (fileDate < oldestDate || fileDate > newestDate)
                {
                    if (IsGeneratedAndUntouched(file))
                    {
                        System.Console.WriteLine($"Deleting {filename}...");
                        File.Delete(file);
                    }
                }
            }
        }

        return existingFiles;
    }

    private static bool IsGeneratedAndUntouched(string file)
    {
        var lines = File.ReadAllLines(file);
        var generated = lines.Contains("generator: CalendarSync");
        var untouched = lines.Contains("==DELETABLE==");

        return generated && untouched;
    }

    private static bool IsUntouchedDaily(string file)
    {
        var lines = File.ReadAllLines(file);

        var untouched = lines[3] == "## Personal" &&
                        lines[7] == "## Work" &&
                        lines[11] == "## Achievements" &&
                        lines[16] == "## Tools";

        return untouched;
    }

    private static string ScrubSubject(string subject)
    {
        var scrubbedSubject = Regex.Replace(subject, @"[^A-Za-z0-9 \-_]", "-").Trim();
        if (scrubbedSubject.Length > 50)
        {
            scrubbedSubject = scrubbedSubject.Substring(0, 50);
        }

        return scrubbedSubject;
    }

    private static bool IsIgnore(string subject, string body, List<EventFilter> filters)
    {
        var ignore = filters.Any(f => subject.ToLower().Contains(f.Search.ToLower()) && f.IsIgnore);
        if (!ignore)
        {
            ignore = body.Contains("==CSIGNORE==");
        }

        return ignore;
    }

    private static bool IsPersonal(string subject, List<EventFilter> filters)
    {
        return filters.Any(f => subject.ToLower().Contains(f.Search.ToLower()) && f.IsPersonal);
    }

    private static async Task Export()
    {
        var builder = new ConfigurationBuilder().AddJsonFile($"appsettings.json", true, true);
        var config = builder.Build();

        using var db = new EventContext();

        var filters = db.EventFilters.ToList();
        var oldestDate = DateTime.Now.Date - TimeSpan.FromDays(7);
        var newestDate = DateTime.Now.Date + TimeSpan.FromDays(90);

        // Clean up anything older than 1 week that had no inputs added
        var files = Directory.GetFiles(config["ObsidianWorkPath"], "*.md", SearchOption.AllDirectories);
        var existingFiles = new List<string>();
        existingFiles.AddRange(CleanFilesInPath(config["ObsidianWorkPath"], oldestDate, newestDate));
        existingFiles.AddRange(CleanFilesInPath(config["ObsidianPersonalPath"], oldestDate, newestDate));

        // Generate anything between -1 week and +3 months
        var events = db.Events
            .Include(e => e.EventBody)
            .Include(e => e.EventLocation)
            .Where(e => e.Start >= oldestDate && e.Start < newestDate)
            .ToList();
        foreach (var e in events)
        {
            var eventDateString = e.IsAllDay ? e.Start.ToString("yyyy-MM-dd") : e.Start.ToLocalTime().ToString("yyyy-MM-dd");
            var eventSubject = ScrubSubject(e.Subject);
            var eventFilename = $"{eventDateString} {eventSubject}.md";
            if (!existingFiles.Contains(eventFilename) && !IsIgnore(e.Subject, e.EventBody != null ? e.EventBody.Content : "", filters))
            {
                string filePath;
                if (IsPersonal(e.Subject, filters))
                {
                    filePath = Path.Join(config["ObsidianPersonalPath"], eventFilename);
                }
                else
                {
                    filePath = Path.Join(config["ObsidianWorkPath"], eventFilename);
                }

                System.Console.WriteLine($"Writing {eventFilename}...");
                using (var writer = new StreamWriter(filePath))
                {
                    writer.WriteLine("---");
                    writer.WriteLine($"title: {e.Subject}");
                    writer.WriteLine("allDay: " + (e.IsAllDay ? "true" : "false"));
                    if (e.IsAllDay)
                    {
                        writer.WriteLine($"date: {eventDateString}");
                        writer.WriteLine("type: single");
                        writer.WriteLine($"endDate: {e.End.ToString("yyyy-MM-dd")}");
                    }
                    else
                    {
                        writer.WriteLine($"startTime: {e.Start.ToLocalTime().ToString("HH:mm")}");
                        writer.WriteLine($"endTime: {e.End.ToLocalTime().ToString("HH:mm")}");
                        writer.WriteLine($"date: {eventDateString}");
                    }
                    writer.WriteLine("generator: CalendarSync");
                    writer.WriteLine("---");
                    writer.WriteLine();
                    writer.WriteLine($"# {e.Subject}");
                    writer.WriteLine();
                    writer.WriteLine("==DELETABLE==");
                }
            }
        }
    }

    private static void Purge()
    {
        var builder = new ConfigurationBuilder().AddJsonFile($"appsettings.json", true, true);
        var config = builder.Build();
        
        PurgeInPath(config["ObsidianWorkPath"]);
        PurgeInPath(config["ObsidianPersonalPath"]);
        PurgeEmptyDailyNotesInPath(config["ObsidianDailyNotesPath"]);
    }

    private static async Task Main(string[] args)
    {
        if (args.Length == 0)
        {
            Purge();
            await Sync();
            await Export();
        }
        else if (args[0] == "--sync")
        {
            await Sync();
        }
        else if (args[0] == "--export")
        {
            await Export();
        }
        else if (args[0] == "--purge")
        {
            Purge();
        }
        else
        {
            Console.WriteLine("--sync, --export, --purge");
        }
    }
}
