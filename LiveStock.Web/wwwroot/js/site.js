// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.


document.addEventListener('DOMContentLoaded', function() {
    // ===== SHEEP MANAGEMENT =====
    const selectAllSheep = document.getElementById('selectAllSheep');
    if (selectAllSheep) {
        selectAllSheep.addEventListener('change', function() {
            const checked = this.checked;
            document.querySelectorAll('.sheep-select').forEach(cb => cb.checked = checked);
        });
    }

    // ===== COW MANAGEMENT =====
    const selectAllCows = document.getElementById('selectAllCows');
    if (selectAllCows) {
        selectAllCows.addEventListener('change', function() {
            const checked = this.checked;
            document.querySelectorAll('.cow-select').forEach(cb => cb.checked = checked);
        });
    }

    // ===== ADD COW - PREGNANCY TOGGLE =====
    const isPregnant = document.getElementById('isPregnant');
    const calvingDateDiv = document.getElementById('calvingDateDiv');
    if (isPregnant && calvingDateDiv) {
        isPregnant.addEventListener('change', function() {
            calvingDateDiv.style.display = this.checked ? 'block' : 'none';
        });
    }

    // ===== TASK MANAGEMENT =====
    const taskForm = document.getElementById('taskForm');
    if (taskForm) {
        taskForm.addEventListener('submit', function(e) {
            e.preventDefault();
            alert('Task submitted successfully!');
        });
    }

    // ===== CHAT FUNCTIONALITY =====
    const chatInput = document.getElementById('chatInput');
    const chatWindow = document.querySelector('.chat-window');
    if (chatInput && chatWindow) {
        chatInput.addEventListener('keypress', function(e) {
            if (e.key === 'Enter') {
                e.preventDefault();
                sendMessage();
            }
        });
    }

    // ===== FINANCIAL RECORDS =====
    const financialRecordForm = document.getElementById('financialRecordForm');
    if (financialRecordForm) {
        // Set default date to today
        const transactionDateInput = document.querySelector('input[name="transactionDate"]');
        if (transactionDateInput) {
            transactionDateInput.value = new Date().toISOString().split('T')[0];
        }

        financialRecordForm.addEventListener('submit', function(e) {
            e.preventDefault();
            
            const formData = new FormData(this);
            const type = formData.get('type');
            const description = formData.get('description');
            const amount = formData.get('amount');
            
            alert(`${type} record "${description}" for $${amount} added successfully!`);
            
            // Close modal
            const modal = document.getElementById('addFinancialRecordModal');
            if (modal) {
                const bsModal = bootstrap.Modal.getInstance(modal);
                if (bsModal) bsModal.hide();
            }
            
            // Reset form
            this.reset();
            if (transactionDateInput) {
                transactionDateInput.value = new Date().toISOString().split('T')[0];
            }
        });
    }

    // ===== WATER MANAGEMENT =====
    // Rainfall form submission is handled in Water.cshtml to avoid duplicate bindings.
    // const rainfallForm = document.getElementById('rainfallForm');
    // if (rainfallForm) {
    //     rainfallForm.addEventListener('submit', function(e) {
    //         e.preventDefault();
    //         const formData = new FormData(this);
    //         const amountMl = formData.get('amountMl');
    //         const campNumber = window.selectedCamp;
    //         // Removed alert-only handler. Submission now handled by page-specific script.
    //     });
    // }

    // ===== ACCESSIBILITY ENHANCEMENTS =====
    document.querySelectorAll('.btn-group .btn').forEach(function(btn) {
        if (btn) {
            btn.setAttribute('tabindex', '0');
            btn.setAttribute('aria-label', btn.textContent.trim());
        }
    });

    document.querySelectorAll('.btn, .nav-link').forEach(function(el) {
        if (el) {
            el.addEventListener('focus', function() {
                el.style.outline = '2px solid #EB9C35';
            });
            el.addEventListener('blur', function() {
                el.style.outline = '';
            });
        }
    });
});

// ===== GLOBAL FUNCTIONS =====

// Sheep bulk actions
function performBulkActionSheep() {
    const actionType = document.getElementById('bulkActionTypeSheep');
    const actionValue = document.getElementById('bulkActionValueSheep');
    
    if (!actionType || !actionValue) return;
    
    const selectedSheep = Array.from(document.querySelectorAll('.sheep-select:checked')).map(cb => cb.value);
    if (!actionType.value || selectedSheep.length === 0) {
        alert('Please select an action and at least one sheep.');
        return;
    }
    alert(`Action '${actionType.value}' applied to sheep: ${selectedSheep.join(', ')}${actionValue.value ? ' with value: ' + actionValue.value : ''}`);
}

