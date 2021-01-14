﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MUNityCore.Models.Simulation.Presets
{
    public class PresetMR_TVT : ISimulationPreset
    {
        public string Id => "tvt_mr";

        public string Name => "TVT Menschenrechtsrat";

        public IEnumerable<SimulationRole> Roles
        {
            get
            {
                yield return new SimulationRole("un", "Vorsitzende(r)", SimulationRole.RoleTypes.Chairman);
                yield return new SimulationRole("ao", "Angola", SimulationRole.RoleTypes.Delegate);
                yield return new SimulationRole("cn", "China", SimulationRole.RoleTypes.Delegate);
                yield return new SimulationRole("de", "Deutschland", SimulationRole.RoleTypes.Delegate);
                yield return new SimulationRole("mn", "Mongolei", SimulationRole.RoleTypes.Delegate);
                yield return new SimulationRole("us", "Vereinigte Staaten", SimulationRole.RoleTypes.Delegate);

                yield return new SimulationRole("n2", "Greenpeace", SimulationRole.RoleTypes.Ngo);


            }
        }
    }
}
