
using Org.LLRP.LTK.LLRPV1;

#nullable disable
namespace LLRPSdk
{
  internal static class StateUnawareActionExtensions
  {
    public static ENUM_C1G2StateUnawareAction ConvertToC1G2StateUnawareAction(
      StateUnawareAction matchingAction,
      StateUnawareAction nonMatchingAction)
    {
      if (matchingAction == StateUnawareAction.Select && nonMatchingAction == StateUnawareAction.Unselect)
        return ENUM_C1G2StateUnawareAction.Select_Unselect;
      if (matchingAction == StateUnawareAction.Select && nonMatchingAction == StateUnawareAction.DoNothing)
        return ENUM_C1G2StateUnawareAction.Select_DoNothing;
      if (matchingAction == StateUnawareAction.Unselect && nonMatchingAction == StateUnawareAction.Select)
        return ENUM_C1G2StateUnawareAction.Unselect_Select;
      if (matchingAction == StateUnawareAction.Unselect && nonMatchingAction == StateUnawareAction.DoNothing)
        return ENUM_C1G2StateUnawareAction.Unselect_DoNothing;
      if (matchingAction == StateUnawareAction.DoNothing && nonMatchingAction == StateUnawareAction.Select)
        return ENUM_C1G2StateUnawareAction.DoNothing_Select;
      if (matchingAction == StateUnawareAction.DoNothing && nonMatchingAction == StateUnawareAction.Unselect)
        return ENUM_C1G2StateUnawareAction.DoNothing_Unselect;
      throw new LLRPSdkException("Error parsing tag select filter list. The matching action cannot be the same as the non-matching action.");
    }

    public static StateUnawareActionPair ConvertFromC1G2StateUnawareAction(
      ENUM_C1G2StateUnawareAction c1g2StateUnawareAction)
    {
      switch (c1g2StateUnawareAction)
      {
        case ENUM_C1G2StateUnawareAction.Select_Unselect:
          return new StateUnawareActionPair()
          {
            MatchingAction = StateUnawareAction.Select,
            NonMatchingAction = StateUnawareAction.Unselect
          };
        case ENUM_C1G2StateUnawareAction.Select_DoNothing:
          return new StateUnawareActionPair()
          {
            MatchingAction = StateUnawareAction.Select,
            NonMatchingAction = StateUnawareAction.DoNothing
          };
        case ENUM_C1G2StateUnawareAction.DoNothing_Unselect:
          return new StateUnawareActionPair()
          {
            MatchingAction = StateUnawareAction.DoNothing,
            NonMatchingAction = StateUnawareAction.Unselect
          };
        case ENUM_C1G2StateUnawareAction.Unselect_DoNothing:
          return new StateUnawareActionPair()
          {
            MatchingAction = StateUnawareAction.Unselect,
            NonMatchingAction = StateUnawareAction.DoNothing
          };
        case ENUM_C1G2StateUnawareAction.Unselect_Select:
          return new StateUnawareActionPair()
          {
            MatchingAction = StateUnawareAction.Unselect,
            NonMatchingAction = StateUnawareAction.Select
          };
        case ENUM_C1G2StateUnawareAction.DoNothing_Select:
          return new StateUnawareActionPair()
          {
            MatchingAction = StateUnawareAction.DoNothing,
            NonMatchingAction = StateUnawareAction.Select
          };
        default:
          throw new LLRPSdkException("Error converting to StateUnawareAction. Unknown C1G2StateUnawareAction.");
      }
    }
  }
}