// Chat functionality
function sendMessage() {
    const chatInput = document.getElementById('chatInput');
    const chatWindow = document.querySelector('.chat-window');
    
    if (!chatInput || !chatWindow) return;
    
    const message = chatInput.value.trim();
    if (message) {
        const messageElement = document.createElement('div');
        messageElement.className = 'message sent';
        messageElement.innerHTML = `
            <div class="message-content">${message}</div>
            <div class="message-time">${new Date().toLocaleTimeString([], {hour: '2-digit', minute:'2-digit'})}</div>
        `;
        chatWindow.appendChild(messageElement);
        chatInput.value = '';
        
        setTimeout(() => {
            const responseElement = document.createElement('div');
            responseElement.className = 'message received';
            responseElement.innerHTML = `
                <div class="message-content">Thanks for your message. I'll look into it.</div>
                <div class="message-time">${new Date().toLocaleTimeString([], {hour: '2-digit', minute:'2-digit'})}</div>
            `;
            chatWindow.appendChild(responseElement);
            chatWindow.scrollTop = chatWindow.scrollHeight;
        }, 1000);
        
        chatWindow.scrollTop = chatWindow.scrollHeight;
    }
}

// Water management - Camp details
window.selectedCamp = null;

function showCampDetails(campNumber) {
    window.selectedCamp = campNumber;
    
    const campHeader = document.getElementById('campHeader');
    const campDetails = document.getElementById('campDetails');
    const campButtons = document.querySelectorAll('.camp-grid-btn');
    
    if (!campHeader || !campDetails) return;
    
    // Update camp header
    campHeader.textContent = `Camp ${campNumber}`;
    
    // Update camp details content
    campDetails.innerHTML = `
        <div class="row">
            <div class="col-6">
                <h6>Previous rainfall in camp ${campNumber}:</h6>
                <ul class="list-unstyled">
                    <li>08/02/2025: 10ml</li>
                    <li>20/03/2025: 8ml</li>
                </ul>
            </div>
            <div class="col-6">
                <h6>Latest rain report for camp ${campNumber}:</h6>
                <p class="text-info">46 days ago you have 8ml rain in camp ${campNumber}</p>
            </div>
        </div>
        <hr>
        <div class="text-center">
            <button class="btn btn-info" onclick="openAddRainfallModal(${campNumber})">
                <i class="fas fa-plus me-2"></i>Add Rain for camp ${campNumber}
            </button>
        </div>
    `;
    
    // Highlight selected camp button
    if (campButtons) {
        campButtons.forEach(btn => {
            if (btn) {
                btn.classList.remove('btn-info');
                btn.classList.add('btn-outline-info');
            }
        });
        
        const clickedButton = Array.from(campButtons).find(btn => 
            btn && btn.textContent.includes(campNumber.toString()));
        
        if (clickedButton) {
            clickedButton.classList.remove('btn-outline-info');
            clickedButton.classList.add('btn-info');
        }
    }
}

// Water management - Rainfall modal
function openAddRainfallModal(campNumber) {
    const modalCampNumber = document.getElementById('modalCampNumber');
    const rainfallForm = document.getElementById('rainfallForm');
    const rainfallDateInput = document.querySelector('input[name="rainfallDate"]');
    const rainfallModal = document.getElementById('addRainfallModal');
    
    if (!modalCampNumber || !rainfallForm || !rainfallDateInput || !rainfallModal) return;
    
    modalCampNumber.textContent = campNumber;
    rainfallForm.reset();
    rainfallDateInput.value = new Date().toISOString().split('T')[0];
    
    const modal = new bootstrap.Modal(rainfallModal);
    modal.show();
}

// Financial record management
function editFinancialRecord(recordId) {
    if (recordId) {
        alert(`Edit financial record ${recordId}`);
    }
}

function removeFinancialRecord(recordId) {
    if (recordId && confirm('Are you sure you want to remove this financial record?')) {
        alert(`Financial record ${recordId} removed successfully`);
    }
}

