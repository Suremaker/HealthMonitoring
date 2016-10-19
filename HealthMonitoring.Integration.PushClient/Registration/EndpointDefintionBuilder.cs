using System;
using System.Net;
using System.Net.NetworkInformation;

namespace HealthMonitoring.Integration.PushClient.Registration
{
    internal class EndpointDefintionBuilder : IEndpointDefintionBuilder
    {
        private string _groupName;
        private string _endpointName;
        private string[] _tags;
        private string _address;
        private string _authenticationToken;

        public IEndpointDefintionBuilder DefineGroup(string groupName)
        {
            if (string.IsNullOrWhiteSpace(groupName))
                throw new ArgumentException("Value cannot be empty", nameof(groupName));
            _groupName = groupName;
            return this;
        }

        public IEndpointDefintionBuilder DefineName(string endpointName)
        {
            if (string.IsNullOrWhiteSpace(endpointName))
                throw new ArgumentException("Value cannot be empty", nameof(endpointName));
            _endpointName = endpointName;
            return this;
        }

        public IEndpointDefintionBuilder DefineTags(string[] tags)
        {
            _tags = tags;
            return this;
        }

        public IEndpointDefintionBuilder DefineAddress(string endpointUniqueName)
        {
            return DefineAddress(InferMachineFQDN(), endpointUniqueName);
        }

        private static string InferMachineFQDN()
        {
            var domainName = IPGlobalProperties.GetIPGlobalProperties().DomainName;
            var hostName = Dns.GetHostName();

            domainName = "." + domainName;
            if (!hostName.EndsWith(domainName))
            {
                hostName += domainName;
            }

            return hostName;
        }

        public IEndpointDefintionBuilder DefineAddress(string host, string endpointUniqueName)
        {
            if (string.IsNullOrWhiteSpace(host))
                throw new ArgumentException("Value cannot be empty", nameof(host));

            if (string.IsNullOrWhiteSpace(endpointUniqueName))
                throw new ArgumentException("Value cannot be empty", nameof(endpointUniqueName));

            _address = $"{host}:{endpointUniqueName}";
            return this;
        }

        public IEndpointDefintionBuilder DefineAuthenticationToken(string authenticationToken)
        {
            if (string.IsNullOrWhiteSpace(authenticationToken))
                throw new ArgumentException("Value cannot be empty", nameof(authenticationToken));
            _authenticationToken = authenticationToken;
            return this;
        }

        public EndpointDefinition Build()
        {
            if (string.IsNullOrWhiteSpace(_endpointName))
                throw new InvalidOperationException("No endpoint name provided");
            if (string.IsNullOrWhiteSpace(_groupName))
                throw new InvalidOperationException("No endpoint group provided");
            if (string.IsNullOrWhiteSpace(_address))
                throw new InvalidOperationException("No endpoint address provided");
            if (string.IsNullOrWhiteSpace(_authenticationToken))
                throw new InvalidOperationException("No endpoint authentication token provided");

            return new EndpointDefinition(_address, _groupName, _endpointName, _tags, _authenticationToken);
        }
    }
}