namespace Aristocrat.Monaco.Gaming.Contracts.Progressives
{
    /// <summary>
    ///     Defines a set of assignable progressive types that can be used
    ///     to associate progressive levels managed by the game with progressive
    ///     levels defined by hosts and operators.
    /// </summary>
    public enum AssignableProgressiveType
    {
        /// <summary>
        ///     Used when the type is not a valid assignable progressive or unknown
        /// </summary>
        None,

        /// <summary>
        ///     Custom Sap is defined by operators and can be assigned to any
        ///     game defined progressive level that is of type Selectable
        /// </summary>
        CustomSap,

        /// <summary>
        ///     Linked Progressives are defined by remote hosts and can
        ///     be assigned by operators to any game defined progressives level
        ///     that is of a type Linked or Selectable
        /// </summary>
        Linked,

        /// <summary>
        ///     Associative Sap is defined by progressives.xml and can be assigned to any
        ///     game defined progressive level that is of type Sap
        /// </summary>
        AssociativeSap
    }
}