document.addEventListener('DOMContentLoaded', function() {
    // Handle chat input for Tasks page
    const chatInput = document.getElementById('chatInput');
    if (chatInput) {
        chatInput.addEventListener('keypress', function(e) {
            if (e.key === 'Enter') {
                sendMessage();
            }
        });
    }

    // Handle select all sheep checkbox
    const selectAllSheep = document.getElementById('selectAllSheep');
    if (selectAllSheep) {
        selectAllSheep.addEventListener('change', function() {
            const checked = this.checked;
            document.querySelectorAll('.sheep-select').forEach(cb => cb.checked = checked);
        });
    }

    // Handle select all cows checkbox
    const selectAllCows = document.getElementById('selectAllCows');
    if (selectAllCows) {
        selectAllCows.addEventListener('change', function() {
            const checked = this.checked;
            document.querySelectorAll('.cow-select').forEach(cb => cb.checked = checked);
        });
    }

    // Handle pregnancy checkbox in AddCow page
    const isPregnant = document.getElementById('isPregnant');
    if (isPregnant) {
        isPregnant.addEventListener('change', function() {
            const calvingDateDiv = document.getElementById('calvingDateDiv');
            if (calvingDateDiv) {
                calvingDateDiv.style.display = this.checked ? 'block' : 'none';
            }
        });
    }

    // Add keyboard accessibility for table actions
    const btnGroupBtns = document.querySelectorAll('.btn-group .btn');
    if (btnGroupBtns.length > 0) {
        btnGroupBtns.forEach(function(btn) {
            btn.setAttribute('tabindex', '0');
            btn.setAttribute('aria-label', btn.textContent.trim());
        });
    }

    // Add focus style for accessibility
    const focusableElements = document.querySelectorAll('.btn, .nav-link');
    if (focusableElements.length > 0) {
        focusableElements.forEach(function(el) {
            el.addEventListener('focus', function() {
                el.style.outline = '2px solid #EB9C35';
            });
            el.addEventListener('blur', function() {
                el.style.outline = '';
            });
        });
    }

    // Handle task form submission
    const taskForm = document.getElementById('taskForm');
    if (taskForm) {
        taskForm.addEventListener('submit', function(e) {
            e.preventDefault();
            alert('Task assigned successfully!');
        });
    }

    // Handle financial record form submission
    const financialRecordForm = document.getElementById('financialRecordForm');
    if (financialRecordForm) {
        financialRecordForm.addEventListener('submit', function(e) {
            e.preventDefault();
            
            const formData = new FormData(this);
            const type = formData.get('type');
            const description = formData.get('description');
            const amount = formData.get('amount');
            
            alert(`${type} record "${description}" for $${amount} added successfully!`);
            
            // Close modal
            const modal = bootstrap.Modal.getInstance(document.getElementById('addFinancialRecordModal'));
            if (modal) {
                modal.hide();
            }
            
            // Reset form
            this.reset();
            const transactionDateInput = document.querySelector('input[name="transactionDate"]');
            if (transactionDateInput) {
                transactionDateInput.value = new Date().toISOString().split('T')[0];
            }
        });
    }


    const transactionDateInput = document.querySelector('input[name="transactionDate"]');
    if (transactionDateInput) {
        transactionDateInput.value = new Date().toISOString().split('T')[0];
    }

    // Handle rainfall form submission (duplicate)
    // Removed duplicate alert-only handler to defer to Water.cshtml page script.
    // const rainfallForm = document.getElementById('rainfallForm');
    // if (rainfallForm) {
    //     rainfallForm.addEventListener('submit', function(e) {
    //         e.preventDefault();
    //         const formData = new FormData(this);
    //         const amountMl = formData.get('amountMl');
    //         const campNumber = window.selectedCamp || 1;
    //         // Removed alert and modal hide here; page handles submission and reload.
    //     });
    // }
});


// Send message function for chat
function sendMessage() {
    const input = document.getElementById('chatInput');
    if (!input) return;
    
    const message = input.value.trim();
    
    if (message) {
        const chatWindow = document.querySelector('.chat-window');
        if (chatWindow) {
            const newMessage = document.createElement('div');
            newMessage.className = 'chat-message mb-2';
            newMessage.innerHTML = `<strong>You:</strong> ${message}`;
            
            chatWindow.appendChild(newMessage);
            chatWindow.scrollTop = chatWindow.scrollHeight;
            input.value = '';
        }
    }
}

