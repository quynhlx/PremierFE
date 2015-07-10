$(document).ready(function () {

    console.log('welcome to world of validation');

    var feedbackIcons = {
        valid: 'glyphicon glyphicon-ok',
        invalid: 'glyphicon glyphicon-remove',
        validating: 'glyphicon glyphicon-refresh'
    };

    var nameVal = {
        validators: {
            notEmpty: { message: 'Name is required' },
            stringLength: { max: 50, min: 6, message: 'Name is too short' }
        }
    };

    var descriptionVal = {
        validators: {
            stringLength: { max: 250, message: 'Description is too long' }
        }
    };

    var fileUploadVal = {
        validators: {
            file: {
                maxSize: 1048576,   // 1MB
                // maxTotalSize: 10485760, // 10MB
                message: 'Maximum size allowed is 1MB'
            }
        }
    };



    var formDocumentSetup = {
        feedbackIcons: feedbackIcons,
        fields: {
            DocName: nameVal,
            FileName: fileUploadVal,
            Notes: descriptionVal
        }
    };

    $('#form-upload-document').bootstrapValidator(formDocumentSetup);
    

});