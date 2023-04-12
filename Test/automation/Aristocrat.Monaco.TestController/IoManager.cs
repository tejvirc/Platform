namespace Aristocrat.Monaco.TestController
{
    using System;
    using System.Collections.Generic;
    using DataModel;
    using Hardware.Contracts.IO;
    using Kernel;

    public class IoManager
    {
        private readonly IIO _iio = ServiceManager.GetInstance().GetService<IIO>();

        /// <summary>
        ///     Set an IO override for a given input
        /// </summary>
        public void SetInput(string input, bool high)
        {
            if (!Enum.TryParse<SharedInput>(input, out var sharedInput))
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

        /// <summary>
        ///     Get the current state of all inputs
        /// </summary>
        public Dictionary<string, string> GetInputs()
        {
            Dictionary<string, string> state = new Dictionary<string, string>();
            var inputs = _iio.GetInputs;

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

                // Report the override if it exists
                if (_override.ContainsKey(input.Key))
                {
                    state[input.Key.ToString()] = _override[input.Key] ? "1" : "0";
                }
                else
                {
                    state[input.Key.ToString()] = ((1ul << _inputMap[input.Key]) & inputs) != 0 ? "1" : "0";
                }
            }

            return state;
        }

        /// <summary>
        ///     Current IO overrides
        /// </summary>
        private readonly Dictionary<SharedInput, bool> _override = new Dictionary<SharedInput, bool>();

        /// <summary>
        ///     Map of SharedInput enum to physical ID
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
    }
}