// Show camp details function for Water page
function showCampDetails(campNumber) {
    if (typeof window !== 'undefined') {
        window.selectedCamp = campNumber;
    }
    
    // Update camp header
    const campHeader = document.getElementById('campHeader');
    if (!campHeader) return;
    
    campHeader.textContent = `Camp ${campNumber}`;
    
    // Update camp details content
    const campDetails = document.getElementById('campDetails');
    if (!campDetails) return;
    
    campDetails.innerHTML = `
        <div class="row">
            <div class="col-6">
                <h6>Previous rainfall in camp ${campNumber}:</h6>
                <ul class="list-unstyled">
                    <li>08/02/2025: 10ml</li>
                    <li>20/03/2025: 8ml</li>
                </ul>
            </div>
            <div class="col-6">
                <h6>Latest rain report for camp ${campNumber}:</h6>
                <p class="text-info">46 days ago you have 8ml rain in camp ${campNumber}</p>
            </div>
        </div>
        <hr>
        <div class="text-center">
            <button class="btn btn-info" onclick="openAddRainfallModal(${campNumber})">
                <i class="fas fa-plus me-2"></i>Add Rain for camp ${campNumber}
            </button>
        </div>
    `;
    
    // Highlight selected camp button
    const campButtons = document.querySelectorAll('.camp-grid-btn');
    if (campButtons.length > 0) {
        campButtons.forEach(btn => {
            btn.classList.remove('btn-info');
            btn.classList.add('btn-outline-info');
        });
        
        const selectedButton = document.querySelector(`.camp-grid-btn[data-camp="${campNumber}"]`) || 
                              document.querySelector(`.camp-grid-btn:nth-child(${campNumber})`);
        if (selectedButton) {
            selectedButton.classList.remove('btn-outline-info');
            selectedButton.classList.add('btn-info');
        }
    }
}

// Open add rainfall modal function
function openAddRainfallModal(campNumber) {
    const modalCampNumber = document.getElementById('modalCampNumber');
    if (modalCampNumber) {
        modalCampNumber.textContent = campNumber;
    }
    
    const rainfallForm = document.getElementById('rainfallForm');
    if (rainfallForm) {
        rainfallForm.reset();
    }
    
    const rainfallDate = document.querySelector('input[name="rainfallDate"]');
    if (rainfallDate) {
        rainfallDate.value = new Date().toISOString().split('T')[0];
    }
    
    const modal = document.getElementById('addRainfallModal');
    if (modal) {
        const bsModal = new bootstrap.Modal(modal);
        bsModal.show();
    }
}

// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.


document.addEventListener('DOMContentLoaded', function() {
    // ===== SHEEP MANAGEMENT =====
    const selectAllSheep = document.getElementById('selectAllSheep');
    if (selectAllSheep) {
        selectAllSheep.addEventListener('change', function() {
            const checked = this.checked;
            document.querySelectorAll('.sheep-select').forEach(cb => cb.checked = checked);
        });
    }

    // ===== COW MANAGEMENT =====
    const selectAllCows = document.getElementById('selectAllCows');
    if (selectAllCows) {
        selectAllCows.addEventListener('change', function() {
            const checked = this.checked;
            document.querySelectorAll('.cow-select').forEach(cb => cb.checked = checked);
        });
    }

    // ===== ADD COW - PREGNANCY TOGGLE =====
    const isPregnant = document.getElementById('isPregnant');
    const calvingDateDiv = document.getElementById('calvingDateDiv');
    if (isPregnant && calvingDateDiv) {
        isPregnant.addEventListener('change', function() {
            calvingDateDiv.style.display = this.checked ? 'block' : 'none';
        });
    }

    // ===== TASK MANAGEMENT =====
    const taskForm = document.getElementById('taskForm');
    if (taskForm) {
        taskForm.addEventListener('submit', function(e) {
            e.preventDefault();
            alert('Task submitted successfully!');
        });
    }

    // ===== CHAT FUNCTIONALITY =====
    const chatInput = document.getElementById('chatInput');
    const chatWindow = document.querySelector('.chat-window');
    if (chatInput && chatWindow) {
        chatInput.addEventListener('keypress', function(e) {
            if (e.key === 'Enter') {
                e.preventDefault();
                sendMessage();
            }
        });
    }

    // ===== FINANCIAL RECORDS =====
    const financialRecordForm = document.getElementById('financialRecordForm');
    if (financialRecordForm) {
        // Set default date to today
        const transactionDateInput = document.querySelector('input[name="transactionDate"]');
        if (transactionDateInput) {
            transactionDateInput.value = new Date().toISOString().split('T')[0];
        }

        financialRecordForm.addEventListener('submit', function(e) {
            e.preventDefault();
            
            const formData = new FormData(this);
            const type = formData.get('type');
            const description = formData.get('description');
            const amount = formData.get('amount');
            
            alert(`${type} record "${description}" for $${amount} added successfully!`);
            
            // Close modal
            const modal = document.getElementById('addFinancialRecordModal');
            if (modal) {
                const bsModal = bootstrap.Modal.getInstance(modal);
                if (bsModal) bsModal.hide();
            }
            
            // Reset form
            this.reset();
            if (transactionDateInput) {
                transactionDateInput.value = new Date().toISOString().split('T')[0];
            }
        });
    }

    // ===== WATER MANAGEMENT =====
    // Rainfall form submission is handled in Water.cshtml to avoid duplicate bindings.
    // const rainfallForm = document.getElementById('rainfallForm');
    // if (rainfallForm) {
    //     rainfallForm.addEventListener('submit', function(e) {
    //         e.preventDefault();
    //         const formData = new FormData(this);
    //         const amountMl = formData.get('amountMl');
    //         const campNumber = window.selectedCamp;
    //         // Removed alert-only handler. Submission now handled by page-specific script.
    //     });
    // }

    // ===== ACCESSIBILITY ENHANCEMENTS =====
    document.querySelectorAll('.btn-group .btn').forEach(function(btn) {
        if (btn) {
            btn.setAttribute('tabindex', '0');
            btn.setAttribute('aria-label', btn.textContent.trim());
        }
    });

    document.querySelectorAll('.btn, .nav-link').forEach(function(el) {
        if (el) {
            el.addEventListener('focus', function() {
                el.style.outline = '2px solid #EB9C35';
            });
            el.addEventListener('blur', function() {
                el.style.outline = '';
            });
        }
    });
});

