using ErkanTatilPlani.Core.Enums;
using Microsoft.AspNetCore.Mvc;

namespace ErkanTatilPlani.API.Controllers;

[ApiController]
[Route("api/type-definitions")]
public class TypeDefinitionsController : ControllerBase
{
    private static object? _cachedResult;

    [HttpGet]
    [ResponseCache(Duration = 3600)]
    public IActionResult GetAll()
    {
        _cachedResult ??= new
        {
            durationUnits = MapItems(DurationUnits.All),
            reservationStatuses = MapItems(ReservationStatuses.All),
            tourStatuses = MapItems(TourStatuses.All),
            paymentStatuses = MapItems(PaymentStatuses.All),
            notificationTypes = MapItems(NotificationTypes.All),
            promotionTypes = MapItems(PromotionTypes.All),
            discountTypes = MapItems(DiscountTypes.All),
            promotionStatuses = MapItems(PromotionStatuses.All),
            guideAssignmentStatuses = MapItems(GuideAssignmentStatuses.All),
            userTypes = MapItems(UserTypes.All),
            messageSenderTypes = MapItems(MessageSenderTypes.All)
        };

        return Ok(_cachedResult);
    }

    private static IEnumerable<object> MapItems(IEnumerable<TypeItem> items) =>
        items.Select(x => new
        {
            x.Id,
            x.SystemName,
            x.NameResourceKey,
            x.Icon,
            x.CssClass,
            x.DisplayOrder,
            x.IsDefault
        });
}
