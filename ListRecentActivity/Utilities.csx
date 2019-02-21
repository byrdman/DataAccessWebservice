
public static class Utilities 
{
   public static DateTime UtcToCst(DateTime timeUtc) 
   {
      try
      {
         TimeZoneInfo cstZone = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");
         DateTime cstTime = TimeZoneInfo.ConvertTimeFromUtc(timeUtc, cstZone);
         return cstTime;
      }
      catch (Exception)
      {
         return timeUtc;
      }                           
   }
}