// ===== GLOBAL FUNCTIONS =====

// Sheep bulk actions
function performBulkActionSheep() {
    const actionType = document.getElementById('bulkActionTypeSheep');
    const actionValue = document.getElementById('bulkActionValueSheep');
    
    if (!actionType || !actionValue) return;
    
    const selectedSheep = Array.from(document.querySelectorAll('.sheep-select:checked')).map(cb => cb.value);
    if (!actionType.value || selectedSheep.length === 0) {
        alert('Please select an action and at least one sheep.');
        return;
    }
    alert(`Action '${actionType.value}' applied to sheep: ${selectedSheep.join(', ')}${actionValue.value ? ' with value: ' + actionValue.value : ''}`);
}

// Chat functionality
function sendMessage() {
    const chatInput = document.getElementById('chatInput');
    const chatWindow = document.querySelector('.chat-window');
    
    if (!chatInput || !chatWindow) return;
    
    const message = chatInput.value.trim();
    if (message) {
        const messageElement = document.createElement('div');
        messageElement.className = 'message sent';
        messageElement.innerHTML = `
            <div class="message-content">${message}</div>
            <div class="message-time">${new Date().toLocaleTimeString([], {hour: '2-digit', minute:'2-digit'})}</div>
        `;
        chatWindow.appendChild(messageElement);
        chatInput.value = '';
        
        setTimeout(() => {
            const responseElement = document.createElement('div');
            responseElement.className = 'message received';
            responseElement.innerHTML = `
                <div class="message-content">Thanks for your message. I'll look into it.</div>
                <div class="message-time">${new Date().toLocaleTimeString([], {hour: '2-digit', minute:'2-digit'})}</div>
            `;
            chatWindow.appendChild(responseElement);
            chatWindow.scrollTop = chatWindow.scrollHeight;
        }, 1000);
        
        chatWindow.scrollTop = chatWindow.scrollHeight;
    }
}

// Water management - Camp details
window.selectedCamp = null;

function showCampDetails(campNumber) {
    window.selectedCamp = campNumber;
    
    const campHeader = document.getElementById('campHeader');
    const campDetails = document.getElementById('campDetails');
    const campButtons = document.querySelectorAll('.camp-grid-btn');
    
    if (!campHeader || !campDetails) return;
    
    // Update camp header
    campHeader.textContent = `Camp ${campNumber}`;
    
    // Update camp details content
    campDetails.innerHTML = `
        <div class="row">
            <div class="col-6">
                <h6>Previous rainfall in camp ${campNumber}:</h6>
                <ul class="list-unstyled">
                    <li>08/02/2025: 10ml</li>
                    <li>20/03/2025: 8ml</li>
                </ul>
            </div>
            <div class="col-6">
                <h6>Latest rain report for camp ${campNumber}:</h6>
                <p class="text-info">46 days ago you have 8ml rain in camp ${campNumber}</p>
            </div>
        </div>
        <hr>
        <div class="text-center">
            <button class="btn btn-info" onclick="openAddRainfallModal(${campNumber})">
                <i class="fas fa-plus me-2"></i>Add Rain for camp ${campNumber}
            </button>
        </div>
    `;
    
    // Highlight selected camp button
    if (campButtons) {
        campButtons.forEach(btn => {
            if (btn) {
                btn.classList.remove('btn-info');
                btn.classList.add('btn-outline-info');
            }
        });
        
        const clickedButton = Array.from(campButtons).find(btn => 
            btn && btn.textContent.includes(campNumber.toString()));
        
        if (clickedButton) {
            clickedButton.classList.remove('btn-outline-info');
            clickedButton.classList.add('btn-info');
        }
    }
}

