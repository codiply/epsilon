namespace Epsilon.Logic.Constants.Enums
{
    // NOTE: If you add a CountryId search for string 'EnumSwitch:GeocodeAddressStatus' for 
    //       all places where you need to add a case in a switch statement.
    public enum GeocodeAddressStatus
    {
        Success,
        NoMatches,
        MultipleMatches,
        ResultInWrongCountry,
        OverQueryLimitTriedMaxTimes,
        ServiceUnavailable
    }
}
