namespace Epsilon.Logic.Constants.Enums
{
    // NOTE 1: Values should be all UPPERCASE.
    // NOTE 2: These codes should match the Id's of all countries in the database flagged 
    //         as available (see reference data in database project).
    // NOTE 3: If you add a CountryId search for string 'EnumSwitch:CountryId' for 
    //         all places where you need to add a case in a switch statement.
    public enum CountryId
    {
        GB,
        GR
    }
}
