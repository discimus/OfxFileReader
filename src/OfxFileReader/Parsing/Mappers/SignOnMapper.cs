using OfxFileReader.Logging;
using OfxFileReader.Models.Common;
using OfxFileReader.Models.SignOn;
using OfxFileReader.Parsing.Converters;
using OfxFileReader.Parsing.Sgml;

namespace OfxFileReader.Parsing.Mappers;

internal static class SignOnMapper
{
    public static SignOnResponse Map(SgmlNode sonrs, IOfxLogger logger)
    {
        var status = ParseStatus(sonrs.FindChild("STATUS"), logger);
        var serverDate = OfxDateConverter.Parse(sonrs.GetChildValue("DTSERVER")) ?? DateTimeOffset.MinValue;
        var language = sonrs.GetChildValue("LANGUAGE");
        var profUp = OfxDateConverter.Parse(sonrs.GetChildValue("DTPROFUP"));
        var acctUp = OfxDateConverter.Parse(sonrs.GetChildValue("DTACCTUP"));
        var sessionCookie = sonrs.GetChildValue("SESSCOOKIE");

        var fiNode = sonrs.FindChild("FI");
        FinancialInstitution? fi = null;
        if (fiNode is not null)
        {
            fi = new FinancialInstitution(
                fiNode.GetChildValue("ORG"),
                fiNode.GetChildValue("FID")
            );
        }

        return new SignOnResponse(
            status, serverDate, language, profUp, acctUp, fi, sessionCookie
        );
    }

    public static Status ParseStatus(SgmlNode? statusNode, IOfxLogger logger)
    {
        if (statusNode is null)
            return new Status(0, Models.Common.Enums.SeverityType.Info);

        var codeStr = statusNode.GetChildValue("CODE");
        var severity = SeverityTypeConverter.Parse(statusNode.GetChildValue("SEVERITY"));
        var message = statusNode.GetChildValue("MESSAGE");

        _ = int.TryParse(codeStr, out var code);
        return new Status(code, severity, message);
    }
}