// Water management - Rainfall modal
function openAddRainfallModal(campNumber) {
    const modalCampNumber = document.getElementById('modalCampNumber');
    const rainfallForm = document.getElementById('rainfallForm');
    const rainfallDateInput = document.querySelector('input[name="rainfallDate"]');
    const rainfallModal = document.getElementById('addRainfallModal');
    
    if (!modalCampNumber || !rainfallForm || !rainfallDateInput || !rainfallModal) return;
    
    modalCampNumber.textContent = campNumber;
    rainfallForm.reset();
    rainfallDateInput.value = new Date().toISOString().split('T')[0];
    
    const modal = new bootstrap.Modal(rainfallModal);
    modal.show();
}

// Financial record management
function editFinancialRecord(recordId) {
    if (recordId) {
        alert(`Edit financial record ${recordId}`);
    }
}

function removeFinancialRecord(recordId) {
    if (recordId && confirm('Are you sure you want to remove this financial record?')) {
        alert(`Financial record ${recordId} removed successfully`);
    }
}

document.addEventListener('DOMContentLoaded', function() {
    // Handle chat input for Tasks page
    const chatInput = document.getElementById('chatInput');
    if (chatInput) {
        chatInput.addEventListener('keypress', function(e) {
            if (e.key === 'Enter') {
                sendMessage();
            }
        });
    }

    // Handle select all sheep checkbox
    const selectAllSheep = document.getElementById('selectAllSheep');
    if (selectAllSheep) {
        selectAllSheep.addEventListener('change', function() {
            const checked = this.checked;
            document.querySelectorAll('.sheep-select').forEach(cb => cb.checked = checked);
        });
    }

    // Handle select all cows checkbox
    const selectAllCows = document.getElementById('selectAllCows');
    if (selectAllCows) {
        selectAllCows.addEventListener('change', function() {
            const checked = this.checked;
            document.querySelectorAll('.cow-select').forEach(cb => cb.checked = checked);
        });
    }

    // Handle pregnancy checkbox in AddCow page
    const isPregnant = document.getElementById('isPregnant');
    if (isPregnant) {
        isPregnant.addEventListener('change', function() {
            const calvingDateDiv = document.getElementById('calvingDateDiv');
            if (calvingDateDiv) {
                calvingDateDiv.style.display = this.checked ? 'block' : 'none';
            }
        });
    }

    // Add keyboard accessibility for table actions
    const btnGroupBtns = document.querySelectorAll('.btn-group .btn');
    if (btnGroupBtns.length > 0) {
        btnGroupBtns.forEach(function(btn) {
            btn.setAttribute('tabindex', '0');
            btn.setAttribute('aria-label', btn.textContent.trim());
        });
    }

    // Add focus style for accessibility
    const focusableElements = document.querySelectorAll('.btn, .nav-link');
    if (focusableElements.length > 0) {
        focusableElements.forEach(function(el) {
            el.addEventListener('focus', function() {
                el.style.outline = '2px solid #EB9C35';
            });
            el.addEventListener('blur', function() {
                el.style.outline = '';
            });
        });
    }

    // Handle task form submission
    const taskForm = document.getElementById('taskForm');
    if (taskForm) {
        taskForm.addEventListener('submit', function(e) {
            e.preventDefault();
            alert('Task assigned successfully!');
        });
    }

    // Handle financial record form submission
    const financialRecordForm = document.getElementById('financialRecordForm');
    if (financialRecordForm) {
        financialRecordForm.addEventListener('submit', function(e) {
            e.preventDefault();
            
            const formData = new FormData(this);
            const type = formData.get('type');
            const description = formData.get('description');
            const amount = formData.get('amount');
            
            alert(`${type} record "${description}" for $${amount} added successfully!`);
            
            // Close modal
            const modal = bootstrap.Modal.getInstance(document.getElementById('addFinancialRecordModal'));
            if (modal) {
                modal.hide();
            }
            
            // Reset form
            this.reset();
            const transactionDateInput = document.querySelector('input[name="transactionDate"]');
            if (transactionDateInput) {
                transactionDateInput.value = new Date().toISOString().split('T')[0];
            }
        });
    }


    const transactionDateInput = document.querySelector('input[name="transactionDate"]');
    if (transactionDateInput) {
        transactionDateInput.value = new Date().toISOString().split('T')[0];
    }

    // Handle rainfall form submission (duplicate)
    // Removed duplicate alert-only handler to defer to Water.cshtml page script.
    // const rainfallForm = document.getElementById('rainfallForm');
    // if (rainfallForm) {
    //     rainfallForm.addEventListener('submit', function(e) {
    //         e.preventDefault();
    //         const formData = new FormData(this);
    //         const amountMl = formData.get('amountMl');
    //         const campNumber = window.selectedCamp || 1;
    //         // Removed alert and modal hide here; page handles submission and reload.
    //     });
    // }
});


