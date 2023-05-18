namespace Stubs;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Aristocrat.Monaco.Accounting.Contracts;
using Aristocrat.Monaco.Gaming.Contracts;
using Aristocrat.Monaco.Gaming.Contracts.Lobby;
using Aristocrat.Monaco.Hardware.Contracts.Button;
using Aristocrat.Monaco.Hardware.Contracts.Door;
using Aristocrat.Monaco.Hardware.Contracts.IO;
using Aristocrat.Monaco.Kernel;
using Aristocrat.Monaco.Kernel.Contracts;
using log4net;
using Vgt.Client12.Testing.Tools;

public class StubGamingRunnable : IStubGamingRunner, IMessageDisplayHandler
{
    private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    private bool _keepRunning = true;

    private ConcurrentDictionary<DisplayableMessageClassification, List<DisplayableMessage>> _messages = new();

    private object _lock = new object ();

    public string Name => "StubGamingRunnable";

    public ICollection<Type> ServiceTypes => new[] { typeof(IStubGamingRunner) };

    public void Initialize()
    {
        _messages[DisplayableMessageClassification.HardError] = new List<DisplayableMessage>();
        _messages[DisplayableMessageClassification.SoftError] = new List<DisplayableMessage>();
        _messages[DisplayableMessageClassification.Informative] = new List<DisplayableMessage>();
        _messages[DisplayableMessageClassification.Diagnostic] = new List<DisplayableMessage>();

        ServiceManager.GetInstance().GetService<IMessageDisplay>().AddMessageDisplayHandler(this);
    }

    public void Run()
    {
        Logger.Info("Run started");

        var eventBus = ServiceManager.GetInstance().GetService<IEventBus>();
        var bank = ServiceManager.GetInstance().GetService<IBank>();
        var cashoutController = ServiceManager.GetInstance().GetService<ICashoutController>();

        // The NoteAcceptorCoordinator disables the note acceptor on PlatformBootedEvent.  See Aristocrat.Monaco.Gaming.addin.xml.
        // This is one of two events that cause it to remove that disable.  It would normally be posted by a removed component.
        eventBus.Publish(new LobbyInitializedEvent());
        Thread.Sleep(500);

        // Loop until told to stop, displaying info and handling user input
        while (_keepRunning)
        {
            Console.WriteLine("\n\n\n----------------------------------------------------------------------------\n\n\n");
            DisplayDisableStatus();

            Console.WriteLine();
            DisplayMessages();

            Console.WriteLine();
            var balance = bank.QueryBalance() / 1000;
            Console.WriteLine($"Balance: {balance:C}");

            Console.WriteLine();
            Console.WriteLine("Press:");
            Console.WriteLine("1 = insert $1");
            Console.WriteLine("c = collect (not working)");
            Console.WriteLine("a = open logic door");
            Console.WriteLine("s = close logic door");
            Console.WriteLine("d = open main door");
            Console.WriteLine("f = close main door");
            Console.WriteLine("q = quit (event)");
            Console.Write("? ");
            var key = Console.ReadKey().KeyChar;
            Console.WriteLine();
            Console.WriteLine();
            switch (key)
            {
                case '1': Console.WriteLine("Inserting $1"); eventBus.Publish(new DebugCoinEvent(1000));        break;
                //case 'c': Console.WriteLine("Collecting"); eventBus.Publish(new UpEvent((int)ButtonLogicalId.Collect)); break;  // gets forwarded to non-existent game
                case 'c': Console.WriteLine("Collecting"); cashoutController.GameRequestedCashout();            break;
                case 'a': Console.WriteLine("Opening logic door"); eventBus.Publish(new InputEvent(45, true));  break;
                case 's': Console.WriteLine("Closing logic door"); eventBus.Publish(new InputEvent(45, false)); break;
                case 'd': Console.WriteLine("Opening main door"); eventBus.Publish(new InputEvent(49, true));   break;
                case 'f': Console.WriteLine("Closing main door"); eventBus.Publish(new InputEvent(49, false));  break;
                case 'q': Console.WriteLine("Requesting shutdown"); eventBus.Publish(new ExitRequestedEvent(ExitAction.ShutDown)); break;
            }

            Thread.Sleep(500);
        }

        ServiceManager.GetInstance().GetService<IMessageDisplay>().RemoveMessageDisplayHandler(this);

        Logger.Info("Stopped");
    }

    public void Stop()
    {
        Logger.Info("Stopping");
        _keepRunning = false;
    }

    public void ClearMessages()
    {
        lock (_lock)
        {
            _messages[DisplayableMessageClassification.HardError].Clear();
            _messages[DisplayableMessageClassification.SoftError].Clear();
            _messages[DisplayableMessageClassification.Informative].Clear();
            _messages[DisplayableMessageClassification.Diagnostic].Clear();
        }
    }

    public void DisplayMessage(DisplayableMessage displayableMessage)
    {
        lock (_lock)
        {
            _messages[displayableMessage.Classification].Add(displayableMessage);
        }
    }

    public void DisplayStatus(string message)
    {
        Console.WriteLine(message);
    }

    public void RemoveMessage(DisplayableMessage displayableMessage)
    {
        lock (_lock)
        {
            _messages[displayableMessage.Classification].Remove(displayableMessage);
        }
    }

    private void DisplayDisableStatus()
    {
        var disableMgr = ServiceManager.GetInstance().GetService<ISystemDisableManager>();
        if (disableMgr.IsDisabled)
        {
            Console.WriteLine("Disabled by:");
            foreach (var disable in disableMgr.CurrentDisableKeys)
            {
                Console.WriteLine($"    {disable}");
            }
        }
        else
        {
            Console.WriteLine("Enabled");
        }
    }

    private void DisplayMessages()
    {
        lock (_lock)
        {
            DisplayMessages(DisplayableMessageClassification.HardError, "Hard Errors");
            Console.WriteLine();
            DisplayMessages(DisplayableMessageClassification.SoftError, "Soft Errors");
            Console.WriteLine();
            DisplayMessages(DisplayableMessageClassification.Informative, "Informative");
            Console.WriteLine();
            DisplayMessages(DisplayableMessageClassification.Diagnostic, "Diagnostic");
        }
    }

    private void DisplayMessages(DisplayableMessageClassification classification, string header)
    {
        var list = _messages[classification];
        if (list.Count == 0)
        {
            Console.WriteLine($"{header}: none");
        }
        else
        {
            Console.WriteLine($"{header}:");
            foreach (var message in list)
            {
                Console.WriteLine($"    {message.Message}");
            }
        }
    }
}
