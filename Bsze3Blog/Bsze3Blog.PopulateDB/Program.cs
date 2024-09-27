using Bsze3Blog.Models;

namespace Bsze3Blog.PopulateDB;

internal class Program
{
    private static void Main()
    {
        /*

        Megjegyzés:

        Jól elszórakoztam vele, de ez cseppet sem lett "szép".

        Van pár hiba: dátumok, amik nem illenek össze az adott felhasználóval.
        Pl.: Inaktívként megtekintett egy blogot és ilyesmik.

        Plusz a DST dátumokkal nem tudom mit kezdjek, hogy elfogadja.
        Ha megoldódna a probléma, akkor jobb lenne NEM egyesével mentegetni az adatokat az adatbázisba
        és úgy kicsit gyorsabban lehetne feltölteni.

        */

        using Bsze3BlogContext bsze3BlogCtx = new()
        {
            DisableValueGeneratedOnAddOrUpdate = true
        };

        PopulateDatabase.Use(bsze3BlogCtx)
            .Reset()
            .Users(150)
            .BlogViews()
            .BlogEditLogs()
            .DeleteSomeUsers();
    }
}
