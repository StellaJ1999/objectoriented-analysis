
using Domain.Common;
using System.ComponentModel.DataAnnotations;


namespace Domain.Rooms
{
    /// Aggregate Root
    public sealed class Room
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public int Capacity { get; private set; }
        public string? Location { get; private set; }
        public bool IsActive { get; private set; }

        // EF Core behöver en tom constructor
        private Room() { }

        public Room(string name, int capacity, string? location = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ValidationException("Rumsnamn måste anges");

            if (capacity <= 0)
                throw new ValidationException("Kapacitet måste vara större än 0");

            Id = Guid.NewGuid();
            Name = name;
            Capacity = capacity;
            Location = location;
            IsActive = true;
        }

        public void UpdateDetails(string name, int capacity, string? location)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ValidationException("Rumsnamn måste anges");

            Name = name;
            Capacity = capacity;
            Location = location;
        }

        public void Deactivate() => IsActive = false;
        public void Activate() => IsActive = true;
    }
}
