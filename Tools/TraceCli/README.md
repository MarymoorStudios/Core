# ![Logo][logo] TraceCLI 

TraceCLI is real-time viewer for EventSourceLogger logging streams.

TraceCLI shows logs written using the [Microsoft.Extensions.Logging][LoggingDocs] in real-time in a window allowing
monitoring and debugging of applications while they are running.  TraceCLI leverages
[EventSourceLogger][EventSourceLogger].  When used properly, `EventSourceLogger` is very low-cost when no Event Session
Tool (like TraceCLI) is running, allowing logging to be liberally emitted by an application with little or no impact to
runtime performance when not actively being monitored.

Unlike other logging collection tools, TraceCLI does NOT write logs to a file.  (If you need persistent collection
take a look at [dotnet-trace][dotnet-trace].)  Instead, TraceCLI streams logging to a window in real-time where it can
be viewed while the application being debugged is being used.

TraceCLI is also tightly integrated with the Marymoor Studios Core Libraries, and so can be used to monitor and debug
MSC Promise RPC interactions (between threads, processes, or machines).

## Contact and Pricing
Use of TraceCLI requires a license to Marymoor Studios Core Libraries.  Both **free** non-commercial use licenses, and
inexpensive paid commercial licenses are available.  Explore license purchase options at the 
[Marymoor Studios Online Store][store]. Or, contact Marymoor Studios, LLC at info@marymoorstudios.com to inquire about
additional license pricing and purchase options.

## Logging
To use TraceCLI to monitor applications, they must emit their logs using an `ILogger` from the 
[Microsoft.Extensions.Logging][LoggingDocs] framework and attach an EventSource provider.

1. First add the required packages to your application (if they aren't added already)

    ```sh
    dotnet add package Microsoft.Extensions.Logging
    dotnet add package Microsoft.Extensions.Logging.EventSource
    ```

2. Add Logging

    The most efficient mechanism for logging using an `ILogger` is to use 
    [compile-time logging source generation][compile-time-logging] which leverages Rosyln code generation to generated
    strongly typed methods for each logging event.  The generated code handles all of the details necessary to avoid
    unnecessary copying and string allocation when logging is turned off.  A simple logger might look like:

    ```cs
      private sealed partial class Logger
      {
        private readonly ILogger<Logger> m_logger;

        public Logger(ILoggerFactory factory)
        {
          m_logger = factory.CreateLogger<Logger>();
          Contract.Unused(m_logger);
        }

        [LoggerMessage(EventId = 1, Message = "{msg}")]
        public partial void Test(LogLevel level, string msg, Exception? ex = default);
      }
    ```

    The logging can then be instantiated with a `ILoggingFactory` and used to emit logs.  For example:

    ```cs
        using ILoggerFactory loggerFactory = LoggerFactory.Create(loggingBuilder => loggingBuilder
         .SetMinimumLevel(LogLevel.Debug)
         .AddEventSourceLogger();

        Logger logger = new(loggerFactory);
        logger.Test(LogLevel.Debug, "Debug message");
        logger.Test(LogLevel.Information, "Info message");
        logger.Test(LogLevel.Warning, "Warning message");
        try
        {
          throw new AbortedException("Some error string");
        }
        catch (AbortedException ex)
        {
          logger.Test(LogLevel.Error, "Error message", ex);
        }
        logger.Test(LogLevel.Critical, "Critical message");
    ```

3. Install TraceCLI as a tool:

    ```sh
    dotnet tool install --global --prerelease MarymoorStudios.Core.Rpc.TraceCli
    ```

4. Invoke the tool:

    ```
    tracecli live cli
    ```

    When you launch TraceCLI it will require you to elevate to admin privileges.  This is required because TraceCLI will
    be able to monitor logging stream from **any application** running on the computer where it is executed.  Only an
    admin should have the power to do this.  

    After accepting the elevation prompt, the CLI window will open in _display mode_.  Hit any key in the CLI window
    to access the TraceCLI interactive shell.  From the shell you can interactively change the filtering settings.
    Logging display will be paused while the _interactive shell mode_ is active.  When you are ready to return to
    _display mode_ simple hit `enter` (an empty command line in the shell) and monitoring will resume.  (Events that
    were sent while monitoring was paused will be buffered in memory and displayed upon resume.  To avoid large memory
    consumption, never leave TraceCLI in _interactive shell mode_ for extended periods.)

5. Run your application.

    Logging produced by your application will appear in the TraceCLI window (sometimes with a slight delay of a few
    seconds due to EventSource internal buffering).

    Use the _interactive shell mode_ (or command line switches) to set filters to restrict the view to only the
    applications you are interested in monitoring.  Multiple applications can be monitored at the same time.  Use the
    _Column Filter_ (cf) to control which columns to display in the output.

## Links
* [Documentation](https://github.com/MarymoorStudios/Core)
* [License](https://github.com/MarymoorStudios/Core/blob/main/LICENSE.md)

[logo]: https://raw.githubusercontent.com/MarymoorStudios/Core/main/Images/Marymoor%20Studios%20Logo%20NM%2064x64.png
[store]: https://marymoorstudios.square.site/
[LoggingDocs]: https://learn.microsoft.com/en-us/dotnet/core/extensions/logging
[EventSourceLogger]: https://learn.microsoft.com/en-us/dotnet/core/extensions/logging-providers#event-source
[dotnet-trace]: https://learn.microsoft.com/en-us/dotnet/core/diagnostics/dotnet-trace
[compile-time-logging]: https://learn.microsoft.com/en-us/dotnet/core/extensions/logger-message-generator
