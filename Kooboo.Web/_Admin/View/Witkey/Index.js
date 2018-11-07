$(function() {
    var NAV_DISCUSSIONS_TOP = $('#nav_discussions')[0].getBoundingClientRect().top,
        NAV_DEMAND_TOP = $('#nav_demand')[0].getBoundingClientRect().top;

    var viewModel = function() {
        var self = this;

        this.userName = ko.observable();

        Kooboo.User.get().then(function(res) {
            if (res.success) {
                self.userName(res.model.userName);

                self.demands([1, 2, 3, 4, 1, 2, 3, 4, 1, 2, 3, 4]);
            }
        })

        this.discussions = ko.observableArray();

        this.demands = ko.observableArray();

        Kooboo.Discussion.getList().then(function(res) {
            if (res.success) {
                self.discussions(res.model.list);
            }
        })

        this.showDiscussionModal = ko.observable(false);
        this.onAddDiscussion = function() {
            self.showDiscussionModal(true);
        }

        this.showDemandModal = ko.observable(false);
        this.onAddDemand = function() {
            self.showDemandModal(true);
        }

    }

    var vm = new viewModel();
    ko.applyBindings(vm, document.getElementById('main'));

    window.onpopstate = function(e) {
        e.preventDefault();
    }

    $(window).scroll(function() {
        var discussionsInfo = $('#discussions')[0].getBoundingClientRect(),
            demandInfo = $('#demands')[0].getBoundingClientRect();

        var discussionsRange = discussionsInfo.top + discussionsInfo.height - 15,
            demandRange = demandInfo.top + demandInfo.height - 15;

        $('#side-nav li').removeClass('active');

        if (discussionsRange > NAV_DISCUSSIONS_TOP) {
            $('#nav_discussions').addClass('active');
        } else if (demandRange > NAV_DEMAND_TOP) {
            $('#nav_demand').addClass('active');
        }
    })
})