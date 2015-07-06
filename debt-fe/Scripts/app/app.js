(function () {

    $(document).ready(function () {

        //
        // active menu
        //        
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

    });

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

})();