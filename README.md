# CalendarSync

---

## What is it?
Calendar sync application that will:

- Copy Outlook calendar events to a local SQLite3 database
- Create markdown files for the events for use in note taking app Obsidian
    - Delete markdown files that are old and never had any notes taken
    - Allows for transient event notes, by keeping the ==DELETABLE== tag in the note I can set a reminder for the purpose of the event, and then remove ==DELETABLE== if any notes are worth keeping after the event
- Potential starting point for a custom built [Calendarly](https://calendly.com)

## What is it not?
- This is not built as a product
    - It is a "living" system where I change the code based on my needs (e.g. changing date ranges or filters)
    - Think of it as a template
- This is **NOT** an example of **Clean Code**
    - Just a quick one-off that may evolve into something better in the future

## Why did I create it?
- I put personal events in my work calendar, but I need to retain them in the case that my current work calendar is no longer available to me
    - I do not want to have to use multiple calendar systems for work/personal
- I use [Obsidian](https://obsidian.md) for note taking throughout my day as a "2nd brain", and this allows me to stay productive in the context of meetings I have

## How do I use it?
1. Setup [Obsidian](https://obsidian.md)
    1. Add the [Full Calendar](https://github.com/davish/obsidian-full-calendar) community plugin
    2. In [Obsidian](https://obsidian.md) I have structured my folders so that I have an ```Events/Personal``` and ```Events/Work``` (among others)
2. Setup the project:
    ```
    git clone https://github.com/craigstjean/ObsidianCalendarSync.git
    cd ObsidianCalendarSync
    dotnet restore
    dotnet ef database update
    ```
3. Configure the project:
    1. Create an application in your work's Azure portal (or create one in your personal portal and have your work whitelist the AppId)
    2. Copy [appsettings.template.json](appsettings.template.json) to [appsettings.json](appsettings.json)
    3. Set ```AppClientId``` and ```AppTenantId``` in [appsettings.json](appsettings.json)
    4. Set ```ObsidianWorkPath``` and ```ObsidianPersonalPath``` in [appsettings.json](appsettings.json)
4. Sync your calendar and generate your [Obsidian](https://obsidian.md) markdown files
    1. Potentially change the ```startDateTime``` and ```endDateTime``` variables in ```Sync``` if you want a different date range
        1. Default is -30 days <=> +365 days
    2. Run the sync:
        ```
        dotnet run
        ```

### Application Parameters
- Sync Calendar and Export to [Obsidian](https://obsidian.md)
    ```
    dotnet run
    ```
- Sync Calendar **ONLY**
    ```
    dotnet run -- --sync
    ```
- Export the current database to [Obsidian](https://obsidian.md) **ONLY**
    ```
    dotnet run -- --export
    ```
- Purge all [Obsidian](https://obsidian.md) markdown files that still contain ==DELETABLE==
    ```
    dotnet run -- --purge
    ```

## Questions
- Q: Why do I have to authenticate each time I run it?
    - A: Because storing access tokens that grant access to your work data is insecure
    - Extended A: Feel free to change the program to store your access tokens (depending on their TTL), or to use a different flow other than ```AcquireTokenInteractive``` into ```DelegateAuthenticationProvider```

## Links
- [Obsidian](https://obsidian.md)
- [Obsidian Full Calendar Plugin](https://github.com/davish/obsidian-full-calendar)
- [Microsoft Graph API](https://docs.microsoft.com/en-us/graph/use-the-api)
- [Microsoft Azure Portal](https://portal.azure.com)
