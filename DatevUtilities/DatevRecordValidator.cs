using System.Text.Json;
using System.Text.RegularExpressions;


namespace DatevUtilities
{

    public class DatevRecordValidator
    {
        Dictionary<int, string> regexDict = Constants.regexDefaultSet;

        public DatevRecordValidator(Dictionary<int, string> regexSet = null)
        {
            if (regexSet != null)
                regexDict = regexSet;
        }

        public string Validate(IEnumerable<string> jsonStrings)
        {
            List<DatevRecord> datevRecords = new List<DatevRecord>();
            ParseInput(jsonStrings, datevRecords);
            List<RecordValidationResult> results = new List<RecordValidationResult>();
            for (int i = 0; i < datevRecords.Count; i++)
            {
                if (ValidateField(datevRecords[i]) == false)
                {
                    return ReturnMessage(RecordValidationResult.illegalField);
                }
                if (ValidateAccount(datevRecords[i]) == false)
                {
                    return ReturnMessage(RecordValidationResult.sameAccount);
                }
            }
            return ReturnMessage(RecordValidationResult.ok);
        }

        private bool ValidateAccount(DatevRecord datevRecord)
        {
            if (datevRecord.fields[007] == datevRecord.fields[008])
            {
                return false;
            }
            return true;
        }

        private string ReturnMessage(RecordValidationResult validResult)
        {
            switch (validResult)
            {
                case RecordValidationResult.ok:
                    return "OK";
                case RecordValidationResult.illegalField:
                    return "The Dataset contains at least one illegal Field";
                case RecordValidationResult.sameAccount:
                    return "booking to the same account was recognized.";
                default:
                    return "unknown error";
            }
        }

        private static void ParseInput(IEnumerable<string> jsonStrings, List<DatevRecord> datevRecords)
        {
            foreach (var item in jsonStrings)
            {
                var dict = JsonSerializer.Deserialize<Dictionary<int, string>>(item);
                if (dict != null)
                {
                    if (dict.Count > 0)
                    {
                        datevRecords.Add(new DatevRecord(dict));
                    }
                }
            }
        }

        bool ValidateField(DatevRecord datevRecord)
        {
            Regex regex;
            MatchCollection matches;
            foreach (var field in datevRecord.fields)
            {
                if (string.IsNullOrEmpty(field.Value))
                {
                    continue;
                }
                if (regexDict.ContainsKey(field.Key))
                {
                    regex = new Regex(field.Value);
                    matches = regex.Matches(datevRecord.fields[field.Key]);
                    if (matches.Count <= 0)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}