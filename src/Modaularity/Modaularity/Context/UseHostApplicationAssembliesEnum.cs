namespace Modaularity.Context;

public enum UseHostApplicationAssembliesEnum
{
    /// <summary>
    /// Nunca usar ensamblados
    /// </summary>
    Never,

    /// <summary>
    /// Solo usar los ensamblados listados
    /// </summary>
    Selected,

    /// <summary>
    /// Siempre intentar usar esamblados
    /// </summary>
    Always,

    /// <summary>
    /// Preferir ensamblados referenciados por módulo
    /// </summary>
    PreferModule
}