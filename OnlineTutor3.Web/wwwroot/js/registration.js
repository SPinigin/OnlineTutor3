// Обработка выбора роли при клике на карточку
$(document).ready(function() {
    // Обработка клика на карточку роли
    $('.role-card').on('click', function() {
        var radio = $(this).find('input[type="radio"]');
        if (radio.length) {
            radio.prop('checked', true);
            updateRoleSelection();
        }
    });

    // Обработка изменения radio button (на случай прямого клика)
    $('input[name="Role"]').on('change', function() {
        updateRoleSelection();
    });

    // Инициализация при загрузке страницы
    updateRoleSelection();
    
    // Инициализация toggle паролей
    initializePasswordToggles();
    
    // Инициализация валидации
    initializeValidation();
    
    // Инициализация маски телефона
    initializePhoneMask();
});

function updateRoleSelection() {
    var selectedRole = $('input[name="Role"]:checked').val();
    
    // Убираем все классы выбора
    $('.role-card').removeClass('selected-student selected-teacher');
    
    // Показываем/скрываем поля в зависимости от выбранной роли
    if (selectedRole === 'Student') {
        $('#roleStudent').closest('.role-card').addClass('selected-student');
        $('#studentFields').show();
        $('#teacherFields').hide();
    } else if (selectedRole === 'Teacher') {
        $('#roleTeacher').closest('.role-card').addClass('selected-teacher');
        $('#studentFields').hide();
        $('#teacherFields').show();
    } else {
        $('#studentFields').hide();
        $('#teacherFields').hide();
    }
}

// Инициализация toggle паролей
function initializePasswordToggles() {
    $('input[type="password"]').each(function() {
        var $input = $(this);
        var inputId = $input.attr('id');
        
        if (!inputId) {
            inputId = 'password_' + Math.random().toString(36).substr(2, 9);
            $input.attr('id', inputId);
        }
        
        // Проверяем, не добавлена ли уже кнопка
        if ($input.parent().find('.password-toggle-btn').length === 0) {
            // Обертываем в контейнер если нужно
            if (!$input.parent().hasClass('password-input-group')) {
                $input.wrap('<div class="password-input-group"></div>');
            }
            
            // Добавляем кнопку
            var $toggleBtn = $('<button type="button" class="password-toggle-btn" onclick="togglePassword(\'' + inputId + '\', this)"><i class="fas fa-eye"></i></button>');
            $input.after($toggleBtn);
        }
    });
}

// Переключение видимости пароля
function togglePassword(inputId, button) {
    var $input = $('#' + inputId);
    var $icon = $(button).find('i');
    
    if ($input.attr('type') === 'password') {
        $input.attr('type', 'text');
        $icon.removeClass('fa-eye').addClass('fa-eye-slash');
    } else {
        $input.attr('type', 'password');
        $icon.removeClass('fa-eye-slash').addClass('fa-eye');
    }
}

// Инициализация валидации
function initializeValidation() {
    // Валидация email в реальном времени
    $('#Email').on('blur', function() {
        validateEmail($(this));
    });
    
    // Валидация телефона в реальном времени
    $('#PhoneNumber').on('blur', function() {
        validatePhone($(this));
    });
    
    // Удаление ошибок при вводе
    $('#Email, #PhoneNumber').on('input', function() {
        clearFieldError($(this));
    });
}

// Валидация email
function validateEmail($field) {
    var email = $field.val().trim();
    
    if (!email) {
        return true; // Пустое поле обрабатывается required
    }
    
    var emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(email)) {
        showFieldError($field, 'Введите корректный email адрес');
        return false;
    }
    
    clearFieldError($field);
    $field.addClass('is-valid');
    return true;
}

// Валидация телефона
function validatePhone($field) {
    var phone = $field.val().trim();
    
    if (!phone) {
        return true; // Пустое поле обрабатывается required
    }
    
    // Убираем все нецифровые символы кроме +
    var cleanPhone = phone.replace(/[^\d+]/g, '');
    
    // Проверяем формат: +7 и 10 цифр или 11 цифр
    if (cleanPhone.startsWith('+7')) {
        cleanPhone = cleanPhone.substring(2);
    } else if (cleanPhone.startsWith('7')) {
        cleanPhone = cleanPhone.substring(1);
    } else if (cleanPhone.startsWith('8')) {
        cleanPhone = cleanPhone.substring(1);
    }
    
    if (cleanPhone.length !== 10) {
        showFieldError($field, 'Номер телефона должен содержать 10 цифр');
        return false;
    }
    
    clearFieldError($field);
    $field.addClass('is-valid');
    return true;
}

// Показать ошибку поля
function showFieldError($field, message) {
    $field.removeClass('is-valid').addClass('is-invalid');
    
    var $errorElement = $field.siblings('.field-error');
    if ($errorElement.length === 0) {
        $errorElement = $('<div class="field-error text-danger small mt-1"></div>');
        $field.after($errorElement);
    }
    
    $errorElement.text(message).show();
}

// Очистить ошибку поля
function clearFieldError($field) {
    $field.removeClass('is-invalid');
    $field.siblings('.field-error').remove();
}

// Инициализация маски телефона
function initializePhoneMask() {
    $('#PhoneNumber').on('input', function() {
        formatPhoneNumber(this);
    });
}

// Форматирование номера телефона
function formatPhoneNumber(input) {
    var value = input.value.replace(/\D/g, '');
    var formattedValue = '';
    
    if (value.length > 0) {
        // Убираем первую цифру если это 8 или 7
        if (value.startsWith('8') || value.startsWith('7')) {
            value = value.substring(1);
        }
        
        if (value.length <= 3) {
            formattedValue = '+7 (' + value;
        } else if (value.length <= 6) {
            formattedValue = '+7 (' + value.substring(0, 3) + ') ' + value.substring(3);
        } else if (value.length <= 8) {
            formattedValue = '+7 (' + value.substring(0, 3) + ') ' + value.substring(3, 6) + '-' + value.substring(6);
        } else {
            formattedValue = '+7 (' + value.substring(0, 3) + ') ' + value.substring(3, 6) + '-' + value.substring(6, 8) + '-' + value.substring(8, 10);
        }
    }
    
    input.value = formattedValue;
}

