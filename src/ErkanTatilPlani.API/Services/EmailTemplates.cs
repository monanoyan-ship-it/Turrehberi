using ErkanTatilPlani.Core.Services;

namespace ErkanTatilPlani.API.Services;

/// <summary>
/// Email HTML sablonlari
/// </summary>
public static class EmailTemplates
{
    private const string PrimaryColor = "#2563eb";
    private const string SuccessColor = "#10b981";
    private const string WarningColor = "#f59e0b";
    private const string DangerColor = "#ef4444";
    private const string GrayColor = "#6b7280";
    private const string LightGray = "#f3f4f6";

    /// <summary>
    /// Temel email sarmalayici sablon
    /// </summary>
    public static string WrapInBaseTemplate(string title, string content, string footerText)
    {
        return $@"
<!DOCTYPE html>
<html lang=""tr"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>{title}</title>
</head>
<body style=""margin: 0; padding: 0; font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: {LightGray};"">
    <table role=""presentation"" cellspacing=""0"" cellpadding=""0"" border=""0"" width=""100%"" style=""background-color: {LightGray};"">
        <tr>
            <td style=""padding: 40px 20px;"">
                <table role=""presentation"" cellspacing=""0"" cellpadding=""0"" border=""0"" width=""600"" style=""margin: 0 auto; background-color: #ffffff; border-radius: 12px; box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);"">
                    <!-- Header -->
                    <tr>
                        <td style=""padding: 30px 40px; background: linear-gradient(135deg, {PrimaryColor}, #1d4ed8); border-radius: 12px 12px 0 0; text-align: center;"">
                            <h1 style=""margin: 0; color: #ffffff; font-size: 28px; font-weight: 700;"">Erkan Tatil Plani</h1>
                        </td>
                    </tr>
                    <!-- Content -->
                    <tr>
                        <td style=""padding: 40px;"">
                            {content}
                        </td>
                    </tr>
                    <!-- Footer -->
                    <tr>
                        <td style=""padding: 20px 40px; background-color: {LightGray}; border-radius: 0 0 12px 12px; text-align: center;"">
                            <p style=""margin: 0; color: {GrayColor}; font-size: 12px;"">{footerText}</p>
                            <p style=""margin: 10px 0 0; color: {GrayColor}; font-size: 12px;"">
                                <a href=""https://erkantatilplani.com"" style=""color: {PrimaryColor}; text-decoration: none;"">erkantatilplani.com</a>
                            </p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
    }

    /// <summary>
    /// Sifre sifirlama email sablonu
    /// </summary>
    public static string GetPasswordResetTemplate(
        string customerName,
        string resetUrl,
        string greeting,
        string title,
        string message,
        string buttonText,
        string expiryText,
        string footerText)
    {
        var content = $@"
            <h2 style=""margin: 0 0 20px; color: #1f2937; font-size: 24px; font-weight: 600;"">{title}</h2>
            <p style=""margin: 0 0 20px; color: {GrayColor}; font-size: 16px; line-height: 1.6;"">{greeting.Replace("{0}", $"<strong>{customerName}</strong>")}</p>
            <p style=""margin: 0 0 30px; color: {GrayColor}; font-size: 16px; line-height: 1.6;"">{message}</p>

            <table role=""presentation"" cellspacing=""0"" cellpadding=""0"" border=""0"" style=""margin: 0 auto 30px;"">
                <tr>
                    <td style=""background-color: {PrimaryColor}; border-radius: 8px;"">
                        <a href=""{resetUrl}"" style=""display: inline-block; padding: 16px 32px; color: #ffffff; text-decoration: none; font-size: 16px; font-weight: 600;"">{buttonText}</a>
                    </td>
                </tr>
            </table>

            <div style=""padding: 20px; background-color: {LightGray}; border-radius: 8px; margin-bottom: 20px;"">
                <p style=""margin: 0; color: {GrayColor}; font-size: 14px; line-height: 1.6;"">
                    <strong style=""color: {WarningColor};"">&#9888;</strong> {expiryText}
                </p>
            </div>

            <p style=""margin: 0; color: {GrayColor}; font-size: 12px; line-height: 1.6;"">
                Link calismiyorsa asagidaki adresi tarayiciniza kopyalayin:<br>
                <a href=""{resetUrl}"" style=""color: {PrimaryColor}; word-break: break-all;"">{resetUrl}</a>
            </p>";

        return WrapInBaseTemplate(title, content, footerText);
    }

    /// <summary>
    /// Email dogrulama sablonu
    /// </summary>
    public static string GetEmailVerificationTemplate(
        string customerName,
        string verifyUrl,
        string greeting,
        string title,
        string message,
        string buttonText,
        string footerText)
    {
        var content = $@"
            <div style=""text-align: center; margin-bottom: 30px;"">
                <div style=""display: inline-block; padding: 20px; background-color: #dbeafe; border-radius: 50%;"">
                    <span style=""font-size: 48px;"">&#9993;</span>
                </div>
            </div>

            <h2 style=""margin: 0 0 20px; color: #1f2937; font-size: 24px; font-weight: 600; text-align: center;"">{title}</h2>
            <p style=""margin: 0 0 20px; color: {GrayColor}; font-size: 16px; line-height: 1.6; text-align: center;"">{greeting.Replace("{0}", $"<strong>{customerName}</strong>")}</p>
            <p style=""margin: 0 0 30px; color: {GrayColor}; font-size: 16px; line-height: 1.6; text-align: center;"">{message}</p>

            <table role=""presentation"" cellspacing=""0"" cellpadding=""0"" border=""0"" style=""margin: 0 auto 30px;"">
                <tr>
                    <td style=""background-color: {SuccessColor}; border-radius: 8px;"">
                        <a href=""{verifyUrl}"" style=""display: inline-block; padding: 16px 32px; color: #ffffff; text-decoration: none; font-size: 16px; font-weight: 600;"">{buttonText}</a>
                    </td>
                </tr>
            </table>

            <p style=""margin: 0; color: {GrayColor}; font-size: 12px; line-height: 1.6; text-align: center;"">
                Link calismiyorsa asagidaki adresi tarayiciniza kopyalayin:<br>
                <a href=""{verifyUrl}"" style=""color: {PrimaryColor}; word-break: break-all;"">{verifyUrl}</a>
            </p>";

        return WrapInBaseTemplate(title, content, footerText);
    }

    /// <summary>
    /// Rezervasyon onaylandi email sablonu
    /// </summary>
    public static string GetReservationConfirmedTemplate(
        ReservationEmailModel model,
        string greeting,
        string title,
        string message,
        string detailsTitle,
        string tourLabel,
        string companyLabel,
        string destinationLabel,
        string dateLabel,
        string peopleLabel,
        string totalLabel,
        string thankYouText,
        string footerText)
    {
        var content = $@"
            <div style=""text-align: center; margin-bottom: 30px;"">
                <div style=""display: inline-block; padding: 20px; background-color: #d1fae5; border-radius: 50%;"">
                    <span style=""font-size: 48px; color: {SuccessColor};"">&#10004;</span>
                </div>
            </div>

            <h2 style=""margin: 0 0 20px; color: {SuccessColor}; font-size: 24px; font-weight: 600; text-align: center;"">{title}</h2>
            <p style=""margin: 0 0 10px; color: {GrayColor}; font-size: 16px; line-height: 1.6;"">{greeting.Replace("{0}", $"<strong>{model.CustomerName}</strong>")}</p>
            <p style=""margin: 0 0 30px; color: {GrayColor}; font-size: 16px; line-height: 1.6;"">{message}</p>

            <div style=""background-color: {LightGray}; border-radius: 12px; padding: 24px; margin-bottom: 30px;"">
                <h3 style=""margin: 0 0 20px; color: #1f2937; font-size: 18px; font-weight: 600; border-bottom: 2px solid {PrimaryColor}; padding-bottom: 10px;"">{detailsTitle}</h3>
                {GetReservationDetailsTable(model, tourLabel, companyLabel, destinationLabel, dateLabel, peopleLabel, totalLabel)}
            </div>

            {(string.IsNullOrEmpty(model.Notes) ? "" : $@"
            <div style=""background-color: #fef3c7; border-left: 4px solid {WarningColor}; padding: 16px; margin-bottom: 30px; border-radius: 0 8px 8px 0;"">
                <p style=""margin: 0; color: #92400e; font-size: 14px;""><strong>Not:</strong> {model.Notes}</p>
            </div>")}

            <p style=""margin: 0; color: {GrayColor}; font-size: 16px; line-height: 1.6; text-align: center;"">{thankYouText}</p>";

        return WrapInBaseTemplate(title, content, footerText);
    }

    /// <summary>
    /// Rezervasyon iptal edildi email sablonu
    /// </summary>
    public static string GetReservationCancelledTemplate(
        ReservationEmailModel model,
        string greeting,
        string title,
        string message,
        string detailsTitle,
        string tourLabel,
        string companyLabel,
        string destinationLabel,
        string dateLabel,
        string peopleLabel,
        string totalLabel,
        string contactText,
        string footerText)
    {
        var content = $@"
            <div style=""text-align: center; margin-bottom: 30px;"">
                <div style=""display: inline-block; padding: 20px; background-color: #fee2e2; border-radius: 50%;"">
                    <span style=""font-size: 48px; color: {DangerColor};"">&#10006;</span>
                </div>
            </div>

            <h2 style=""margin: 0 0 20px; color: {DangerColor}; font-size: 24px; font-weight: 600; text-align: center;"">{title}</h2>
            <p style=""margin: 0 0 10px; color: {GrayColor}; font-size: 16px; line-height: 1.6;"">{greeting.Replace("{0}", $"<strong>{model.CustomerName}</strong>")}</p>
            <p style=""margin: 0 0 30px; color: {GrayColor}; font-size: 16px; line-height: 1.6;"">{message}</p>

            <div style=""background-color: {LightGray}; border-radius: 12px; padding: 24px; margin-bottom: 30px; opacity: 0.8;"">
                <h3 style=""margin: 0 0 20px; color: #1f2937; font-size: 18px; font-weight: 600; border-bottom: 2px solid {DangerColor}; padding-bottom: 10px;"">{detailsTitle}</h3>
                {GetReservationDetailsTable(model, tourLabel, companyLabel, destinationLabel, dateLabel, peopleLabel, totalLabel)}
            </div>

            <p style=""margin: 0; color: {GrayColor}; font-size: 16px; line-height: 1.6; text-align: center;"">{contactText}</p>";

        return WrapInBaseTemplate(title, content, footerText);
    }

    /// <summary>
    /// Rezervasyon reddedildi email sablonu
    /// </summary>
    public static string GetReservationRejectedTemplate(
        ReservationEmailModel model,
        string greeting,
        string title,
        string message,
        string detailsTitle,
        string tourLabel,
        string companyLabel,
        string destinationLabel,
        string dateLabel,
        string peopleLabel,
        string totalLabel,
        string reasonLabel,
        string contactText,
        string footerText)
    {
        var content = $@"
            <div style=""text-align: center; margin-bottom: 30px;"">
                <div style=""display: inline-block; padding: 20px; background-color: #fef3c7; border-radius: 50%;"">
                    <span style=""font-size: 48px; color: {WarningColor};"">&#9888;</span>
                </div>
            </div>

            <h2 style=""margin: 0 0 20px; color: {WarningColor}; font-size: 24px; font-weight: 600; text-align: center;"">{title}</h2>
            <p style=""margin: 0 0 10px; color: {GrayColor}; font-size: 16px; line-height: 1.6;"">{greeting.Replace("{0}", $"<strong>{model.CustomerName}</strong>")}</p>
            <p style=""margin: 0 0 30px; color: {GrayColor}; font-size: 16px; line-height: 1.6;"">{message}</p>

            {(string.IsNullOrEmpty(model.RejectionReason) ? "" : $@"
            <div style=""background-color: #fee2e2; border-left: 4px solid {DangerColor}; padding: 16px; margin-bottom: 30px; border-radius: 0 8px 8px 0;"">
                <p style=""margin: 0; color: #991b1b; font-size: 14px;""><strong>{reasonLabel}:</strong> {model.RejectionReason}</p>
            </div>")}

            <div style=""background-color: {LightGray}; border-radius: 12px; padding: 24px; margin-bottom: 30px; opacity: 0.8;"">
                <h3 style=""margin: 0 0 20px; color: #1f2937; font-size: 18px; font-weight: 600; border-bottom: 2px solid {WarningColor}; padding-bottom: 10px;"">{detailsTitle}</h3>
                {GetReservationDetailsTable(model, tourLabel, companyLabel, destinationLabel, dateLabel, peopleLabel, totalLabel)}
            </div>

            <p style=""margin: 0; color: {GrayColor}; font-size: 16px; line-height: 1.6; text-align: center;"">{contactText}</p>";

        return WrapInBaseTemplate(title, content, footerText);
    }

    private static string GetReservationDetailsTable(
        ReservationEmailModel model,
        string tourLabel,
        string companyLabel,
        string destinationLabel,
        string dateLabel,
        string peopleLabel,
        string totalLabel)
    {
        return $@"
            <table role=""presentation"" cellspacing=""0"" cellpadding=""0"" border=""0"" width=""100%"">
                <tr>
                    <td style=""padding: 8px 0; color: {GrayColor}; font-size: 14px;"">{tourLabel}:</td>
                    <td style=""padding: 8px 0; color: #1f2937; font-size: 14px; font-weight: 600; text-align: right;"">{model.TourName}</td>
                </tr>
                <tr>
                    <td style=""padding: 8px 0; color: {GrayColor}; font-size: 14px;"">{companyLabel}:</td>
                    <td style=""padding: 8px 0; color: #1f2937; font-size: 14px; text-align: right;"">{model.CompanyName}</td>
                </tr>
                <tr>
                    <td style=""padding: 8px 0; color: {GrayColor}; font-size: 14px;"">{destinationLabel}:</td>
                    <td style=""padding: 8px 0; color: #1f2937; font-size: 14px; text-align: right;"">{model.Destination}</td>
                </tr>
                <tr>
                    <td style=""padding: 8px 0; color: {GrayColor}; font-size: 14px;"">{dateLabel}:</td>
                    <td style=""padding: 8px 0; color: #1f2937; font-size: 14px; text-align: right;"">{model.StartDate:dd.MM.yyyy} - {model.EndDate:dd.MM.yyyy}</td>
                </tr>
                <tr>
                    <td style=""padding: 8px 0; color: {GrayColor}; font-size: 14px;"">{peopleLabel}:</td>
                    <td style=""padding: 8px 0; color: #1f2937; font-size: 14px; text-align: right;"">{model.NumberOfPeople}</td>
                </tr>
                <tr style=""border-top: 1px solid #e5e7eb;"">
                    <td style=""padding: 12px 0 8px; color: #1f2937; font-size: 16px; font-weight: 600;"">{totalLabel}:</td>
                    <td style=""padding: 12px 0 8px; color: {PrimaryColor}; font-size: 18px; font-weight: 700; text-align: right;"">{model.TotalPrice:N2} TL</td>
                </tr>
            </table>";
    }
}
