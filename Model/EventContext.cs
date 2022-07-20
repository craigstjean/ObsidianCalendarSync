namespace CalendarSync.Model;

using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

public class EventContext : DbContext
{
    public DbSet<Event> Events { get; set; }
    public DbSet<EventBody> EventBodies { get; set; }
    public DbSet<EventLocation> EventLocations { get; set; }
    public DbSet<AdditionalData> AdditionalDatas { get; set; }
    public DbSet<EventFilter> EventFilters { get; set; }

    public string DbPath { get; }

    public EventContext() => DbPath = "events.db";

    // The following configures EF to create a Sqlite database file in the
    // special "local" folder for your platform.
    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite($"Data Source={DbPath}");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EventFilter>().HasData(new EventFilter
        {
            EventFilterId = 1,
            Search = "Dentist",
            IsIgnore = false,
            IsPersonal = true
        });

        modelBuilder.Entity<EventFilter>().HasData(new EventFilter
        {
            EventFilterId = 2,
            Search = "Orthodontist",
            IsIgnore = false,
            IsPersonal = true
        });

        modelBuilder.Entity<EventFilter>().HasData(new EventFilter
        {
            EventFilterId = 3,
            Search = "Therapist",
            IsIgnore = false,
            IsPersonal = true
        });

        modelBuilder.Entity<EventFilter>().HasData(new EventFilter
        {
            EventFilterId = 4,
            Search = "Therapy",
            IsIgnore = false,
            IsPersonal = true
        });

        modelBuilder.Entity<EventFilter>().HasData(new EventFilter
        {
            EventFilterId = 5,
            Search = "Doctor",
            IsIgnore = false,
            IsPersonal = true
        });

        modelBuilder.Entity<EventFilter>().HasData(new EventFilter
        {
            EventFilterId = 6,
            Search = "Dermatologist",
            IsIgnore = false,
            IsPersonal = true
        });

        modelBuilder.Entity<EventFilter>().HasData(new EventFilter
        {
            EventFilterId = 7,
            Search = "Change furnace filters",
            IsIgnore = true,
            IsPersonal = true
        });
    }
}

public class AccessSetting
{
    public int AccessSettingId { get; set; }
    public string Email { get; set; }
    public string AccessToken { get; set; }
    public DateTime ExpirationDate { get; set; }
}

[Index(nameof(ICalUid), nameof(Uid))]
public class Event
{
    public int EventId { get; set; }

    public string ICalUid { get; set; }
    public string Uid { get; set; }

    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public bool IsAllDay { get; set; } = false;

    public int? EventBodyId { get; set; }
    public EventBody? EventBody { get; set; }

    public string? Subject { get; set; }

    public string? OnlineMeetingUrl { get; set; }
    public int? EventLocationId { get; set; }
    public EventLocation? EventLocation { get; set; }
    
    public string ShowAs { get; set; }

    public bool IsCancelled { get; set; } = false;

    public List<AdditionalData> AdditionalDatas { get; set; } = new List<AdditionalData>();
}

public class EventBody
{
    public int EventBodyId { get; set; }

    public string ContentType { get; set; }
    public string Content { get; set; }

    public List<AdditionalData> AdditionalDatas { get; set; } = new List<AdditionalData>();
}

public class EventLocation
{
    public int EventLocationId { get; set; }

    public string? DisplayName { get; set; }
    public string? LocationUrl  { get; set; }

    public List<AdditionalData> AdditionalDatas { get; set; } = new List<AdditionalData>();
}

public class AdditionalData
{
    public int AdditionalDataId { get; set; }
    public string Key { get; set; }
    public string? Value { get; set; }
}

public class EventFilter
{
    public int EventFilterId { get; set; }
    public string Search { get; set; }
    public bool IsIgnore { get; set; }
    public bool IsPersonal { get; set; }
}