// Send message function for chat
function sendMessage() {
    const input = document.getElementById('chatInput');
    if (!input) return;
    
    const message = input.value.trim();
    
    if (message) {
        const chatWindow = document.querySelector('.chat-window');
        if (chatWindow) {
            const newMessage = document.createElement('div');
            newMessage.className = 'chat-message mb-2';
            newMessage.innerHTML = `<strong>You:</strong> ${message}`;
            
            chatWindow.appendChild(newMessage);
            chatWindow.scrollTop = chatWindow.scrollHeight;
            input.value = '';
        }
    }
}

// Show camp details function for Water page
function showCampDetails(campNumber) {
    if (typeof window !== 'undefined') {
        window.selectedCamp = campNumber;
    }
    
    // Update camp header
    const campHeader = document.getElementById('campHeader');
    if (!campHeader) return;
    
    campHeader.textContent = `Camp ${campNumber}`;
    
    // Update camp details content
    const campDetails = document.getElementById('campDetails');
    if (!campDetails) return;
    
    campDetails.innerHTML = `
        <div class="row">
            <div class="col-6">
                <h6>Previous rainfall in camp ${campNumber}:</h6>
                <ul class="list-unstyled">
                    <li>08/02/2025: 10ml</li>
                    <li>20/03/2025: 8ml</li>
                </ul>
            </div>
            <div class="col-6">
                <h6>Latest rain report for camp ${campNumber}:</h6>
                <p class="text-info">46 days ago you have 8ml rain in camp ${campNumber}</p>
            </div>
        </div>
        <hr>
        <div class="text-center">
            <button class="btn btn-info" onclick="openAddRainfallModal(${campNumber})">
                <i class="fas fa-plus me-2"></i>Add Rain for camp ${campNumber}
            </button>
        </div>
    `;
    
    // Highlight selected camp button
    const campButtons = document.querySelectorAll('.camp-grid-btn');
    if (campButtons.length > 0) {
        campButtons.forEach(btn => {
            btn.classList.remove('btn-info');
            btn.classList.add('btn-outline-info');
        });
        
        const selectedButton = document.querySelector(`.camp-grid-btn[data-camp="${campNumber}"]`) || 
                              document.querySelector(`.camp-grid-btn:nth-child(${campNumber})`);
        if (selectedButton) {
            selectedButton.classList.remove('btn-outline-info');
            selectedButton.classList.add('btn-info');
        }
    }
}

// Open add rainfall modal function
function openAddRainfallModal(campNumber) {
    const modalCampNumber = document.getElementById('modalCampNumber');
    if (modalCampNumber) {
        modalCampNumber.textContent = campNumber;
    }
    
    const rainfallForm = document.getElementById('rainfallForm');
    if (rainfallForm) {
        rainfallForm.reset();
    }
    
    const rainfallDate = document.querySelector('input[name="rainfallDate"]');
    if (rainfallDate) {
        rainfallDate.value = new Date().toISOString().split('T')[0];
    }
    
    const modal = document.getElementById('addRainfallModal');
    if (modal) {
        const bsModal = new bootstrap.Modal(modal);
        bsModal.show();
    }
}

