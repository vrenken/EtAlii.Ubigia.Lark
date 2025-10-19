using System.ComponentModel;

namespace EtAlii.OpenAI.Tests;

public class OrbatTestTools
{
    public Organisation[] Orbat => _orbat.ToArray();
    private readonly List<Organisation> _orbat = new();

    [Description(
        $"Provides a full hierarchical view of the existing ORBAT. Call this method to find out what organisations and units are already available and what their Id's are.")]
    public Organisation[] GetOrbat()
    {
        return _orbat.ToArray();
    }

    [Description(
        $"Adds an organisation to the ORBAT. Call this method each time for all organisations that need to be added. Use parentId = -1 for any root organisations, Use an existing organisation Id for parentId to add organisations as a child below another one. This method will return the newly created organisation with a new Id assigned.")]
    public Organisation AddOrganisation(
        int parentId,
        string name,
        string sidc,
        string echelonLevel,
        string description)
    {
        if (parentId < -1)
        {
            throw new ArgumentOutOfRangeException(nameof(parentId), "Parent Id must be -1 or a valid Id.");
        }

        var allEntities = Organisation.Flatten(_orbat);

        var newEntity = new Organisation
        {
            Id = allEntities.Length + 1,
            Name = name,
            Sidc = sidc,
            Description = description,
            EchelonLevel = echelonLevel,
        };

        if (parentId == -1)
        {
            _orbat.Add(newEntity);
        }
        else
        {
            var parent = allEntities
                .OfType<Organisation>()
                .Single(e => e.Id == parentId);

            parent.Children.Add(newEntity);
        }
        return newEntity;
    }
    
    [Description("Adds a unit to the ORBAT. Call this method each time for all units that need to be added. Always use an existing organisation Id for parentId to add units as a child below a organisation. This method will return the newly created unit with a new Id assigned.")]
    public Unit AddUnit(
        int parentId,
        string name,
        string sidc,
        string description)
    {
        if (parentId < -1)
        {
            throw new ArgumentOutOfRangeException(nameof(parentId), "Parent Id must be the Id of a valid organisation.");
        }

        var allEntities = Organisation.Flatten(_orbat);
        var parent = allEntities
            .OfType<Organisation>()
            .Single(e => e.Id == parentId);

        var newEntity = new Unit
        {
            Id = allEntities.Length + 1,
            Name = name,
            Sidc = sidc,
            Description = description,
        };
        parent.Children.Add(newEntity);
        return newEntity;
    }
}