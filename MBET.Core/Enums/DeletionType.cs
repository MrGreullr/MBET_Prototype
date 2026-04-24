namespace MBET.Core.Enums
{
    public enum DeletionType
    {
        /// <summary>
        /// Marks the entity as deleted (IsDeleted = true) without removing it from the database.
        /// Preserves referential integrity for historical data (e.g., Orders referencing a deleted Product).
        /// </summary>
        SoftDelete,

        /// <summary>
        /// Permanently removes the entity from the database.
        /// Warning: This operation is irreversible.
        /// </summary>
        HardDelete
    }
}