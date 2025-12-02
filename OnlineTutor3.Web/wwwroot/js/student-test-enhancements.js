/**
 * Улучшения UX для студентских тестов
 * - Плавные анимации
 * - Визуальная обратная связь
 * - Мобильная оптимизация
 */

(function() {
    'use strict';

    // Создаем индикатор сохранения
    function createSaveIndicator() {
        if (document.getElementById('saveIndicator')) {
            return;
        }
        
        const indicator = document.createElement('div');
        indicator.id = 'saveIndicator';
        indicator.className = 'save-indicator';
        indicator.innerHTML = '<i class="fas fa-check-circle me-2"></i><span>Ответ сохранен</span>';
        document.body.appendChild(indicator);
    }

    // Показать индикатор сохранения
    function showSaveIndicator(success = true, message = null) {
        createSaveIndicator();
        const indicator = document.getElementById('saveIndicator');
        
        if (success) {
            indicator.className = 'save-indicator show';
            indicator.innerHTML = '<i class="fas fa-check-circle me-2"></i><span>' + (message || 'Ответ сохранен') + '</span>';
        } else {
            indicator.className = 'save-indicator show error';
            indicator.innerHTML = '<i class="fas fa-exclamation-circle me-2"></i><span>' + (message || 'Ошибка сохранения') + '</span>';
        }
        
        // Автоматически скрыть через 3 секунды
        setTimeout(() => {
            indicator.classList.remove('show');
            setTimeout(() => {
                indicator.remove();
            }, 300);
        }, 3000);
    }

    // Плавное переключение вопросов
    function animateQuestionTransition(container, direction = 'next') {
        if (!container) return;
        
        container.style.opacity = '0';
        container.style.transform = direction === 'next' ? 'translateX(20px)' : 'translateX(-20px)';
        
        setTimeout(() => {
            container.style.transition = 'all 0.3s ease-in-out';
            container.style.opacity = '1';
            container.style.transform = 'translateX(0)';
        }, 50);
    }

    // Показать индикатор загрузки
    function showLoadingOverlay() {
        let overlay = document.getElementById('loadingOverlay');
        if (!overlay) {
            overlay = document.createElement('div');
            overlay.id = 'loadingOverlay';
            overlay.className = 'loading-overlay';
            overlay.innerHTML = '<div class="loading-spinner"></div>';
            document.body.appendChild(overlay);
        }
        overlay.classList.add('show');
    }

    // Скрыть индикатор загрузки
    function hideLoadingOverlay() {
        const overlay = document.getElementById('loadingOverlay');
        if (overlay) {
            overlay.classList.remove('show');
        }
    }

    // Улучшенная обработка автосохранения с визуальной обратной связью
    function enhanceAutoSave(saveFunction) {
        let saveTimeout;
        let isSaving = false;
        
        return function(questionId, answer) {
            clearTimeout(saveTimeout);
            
            // Показываем индикатор "сохранение..."
            if (!isSaving) {
                const indicator = document.getElementById('saveIndicator');
                if (indicator) {
                    indicator.className = 'save-indicator show';
                    indicator.innerHTML = '<i class="fas fa-spinner fa-spin me-2"></i><span>Сохранение...</span>';
                }
            }
            
            saveTimeout = setTimeout(async () => {
                isSaving = true;
                try {
                    await saveFunction(questionId, answer);
                    showSaveIndicator(true);
                } catch (error) {
                    showSaveIndicator(false, 'Ошибка сохранения. Попробуйте еще раз.');
                } finally {
                    isSaving = false;
                }
            }, 500); // Задержка 500мс перед сохранением
        };
    }

    // Предотвращение случайного закрытия страницы
    function preventAccidentalClose() {
        let formChanged = false;
        
        // Отслеживаем изменения в форме
        document.addEventListener('input', function() {
            formChanged = true;
        });
        
        // Предупреждение при закрытии
        window.addEventListener('beforeunload', function(e) {
            if (formChanged) {
                e.preventDefault();
                e.returnValue = 'Вы уверены, что хотите покинуть страницу? Ваши ответы могут быть не сохранены.';
                return e.returnValue;
            }
        });
        
        // Сбрасываем флаг после успешного сохранения
        document.addEventListener('answerSaved', function() {
            formChanged = false;
        });
    }

    // Улучшенная валидация на клиенте
    function enhanceClientValidation() {
        // Добавляем визуальную обратную связь при вводе
        const inputs = document.querySelectorAll('.answer-input');
        inputs.forEach(input => {
            input.addEventListener('input', function() {
                if (this.value.trim()) {
                    this.classList.add('valid');
                    this.classList.remove('invalid');
                } else {
                    this.classList.remove('valid');
                }
            });
            
            input.addEventListener('blur', function() {
                if (!this.value.trim()) {
                    this.classList.add('invalid');
                }
            });
        });
    }

    // Оптимизация для мобильных устройств
    function optimizeForMobile() {
        if (window.innerWidth <= 768) {
            // Увеличиваем размер кнопок для удобного нажатия
            const buttons = document.querySelectorAll('.btn');
            buttons.forEach(btn => {
                if (!btn.style.minHeight) {
                    btn.style.minHeight = '44px';
                }
            });
            
            // Улучшаем прокрутку на мобильных
            document.body.style.overflowX = 'hidden';
        }
    }

    // Инициализация улучшений
    function initializeEnhancements() {
        createSaveIndicator();
        preventAccidentalClose();
        enhanceClientValidation();
        optimizeForMobile();
        
        // Оптимизация при изменении размера окна
        let resizeTimeout;
        window.addEventListener('resize', function() {
            clearTimeout(resizeTimeout);
            resizeTimeout = setTimeout(optimizeForMobile, 250);
        });
    }

    // Экспорт функций для использования в других скриптах
    window.StudentTestEnhancements = {
        showSaveIndicator: showSaveIndicator,
        animateQuestionTransition: animateQuestionTransition,
        showLoadingOverlay: showLoadingOverlay,
        hideLoadingOverlay: hideLoadingOverlay,
        enhanceAutoSave: enhanceAutoSave,
        initialize: initializeEnhancements
    };

    // Автоматическая инициализация при загрузке DOM
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initializeEnhancements);
    } else {
        initializeEnhancements();
    }
})();

