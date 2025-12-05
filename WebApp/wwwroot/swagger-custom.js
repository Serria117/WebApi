// wwwroot/swagger-custom.js
(function() {
    // Wait for Swagger UI to load
    window.addEventListener('load', function() {
        // Create login button
        const authorizeBtn = document.querySelector('.btn.authorize');
        if (authorizeBtn) {
            const loginBtn = document.createElement('button');
            loginBtn.className = 'btn';
            loginBtn.textContent = '🔐 Login';
            loginBtn.style.marginRight = '10px';
            loginBtn.style.backgroundColor = '#49cc90';
            loginBtn.style.color = 'white';
            loginBtn.style.border = 'none';
            loginBtn.style.padding = '8px 15px';
            loginBtn.style.borderRadius = '4px';
            loginBtn.style.cursor = 'pointer';

            loginBtn.addEventListener('click', function() {
                showLoginModal();
            });

            authorizeBtn.parentNode.insertBefore(loginBtn, authorizeBtn);
        }
    });

    function showLoginModal() {
        // Create modal
        const modal = document.createElement('div');
        modal.style.position = 'fixed';
        modal.style.top = '0';
        modal.style.left = '0';
        modal.style.width = '100%';
        modal.style.height = '100%';
        modal.style.backgroundColor = 'rgba(0,0,0,0.5)';
        modal.style.display = 'flex';
        modal.style.justifyContent = 'center';
        modal.style.alignItems = 'center';
        modal.style.zIndex = '9999';

        // Create modal content
        const modalContent = document.createElement('div');
        modalContent.style.backgroundColor = 'white';
        modalContent.style.padding = '20px';
        modalContent.style.borderRadius = '8px';
        modalContent.style.width = '300px';

        modalContent.innerHTML = `
            <h3>Login</h3>
            <div style="margin-bottom: 15px;">
                <label style="display: block; margin-bottom: 5px;">Username:</label>
                <input type="text" id="swagger-username" style="width: 100%; padding: 8px; border: 1px solid #ccc; border-radius: 4px;" value="admin">
            </div>
            <div style="margin-bottom: 15px;">
                <label style="display: block; margin-bottom: 5px;">Password:</label>
                <input type="password" id="swagger-password" style="width: 100%; padding: 8px; border: 1px solid #ccc; border-radius: 4px;" value="password">
            </div>
            <div style="display: flex; justify-content: space-between;">
                <button id="swagger-login-btn" style="background-color: #49cc90; color: white; border: none; padding: 8px 15px; border-radius: 4px; cursor: pointer;">Login</button>
                <button id="swagger-cancel-btn" style="background-color: #ccc; color: black; border: none; padding: 8px 15px; border-radius: 4px; cursor: pointer;">Cancel</button>
            </div>
            <div id="swagger-login-message" style="margin-top: 10px; color: red;"></div>
        `;

        modal.appendChild(modalContent);
        document.body.appendChild(modal);

        // Add event listeners
        document.getElementById('swagger-login-btn').addEventListener('click', performLogin);
        document.getElementById('swagger-cancel-btn').addEventListener('click', function() {
            document.body.removeChild(modal);
        });

        // Close modal when clicking outside
        modal.addEventListener('click', function(e) {
            if (e.target === modal) {
                document.body.removeChild(modal);
            }
        });

        // Handle Enter key
        document.getElementById('swagger-password').addEventListener('keypress', function(e) {
            if (e.key === 'Enter') {
                performLogin();
            }
        });
    }

    function performLogin() {
        const username = document.getElementById('swagger-username').value;
        const password = document.getElementById('swagger-password').value;
        const messageDiv = document.getElementById('swagger-login-message');

        messageDiv.textContent = 'Logging in...';
        messageDiv.style.color = 'blue';

        fetch('/api/Auth/login', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({
                username: username,
                password: password
            })
        })
            .then(response => {
                if (!response.ok) {
                    throw new Error('Login failed');
                }
                return response.json();
            })
            .then(data => {
                if (data.token) {
                    // Set the token in Swagger UI
                    const authInput = document.querySelector('input[placeholder="Bearer token"]');
                    if (authInput) {
                        authInput.value = data.token;

                        // Trigger input event to update Swagger UI
                        const event = new Event('input', { bubbles: true });
                        authInput.dispatchEvent(event);

                        // Click authorize button to close the modal
                        const authorizeBtn = document.querySelector('.btn.authorize');
                        if (authorizeBtn) {
                            authorizeBtn.click();
                        }
                    }

                    messageDiv.textContent = 'Login successful!';
                    messageDiv.style.color = 'green';

                    // Close modal after success
                    setTimeout(() => {
                        const modal = document.querySelector('div[style*="position: fixed"]');
                        if (modal) {
                            document.body.removeChild(modal);
                        }
                    }, 1000);
                }
            })
            .catch(error => {
                messageDiv.textContent = 'Login failed: ' + error.message;
                messageDiv.style.color = 'red';
            });
    }
})();