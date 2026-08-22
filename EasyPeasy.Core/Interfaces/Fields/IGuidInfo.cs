namespace EasyPeasy.Core.Interfaces.Fields;

/// <summary>
/// Implemented by entities/models that expose a stable <see cref="RecordGuid"/> identity, independent
/// of the auto-increment <c>Id</c>. Deliberately separate from <see cref="MukhaLab.Database.IGuidRecord"/>
/// (which <c>MukhaLab.Database</c>'s guid-based repository methods key off): that interface's
/// <c>RecordGuid</c> is get-only, while this one is settable — needed wherever code actually assigns
/// or regenerates the GUID (e.g. <c>UnitMappingAction.RegenerateGuid</c>). Most entities/models that
/// implement <see cref="MukhaLab.Database.IGuidRecord"/> also implement this one.
/// </summary>
public interface IGuidInfo
{
    Guid RecordGuid { get; set; }
}
