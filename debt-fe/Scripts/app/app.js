(function () {

    function displayMessage (message, msgType) {
        toastr.options = {
            "closeButton": true,
            "debug": false,
            "positionClass": "toast-top-right",
            "onClick": null,
            "showDuration": "300",
            "hideDuration": "1000",
            "timeOut": "8000",
            "extendedTimeOut": "1000",
            "showEasing": "swing",
            "hideEasing": "linear",
            "showMethod": "fadeIn",
            "hideMethod": "fadeOut"
        };
        toastr[msgType](message);
    };

    function getUrlParam(name) {
        var results = new RegExp('[\\?&]' + name + '=([^&#]*)').exec(window.location.href);

        return (results && results[1]) || undefined;
    }

    function getController(href) {
        if (href === undefined || href === '') {
            return '';
        }

        var controller = '';

        var lastSlash = href.lastIndexOf('/');

        if (isNaN(lastSlash) || lastSlash < 0) {
            return '';
        }

        controller = href.substring(lastSlash + 1);

        return controller;
    }

    function settings() {
        $('[data-toggle=tooltip]').tooltip();

        if ($('#success').val()) {
            displayMessage($('#success').val(), 'success');
        }
        if ($('#info').val()) {
            displayMessage($('#info').val(), 'info');
        }
        if ($('#warning').val()) {
            displayMessage($('#warning').val(), 'warning');
        }
        if ($('#error').val()) {
            displayMessage($('#error').val(), 'error');
        }
    }


    $(document).ready(function () {

        settings();

        //
        // active menu
        //     
        /*
        $('.menu').children().each(function (i, e) {

            var a = $(e).children('a');
            var href = a.prop('href');
            
            // console.log('href', href);

            var controller = getController(href);


            if (window.location.href.indexOf(controller) >= 0) {
                console.log('controller =', controller);
                a.addClass('active');                
            } else {
                a.removeClass('active');
            }
            
        });
        */

    });

})();