(function () {
  function qs(sel, ctx) { return (ctx || document).querySelector(sel); }
  function qsa(sel, ctx) { return Array.from((ctx || document).querySelectorAll(sel)); }

  function applyFilters(table, filters, searchTerm) {
    const rows = qsa('tbody tr', table);
    const term = (searchTerm || '').trim().toLowerCase();
    rows.forEach(row => {
      let visible = true;
      // Apply chip filters
      Object.entries(filters).forEach(([key, value]) => {
        if (!value) return;
        const attr = (row.getAttribute(`data-${key}`) || '').toLowerCase();
        const v = String(value).toLowerCase();
        if (attr !== v) visible = false;
      });
      // Apply text search over text content
      if (visible && term) {
        const text = row.textContent.toLowerCase();
        if (!text.includes(term)) visible = false;
      }
      row.style.display = visible ? '' : 'none';
    });
  }

  function wireTableControls(container) {
    const search = qs('[data-role="table-search"]', container) || container;
    const chips = qsa('.filter-chip', container);
    const clearBtn = qs('.clear-filters', container);
    const density = qs('.density-toggle', container);
    const targetSel = (search.getAttribute('data-target') || density?.getAttribute('data-target') || chips[0]?.getAttribute('data-target'));
    const table = targetSel ? qs(targetSel) : qs('table', container);
    if (!table) return;

    const filters = {};

    chips.forEach(chip => {
      chip.addEventListener('click', () => {
        const key = chip.getAttribute('data-filter');
        const value = chip.getAttribute('data-value');
        // toggle selection
        if (filters[key] === value) {
          filters[key] = null;
          chip.classList.remove('active');
        } else {
          // unset other chips of same key
          qsa(`.filter-chip[data-filter="${key}"]`, container).forEach(c => c.classList.remove('active'));
          filters[key] = value;
          chip.classList.add('active');
        }
        applyFilters(table, filters, search.value);
      });
    });

    if (search && search.tagName === 'INPUT') {
      search.addEventListener('input', () => applyFilters(table, filters, search.value));
    }

    if (clearBtn) {
      clearBtn.addEventListener('click', () => {
        Object.keys(filters).forEach(k => filters[k] = null);
        chips.forEach(c => c.classList.remove('active'));
        if (search && search.tagName === 'INPUT') search.value = '';
        applyFilters(table, filters, '');
      });
    }

    if (density) {
      density.addEventListener('click', () => {
        table.classList.toggle('table-compact');
      });
    }
  }

  document.addEventListener('DOMContentLoaded', () => {
    qsa('.card-body').forEach(wireTableControls);
  });
})();
 
 
 // Send message function for chat
function sendMessage() {
    const input = document.getElementById('chatInput');
    if (!input) return;
    
    const message = input.value.trim();
    
    if (message) {
        const chatWindow = document.querySelector('.chat-window');
        if (chatWindow) {
            const newMessage = document.createElement('div');
            newMessage.className = 'chat-message mb-2';
            newMessage.innerHTML = `<strong>You:</strong> ${message}`;
            
            chatWindow.appendChild(newMessage);
            chatWindow.scrollTop = chatWindow.scrollHeight;
            input.value = '';
        }
    }
}

// Show camp details function for Water page
function showCampDetails(campNumber) {
    if (typeof window !== 'undefined') {
        window.selectedCamp = campNumber;
    }
    
    // Update camp header
    const campHeader = document.getElementById('campHeader');
    if (!campHeader) return;
    
    campHeader.textContent = `Camp ${campNumber}`;
    
    // Update camp details content
    const campDetails = document.getElementById('campDetails');
    if (!campDetails) return;
    
    campDetails.innerHTML = `
        <div class="row">
            <div class="col-6">
                <h6>Previous rainfall in camp ${campNumber}:</h6>
                <ul class="list-unstyled">
                    <li>08/02/2025: 10ml</li>
                    <li>20/03/2025: 8ml</li>
                </ul>
            </div>
            <div class="col-6">
                <h6>Latest rain report for camp ${campNumber}:</h6>
                <p class="text-info">46 days ago you have 8ml rain in camp ${campNumber}</p>
            </div>
        </div>
        <hr>
        <div class="text-center">
            <button class="btn btn-info" onclick="openAddRainfallModal(${campNumber})">
                <i class="fas fa-plus me-2"></i>Add Rain for camp ${campNumber}
            </button>
        </div>
    `;
    
    // Highlight selected camp button
    const campButtons = document.querySelectorAll('.camp-grid-btn');
    if (campButtons.length > 0) {
        campButtons.forEach(btn => {
            btn.classList.remove('btn-info');
            btn.classList.add('btn-outline-info');
        });
        
        const selectedButton = document.querySelector(`.camp-grid-btn[data-camp="${campNumber}"]`) || 
                              document.querySelector(`.camp-grid-btn:nth-child(${campNumber})`);
        if (selectedButton) {
            selectedButton.classList.remove('btn-outline-info');
            selectedButton.classList.add('btn-info');
        }
    }
}

// Open add rainfall modal function
function openAddRainfallModal(campNumber) {
    const modalCampNumber = document.getElementById('modalCampNumber');
    if (modalCampNumber) {
        modalCampNumber.textContent = campNumber;
    }
    
    const rainfallForm = document.getElementById('rainfallForm');
    if (rainfallForm) {
        rainfallForm.reset();
    }
    
    const rainfallDate = document.querySelector('input[name="rainfallDate"]');
    if (rainfallDate) {
        rainfallDate.value = new Date().toISOString().split('T')[0];
    }
    
    const modal = document.getElementById('addRainfallModal');
    if (modal) {
        const bsModal = new bootstrap.Modal(modal);
        bsModal.show();
    }
}
