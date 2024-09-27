using Bogus;

namespace Bsze3Blog.PopulateDB;

internal static class BogusExtension
{
    /// <summary>
    /// <list type="table">
    ///     <listheader>
    ///         <term>AC</term>
    ///         <description>Szolgáltatás/Hálózat</description>
    ///     </listheader>
    ///     <item>
    ///         <term>20</term>
    ///         <description>Yettel</description>
    ///     </item>
    ///     <item>
    ///         <term>30</term>
    ///         <description>Telekom</description>
    ///     </item>
    ///     <item>
    ///         <term>70</term>
    ///         <description>Vodafone</description>
    ///     </item>
    /// </list>
    /// <seealso href="https://hu.wikipedia.org/wiki/K%C3%B6rzeth%C3%ADv%C3%B3sz%C3%A1m#Magyarorsz%C3%A1g">Wikipedia - Körzethívószám</seealso>
    /// </summary>
    private static readonly string[] AreaCodes = ["20", "30", "70"];

    /// <summary>
    /// Egy random telefonszámot generál a következő formátumban:
    /// <br/> 06-AC-###-####
    /// <inheritdoc cref="AreaCodes"/>
    /// </summary>
    /// <param name="p">Person</param>
    /// <returns>A generált telefonszám</returns>
    public static string TelephoneNumber(this Bogus.Person p)
    {
        return p.Random.ReplaceNumbers($"06-{p.Random.ArrayElement(AreaCodes)}-###-####");
    }
}
