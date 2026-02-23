function AnalyticsViewModel() {
    var self = this;

    self.isLoading = ko.observable(true);
    self.selectedMonths = ko.observable('12');

    var revenueChart = null;
    var occupancyChart = null;
    var cancellationChart = null;
    var tourComparisonChart = null;

    self.selectedMonths.subscribe(function() {
        self.loadAll();
    });

    self.loadAll = function() {
        self.isLoading(true);
        var months = self.selectedMonths();

        var loaded = 0;
        var total = 4;
        var checkDone = function() {
            loaded++;
            if (loaded >= total) self.isLoading(false);
        };

        // Revenue
        $.ajax({
            url: apiBaseUrl + '/api/companies/analytics/revenue?months=' + months,
            success: function(data) { self.renderRevenueChart(data); checkDone(); },
            error: function() { checkDone(); }
        });

        // Occupancy
        $.ajax({
            url: apiBaseUrl + '/api/companies/analytics/occupancy?months=' + months,
            success: function(data) { self.renderOccupancyChart(data); checkDone(); },
            error: function() { checkDone(); }
        });

        // Cancellations
        $.ajax({
            url: apiBaseUrl + '/api/companies/analytics/cancellations?months=' + months,
            success: function(data) { self.renderCancellationChart(data); checkDone(); },
            error: function() { checkDone(); }
        });

        // Tour Comparison
        $.ajax({
            url: apiBaseUrl + '/api/companies/analytics/tour-comparison',
            success: function(data) { self.renderTourComparisonChart(data); checkDone(); },
            error: function() { checkDone(); }
        });
    };

    self.renderRevenueChart = function(data) {
        if (revenueChart) revenueChart.destroy();
        var ctx = document.getElementById('revenueChart').getContext('2d');
        revenueChart = new Chart(ctx, {
            type: 'line',
            data: {
                labels: data.map(function(d) { return d.label; }),
                datasets: [{
                    label: T('Analytics.Revenue') || 'Gelir (TL)',
                    data: data.map(function(d) { return d.revenue; }),
                    borderColor: '#0d6efd',
                    backgroundColor: 'rgba(13, 110, 253, 0.1)',
                    fill: true,
                    tension: 0.3
                }]
            },
            options: {
                responsive: true,
                plugins: { legend: { display: false } },
                scales: { y: { beginAtZero: true } }
            }
        });
    };

    self.renderOccupancyChart = function(data) {
        if (occupancyChart) occupancyChart.destroy();
        var ctx = document.getElementById('occupancyChart').getContext('2d');
        occupancyChart = new Chart(ctx, {
            type: 'line',
            data: {
                labels: data.map(function(d) { return d.label; }),
                datasets: [{
                    label: T('Analytics.Occupancy') || 'Doluluk (%)',
                    data: data.map(function(d) { return d.occupancyRate; }),
                    borderColor: '#198754',
                    backgroundColor: 'rgba(25, 135, 84, 0.1)',
                    fill: true,
                    tension: 0.3
                }]
            },
            options: {
                responsive: true,
                plugins: { legend: { display: false } },
                scales: { y: { beginAtZero: true, max: 100 } }
            }
        });
    };

    self.renderCancellationChart = function(data) {
        if (cancellationChart) cancellationChart.destroy();
        var ctx = document.getElementById('cancellationChart').getContext('2d');
        cancellationChart = new Chart(ctx, {
            type: 'line',
            data: {
                labels: data.map(function(d) { return d.label; }),
                datasets: [{
                    label: T('Analytics.CancellationRate') || 'Iptal Orani (%)',
                    data: data.map(function(d) { return d.cancellationRate; }),
                    borderColor: '#dc3545',
                    backgroundColor: 'rgba(220, 53, 69, 0.1)',
                    fill: true,
                    tension: 0.3
                }]
            },
            options: {
                responsive: true,
                plugins: { legend: { display: false } },
                scales: { y: { beginAtZero: true, max: 100 } }
            }
        });
    };

    self.renderTourComparisonChart = function(data) {
        if (tourComparisonChart) tourComparisonChart.destroy();
        var ctx = document.getElementById('tourComparisonChart').getContext('2d');
        var colors = ['#0d6efd', '#198754', '#ffc107', '#dc3545', '#6f42c1', '#0dcaf0', '#fd7e14', '#20c997'];
        tourComparisonChart = new Chart(ctx, {
            type: 'bar',
            data: {
                labels: data.map(function(d) { return d.name; }),
                datasets: [{
                    label: T('Analytics.Revenue') || 'Gelir (TL)',
                    data: data.map(function(d) { return d.revenue; }),
                    backgroundColor: data.map(function(d, i) { return colors[i % colors.length]; })
                }]
            },
            options: {
                responsive: true,
                plugins: { legend: { display: false } },
                scales: { y: { beginAtZero: true } }
            }
        });
    };

    // Init
    $(document).ready(function() {
        self.loadAll();
    });
}

ko.applyBindings(new AnalyticsViewModel(), document.getElementById('analyticsApp'));
