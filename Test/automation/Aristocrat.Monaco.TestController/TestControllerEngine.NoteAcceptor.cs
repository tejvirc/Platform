namespace Aristocrat.Monaco.TestController
{
    using System;
    using System.Collections.Generic;
    using Application.Contracts;
    using DataModel;
    using Hardware.Contracts;
    using Hardware.Contracts.IO;
    using Hardware.Contracts.NoteAcceptor;
    using Hardware.Gds;
    using Hardware.NoteAcceptor;
    using Kernel;
    using Kernel.Contracts;
    using Microsoft.AspNetCore.Mvc;
    using TestController.Models.Request;

    public partial class TestControllerEngine
    {
        private const string NoteAcceptorConnectMessage = "NoteAcceptorConnect";

        private const string NoteAcceptorDisconnectMessage = "NoteAcceptorDisconnect";

        public Dictionary<string, object> NoteAcceptorStatus(string id)
        {
            var bna = ServiceManager.GetInstance().TryGetService<INoteAcceptor>();

            return new Dictionary<string, object>
            {
                ["response-to"] = $"/BNA/{id}/Status",
                ["status"] = bna.Enabled ? "Enabled" : "Disabled"
            };
        }

        public Dictionary<string, object> NoteAcceptorGetMask(string id)
        {
            var bna = ServiceManager.GetInstance().TryGetService<INoteAcceptor>();

            var bitMask = 0;

            if (bna == null)
            {
                return new Dictionary<string, object>
                {
                    ["response-to"] = $"/BNA/{id}/Bill/Mask/Get",
                    ["Mask"] = bitMask.ToString(),
                    ["error"] = "Note acceptor service unavailable"
                };
            }

            var denoms = bna.GetSupportedNotes();


            if ((bool)_pm.GetProperty(PropertyKey.VoucherIn, false))
            {
                bitMask |= (int)Math.Pow(2, 30);
            }

            for (int i = 0; i < denoms.Count; ++i)
            {
                if (bna.GetSupportedNotes().Contains(denoms[i]))
                {
                    bitMask |= i ^ 2;
                }
            }

            return new Dictionary<string, object>
            {
                ["response-to"] = $"/BNA/{id}/Bill/Mask/Get",
                ["Mask"] = bitMask.ToString()
            };
        }

        public Dictionary<string, object> NoteAcceptorSetMask(string id, NoteAcceptorSetMaskRequest request)
        {
            var bna = ServiceManager.GetInstance().TryGetService<INoteAcceptor>();

            Int32.TryParse(request.Mask, out int value);

            if (bna == null)
            {
                return new Dictionary<string, object>
                {
                    ["response-to"] = $"/BNA/{id}/Mask/Set",
                    ["error"] = "Note acceptor service unavailable"
                };
            }

            var denoms = bna.GetSupportedNotes();

            for (var i = 0; i < denoms.Count; ++i)
            {
                bna.UpdateDenom(denoms[i], (value & i ^ 2) > 0);
            }

            //31st bit is for vouchers.
            _pm.SetProperty(PropertyKey.VoucherIn, ((2 ^ 31) & value) != 0);

            return new Dictionary<string, object>
            {
                ["response-to"] = $"/BNA/{id}/Mask/Set"
            };
        }

        public Dictionary<string, object> NoteAcceptorGetNotes(string id)
        {
            var bna = ServiceManager.GetInstance().TryGetService<INoteAcceptor>();

            if (bna == null)
            {
                return new Dictionary<string, object>
                {
                    ["response-to"] = $"/BNA/{id}/Bill/Notes/Get",
                    ["Notes"] = "",
                    ["error"] = "Note acceptor service unavailable"
                };
            }

            var currencyCode = _pm.GetValue(ApplicationConstants.CurrencyId, ApplicationConstants.DefaultCurrencyId);
            var supportedNotes = bna.GetSupportedNotes(currencyCode);
            var notes = string.Empty;

            foreach (var note in supportedNotes)
            {
                notes = notes + note.ToString() + " ";
            }

            return new Dictionary<string, object>
            {
                ["response-to"] = $"/BNA/{id}/Bill/Notes/Get",
                ["Notes"] = notes
            };
        }

        public Dictionary<string, object> NoteAcceptorCheat(string id)
        {
            _eventBus.Publish(new FakeNoteAcceptorEvent { Cheat = true });

            return new Dictionary<string, object>
            {
                ["response-to"] = $"/BNA/{id}/Cheat",
                ["Command"] = "NoteAcceptorCheat"
            };
        }

        public Dictionary<string, object> NoteAcceptorConnect(string id)
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

            return new Dictionary<string, object>
            {
                [ResponseTo] = $"/BNA/{id}/Connect",
                ["Command"] = NoteAcceptorConnectMessage
            };
        }

        public Dictionary<string, object> NoteAcceptorDisconnect(string id)
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

            return new Dictionary<string, object>
            {
                [ResponseTo] = $"/BNA/{id}/Disconnect",
                ["Command"] = NoteAcceptorDisconnectMessage
            };
        }

        public Dictionary<string, object> NoteAcceptorGetFirmware(string id)
        {
            var bna = ServiceManager.GetInstance().GetService<IDeviceRegistryService>().GetDevice<INoteAcceptor>();

            string currencyId = (string)_pm.GetProperty(ApplicationConstants.CurrencyId, "");
            return new Dictionary<string, object>
            {
                ["response-to"] = $"/BNA/{id}/Firmware/Get",
                ["FirmwareId"] = currencyId,
                ["Command"] = "NoteAcceptorGetFirmware - Not Implemented."
            };
        }

        public Dictionary<string, object> NoteAcceptorSetFirmware(
            string id,
            NoteAcceptorSetFirmwareRequest request)
        {
            return new Dictionary<string, object>
            {
                ["response-to"] = $"/BNA/{id}/Firmware/Set",
                ["error"] = "Method not implemented.",
                ["Command"] = "NoteAcceptorSetFirmware - Not Implemented."
            };
        }

        public Dictionary<string, object> NoteAcceptorAttachStacker(string id)
        {
            _eventBus.Publish(new FakeStackerEvent { Disconnect = false, Full = false });
            return new Dictionary<string, object>
            {
                ["response-to"] = $"/BNA/{id}/Stacker/Attach",
                ["Command"] = "NoteAcceptorAttachStacker"
            };
        }

        public Dictionary<string, object> NoteAcceptorStackerFull(string id)
        {
            _eventBus.Publish(new FakeStackerEvent { Full = true });
            return new Dictionary<string, object>
            {
                ["response-to"] = $"/BNA/{id}/Stacker/Full",
                ["Command"] = "NoteAcceptorStackerFull"
            };
        }

        public Dictionary<string, object> NoteAcceptorStackerJam(string id)
        {
            _eventBus.Publish(new FakeStackerEvent { Jam = true });
            return new Dictionary<string, object>
            {
                ["response-to"] = $"/BNA/{id}/Stacker/Jam",
                ["Command"] = "NoteAcceptorStackerJam"
            };
        }

        public Dictionary<string, object> NoteAcceptorStackerRemove(string id)
        {
            _eventBus.Publish(new FakeStackerEvent { Disconnect = true });
            return new Dictionary<string, object>
            {
                ["response-to"] = $"/BNA/{id}/Stacker/Remove",
                ["Command"] = "NoteAcceptorStackerRemove"
            };
        }

        public Dictionary<string, object> NoteAcceptorInsertTicket(string validation_id, string id)
        {
            var noteAcceptor = ServiceManager.GetInstance().GetService<IDeviceRegistryService>().GetDevice<INoteAcceptor>();
            if (noteAcceptor != null)
            {
                _eventBus.Publish(new VoucherEscrowedEvent(noteAcceptor.NoteAcceptorId, validation_id));
            }

            return new Dictionary<string, object>
            {
                ["response-to"] = $"/BNA/{id}/Ticket/Insert",
                ["Command"] = "NoteAcceptorInsertTicket"
            };
        }

        public Dictionary<string, object> NoteAcceptorEscrowStatus(string id)
        {
            var bna = ServiceManager.GetInstance().GetService<IDeviceRegistryService>().GetDevice<INoteAcceptor>();
            return new Dictionary<string, object>
            {
                ["response-to"] = $"/BNA/{id}/Escrow/Status",
                ["escrowstatus"] = bna.LastDocumentResult.ToString()
            };
        }
    }
}