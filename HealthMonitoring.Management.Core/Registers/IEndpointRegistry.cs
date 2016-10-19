﻿using System;
using System.Collections.Generic;
using HealthMonitoring.Model;

namespace HealthMonitoring.Management.Core.Registers
{
    public interface IEndpointRegistry
    {
        Guid RegisterOrUpdate(string monitorType, string address, string group, string name, string[] tags, string token);
        bool TryUpdateEndpointTags(Guid id, string[] tags);
        Endpoint GetById(Guid id);
        Endpoint GetByNaturalKey(string monitorType, string address);
        bool TryUnregisterById(Guid id);
        IEnumerable<Endpoint> Endpoints { get; }
        bool UpdateHealth(Guid endpointId, EndpointHealth health);
        event Action<Endpoint> EndpointAdded;
    }
}