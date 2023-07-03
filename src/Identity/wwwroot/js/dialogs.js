function showError(title, message, image, timer) {
    if (title) {
        $('#errorModalLabel').text(title);
    } else {
        $('#errorModalLabel').text('Error');
    }

    if (image) {
        $('#errorModalImage').attr('src', image);
        $('#errorModalImage').css('display', 'block');
    } else {
        $('#errorModalImage').css('display', 'none');
    }

    $('#errorMessage').text(message);
    $('#errorModal').modal('show');
    
    if (timer) {
        setTimeout(function () {
            $('#errorModal').modal('hide');
        }, timer);
    }
}

function showSuccess(title, message, image, timer) {
    if (title) {
        $('#successModalLabel').text(title);
    } else {
        $('#successModalLabel').text('Success!');
    }
    
    if (image) {
        $('#successModalImage').attr('src', image);
        $('#successModalImage').css('display', 'block');
    } else {
        $('#successModalImage').css('display', 'none');
    }

    $('#successMessage').text(message);
    $('#successModal').modal('show');
    
    if (timer) {
        setTimeout(function () {
            $('#successModal').modal('hide');
        }, timer);
    }
}
