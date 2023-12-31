﻿using System;
using System.Collections.Generic;

namespace Aristocrat.Monaco.TestController
{
    using DataModel;
    using Gaming.Contracts;
    using Hardware.Contracts.IO;
    using Kernel;

    public class IoManager
    {
        public void SetInput(string input, bool high)
        {
            if (!Enum.TryParse<SharedInput>(input, out var sharedInput))
            {
                return;
            }

            if (HandleSpecialCases(sharedInput))
            {
                return;
            }

            var io = ServiceManager.GetInstance().TryGetService<IIO>();

            if (io == null)
            {
                return;
            }

            var id = _inputMap[sharedInput];

            var evtBus = ServiceManager.GetInstance().TryGetService<IEventBus>();

            evtBus?.Publish(new InputEvent(id, high));

            io.SetOutput32(id, high, false);

            if (high)
            {
                _override[sharedInput] = true;
            }
            else
            {
                _override.Remove(sharedInput);
            }
        }

        public Dictionary<string,string> GetInputs()
        {
            Dictionary<string,string> state = new Dictionary<string, string>();

            var inputs = (int)ServiceManager.GetInstance().GetService<IIO>().GetInputs;

            foreach (var input in _inputMap)
            {
                if (_inputMap[input.Key] < 0)
                {
                    continue;
                }

                if (!_inputMap.ContainsKey(input.Key))
                {
                    continue;
                }

                //report the override if it exists
                if (_override.ContainsKey(input.Key))
                {
                    state[Enum.GetName(typeof(SharedInput), input.Key)] = _override[input.Key] ? "1" : "0";
                }
                else
                {
                    state[Enum.GetName(typeof(SharedInput), input.Key)] = ((int)Math.Pow(_inputMap[input.Key], 2) & inputs) != 0 ? "1" : "0";
                }
            }

            return state;
        }

        private bool HandleSpecialCases(SharedInput input)
        {
            switch (input)
            {
                case SharedInput.ReserveKey:
                {
                    
                    break;
                }
            }
            return false;
        }

        /// <summary>
        /// Current IO overrides
        /// </summary>
        private readonly Dictionary<SharedInput, bool> _override = new Dictionary<SharedInput, bool>();

        /// <summary>
        /// Map of SharedInput enum to physical id
        /// </summary>
        private readonly Dictionary<SharedInput, int> _inputMap = new Dictionary<SharedInput, int>
        {
            [SharedInput.ReserveKey] = 30,
            [SharedInput.CollectKey] = 29,
            [SharedInput.MaxLineKey] = -1,
            [SharedInput.PlayKey] = 22,
            [SharedInput.Linex1Key] = 30,
            [SharedInput.Linex2Key] = 31,
            [SharedInput.Linex3Key] = 32,
            [SharedInput.Linex4Key] = 33,
            [SharedInput.Linex5Key] = 34,
            [SharedInput.Betx1Key] = 27,
            [SharedInput.Betx2Key] = 26,
            [SharedInput.Betx3Key] = 25,
            [SharedInput.Betx4Key] = 24,
            [SharedInput.Betx5Key] = 23,
            [SharedInput.Betx6Key] = -1,
            [SharedInput.Betx7Key] = -1,
            [SharedInput.Betx8Key] = -1,
            [SharedInput.Betx9Key] = -1,
            [SharedInput.Betx10Key] = -1,
            [SharedInput.JackpotKey] = 1,
            [SharedInput.AuditKey] = 2,
            [SharedInput.OperatorKey3] = 3,
            [SharedInput.OperatorKey4] = 4,
            [SharedInput.OperatorKey5] = 5,
            [SharedInput.LogicDoor] = 45,
            [SharedInput.TopBoxDoor] = 46,
            [SharedInput.MeterDoor] = 4,
            [SharedInput.CashboxDoor] = 50,
            [SharedInput.MainDoor] = 49,
            [SharedInput.NoteDoor] = 48,
            [SharedInput.BellyDoor] = 51,
        };

        private readonly Dictionary<SharedInput, Type> _specialCases = new Dictionary<SharedInput, Type>
        {
            [SharedInput.ReserveKey] = typeof(CallAttendantButtonOnEvent),
        };
    }
}
