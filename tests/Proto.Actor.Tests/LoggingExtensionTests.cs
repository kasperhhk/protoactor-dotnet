// -----------------------------------------------------------------------
// <copyright file="LoggingExtensionTests.cs" company="Asynkron AB">
//      Copyright (C) 2015-2024 Asynkron AB All rights reserved
// </copyright>
// -----------------------------------------------------------------------

using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Proto.Context;
using Proto.Logging;
using Xunit;

namespace Proto.Tests;

public class LoggingExtensionTests
{
    [Fact]
    public async Task CanGetLoggingExtension()
    {
        var system = new ActorSystem();
        await using var _ = system;
        system.Extensions.Register(new InstanceLogger(LogLevel.Debug));

        var logger = system.Logger();

        Assert.NotNull(logger);
    }

    [Fact]
    public async Task CanLogToLogStore()
    {
        var system = new ActorSystem();
        await using var _ = system;
        var logStore = new LogStore();
        system.Extensions.Register(new InstanceLogger(LogLevel.Debug, logStore));

        var logger = system.Logger();

        Assert.Empty(logStore.GetEntries());
        logger?.LogDebug("Hello {World}", "World");

        Assert.Single(logStore.GetEntries());
    }

    [Fact]
    public async Task CanCompareEntriesInStore()
    {
        var system = new ActorSystem();
        await using var _ = system;
        var logStore = new LogStore();
        system.Extensions.Register(new InstanceLogger(LogLevel.Debug, logStore));

        var logger = system.Logger();

        logger?.LogDebug("...123....Hello...456...");
        logger?.LogDebug("...789....World...012...");

        var hello = logStore.FindEntry("Hello")!;
        Assert.NotNull(hello);

        var world = logStore.FindEntry("World")!;
        Assert.NotNull(world);

        Assert.True(hello.IsBefore(world));
    }

    [Fact]
    public async Task DoesNotCauseSideEffects()
    {
        // this is not really a logging test, its just to highlight that ?. really ignores the right-side if null

        var system = new ActorSystem();
        await using var _ = system;
        //instance logger is null
        var logger = system.Logger();

        var i = 0;

        logger?.LogDebug("hello",
            ++i); //we can pass a lot of args, call ToString etc. if logger is not enabled, it will be free

        Assert.Equal(0, i);
    }

    [Fact]
    public async Task CanLogByCategory()
    {
        var system = new ActorSystem();
        await using var _ = system;
        var logStore = new LogStore();
        system.Extensions.Register(new InstanceLogger(LogLevel.Debug, logStore));

        var logger = system.Logger()?.BeginScope<ActorContext>();

        logger?.LogDebug("....Hello..123");

        var hello = logStore.FindEntryByCategory("ActorContext", "Hello")!;

        Assert.NotNull(hello);
        Assert.Equal("ActorContext", hello.Category);
    }
}