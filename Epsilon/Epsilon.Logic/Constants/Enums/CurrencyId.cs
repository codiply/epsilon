namespace Epsilon.Logic.Constants.Enums
{
    // NOTE 1: Values should be all UPPERCASE.
    // NOTE 2: These codes should match the Id's of all currencies in the database (see reference data in database project).
    // NOTE 3: If you add a CurrencyId search for string 'EnumSwitch:CurrencyId' for 
    //         all places where you need to add a case in a switch statement.
    public enum CurrencyId
    {
        EUR,
        GBP,
        USD
    }
}
