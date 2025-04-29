(function () {
    window.addEventListener("load", function () {
        // Can thiệp vào Swagger UI
        const ui = SwaggerUIBundle({
            url: "/swagger/v1/swagger.json",
            dom_id: "#swagger-ui",
            presets: [
                SwaggerUIBundle.presets.apis,
                SwaggerUIStandalonePreset
            ],
            onComplete: function () {
                // Lắng nghe sự kiện gửi yêu cầu
                ui.preauthorizeApiKey = function (key, value) {
                    if (key === "Bearer") {
                        // Lưu token vào localStorage hoặc xử lý theo cách bạn muốn
                        localStorage.setItem("access_token", value);
                    }
                };

                // Tự động thêm token vào header Authorization
                ui.interceptors.requestInterceptor = function (request) {
                    const token = localStorage.getItem("access_token");
                    if (token) {
                        request.headers.Authorization = `Bearer ${token}`;
                    }
                    return request;
                };
            }
        });

        // Lắng nghe response từ API login
        ui.interceptors.responseInterceptor = function (response) {
            if (response.url.includes("/api/login") && response.status === 200) {
                try {
                    const data = JSON.parse(response.body);
                    const token = data.accessToken; // Giả sử API trả về access_token
                    if (token) {
                        localStorage.setItem("access_token", token);
                        ui.preauthorizeApiKey("Bearer", token);
                    }
                } catch (e) {
                    console.error("Error parsing login response", e);
                }
            }
            return response;
        };
    });
})();