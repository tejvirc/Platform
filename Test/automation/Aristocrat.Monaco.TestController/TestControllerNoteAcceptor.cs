namespace Aristocrat.Monaco.TestController
{
    using DataModel;
    using Hardware.Contracts;
    using Hardware.Contracts.IO;
    using Hardware.Contracts.NoteAcceptor;
    using Hardware.Gds;
    using Hardware.NoteAcceptor;
    using Kernel;
    using Kernel.Contracts;
    using System;
    using System.Collections.Generic;
    using Application.Contracts;

    public partial class TestControllerEngine : ITestController
    {
        private const string NoteAcceptorConnectMessage = "NoteAcceptorConnect";

        private const string NoteAcceptorDisconnectMessage = "NoteAcceptorDisconnect";

        public CommandResult NoteAcceptorStatus(string id)
        {

            var bna = ServiceManager.GetInstance().TryGetService<INoteAcceptor>();

            return new CommandResult
            {
                data = new Dictionary<string, object>
                {
                    ["response-to"] = $"/BNA/{id}/Status",
                    ["status"] = bna.Enabled ? "Enabled" : "Disabled"
                }
            };
        }

        public CommandResult NoteAcceptorGetMask(string id)
        {
            var bna = ServiceManager.GetInstance().TryGetService<INoteAcceptor>();

            var bitMask = 0;

            if (bna == null)
            {
                return new CommandResult
                {
                    data = new Dictionary<string, object>
                    {
                        ["response-to"] = $"/BNA/{id}/Bill/Mask/Get", ["Mask"] = bitMask.ToString(),
                        ["error"] = "Note acceptor service unavailable"
                    },
                    Result = false
                };
            }

            var denoms = bna.GetSupportedNotes();
            

            if ((bool)_pm.GetProperty(PropertyKey.VoucherIn, false))
            {
                bitMask |= (int)Math.Pow(2,30);
            }

            for (int i = 0; i < denoms.Count; ++i)
            {
                if (bna.GetSupportedNotes().Contains(denoms[i]))
                {
                    bitMask |= i ^ 2;
                }
            }
           
            return new CommandResult
            {
                data = new Dictionary<string, object>
                {
                    ["response-to"] = $"/BNA/{id}/Bill/Mask/Get", ["Mask"] = bitMask.ToString()
                },
                Result = true
            };
        }

        public CommandResult NoteAcceptorSetMask(string id, string mask)
        {
            var bna = ServiceManager.GetInstance().TryGetService<INoteAcceptor>();

            Int32.TryParse(mask, out int value);

            if (bna == null)
            {
                return new CommandResult
                {
                    data = new Dictionary<string, object> { ["response-to"] = $"/BNA/{id}/Mask/Set", ["error"] = "Note acceptor service unavailable" }, Result = false
                };
            }

            var denoms = bna.GetSupportedNotes();

            for (var i = 0; i < denoms.Count; ++i)
            {
                bna.UpdateDenom(denoms[i], (value & i ^2) > 0);
            }

            //31st bit is for vouchers.
            _pm.SetProperty(PropertyKey.VoucherIn, ((2 ^ 31) & value) != 0);

            return new CommandResult
            {
                data = new Dictionary<string, object> { ["response-to"] = $"/BNA/{id}/Mask/Set" },
                Result = true
            };
        }

        public CommandResult NoteAcceptorGetNotes(string id)
        {
            var bna = ServiceManager.GetInstance().TryGetService<INoteAcceptor>();

            if (bna == null)
            {
                return new CommandResult
                {
                    data = new Dictionary<string, object>
                    {
                        ["response-to"] = $"/BNA/{id}/Bill/Notes/Get", ["Notes"] = "",
                        ["error"] = "Note acceptor service unavailable"
                    },
                    Result = false
                };
            }
            
            var currencyCode = _pm.GetValue(ApplicationConstants.CurrencyId, ApplicationConstants.DefaultCurrencyId);
            var supportedNotes = bna.GetSupportedNotes(currencyCode);
            var notes = string.Empty;

            foreach (var note in supportedNotes)
            {
                notes = notes + note.ToString() + " ";
            }

            return new CommandResult
            {
                data = new Dictionary<string, object>
                {
                    ["response-to"] = $"/BNA/{id}/Bill/Notes/Get", ["Notes"] = notes
                },
                Result = true
            };

        }

        public CommandResult NoteAcceptorCheat(string id)
        {
            _eventBus.Publish(new FakeNoteAcceptorEvent { Cheat = true });

            return new CommandResult
            {
                data = new Dictionary<string, object> { ["response-to"] = $"/BNA/{id}/Cheat" },
                Command = "NoteAcceptorCheat",
                Result = true
            };
        }

        public CommandResult NoteAcceptorConnect(string id)
        {
            bool result = false;
            var noteAcceptor = ServiceManager.GetInstance().GetService<INoteAcceptor>();

            if (noteAcceptor != null)
            {
                NoteAcceptorAdapter adapter = noteAcceptor as NoteAcceptorAdapter;
                GdsDeviceBase device = adapter?.NoteAcceptorImplementation as GdsDeviceBase;

                if (device != null)
                {
                    device.Open();
                    result = device.Initialize(device.Communicator).Result;
                }
            }

            return new CommandResult
            {
                data = new Dictionary<string, object> { [ResponseTo] = $"/BNA/{id}/Connect" },
                Command = NoteAcceptorConnectMessage,
                Result = result
            };
        }

        public CommandResult NoteAcceptorDisconnect(string id)
        {
            bool result = false;
            var noteAcceptor = ServiceManager.GetInstance().GetService<INoteAcceptor>();

            if (noteAcceptor != null)
            {
                NoteAcceptorAdapter adapter = noteAcceptor as NoteAcceptorAdapter;
                GdsDeviceBase device = adapter?.NoteAcceptorImplementation as GdsDeviceBase;

                if (device != null)
                {
                    result = device.Close();
                }
            }

            return new CommandResult
            {
                data = new Dictionary<string, object> { [ResponseTo] = $"/BNA/{id}/Disconnect" },
                Command = NoteAcceptorDisconnectMessage,
                Result = result
            };
        }

        public CommandResult NoteAcceptorGetFirmware(string id)
        {
            var bna = ServiceManager.GetInstance().GetService<IDeviceRegistryService>().GetDevice<INoteAcceptor>();

            string currencyId = (string)_pm.GetProperty(ApplicationConstants.CurrencyId, "");
            return new CommandResult
            {
                data = new Dictionary<string, object>
                {
                    ["response-to"] = $"/BNA/{id}/Firmware/Get",
                    ["FirmwareId"] = currencyId

                },
                Command = "NoteAcceptorGetFirmware - Not Implemented."
            };
        }

        public CommandResult NoteAcceptorSetFirmware(string contents, string id)
        {
            return new CommandResult
            {
                data = new Dictionary<string, object>
                {
                    ["response-to"] = $"/BNA/{id}/Firmware/Set", ["error"] = "Method not implemented."
                },
                Command = "NoteAcceptorSetFirmware - Not Implemented."
            };
        }

        public CommandResult NoteAcceptorAttachStacker(string id)
        {
            _eventBus.Publish(new FakeStackerEvent { Disconnect = false, Full = false });
            return new CommandResult{
                data = new Dictionary<string, object>
                {
                    ["response-to"] = $"/BNA/{id}/Stacker/Attach",
                },
                Command = "NoteAcceptorAttachStacker", Result = true};
        }

        public CommandResult NoteAcceptorStackerFull(string id)
        {
            _eventBus.Publish(new FakeStackerEvent { Full = true });
            return new CommandResult {
                data = new Dictionary<string, object>
                {
                    ["response-to"] = $"/BNA/{id}/Stacker/Full",
                },
                Command = "NoteAcceptorStackerFull", Result = true };
        }

        public CommandResult NoteAcceptorStackerRemove(string id)
        {
            _eventBus.Publish(new FakeStackerEvent { Disconnect = true });
            return new CommandResult {
                data = new Dictionary<string, object>
                {
                    ["response-to"] = $"/BNA/{id}/Stacker/Remove",
                },
                Command = "NoteAcceptorStackerRemove", Result = true };
        }

        public CommandResult NoteAcceptorInsertTicket(string validation_id, string id)
        {
            var noteAcceptor = ServiceManager.GetInstance().GetService<IDeviceRegistryService>().GetDevice<INoteAcceptor>();
            if (noteAcceptor != null)
            {
                _eventBus.Publish(new VoucherEscrowedEvent(noteAcceptor.NoteAcceptorId, validation_id));
            }

            return new CommandResult {
                data = new Dictionary<string, object>
                {
                    ["response-to"] = $"/BNA/{id}/Ticket/Insert",
                },
                Command = "NoteAcceptorInsertTicket",
                Result = noteAcceptor != null
            };
        }

        public CommandResult NoteAcceptorEscrowStatus(string id)
        {
            var bna = ServiceManager.GetInstance().GetService<IDeviceRegistryService>().GetDevice<INoteAcceptor>();

            return new CommandResult
            {
                data = new Dictionary<string, object>
                {
                    ["response-to"] = $"/BNA/{id}/Escrow/Status",
                    ["escrowstatus"] = bna.LastDocumentResult.ToString()
                }
            };
        }
    }
}
