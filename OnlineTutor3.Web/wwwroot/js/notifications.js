// Обработка уведомлений из TempData
$(document).ready(function() {
    // Показываем уведомления из TempData
    showTempDataNotifications();
    
    // Автоматическое скрытие уведомлений через 5 секунд
    setTimeout(function() {
        $('.alert').each(function() {
            if ($(this).hasClass('alert-dismissible')) {
                $(this).fadeTo(500, 0).slideUp(500, function() {
                    $(this).alert('close');
                });
            }
        });
    }, 5000);
});

function showTempDataNotifications() {
    // Успех
    var successMsg = $('#tempSuccessMessage');
    if (successMsg.length) {
        showNotification(successMsg.text(), 'success');
        successMsg.remove();
    }

    // Ошибка
    var errorMsg = $('#tempErrorMessage');
    if (errorMsg.length) {
        showNotification(errorMsg.text(), 'error');
        errorMsg.remove();
    }

    // Предупреждение
    var warningMsg = $('#tempWarningMessage');
    if (warningMsg.length) {
        showNotification(warningMsg.text(), 'warning');
        warningMsg.remove();
    }

    // Информация
    var infoMsg = $('#tempInfoMessage');
    if (infoMsg.length) {
        showNotification(infoMsg.text(), 'info');
        infoMsg.remove();
    }
}

function showNotification(message, type = 'info') {
    var alertClass = 'alert-info';
    var icon = 'fa-info-circle';
    
    if (type === 'success') {
        alertClass = 'alert-success';
        icon = 'fa-check-circle';
    } else if (type === 'error') {
        alertClass = 'alert-danger';
        icon = 'fa-exclamation-circle';
    } else if (type === 'warning') {
        alertClass = 'alert-warning';
        icon = 'fa-exclamation-triangle';
    }

    var notification = $(
        '<div class="alert ' + alertClass + ' alert-dismissible fade show notification-alert" role="alert" style="position: fixed; top: 80px; right: 20px; z-index: 1060; min-width: 300px; max-width: 400px; animation: slideInRight 0.3s ease;">' +
        '<i class="fas ' + icon + ' me-2"></i>' +
        '<strong>' + message + '</strong>' +
        '<button type="button" class="btn-close" data-bs-dismiss="alert"></button>' +
        '</div>'
    );

    $('body').append(notification);

    // Автоматическое скрытие через 5 секунд
    setTimeout(function() {
        notification.fadeTo(500, 0).slideUp(500, function() {
            $(this).alert('close');
        });
    }, 5000);
}

