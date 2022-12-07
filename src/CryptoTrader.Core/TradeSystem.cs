using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CryptoTrader.Core.Infrastructure;
using CryptoTrader.Core.Models;

namespace CryptoTrader.Core
{
    public class TradeSystem
    {
        private bool started;
        private readonly List<ITradingModule> modules = new();
        private readonly List<IPreStartModule> preStartModules = new();

        public TradeState State { get; }

        public TradeSystem(TradeState state)
        {
            State = state;
        }

        public void AddModule(ITradingModule module)
        {
            if (modules.Any())
            {
                modules.Last().Next = module;
            }
            modules.Add(module);
        }

        public void AddPreStartModule(IPreStartModule preStartModule)
        {
            preStartModules.Add(preStartModule);
        }

        public async Task Start()
        {
            foreach (var preStartModule in preStartModules)
            {
                await preStartModule.Invoke(State);
            }
            started = true;
        }

        public async Task Update(string tickId)
        {
            if (!started)
                throw new InvalidOperationException("System not started");

            if (modules.Count == 0)
                return;

            State.TickId = tickId;
            await modules.First().ProcessState(State);
        }
    }
}
