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
    using System;
    using System.Collections.Generic;
    using Application.Contracts;
    using Microsoft.AspNetCore.Mvc;
    using Aristocrat.Monaco.TestController.Models.Request;

    public partial class TestControllerEngine
    {
        private const string NoteAcceptorConnectMessage = "NoteAcceptorConnect";

        private const string NoteAcceptorDisconnectMessage = "NoteAcceptorDisconnect";

        [HttpGet]
        [Route("BNA/{id}/Status")]
        public ActionResult<CommandResult> NoteAcceptorStatus([FromRoute] string id)
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

        [HttpGet]
        [Route("BNA/{id}/Bill/Mask/Get")]
        public ActionResult<CommandResult> NoteAcceptorGetMask([FromRoute] string id)
        {
            var bna = ServiceManager.GetInstance().TryGetService<INoteAcceptor>();

            var bitMask = 0;

            if (bna == null)
            {
                return new CommandResult
                {
                    data = new Dictionary<string, object>
                    {
                        ["response-to"] = $"/BNA/{id}/Bill/Mask/Get",
                        ["Mask"] = bitMask.ToString(),
                        ["error"] = "Note acceptor service unavailable"
                    },
                    Result = false
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

            return new CommandResult
            {
                data = new Dictionary<string, object>
                {
                    ["response-to"] = $"/BNA/{id}/Bill/Mask/Get",
                    ["Mask"] = bitMask.ToString()
                },
                Result = true
            };
        }

        [HttpPost]
        [Route("BNA/{id}/Bill/Mask/Set")]
        public ActionResult<CommandResult> NoteAcceptorSetMask([FromRoute] string id, [FromBody] NoteAcceptorSetMaskRequest request)
        {
            var bna = ServiceManager.GetInstance().TryGetService<INoteAcceptor>();

            Int32.TryParse(request.Mask, out int value);

            if (bna == null)
            {
                return new CommandResult
                {
                    data = new Dictionary<string, object> { ["response-to"] = $"/BNA/{id}/Mask/Set", ["error"] = "Note acceptor service unavailable" },
                    Result = false
                };
            }

            var denoms = bna.GetSupportedNotes();

            for (var i = 0; i < denoms.Count; ++i)
            {
                bna.UpdateDenom(denoms[i], (value & i ^ 2) > 0);
            }

            //31st bit is for vouchers.
            _pm.SetProperty(PropertyKey.VoucherIn, ((2 ^ 31) & value) != 0);

            return new CommandResult
            {
                data = new Dictionary<string, object> { ["response-to"] = $"/BNA/{id}/Mask/Set" },
                Result = true
            };
        }

        [HttpGet]
        [Route("BNA/{id}/Bill/Notes/Get")]
        public ActionResult<CommandResult> NoteAcceptorGetNotes([FromRoute] string id)
        {
            var bna = ServiceManager.GetInstance().TryGetService<INoteAcceptor>();

            if (bna == null)
            {
                return new CommandResult
                {
                    data = new Dictionary<string, object>
                    {
                        ["response-to"] = $"/BNA/{id}/Bill/Notes/Get",
                        ["Notes"] = "",
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
                    ["response-to"] = $"/BNA/{id}/Bill/Notes/Get",
                    ["Notes"] = notes
                },
                Result = true
            };

        }

        [HttpPost]
        [Route("BNA/{id}/Cheat")]
        public ActionResult<CommandResult> NoteAcceptorCheat([FromRoute] string id)
        {
            _eventBus.Publish(new FakeNoteAcceptorEvent { Cheat = true });

            return new CommandResult
            {
                data = new Dictionary<string, object> { ["response-to"] = $"/BNA/{id}/Cheat" },
                Command = "NoteAcceptorCheat",
                Result = true
            };
        }

        [HttpPost]
        [Route("BNA/{id}/Connect")]
        public ActionResult<CommandResult> NoteAcceptorConnect([FromRoute] string id)
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

        [HttpPost]
        [Route("BNA/{id}/Disconnect")]
        public ActionResult<CommandResult> NoteAcceptorDisconnect([FromRoute] string id)
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

        [HttpPost]
        [Route("BNA/{id}/Firmware/Get")]
        public ActionResult<CommandResult> NoteAcceptorGetFirmware([FromRoute] string id)
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

        [HttpPost]
        [Route("BNA/{id}/Firmware/Set")]
        public ActionResult<CommandResult> NoteAcceptorSetFirmware(
            [FromRoute] string id,
            [FromBody] NoteAcceptorSetFirmwareRequest request)
        {
            return new CommandResult
            {
                data = new Dictionary<string, object>
                {
                    ["response-to"] = $"/BNA/{id}/Firmware/Set",
                    ["error"] = "Method not implemented."
                },
                Command = "NoteAcceptorSetFirmware - Not Implemented."
            };
        }

        [HttpPost]
        [Route("BNA/{id}/Stacker/Attach")]
        public ActionResult<CommandResult> NoteAcceptorAttachStacker([FromRoute] string id)
        {
            _eventBus.Publish(new FakeStackerEvent { Disconnect = false, Full = false });
            return new CommandResult
            {
                data = new Dictionary<string, object>
                {
                    ["response-to"] = $"/BNA/{id}/Stacker/Attach",
                },
                Command = "NoteAcceptorAttachStacker",
                Result = true
            };
        }

        [HttpPost]
        [Route("BNA/{id}/Stacker/Full")]
        public ActionResult<CommandResult> NoteAcceptorStackerFull([FromRoute] string id)
        {
            _eventBus.Publish(new FakeStackerEvent { Full = true });
            return new CommandResult
            {
                data = new Dictionary<string, object>
                {
                    ["response-to"] = $"/BNA/{id}/Stacker/Full",
                },
                Command = "NoteAcceptorStackerFull",
                Result = true
            };
        }

        public CommandResult NoteAcceptorStackerJam(string id)
        {
            _eventBus.Publish(new FakeStackerEvent { Jam = true });
            return new CommandResult
            {
                data = new Dictionary<string, object>
                {
                    ["response-to"] = $"/BNA/{id}/Stacker/Jam",
                },
                Command = "NoteAcceptorStackerJam",
                Result = true
            };
        }

        [HttpPost]
        [Route("BNA/{id}/Stacker/Remove")]
        public ActionResult<CommandResult> NoteAcceptorStackerRemove([FromRoute] string id)
        {
            _eventBus.Publish(new FakeStackerEvent { Disconnect = true });
            return new CommandResult
            {
                data = new Dictionary<string, object>
                {
                    ["response-to"] = $"/BNA/{id}/Stacker/Remove",
                },
                Command = "NoteAcceptorStackerRemove",
                Result = true
            };
        }

        public CommandResult NoteAcceptorInsertTicket(string validation_id, string id)
        {
            var noteAcceptor = ServiceManager.GetInstance().GetService<IDeviceRegistryService>().GetDevice<INoteAcceptor>();
            if (noteAcceptor != null)
            {
                _eventBus.Publish(new VoucherEscrowedEvent(noteAcceptor.NoteAcceptorId, validation_id));
            }

            return new CommandResult
            {
                data = new Dictionary<string, object>
                {
                    ["response-to"] = $"/BNA/{id}/Ticket/Insert",
                },
                Command = "NoteAcceptorInsertTicket",
                Result = noteAcceptor != null
            };
        }

        [HttpPost]
        [Route("BNA/{id}/Escrow/Status")]
        public ActionResult<CommandResult> NoteAcceptorEscrowStatus([FromRoute] string id)
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
