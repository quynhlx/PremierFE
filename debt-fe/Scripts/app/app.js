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
        $('.menu > li').each(function (i, ele) {

        	var li = $(ele);
        	var href = li.children('a').prop('href');

        	var controller = li.data('controller');
        	var action = li.children('a').data('action');

        	if (action === undefined || action === null) {
				action = '';
        	}

        	var winHref = window.location.href;
        	var pathName = window.location.pathname;

        	if (winHref.indexOf(controller) >= 0) {

        		if (controller === 'Profile') {
        			
        			var arr = li.children('.sub-menu').find('a');

        			$(arr).each(function (i, ale) {
        				
        				var actionSub = $(ale).data('action');
        				if (pathName.indexOf(actionSub) >= 0) {
        					$(ale).addClass('active').closest('.has-sub').addClass('active');        					
        				}
        			});
        		}

            	if (pathName.indexOf(action) >= 0) {
            		li.addClass('active');
            	} else {
            		if (action.toLowerCase() === 'index') {
            			li.addClass('active');
            		}
            	}
            } else {
            	if (controller.toLowerCase() === 'document' && pathName.replace('/','') === '') {
            		li.addClass('active');
            	} else {
            		li.removeClass('active');
            	}
            }
            
        });
        

        //
        // toggle sub menu
        //
        $('.menu').on('click', '.has-sub > a', function (e) {

            e.preventDefault();

            var self = $(this);

            self.toggleClass('active').next('.sub-menu').slideToggle();
        });

    });

})();