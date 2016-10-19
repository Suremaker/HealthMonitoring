
using System;

namespace HealthMonitoring.Security
{
    public class Credentials
    {
        public Credentials(Guid id, string password)
        {
            Id = id;
            Password = password;
        }

        public override bool Equals(object obj)
        {
            Credentials cred = obj as Credentials;

            return !ReferenceEquals(null, cred)
                   && string.Equals(Password, cred.Password)
                   && Id == cred.Id;
        }

        public override int GetHashCode()
        {
            return Password.GetHashCode() ^ Id.GetHashCode();
        }

        public string Password { get; }
        public Guid Id { get; }
    }
}
