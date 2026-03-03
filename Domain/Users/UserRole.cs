using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Users
{
    public sealed class UserRole
    {
        public string Name { get; }
        public int Value { get; }
        public string Description { get; }

        private UserRole(int value, string name, string description)
        {
            Value = value;
            Name = name;
            Description = description;
        }

        public static UserRole Employee { get; } = new(1, nameof(Employee), "Vanlig anställd - kan boka och hantera egna bokningar");
        public static UserRole Receptionist { get; } = new(2, nameof(Receptionist), "Receptionist - kan boka åt andra och hantera konflikter");

        public static IEnumerable<UserRole> GetAll() => new[] { Employee, Receptionist };

        public bool CanBookForOthers() => this == Receptionist;

        public override string ToString() => Name;
    }
}
