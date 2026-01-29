using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Core.Enums;

namespace ErkanTatilPlani.Tests.Entities;

public class EntityTests
{
    [Fact]
    public void Tour_Should_Have_Default_Values()
    {
        // Arrange & Act
        var tour = new Tour();

        // Assert
        Assert.Equal(0, tour.Id);
        Assert.True(tour.IsActive);
        Assert.Equal(0, tour.ReviewCount);
        Assert.Equal(0, tour.AverageRating);
    }

    [Fact]
    public void Company_Should_Have_Pending_Status_By_Default()
    {
        // Arrange & Act
        var company = new Company();

        // Assert
        Assert.Equal(CompanyStatuses.Ids.Pending, company.StatusId);
    }

    [Fact]
    public void Visitor_Should_Have_Visitor_Type_By_Default()
    {
        // Arrange & Act
        var visitor = new Visitor();

        // Assert
        Assert.Equal(UserTypes.Ids.Visitor, visitor.UserTypeId);
    }

    [Fact]
    public void Reservation_Should_Have_Pending_Status_By_Default()
    {
        // Arrange & Act
        var reservation = new Reservation();

        // Assert
        Assert.Equal(ReservationStatus.Pending, reservation.Status);
    }

    [Fact]
    public void TourReview_Should_Have_SubRatings()
    {
        // Arrange
        var review = new TourReview
        {
            OverallRating = 4,
            GuideRating = 4,
            ValueRating = 5,
            ServiceRating = 4,
            LocationRating = 5,
            OrganizationRating = 4
        };

        // Assert - Sub-ratings are properly set
        Assert.Equal(4, review.OverallRating);
        Assert.Equal(4, review.GuideRating);
        Assert.Equal(5, review.ValueRating);
        Assert.Equal(4, review.ServiceRating);
        Assert.Equal(5, review.LocationRating);
        Assert.Equal(4, review.OrganizationRating);
    }

    [Fact]
    public void AppLog_Should_Have_Info_Level_By_Default()
    {
        // Arrange & Act
        var log = new AppLog();

        // Assert
        Assert.Equal("Info", log.Level);
    }

    [Fact]
    public void FavoriteTour_Should_Have_Required_FKs()
    {
        // Arrange
        var favorite = new FavoriteTour
        {
            VisitorId = 1,
            TourId = 2
        };

        // Assert
        Assert.Equal(1, favorite.VisitorId);
        Assert.Equal(2, favorite.TourId);
    }
}

public class UserTypesTests
{
    [Fact]
    public void UserTypes_Should_Have_Correct_Ids()
    {
        Assert.Equal(0, UserTypes.Ids.Visitor);
        Assert.Equal(1, UserTypes.Ids.CompanyOwner);
        Assert.Equal(2, UserTypes.Ids.Staff);
        Assert.Equal(3, UserTypes.Ids.Admin);
    }

    [Fact]
    public void UserTypes_GetById_Should_Return_Correct_Type()
    {
        // Act
        var visitor = UserTypes.GetById(0);
        var companyOwner = UserTypes.GetById(1);
        var staff = UserTypes.GetById(2);
        var admin = UserTypes.GetById(3);

        // Assert
        Assert.NotNull(visitor);
        Assert.Equal("Visitor", visitor.SystemName);

        Assert.NotNull(companyOwner);
        Assert.Equal("CompanyOwner", companyOwner.SystemName);

        Assert.NotNull(staff);
        Assert.Equal("Staff", staff.SystemName);

        Assert.NotNull(admin);
        Assert.Equal("Admin", admin.SystemName);
    }
}

public class CompanyStatusesTests
{
    [Fact]
    public void CompanyStatuses_Should_Have_Correct_Ids()
    {
        Assert.Equal(0, CompanyStatuses.Ids.Pending);
        Assert.Equal(1, CompanyStatuses.Ids.Approved);
        Assert.Equal(2, CompanyStatuses.Ids.Rejected);
        Assert.Equal(3, CompanyStatuses.Ids.Suspended);
    }
}

public class ReservationStatusTests
{
    [Fact]
    public void ReservationStatus_Should_Have_Correct_Values()
    {
        Assert.Equal(0, (int)ReservationStatus.Pending);
        Assert.Equal(1, (int)ReservationStatus.Confirmed);
        Assert.Equal(2, (int)ReservationStatus.Cancelled);
        Assert.Equal(3, (int)ReservationStatus.Completed);
    }
}
