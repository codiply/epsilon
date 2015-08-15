namespace Epsilon.Logic.Constants.Enums
{
    // NOTE: If you add a CountryId search for string 'EnumSwitch:GeocodePostcodeStatus' for 
    //       all places where you need to add a case in a switch statement.
    public enum GeocodePostcodeStatus
    {
        Success,
        NoMatches,
        MultipleMatches,
        ResultInWrongCountry,
        ResultWithWrongType,
        OverQueryLimitTriedMaxTimes,
        ServiceUnavailable
    }
}
