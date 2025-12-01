using System;
using System.Linq;
using GrapheneTraceWeb.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GrapheneTraceWeb.Data
{
    public static class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new AppDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<AppDbContext>>()))
            {
                // Ensure the database and tables exist
                context.Database.EnsureCreated();

                // If there are already users, assume seeding has been done
                if (context.Users.Any())
                {
                    return;
                }

                // 1) Seed demo users
                var alice = new User
                {
                    Name = "Alice Johnson",
                    Email = "alice@example.com",
                    Password = "1234",
                    Role = "User"
                };

                var bob = new User
                {
                    Name = "Bob Martinez",
                    Email = "bob@example.com",
                    Password = "1234",
                    Role = "User"
                };

                var charlie = new User
                {
                    Name = "Charlie Lopez",
                    Email = "charlie@example.com",
                    Password = "1234",
                    Role = "User"
                };

                var clinician = new User
                {
                    Name = "Dr. Smith",
                    Email = "drsmith@example.com",
                    Password = "1234",
                    Role = "Clinician"
                };

                var admin = new User
                {
                    Name = "Admin User",
                    Email = "admin@example.com",
                    Password = "admin",
                    Role = "Admin"
                };

                context.Users.AddRange(alice, bob, charlie, clinician, admin);
                context.SaveChanges();

                // 2) Seed demo pressure data for Alice, Bob, and Charlie
                var now = DateTime.UtcNow;

                context.PressureData.AddRange(
                    // Alice - example values
                    new PressureData
                    {
                        UserId = alice.Id,
                        Timestamp = now.AddMinutes(-30),
                        PeakPressure = 95,
                        ContactArea = 68,
                        DataMatrix = "N/A"
                    },
                    new PressureData
                    {
                        UserId = alice.Id,
                        Timestamp = now.AddMinutes(-20),
                        PeakPressure = 102,
                        ContactArea = 71,
                        DataMatrix = "N/A"
                    },
                    new PressureData
                    {
                        UserId = alice.Id,
                        Timestamp = now.AddMinutes(-10),
                        PeakPressure = 110,
                        ContactArea = 74,
                        DataMatrix = "N/A"
                    },

                    // Bob - example values
                    new PressureData
                    {
                        UserId = bob.Id,
                        Timestamp = now.AddMinutes(-40),
                        PeakPressure = 80,
                        ContactArea = 60,
                        DataMatrix = "N/A"
                    },
                    new PressureData
                    {
                        UserId = bob.Id,
                        Timestamp = now.AddMinutes(-25),
                        PeakPressure = 88,
                        ContactArea = 63,
                        DataMatrix = "N/A"
                    },
                    new PressureData
                    {
                        UserId = bob.Id,
                        Timestamp = now.AddMinutes(-5),
                        PeakPressure = 92,
                        ContactArea = 65,
                        DataMatrix = "N/A"
                    },

                    // Charlie - example values
                    new PressureData
                    {
                        UserId = charlie.Id,
                        Timestamp = now.AddMinutes(-50),
                        PeakPressure = 120,
                        ContactArea = 78,
                        DataMatrix = "N/A"
                    },
                    new PressureData
                    {
                        UserId = charlie.Id,
                        Timestamp = now.AddMinutes(-35),
                        PeakPressure = 130,
                        ContactArea = 80,
                        DataMatrix = "N/A"
                    },
                    new PressureData
                    {
                        UserId = charlie.Id,
                        Timestamp = now.AddMinutes(-15),
                        PeakPressure = 125,
                        ContactArea = 79,
                        DataMatrix = "N/A"
                    }
                );

                context.SaveChanges();
            }
        }
    }
}
